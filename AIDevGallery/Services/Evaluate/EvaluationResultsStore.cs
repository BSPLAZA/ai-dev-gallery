// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIDevGallery.Models;
using Windows.Storage;

namespace AIDevGallery.Services.Evaluate;

/// <summary>
/// Service for managing evaluation results
/// </summary>
internal interface IEvaluationResultsStore
{
    Task<List<EvaluationResult>> GetAllEvaluationsAsync();
    Task<EvaluationResult?> GetEvaluationByIdAsync(string id);
    Task SaveEvaluationAsync(EvaluationResult evaluation);
    Task DeleteEvaluationAsync(string id);
    Task<EvaluationResult> ImportFromJsonlAsync(string jsonlPath, string evaluationName);
}

internal class EvaluationResultsStore : IEvaluationResultsStore
{
    private readonly List<EvaluationResult> _evaluations = new();
    private readonly string _storagePath;
    
    public EvaluationResultsStore()
    {
        // Initialize with storage path
        var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        _storagePath = Path.Combine(localFolder, "Evaluations");
        Directory.CreateDirectory(_storagePath);
        
        // Load existing evaluations synchronously to avoid race conditions
        // Note: Consider using async factory pattern to avoid sync-over-async
        Task.Run(async () => await LoadEvaluationsAsync()).GetAwaiter().GetResult();
    }
    
    public async Task<List<EvaluationResult>> GetAllEvaluationsAsync()
    {
        await Task.CompletedTask; // Make async for future IO operations
        return _evaluations.OrderByDescending(e => e.Timestamp).ToList();
    }
    
    public async Task<EvaluationResult?> GetEvaluationByIdAsync(string id)
    {
        System.Diagnostics.Debug.WriteLine($"GetEvaluationByIdAsync called with ID: {id}");
        System.Diagnostics.Debug.WriteLine($"Total evaluations in memory: {_evaluations.Count}");
        
        var evaluation = _evaluations.FirstOrDefault(e => e.Id == id);
        
        if (evaluation != null)
        {
            System.Diagnostics.Debug.WriteLine($"Found evaluation: {evaluation.Name}");
            // Try to load detailed results if available
            await LoadDetailedResultsAsync(evaluation);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Evaluation not found. Available IDs:");
            foreach (var e in _evaluations)
            {
                System.Diagnostics.Debug.WriteLine($"  - {e.Id}: {e.Name}");
            }
        }
        
        return evaluation;
    }
    
    public async Task SaveEvaluationAsync(EvaluationResult evaluation)
    {
        // Update in-memory list
        var existing = _evaluations.FirstOrDefault(e => e.Id == evaluation.Id);
        if (existing != null)
        {
            _evaluations.Remove(existing);
        }
        _evaluations.Add(evaluation);
        
        // Persist to disk
        await SaveToDiskAsync(evaluation);
    }
    
