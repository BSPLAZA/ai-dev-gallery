// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
    public required string Name { get; set; }
    public required string FilePath { get; set; }  // Local file path provided by user
    public DatasetFormat Format { get; set; }
    public DateTime? LastValidated { get; set; }
    public int? EstimatedItemCount { get; set; }
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
    Cancelled   // Stopped by user
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