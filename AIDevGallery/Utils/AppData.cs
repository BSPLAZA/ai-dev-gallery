// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using AIDevGallery.Telemetry;
using Microsoft.ML.OnnxRuntime;
using Microsoft.Windows.AI.ContentSafety;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace AIDevGallery.Utils;

internal class AppData
{
    private static readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    public required string ModelCachePath { get; set; }
    public required LinkedList<MostRecentlyUsedItem> MostRecentlyUsedItems { get; set; }
    public CustomParametersState? LastCustomParamtersState { get; set; }

    public LinkedList<UsageHistory>? UsageHistoryV2 { get; set; }

    public bool IsDiagnosticDataEnabled { get; set; }

    public bool IsFirstRun { get; set; }

    public bool IsDiagnosticsMessageDismissed { get; set; }
    public Dictionary<string, List<string>>? ModelTypeToUserAddedModelsMapping { get; set; }

    public string LastAdapterPath { get; set; }

    public string LastSystemPrompt { get; set; }

    public WinMlSampleOptions WinMLSampleOptions { get; set; }

    // Evaluation Management (following existing patterns)
    public LinkedList<EvaluationConfiguration>? EvaluationHistory { get; set; }
    public LinkedList<EvaluationRun>? EvaluationRuns { get; set; }

    private Dictionary<string, Dictionary<string, string>>? SampleData { get; set; }

    public AppData()
    {
        IsDiagnosticDataEnabled = !PrivacyConsentHelpers.IsPrivacySensitiveRegion();
        IsFirstRun = true;
        IsDiagnosticsMessageDismissed = false;
        LastAdapterPath = string.Empty;
        LastSystemPrompt = string.Empty;
        WinMLSampleOptions = new WinMlSampleOptions(ExecutionProviderDevicePolicy.DEFAULT, null, false);
    }

    private static string GetConfigFilePath()
    {
        var appDataFolder = ApplicationData.Current.LocalFolder.Path;
        return Path.Combine(appDataFolder, "state.json");
    }