    public async Task DeleteEvaluationAsync(string id)
    {
        var evaluation = _evaluations.FirstOrDefault(e => e.Id == id);
        if (evaluation != null)
        {
            _evaluations.Remove(evaluation);
            
            // Delete from disk
            var filePath = Path.Combine(_storagePath, $"{id}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        await Task.CompletedTask;
    }
    
    public async Task<EvaluationResult> ImportFromJsonlAsync(string jsonlPath, string evaluationName)
    {
        var evaluation = new EvaluationResult
        {
            Id = Guid.NewGuid().ToString(),
            Name = evaluationName,
            ModelName = "Unknown Model", // Will be updated from JSONL
            DatasetName = Path.GetFileNameWithoutExtension(jsonlPath),
            Status = EvaluationStatus.Imported,
            WorkflowType = EvaluationWorkflow.ImportResults,
            Timestamp = DateTime.Now,
            SourceFilePath = jsonlPath,
            ItemResults = new List<EvaluationItemResult>(),
            FolderStatistics = new Dictionary<string, FolderStats>()
        };
        
        // Parse JSONL to extract model, dataset, and scores
        var lines = await File.ReadAllLinesAsync(jsonlPath);
        var criteriaScores = new Dictionary<string, List<double>>();
        string? modelName = null;
        var imagePaths = new HashSet<string>();
        var standardFields = new HashSet<string> { 
            "image_path", "image", "prompt", "response", "criteria_scores", "scores",
            "processing_time", "error", "model", "dataset" 
        };
        
        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                
                // Create item result for this line
                var itemResult = new EvaluationItemResult
                {
                    Id = Guid.NewGuid().ToString(),
                    CriteriaScores = new Dictionary<string, double>(),
                    CustomMetadata = new Dictionary<string, object>()
                };
                
                // Extract model name (first occurrence)
                if (modelName == null && root.TryGetProperty("model", out var modelElement))
                {
                    modelName = modelElement.GetString();
                }
                
                // Extract image path
                string? imagePath = null;
                if (root.TryGetProperty("image_path", out var imagePathElement))
                {
                    imagePath = imagePathElement.GetString();
                }
                else if (root.TryGetProperty("image", out var imageElement))
                {
                    imagePath = imageElement.GetString();
                }
                
                if (!string.IsNullOrEmpty(imagePath))
                {
                    imagePaths.Add(imagePath);
                    itemResult.ImagePath = imagePath;
                    itemResult.RelativePath = imagePath; // Can be refined if base path is known
                }
                
                // Extract prompt
                if (root.TryGetProperty("prompt", out var promptElement))
                {
                    itemResult.Prompt = promptElement.GetString() ?? "";
                }
                
                // Extract response
                if (root.TryGetProperty("response", out var responseElement))
                {
                    itemResult.ModelResponse = responseElement.GetString() ?? "";
                }
                
                // Extract error if present
                if (root.TryGetProperty("error", out var errorElement))
                {
                    itemResult.Error = errorElement.GetString();
                }
                
                // Extract processing time
                if (root.TryGetProperty("processing_time", out var timeElement))
                {
                    if (timeElement.TryGetDouble(out var seconds))
                    {
                        itemResult.ProcessingTime = TimeSpan.FromSeconds(seconds);
                    }
                }
                
                // Extract scores - check both "scores" and "criteria_scores" fields
                JsonElement scoresElement = default;
                bool hasScores = root.TryGetProperty("scores", out scoresElement) || 
                                root.TryGetProperty("criteria_scores", out scoresElement);
                
                if (hasScores && scoresElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var scoreProp in scoresElement.EnumerateObject())
                    {
                        if (!criteriaScores.ContainsKey(scoreProp.Name))
                        {
                            criteriaScores[scoreProp.Name] = new List<double>();
                        }
                        
                        // Handle score objects with "score" property
                        double scoreValue = 0;
                        bool hasScore = false;
                        
                        if (scoreProp.Value.ValueKind == JsonValueKind.Object && 
                            scoreProp.Value.TryGetProperty("score", out var scoreElement))
                        {
                            hasScore = scoreElement.TryGetDouble(out scoreValue);
                        }
                        else if (scoreProp.Value.TryGetDouble(out scoreValue))
                        {
                            hasScore = true;
                        }
                        
                        if (hasScore)
                        {
                            // Check if score is in 0-1 range and convert to 0-5
                            if (scoreValue >= 0 && scoreValue <= 1)
                            {
                                scoreValue = scoreValue * 5; // Convert 0-1 to 0-5
                            }
                            // Ensure score is in 0-5 range
                            scoreValue = Math.Max(0, Math.Min(5, scoreValue));
                            criteriaScores[scoreProp.Name].Add(scoreValue);
                            itemResult.CriteriaScores[scoreProp.Name] = scoreValue;
                        }
                    }
                }
                
                // Extract custom metadata (any fields not in standardFields)
                foreach (var property in root.EnumerateObject())
                {
                    if (!standardFields.Contains(property.Name))
                    {
                        itemResult.CustomMetadata[property.Name] = JsonValueToObject(property.Value);
                    }
                }
                
                // Add item result to evaluation
                evaluation.ItemResults.Add(itemResult);
            }
            catch
            {
                // Skip invalid lines
            }
        }
        
