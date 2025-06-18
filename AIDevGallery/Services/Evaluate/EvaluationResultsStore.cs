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
        
        // Load existing evaluations
        _ = LoadEvaluationsAsync();
    }
    
    public async Task<List<EvaluationResult>> GetAllEvaluationsAsync()
    {
        await Task.CompletedTask; // Make async for future IO operations
        return _evaluations.OrderByDescending(e => e.Timestamp).ToList();
    }
    
    public async Task<EvaluationResult?> GetEvaluationByIdAsync(string id)
    {
        await Task.CompletedTask;
        return _evaluations.FirstOrDefault(e => e.Id == id);
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
            SourceFilePath = jsonlPath
        };
        
        // Parse JSONL to extract model, dataset, and scores
        var lines = await File.ReadAllLinesAsync(jsonlPath);
        var criteriaScores = new Dictionary<string, List<double>>();
        string? modelName = null;
        var imagePaths = new HashSet<string>();
        
        foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                
                // Extract model name (first occurrence)
                if (modelName == null && root.TryGetProperty("model", out var modelElement))
                {
                    modelName = modelElement.GetString();
                }
                
                // Extract image path
                if (root.TryGetProperty("image", out var imageElement))
                {
                    imagePaths.Add(imageElement.GetString() ?? "");
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
                        
                        if (scoreProp.Value.TryGetDouble(out var score))
                        {
                            // Check if score is in 0-1 range and convert to 1-5
                            if (score >= 0 && score <= 1)
                            {
                                score = score * 4 + 1; // Convert 0-1 to 1-5
                            }
                            // Ensure score is in 1-5 range
                            score = Math.Max(1, Math.Min(5, score));
                            criteriaScores[scoreProp.Name].Add(score);
                        }
                    }
                }
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
        foreach (var (criterion, scores) in criteriaScores)
        {
            if (scores.Count > 0)
            {
                evaluation.CriteriaScores[criterion] = Math.Round(scores.Average(), 1);
            }
        }
        
        // Debug logging
        System.Diagnostics.Debug.WriteLine($"Import Summary:");
        System.Diagnostics.Debug.WriteLine($"  Model: {evaluation.ModelName}");
        System.Diagnostics.Debug.WriteLine($"  Dataset: {evaluation.DatasetName}");
        System.Diagnostics.Debug.WriteLine($"  Item Count: {evaluation.DatasetItemCount}");
        System.Diagnostics.Debug.WriteLine($"  Criteria Count: {evaluation.CriteriaScores.Count}");
        foreach (var (criterion, score) in evaluation.CriteriaScores)
        {
            System.Diagnostics.Debug.WriteLine($"  - {criterion}: {score}");
        }
        System.Diagnostics.Debug.WriteLine($"  Average Score: {evaluation.AverageScore}");
        
        // Save the evaluation
        await SaveEvaluationAsync(evaluation);
        
        return evaluation;
    }
    
    private async Task LoadEvaluationsAsync()
    {
        try
        {
            var files = Directory.GetFiles(_storagePath, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var evaluation = JsonSerializer.Deserialize<EvaluationResult>(json);
                    if (evaluation != null)
                    {
                        _evaluations.Add(evaluation);
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
        var json = JsonSerializer.Serialize(evaluation, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(filePath, json);
    }
}