    public static async Task<AppData> GetForApp()
    {
        AppData? appData = null;

        var configFile = GetConfigFilePath();
        await _saveSemaphore.WaitAsync();

        try
        {
            if (File.Exists(configFile))
            {
                var file = await File.ReadAllTextAsync(configFile);
                appData = JsonSerializer.Deserialize(file, AppDataSourceGenerationContext.Default.AppData);
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            appData ??= GetDefault();
            _saveSemaphore.Release();
        }

        return appData;
    }

    public async Task SaveAsync()
    {
        await _saveSemaphore.WaitAsync();
        try
        {
            var str = JsonSerializer.Serialize(this, AppDataSourceGenerationContext.Default.AppData);
            await File.WriteAllTextAsync(GetConfigFilePath(), str);
        }
        finally
        {
            _saveSemaphore.Release();
        }
    }

    public async Task AddMru(MostRecentlyUsedItem item, List<(string Id, HardwareAccelerator HardwareAccelerator)>? modelOrApiUsage)
    {
        UsageHistoryV2 ??= new LinkedList<UsageHistory>();

        foreach (var toRemove in MostRecentlyUsedItems.Where(i => i.ItemId == item.ItemId).ToArray())
        {
            MostRecentlyUsedItems.Remove(toRemove);
        }

        if (MostRecentlyUsedItems.Count > 5)
        {
            MostRecentlyUsedItems.RemoveLast();
        }

        if (modelOrApiUsage != null)
        {
            foreach (var (modelOrApiId, hardwareAccelerator) in modelOrApiUsage)
            {
                var existingItem = UsageHistoryV2.Where(u => u.Id == modelOrApiId).FirstOrDefault();
                if (existingItem != default)
                {
                    UsageHistoryV2.Remove(existingItem);
                }

                UsageHistoryV2.AddFirst(new UsageHistory(modelOrApiId, hardwareAccelerator));
            }
        }

        MostRecentlyUsedItems.AddFirst(item);
        await SaveAsync();
    }

    public async Task AddModelUsage(List<(string Id, HardwareAccelerator HardwareAccelerator)> usage)
    {
        UsageHistoryV2 ??= new LinkedList<UsageHistory>();

        foreach (var (modelOrApiId, hardwareAccelerator) in usage)
        {
            var existingItem = UsageHistoryV2.Where(u => u.Id == modelOrApiId).FirstOrDefault();
            if (existingItem != default)
            {
                UsageHistoryV2.Remove(existingItem);
            }

            UsageHistoryV2.AddFirst(new UsageHistory(modelOrApiId, hardwareAccelerator));
        }

        await SaveAsync();
    }

    public void AddModelTypeToUserAddedModelsMappingEntry(ModelType modelType, string modelId)
    {
        string modelTypeString = modelType.ToString();

        if(ModelTypeToUserAddedModelsMapping is null)
        {
            ModelTypeToUserAddedModelsMapping = new Dictionary<string, List<string>>();
        }

        if(ModelTypeToUserAddedModelsMapping.TryGetValue(modelTypeString, out List<string>? value))
        {
            value.Add(modelId);
        }
        else
        {
            ModelTypeToUserAddedModelsMapping[modelTypeString] = [modelId];
        }
    }

    public async Task DeleteUserAddedModelMapping(string modelId)
    {
        if(ModelTypeToUserAddedModelsMapping == null)
        {
            return;
        }

        foreach (string modelType in ModelTypeToUserAddedModelsMapping.Keys)
        {
            ModelTypeToUserAddedModelsMapping[modelType].Remove(modelId);
        }

        await SaveAsync();
    }

    // does not persist between sessions
    public async Task SetSampleDataAsync(string sampleName, string key, string data)
    {
        if (SampleData == null)
        {
            SampleData = new Dictionary<string, Dictionary<string, string>>();
        }

        if (!SampleData.TryGetValue(sampleName, out Dictionary<string, string>? value))
        {
            value = new Dictionary<string, string>();
            SampleData[sampleName] = value;
        }

        if (!value.TryAdd(key, data))
        {
            value[key] = data;
        }

        await SaveAsync();
    }

    public string? GetSampleData(string sampleName, string key)
    {
        if (SampleData == null)
        {
            return null;
        }

        if (SampleData.TryGetValue(sampleName, out Dictionary<string, string>? value))
        {
            if (value.TryGetValue(key, out string? data))
            {
                return data;
            }
        }

        return null;
    }

    public bool TryGetUserAddedModelIds(ModelType type, out List<string>? modelIds)
    {
        if (ModelTypeToUserAddedModelsMapping == null)
        {
            modelIds = null;
            return false;
        }

        return ModelTypeToUserAddedModelsMapping.TryGetValue(type.ToString(), out modelIds);
    }

    /// <summary>
    /// Adds or updates an evaluation configuration following existing MRU patterns
    /// </summary>
    public async Task AddOrUpdateEvaluationAsync(EvaluationConfiguration evaluation)
    {
        EvaluationHistory ??= new LinkedList<EvaluationConfiguration>();

        // Remove existing evaluation with same ID (update scenario)
        var existing = EvaluationHistory.FirstOrDefault(e => e.Id == evaluation.Id);
        if (existing != null)
        {
            EvaluationHistory.Remove(existing);
        }

        // Add to front (most recent first, following MRU pattern)
        EvaluationHistory.AddFirst(evaluation);

        // Keep only last 20 evaluations (following existing capacity patterns)
        while (EvaluationHistory.Count > 20)
        {
            EvaluationHistory.RemoveLast();
        }

        await SaveAsync();
    }

    /// <summary>
    /// Gets an evaluation configuration by ID
    /// </summary>
    public EvaluationConfiguration? GetEvaluation(string evaluationId)
    {
        return EvaluationHistory?.FirstOrDefault(e => e.Id == evaluationId);
    }

    /// <summary>
    /// Deletes an evaluation configuration
    /// </summary>
    public async Task DeleteEvaluationAsync(string evaluationId)
    {
        if (EvaluationHistory == null) return;

        var evaluation = EvaluationHistory.FirstOrDefault(e => e.Id == evaluationId);
        if (evaluation != null)
        {
            EvaluationHistory.Remove(evaluation);
            await SaveAsync();
        }
    }

    /// <summary>
    /// Adds an evaluation run record (following existing usage tracking patterns)
    /// </summary>
    public async Task AddEvaluationRunAsync(EvaluationRun run)
    {
        EvaluationRuns ??= new LinkedList<EvaluationRun>();

        // Add to front (most recent first)
        EvaluationRuns.AddFirst(run);

        // Keep only last 100 runs (reasonable for performance tracking)
        while (EvaluationRuns.Count > 100)
        {
            EvaluationRuns.RemoveLast();
        }

        await SaveAsync();
    }

    /// <summary>
    /// Gets evaluation runs for a specific evaluation
    /// </summary>
    public List<EvaluationRun> GetEvaluationRuns(string evaluationId)
    {
        if (EvaluationRuns == null) return new List<EvaluationRun>();
        
        return EvaluationRuns.Where(r => r.EvaluationId == evaluationId)
                            .OrderByDescending(r => r.Started)
                            .ToList();
    }

    /// <summary>
    /// Validates dataset file paths and removes invalid evaluations (following ModelCache validation pattern)
    /// </summary>
    public async Task ValidateEvaluationsAsync()
    {
        if (EvaluationHistory == null) return;

        var validEvaluations = new List<EvaluationConfiguration>();
        
        foreach (var evaluation in EvaluationHistory)
        {
            bool isValid = true;
            
            // Validate dataset file if specified
            if (evaluation.Dataset?.FilePath != null)
            {
                try
                {
                    isValid = File.Exists(evaluation.Dataset.FilePath);
                }
                catch
                {
                    isValid = false;
                }
            }
            
            if (isValid)
            {
                validEvaluations.Add(evaluation);
            }
        }

        if (validEvaluations.Count != EvaluationHistory.Count)
        {
            EvaluationHistory.Clear();
            foreach (var evaluation in validEvaluations)
            {
                EvaluationHistory.AddLast(evaluation);
            }
            await SaveAsync();
        }
    }

    private static AppData GetDefault()
    {
        var homeDirPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var cacheDir = Path.Combine(homeDirPath, ".cache", "aigallery");

        return new AppData
        {
            ModelCachePath = cacheDir,
            MostRecentlyUsedItems = new(),
            UsageHistoryV2 = new()
        };
    }
}

internal class CustomParametersState
{
    public bool? DoSample { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public int? TopK { get; set; }
    public float? TopP { get; set; }
    public float? Temperature { get; set; }
    public string? UserPrompt { get; set; }
    public string? SystemPrompt { get; set; }
    public SeverityLevel? InputContentModeration { get; set; }
    public SeverityLevel? OutputContentModeration { get; set; }
}

internal record UsageHistory(string Id, HardwareAccelerator? HardwareAccelerator);

internal record WinMlSampleOptions(ExecutionProviderDevicePolicy? Policy, string? EpName, bool CompileModel);