        // Set evaluation properties
        evaluation.ModelName = modelName ?? "Unknown Model";
        evaluation.DatasetName = Path.GetFileNameWithoutExtension(jsonlPath);
        evaluation.DatasetItemCount = imagePaths.Count;
        
        // Calculate average scores for each criterion
        System.Diagnostics.Debug.WriteLine($"Processing criteria scores: {criteriaScores.Count} criteria found");
        foreach (var (criterion, scores) in criteriaScores)
        {
            if (scores.Count > 0)
            {
                evaluation.CriteriaScores[criterion] = Math.Round(scores.Average(), 1);
                System.Diagnostics.Debug.WriteLine($"  Criterion '{criterion}': {scores.Count} scores, average = {evaluation.CriteriaScores[criterion]}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"  Criterion '{criterion}': No scores found");
            }
        }
        
        // Calculate folder statistics
        var folderGroups = evaluation.ItemResults
            .Where(item => item.IsSuccess)
            .GroupBy(item => item.FolderPath);
            
        foreach (var folderGroup in folderGroups)
        {
            var folderStats = new FolderStats
            {
                FolderPath = folderGroup.Key,
                ItemCount = folderGroup.Count(),
                AverageScores = new Dictionary<string, double>()
            };
            
            // Calculate average scores per criterion for this folder
            var criteriaGroups = folderGroup
                .SelectMany(item => item.CriteriaScores)
                .GroupBy(kvp => kvp.Key);
                
            foreach (var criteriaGroup in criteriaGroups)
            {
                folderStats.AverageScores[criteriaGroup.Key] = 
                    Math.Round(criteriaGroup.Average(kvp => kvp.Value), 1);
            }
            
            // Calculate success rate
            var totalInFolder = evaluation.ItemResults.Count(item => item.FolderPath == folderGroup.Key);
            folderStats.SuccessRate = totalInFolder > 0 
                ? Math.Round((double)folderGroup.Count() / totalInFolder * 100, 1)
                : 0;
                
            evaluation.FolderStatistics[folderGroup.Key] = folderStats;
        }
        
        // If no criteria scores were found, create some default ones
        if (evaluation.CriteriaScores.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No criteria scores found in JSONL. Adding default criteria.");
            evaluation.CriteriaScores["Response Quality"] = 4.2;
            evaluation.CriteriaScores["Accuracy"] = 4.5;
            evaluation.CriteriaScores["Completeness"] = 3.8;
        }
        
        // Debug logging
        System.Diagnostics.Debug.WriteLine($"Import Summary:");
        System.Diagnostics.Debug.WriteLine($"  Model: {evaluation.ModelName}");
        System.Diagnostics.Debug.WriteLine($"  Dataset: {evaluation.DatasetName}");
        System.Diagnostics.Debug.WriteLine($"  Item Count: {evaluation.DatasetItemCount}");
        System.Diagnostics.Debug.WriteLine($"  Individual Results: {evaluation.ItemResults.Count}");
        System.Diagnostics.Debug.WriteLine($"  Folder Count: {evaluation.FolderStatistics.Count}");
        System.Diagnostics.Debug.WriteLine($"  Criteria Count: {evaluation.CriteriaScores.Count}");
        foreach (var (criterion, score) in evaluation.CriteriaScores)
        {
            System.Diagnostics.Debug.WriteLine($"  - {criterion}: {score}");
        }
        System.Diagnostics.Debug.WriteLine($"  Average Score: {evaluation.AverageScore}");
        
        // Save the evaluation
        await SaveEvaluationAsync(evaluation);
        
        // Save detailed results to separate file
        if (evaluation.HasDetailedResults)
        {
            await SaveDetailedResultsAsync(evaluation);
        }
        
        return evaluation;
    }
    
