// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AIDevGallery.Models;

/// <summary>
/// Represents a complete evaluation configuration following established AppData patterns
/// </summary>
internal class EvaluationConfiguration
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public EvaluationType Type { get; set; }
    public EvaluationWorkflow Workflow { get; set; } = EvaluationWorkflow.TestModel;
    public string? Goal { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    
    // Model Configuration
    public required string SelectedModelId { get; set; }
    public required string SelectedModelName { get; set; }
    public required string ApiEndpoint { get; set; }
    public required string BaselinePrompt { get; set; }
    
    // Evaluation Criteria (Step 3)
    public List<EvaluationCriteria>? Criteria { get; set; }
    
    // Dataset Configuration (Step 4) - Following local file path approach
    public DatasetConfiguration? Dataset { get; set; }
    
    // Metrics Configuration (Step 5)
    public EvaluationMetrics? Metrics { get; set; }
    
    // Status tracking
    public EvaluationStatus Status { get; set; }
    public DateTime? LastRun { get; set; }
}

/// <summary>
/// Individual evaluation criteria/metric
/// </summary>
internal class EvaluationCriteria
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public CriteriaType Type { get; set; }
    public double Weight { get; set; } = 1.0;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Dataset configuration for local file paths
/// </summary>
internal class DatasetConfiguration
{
    // Basic info
    public string Name { get; set; } = "";
    public string FilePath => SourcePath;  // Alias for compatibility
    public int EstimatedItemCount => ValidEntries;  // Alias for compatibility
    
    // Source information
    public required string SourcePath { get; set; }  // JSONL file or folder path
    public DatasetSourceType SourceType { get; set; }
    public string BaseDirectory { get; set; } = "";  // For relative path resolution
    
    // Dataset statistics
    public int TotalEntries { get; set; }
    public int ValidEntries { get; set; }
    public bool ExceedsLimit { get; set; }
    
    // Organization
    public Dictionary<string, int> FolderStructure { get; set; } = new();
    public PathType PathTypes { get; set; }  // Absolute, Relative, or Mixed
    
    // Entries (limited to 1,000)
    public List<DatasetEntry> Entries { get; set; } = new();
    
    // Validation results
    public ValidationResult ValidationResult { get; set; } = new();
}

/// <summary>
/// Individual dataset entry
/// </summary>
internal class DatasetEntry
{
    public required string OriginalImagePath { get; set; }  // As specified in JSONL
    public required string ResolvedImagePath { get; set; }  // Full resolved path
    public required string Prompt { get; set; }
    
    // Workflow-specific fields
    public string? Response { get; set; }  // For EvaluateResponses
    public string? Model { get; set; }  // For EvaluateResponses
    public Dictionary<string, object>? Scores { get; set; }  // For ImportResults
}

/// <summary>
/// Validation result for dataset
/// </summary>
internal class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public ValidationSummary? Summary { get; set; }
}

/// <summary>
/// Individual validation issue
/// </summary>
internal class ValidationIssue
{
    public IssueType Type { get; set; }
    public required string Message { get; set; }
    public int? LineNumber { get; set; }
}

/// <summary>
/// Validation summary statistics
/// </summary>
internal class ValidationSummary
{
    public int TotalFiles { get; set; }
    public int ValidFiles { get; set; }
    public int MissingFiles { get; set; }
    public int UnsupportedFormats { get; set; }
}

/// <summary>
/// Represents an evaluation run execution (following existing UsageHistory pattern)
/// </summary>
internal class EvaluationRun
{
    public required string Id { get; set; }
    public required string EvaluationId { get; set; }
    public DateTime Started { get; set; }
    public DateTime? Completed { get; set; }
    public RunStatus Status { get; set; }
    public int Progress { get; set; } = 0;
    public int? ItemsProcessed { get; set; }
    public int? TotalItems { get; set; }
    public string? ResultsPath { get; set; }  // Path to detailed results file
}

/// <summary>
/// Evaluation metrics configuration
/// </summary>
internal class EvaluationMetrics
{
    // Automated metrics
    public bool UseSpice { get; set; } = true;  // Default on
    public bool UseClipScore { get; set; } = true;  // Default on
    public bool UseMeteor { get; set; }
    public bool UseLengthStats { get; set; }
    
    // AI Judge
    public bool UseAIJudge { get; set; }
    public List<CustomCriterion> CustomCriteria { get; set; } = new();
}

/// <summary>
/// Custom criterion for AI Judge evaluation
/// </summary>
internal class CustomCriterion : INotifyPropertyChanged
{
    public int Id { get; set; }
    public required string Name { get; set; }  // Free text
    public required string Description { get; set; }  // Free text
    public bool IsEnabled { get; set; } = true;
    
    // For UI display
    public string DisplayNumber => $"Criterion {Id}";
    
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Evaluation type enumeration
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EvaluationType>))]
internal enum EvaluationType
{
    ImageDescription,
    TextSummarization,
    Translation,
    QuestionAnswering,
    CustomTask
}

/// <summary>
/// Evaluation workflow type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EvaluationWorkflow>))]
internal enum EvaluationWorkflow
{
    TestModel,          // Generate responses + evaluate
    EvaluateResponses,  // Evaluate existing responses
    ImportResults       // Import completed results
}

/// <summary>
/// Evaluation status following existing app state patterns
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EvaluationStatus>))]
internal enum EvaluationStatus
{
    Draft,      // Being configured
    Ready,      // Ready to run
    Running,    // Currently executing
    Completed,  // Finished successfully
    Failed,     // Execution failed
    Cancelled,  // Stopped by user
    Imported    // Imported from external JSONL file
}

/// <summary>
/// Criteria type for evaluation metrics
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CriteriaType>))]
internal enum CriteriaType
{
    Numeric,    // 0.0 - 1.0 score
    Boolean,    // Pass/Fail
    Text,       // Descriptive feedback
    Scale       // 1-5 rating scale
}

/// <summary>
/// Dataset format types for local files
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DatasetFormat>))]
internal enum DatasetFormat
{
    JSONL,      // JSON Lines format
    CSV,        // Comma-separated values
    JSON,       // Single JSON file
    Custom      // User-defined format
}

/// <summary>
/// Run status for evaluation execution
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<RunStatus>))]
internal enum RunStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Dataset source type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DatasetSourceType>))]
internal enum DatasetSourceType
{
    JsonlFile,
    ImageFolder
}

/// <summary>
/// Path type in dataset
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PathType>))]
internal enum PathType
{
    Absolute,
    Relative,
    Mixed
}

/// <summary>
/// Issue type for validation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<IssueType>))]
internal enum IssueType
{
    Error,
    Warning,
    Info,
    InvalidJson
}