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
    /// <summary>
    /// Gets or sets the unique identifier for this evaluation result
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the evaluation
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the model that was evaluated
    /// </summary>
    public required string ModelName { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the dataset used for evaluation
    /// </summary>
    public required string DatasetName { get; set; }
    
    /// <summary>
    /// Gets or sets the number of items in the dataset
    /// </summary>
    public int DatasetItemCount { get; set; }
    
    /// <summary>
    /// Gets the number of items in the dataset (alias for compatibility)
    /// </summary>
    public int ItemCount => DatasetItemCount; // Alias for compatibility
    
    /// <summary>
    /// Gets or sets the type of evaluation workflow used
    /// </summary>
    public EvaluationWorkflow WorkflowType { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the evaluation
    /// </summary>
    public EvaluationStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the evaluation was created or completed
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the duration of the evaluation execution
    /// </summary>
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
    /// <summary>
    /// For running evaluations - progress percentage
    /// </summary>
    public double? ProgressPercentage { get; set; }
    
    /// <summary>
    /// Gets the running progress percentage (alias for compatibility)
    /// </summary>
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
    /// <summary>
    /// Excellent score (4.5-5.0)
    /// </summary>
    Excellent,        // 4.5-5.0
    
    /// <summary>
    /// Good score (3.75-4.49)
    /// </summary>
    Good,            // 3.75-4.49
    
    /// <summary>
    /// Fair score (3.0-3.74)
    /// </summary>
    Fair,            // 3.0-3.74
    
    /// <summary>
    /// Needs improvement (below 3.0)
    /// </summary>
    NeedsImprovement // Below 3.0
}