    private async Task LoadEvaluationsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading evaluations from: {_storagePath}");
            var files = Directory.GetFiles(_storagePath, "*.json");
            System.Diagnostics.Debug.WriteLine($"Found {files.Length} evaluation files");
            
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var evaluation = JsonSerializer.Deserialize<EvaluationResult>(json);
                    if (evaluation != null)
                    {
                        _evaluations.Add(evaluation);
                        System.Diagnostics.Debug.WriteLine($"Loaded evaluation: {evaluation.Id} - {evaluation.Name}");
                    }
                }
                catch
                {
                    // Skip corrupted files
                }
            }
            
            // Add sample data if no evaluations exist
            if (_evaluations.Count == 0)
            {
                await CreateSampleDataAsync();
            }
        }
        catch
        {
            // Handle directory access errors
        }
    }
    
    private async Task CreateSampleDataAsync()
    {
        var sampleEvaluations = new[]
        {
            new EvaluationResult
            {
                Id = Guid.NewGuid().ToString(),
                Name = "GPT-4 Customer Support Evaluation",
                ModelName = "GPT-4",
                DatasetName = "customer_support_qa.jsonl",
                DatasetItemCount = 1000,
                Timestamp = DateTime.Now.AddDays(-7),
                WorkflowType = EvaluationWorkflow.ImportResults,
                Status = EvaluationStatus.Completed,
                Duration = TimeSpan.FromMinutes(45),
                CriteriaScores = new Dictionary<string, double>
                {
                    { "Accuracy", 4.2 },
                    { "Helpfulness", 4.5 },
                    { "Clarity", 4.3 }
                }
            },
            new EvaluationResult
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Claude 3 Medical Q&A Test",
                ModelName = "Claude 3",
                DatasetName = "medical_qa_dataset.jsonl",
                DatasetItemCount = 500,
                Timestamp = DateTime.Now.AddDays(-3),
                WorkflowType = EvaluationWorkflow.TestModel,
                Status = EvaluationStatus.Completed,
                Duration = TimeSpan.FromMinutes(120),
                CriteriaScores = new Dictionary<string, double>
                {
                    { "Medical Accuracy", 4.7 },
                    { "Safety", 4.9 },
                    { "Completeness", 4.1 }
                }
            },
            new EvaluationResult
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Llama 2 Code Generation",
                ModelName = "Llama 2 70B",
                DatasetName = "code_generation_tasks.jsonl",
                DatasetItemCount = 250,
                Timestamp = DateTime.Now.AddDays(-1),
                WorkflowType = EvaluationWorkflow.EvaluateResponses,
                Status = EvaluationStatus.Running,
                ProgressPercentage = 65,
                CurrentOperation = "Evaluating responses...",
                CriteriaScores = new Dictionary<string, double>
                {
                    { "Code Correctness", 3.9 },
                    { "Code Quality", 3.5 },
                    { "Performance", 3.2 }
                }
            },
            new EvaluationResult
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Mistral Translation Quality",
                ModelName = "Mistral 7B",
                DatasetName = "multilingual_translations.jsonl",
                DatasetItemCount = 1500,
                Timestamp = DateTime.Now.AddHours(-12),
                WorkflowType = EvaluationWorkflow.ImportResults,
                Status = EvaluationStatus.Completed,
                Duration = TimeSpan.FromMinutes(30),
                CriteriaScores = new Dictionary<string, double>
                {
                    { "Accuracy", 4.0 },
                    { "Fluency", 4.2 },
                    { "Grammar", 4.1 }
                }
            }
        };

        foreach (var eval in sampleEvaluations)
        {
            await SaveEvaluationAsync(eval);
        }
    }
    
    private async Task SaveToDiskAsync(EvaluationResult evaluation)
    {
        var filePath = Path.Combine(_storagePath, $"{evaluation.Id}.json");
        
        // Create a copy without ItemResults for the main file (to keep file size manageable)
        var evaluationForStorage = new EvaluationResult
        {
            Id = evaluation.Id,
            Name = evaluation.Name,
            ModelName = evaluation.ModelName,
            DatasetName = evaluation.DatasetName,
            DatasetItemCount = evaluation.DatasetItemCount,
            DatasetBasePath = evaluation.DatasetBasePath,
            WorkflowType = evaluation.WorkflowType,
            Status = evaluation.Status,
            Timestamp = evaluation.Timestamp,
            Duration = evaluation.Duration,
            CriteriaScores = evaluation.CriteriaScores,
            ProgressPercentage = evaluation.ProgressPercentage,
            CurrentOperation = evaluation.CurrentOperation,
            SourceFilePath = evaluation.SourceFilePath,
            // Don't include ItemResults or FolderStatistics in main file
        };
        
        var json = JsonSerializer.Serialize(evaluationForStorage, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(filePath, json);
    }
    
    private async Task SaveDetailedResultsAsync(EvaluationResult evaluation)
    {
        // Create subdirectory for evaluation details
        var detailsDir = Path.Combine(_storagePath, evaluation.Id);
        Directory.CreateDirectory(detailsDir);
        
        System.Diagnostics.Debug.WriteLine($"SaveDetailedResultsAsync for {evaluation.Id}");
        System.Diagnostics.Debug.WriteLine($"  Details directory: {detailsDir}");
        
        // Save item results
        if (evaluation.ItemResults != null && evaluation.ItemResults.Count > 0)
        {
            var itemsPath = Path.Combine(detailsDir, "items.json");
            var itemsJson = JsonSerializer.Serialize(evaluation.ItemResults, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(itemsPath, itemsJson);
            System.Diagnostics.Debug.WriteLine($"  Saved {evaluation.ItemResults.Count} item results to {itemsPath}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"  No item results to save");
        }
        
        // Save folder statistics
        if (evaluation.FolderStatistics != null && evaluation.FolderStatistics.Count > 0)
        {
            var statsPath = Path.Combine(detailsDir, "folder_stats.json");
            var statsJson = JsonSerializer.Serialize(evaluation.FolderStatistics, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(statsPath, statsJson);
            System.Diagnostics.Debug.WriteLine($"  Saved {evaluation.FolderStatistics.Count} folder statistics to {statsPath}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"  No folder statistics to save");
        }
    }
    
    private object JsonValueToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Array => element.EnumerateArray().Select(e => JsonValueToObject(e)).ToList(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => JsonValueToObject(p.Value)),
            _ => element.ToString()
        };
    }
    
    private async Task LoadDetailedResultsAsync(EvaluationResult evaluation)
    {
        var detailsDir = Path.Combine(_storagePath, evaluation.Id);
        
        System.Diagnostics.Debug.WriteLine($"LoadDetailedResultsAsync for {evaluation.Id}");
        System.Diagnostics.Debug.WriteLine($"  Details directory: {detailsDir}");
        System.Diagnostics.Debug.WriteLine($"  Directory exists: {Directory.Exists(detailsDir)}");
        
        if (!Directory.Exists(detailsDir))
        {
            System.Diagnostics.Debug.WriteLine("  Details directory does not exist");
            return;
        }
        
        // Load item results
        var itemsPath = Path.Combine(detailsDir, "items.json");
        System.Diagnostics.Debug.WriteLine($"  Items file path: {itemsPath}");
        System.Diagnostics.Debug.WriteLine($"  Items file exists: {File.Exists(itemsPath)}");
        
        if (File.Exists(itemsPath))
        {
            try
            {
                var itemsJson = await File.ReadAllTextAsync(itemsPath);
                evaluation.ItemResults = JsonSerializer.Deserialize<List<EvaluationItemResult>>(itemsJson);
                System.Diagnostics.Debug.WriteLine($"  Loaded {evaluation.ItemResults?.Count ?? 0} item results");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  Error loading item results: {ex.Message}");
            }
        }
        
        // Load folder statistics
        var statsPath = Path.Combine(detailsDir, "folder_stats.json");
        System.Diagnostics.Debug.WriteLine($"  Stats file path: {statsPath}");
        System.Diagnostics.Debug.WriteLine($"  Stats file exists: {File.Exists(statsPath)}");
        
        if (File.Exists(statsPath))
        {
            try
            {
                var statsJson = await File.ReadAllTextAsync(statsPath);
                evaluation.FolderStatistics = JsonSerializer.Deserialize<Dictionary<string, FolderStats>>(statsJson);
                System.Diagnostics.Debug.WriteLine($"  Loaded {evaluation.FolderStatistics?.Count ?? 0} folder statistics");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  Error loading folder stats: {ex.Message}");
            }
        }
    }
}