// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AIDevGallery.Models;

/// <summary>
/// Represents an evaluation result with scores on a 0-5 scale
/// </summary>
public class EvaluationResult
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string ModelName { get; set; }
    public required string DatasetName { get; set; }
    public int DatasetItemCount { get; set; }
    public int ItemCount => DatasetItemCount; // Alias for compatibility
    public EvaluationWorkflow WorkflowType { get; set; }
    public EvaluationStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeSpan? Duration { get; set; }
    
    /// <summary>
    /// Criteria scores on a 0-5 scale
    /// </summary>
    public Dictionary<string, double> CriteriaScores { get; set; } = new();
    
    /// <summary>
    /// Calculate average score from all criteria
    /// </summary>
    public double AverageScore => CriteriaScores.Count > 0 
        ? Math.Round(CriteriaScores.Values.Average(), 1) 
        : 0.0;
    
    /// <summary>
    /// Get score rating category based on average
    /// </summary>
    public ScoreRating Rating => AverageScore switch
    {
        >= 4.5 => ScoreRating.Excellent,
        >= 3.75 => ScoreRating.Good,
        >= 3.0 => ScoreRating.Fair,
        _ => ScoreRating.NeedsImprovement
    };
    
    /// <summary>
    /// For running evaluations - progress percentage
    /// </summary>
    public double? ProgressPercentage { get; set; }
    public double? RunningProgress => ProgressPercentage; // Alias for compatibility
    
    /// <summary>
    /// For running evaluations - current operation
    /// </summary>
    public string? CurrentOperation { get; set; }
    
    /// <summary>
    /// Path to the imported JSONL file (for Import Results workflow)
    /// </summary>
    public string? SourceFilePath { get; set; }
    
    /// <summary>
    /// Base directory path for the dataset (used to resolve relative image paths)
    /// </summary>
    public string? DatasetBasePath { get; set; }
    
    /// <summary>
    /// Individual results for each evaluated item (e.g., images).
    /// This is populated when detailed results are available.
    /// </summary>
    public List<EvaluationItemResult>? ItemResults { get; set; }
    
    /// <summary>
    /// Statistics grouped by folder.
    /// Key: folder path, Value: folder statistics.
    /// </summary>
    public Dictionary<string, FolderStats>? FolderStatistics { get; set; }
    
    /// <summary>
    /// Gets a value indicating whether individual item results are available.
    /// </summary>
    public bool HasDetailedResults => ItemResults?.Count > 0;
    
    /// <summary>
    /// Gets a value indicating whether folder statistics are available.
    /// </summary>
    public bool HasFolderStatistics => FolderStatistics?.Count > 0;
}

/// <summary>
/// Score rating categories
/// </summary>
public enum ScoreRating
{
    Excellent,        // 4.5-5.0
    Good,            // 3.75-4.49
    Fair,            // 3.0-3.74
    NeedsImprovement // Below 3.0
}