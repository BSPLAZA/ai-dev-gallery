# Comprehensive Evaluation Data Model Design

## Current Architecture Integration

### 1. Extend AppData.cs (Main State)
```csharp
internal class AppData
{
    // Existing properties...
    public required string ModelCachePath { get; set; }
    public LinkedList<UsageHistory>? UsageHistoryV2 { get; set; }
    
    // NEW: Evaluation Management
    public Dictionary<string, EvaluationConfiguration>? SavedEvaluations { get; set; }
    public LinkedList<EvaluationRun>? EvaluationHistory { get; set; }
    public Dictionary<string, DatasetConfiguration>? Datasets { get; set; }
    public string? DefaultEvaluationsPath { get; set; } // Where evaluation files are stored
}
```

### 2. Core Evaluation Data Structures

```csharp
// Complete Evaluation Configuration
internal class EvaluationConfiguration
{
    public required string Id { get; set; }                    // Unique identifier (GUID)
    public required string Name { get; set; }                  // User-friendly name
    public EvaluationType Type { get; set; }                   // ImageDescription, etc.
    public string? Goal { get; set; }                          // What user wants to achieve
    public DateTime Created { get; set; }                      // Creation timestamp
    public DateTime? LastModified { get; set; }                // Last edit timestamp
    
    // Model Configuration
    public required ModelConfiguration ModelConfig { get; set; }
    
    // Evaluation Criteria & Metrics
    public required List<EvaluationCriteria> Criteria { get; set; }
    
    // Test Dataset
    public required DatasetConfiguration Dataset { get; set; }
    
    // Status & Progress
    public EvaluationStatus Status { get; set; }               // Draft, Ready, Running, Complete
    public int? LastRunProgress { get; set; }                  // 0-100 percentage
}

// Model Configuration (API Settings)
internal class ModelConfiguration
{
    public required string ModelId { get; set; }               // "gpt-4o", "phi-4-multimodal"
    public required string ModelName { get; set; }             // Display name
    public required string ApiEndpoint { get; set; }           // API URL
    public required string BaselinePrompt { get; set; }        // Evaluation prompt
    public Dictionary<string, object>? Parameters { get; set; } // temperature, max_tokens, etc.
    
    // API Key stored separately in Windows Credential Manager
    // Key: "AI_DEV_GALLERY_EVAL_{ModelId.ToUpper()}_API_KEY"
}

// Evaluation Criteria & Metrics
internal class EvaluationCriteria
{
    public required string Id { get; set; }                    // "accuracy", "relevance"
    public required string Name { get; set; }                  // "Accuracy", "Relevance"
    public string? Description { get; set; }                   // What this measures
    public CriteriaType Type { get; set; }                     // Numeric, Boolean, Text
    public double Weight { get; set; } = 1.0;                  // Importance multiplier
    public bool IsRequired { get; set; } = true;               // Must be evaluated
    public Dictionary<string, object>? Configuration { get; set; } // Criteria-specific settings
}

// Dataset Configuration
internal class DatasetConfiguration
{
    public required string Id { get; set; }                    // Unique identifier
    public required string Name { get; set; }                  // "COCO Validation Set"
    public DatasetType Type { get; set; }                      // Local, Remote, Generated
    public required string Path { get; set; }                  // File path or URL
    public DatasetFormat Format { get; set; }                  // JSONL, CSV, Images
    public int? ItemCount { get; set; }                        // Number of test cases
    public DateTime? LastValidated { get; set; }               // When dataset was verified
    public Dictionary<string, object>? Metadata { get; set; }  // Additional dataset info
}

// Evaluation Run (Individual Execution)
internal class EvaluationRun
{
    public required string Id { get; set; }                    // Unique run identifier
    public required string EvaluationId { get; set; }          // Links to EvaluationConfiguration
    public DateTime Started { get; set; }                      // Execution start time
    public DateTime? Completed { get; set; }                   // Execution end time
    public RunStatus Status { get; set; }                      // Running, Completed, Failed
    public int Progress { get; set; } = 0;                     // 0-100 percentage
    public int? ItemsProcessed { get; set; }                   // Number of items completed
    public int? TotalItems { get; set; }                       // Total items to process
    
    // Results & Metrics
    public Dictionary<string, double>? AggregateScores { get; set; } // Overall scores by criteria
    public string? ResultsPath { get; set; }                   // Path to detailed JSONL results
    public Dictionary<string, object>? Metadata { get; set; }  // Run-specific data
}

// Enums
internal enum EvaluationType
{
    ImageDescription,
    TextSummarization,
    Translation,
    QuestionAnswering,
    CustomTask
}

internal enum EvaluationStatus
{
    Draft,      // Being configured
    Ready,      // Ready to run
    Running,    // Currently executing
    Completed,  // Finished successfully
    Failed,     // Execution failed
    Cancelled   // Stopped by user
}

internal enum CriteriaType
{
    Numeric,    // 0.0 - 1.0 score
    Boolean,    // Pass/Fail
    Text,       // Descriptive feedback
    MultiChoice // Selected from options
}

internal enum DatasetType
{
    Local,      // Files on disk
    Remote,     // URL download
    Generated   // Programmatically created
}

internal enum DatasetFormat
{
    JSONL,      // JSON Lines format
    CSV,        // Comma-separated values
    Images,     // Directory of images
    Custom      // User-defined format
}

internal enum RunStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}
```

## 3. File System Organization

### Directory Structure
```
%LocalAppData%/Packages/[AppId]/LocalState/
├── state.json                           # Main app state (includes evaluation configs)
├── evaluations/                         # Evaluation-specific data
│   ├── {evaluation-id}/                 # Per-evaluation folder
│   │   ├── config.json                  # Detailed configuration backup
│   │   ├── datasets/                    # Dataset files
│   │   │   ├── images/                  # Image files for testing
│   │   │   └── test-data.jsonl          # Test cases
│   │   └── results/                     # Evaluation results
│   │       ├── {run-id}-results.jsonl   # Detailed per-item results
│   │       ├── {run-id}-summary.json    # Aggregate scores
│   │       └── {run-id}-logs.txt        # Execution logs
│   └── shared-datasets/                 # Reusable datasets
│       ├── coco-validation/
│       └── custom-images/
└── model-cache/                         # Existing model storage
```

### JSONL Results Format
```jsonl
{"item_id": "001", "input": "path/to/image.jpg", "expected": "A cat sitting on a windowsill", "actual": "A tabby cat resting by a window", "scores": {"accuracy": 0.85, "relevance": 0.90}, "timestamp": "2025-01-15T10:30:00Z"}
{"item_id": "002", "input": "path/to/image2.jpg", "expected": "A red car in traffic", "actual": "A red sedan in busy street", "scores": {"accuracy": 0.92, "relevance": 0.88}, "timestamp": "2025-01-15T10:30:15Z"}
```

## 4. Source Generation Updates

```csharp
[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true)]
[JsonSerializable(typeof(AppData))]
[JsonSerializable(typeof(Dictionary<string, EvaluationConfiguration>))]
[JsonSerializable(typeof(LinkedList<EvaluationRun>))]
[JsonSerializable(typeof(Dictionary<string, DatasetConfiguration>))]
[JsonSerializable(typeof(EvaluationConfiguration))]
[JsonSerializable(typeof(EvaluationRun))]
[JsonSerializable(typeof(DatasetConfiguration))]
[JsonSerializable(typeof(ModelConfiguration))]
[JsonSerializable(typeof(EvaluationCriteria))]
internal partial class AppDataSourceGenerationContext : JsonSerializerContext
```

## 5. Data Flow & Management

### Wizard Data Flow
```
Step 1: Select Type → EvaluationConfiguration.Type
Step 2: Basic Details → EvaluationConfiguration.Name, Goal, ModelConfig
Step 3: Criteria → EvaluationConfiguration.Criteria[]
Step 4: Dataset → EvaluationConfiguration.Dataset
Step 5: Review → Save to AppData.SavedEvaluations[id]
```

### Execution Flow
```
Start Evaluation → Create EvaluationRun
├── Load Configuration
├── Validate Dataset
├── Execute Tests (with progress updates)
├── Save Results to JSONL
├── Calculate Aggregate Scores
└── Update EvaluationRun.Status = Completed
```

### Storage Operations
```csharp
// Save evaluation configuration
await appData.SaveEvaluationAsync(evaluationConfig);

// Start evaluation run
var run = await appData.StartEvaluationRunAsync(evaluationId);

// Update progress
await appData.UpdateRunProgressAsync(runId, progress, itemsCompleted);

// Save results
await appData.SaveEvaluationResultsAsync(runId, results);
```

## 6. Integration with Existing Patterns

### Thread Safety
- Use existing SemaphoreSlim pattern from AppData
- Atomic saves for evaluation configurations
- Progress updates use existing thread-safe patterns

### Credential Management
- API keys continue using Windows Credential Manager
- Same naming convention: "AI_DEV_GALLERY_EVAL_{MODEL}_API_KEY"
- No credentials stored in JSON files

### MRU Integration
- Add recent evaluations to existing MostRecentlyUsedItems
- Follow same deduplication and capacity limits
- Use existing navigation patterns

### Usage Tracking
- Extend existing UsageHistory to track evaluation runs
- Maintain compatibility with current telemetry
- Track model usage during evaluations

This design provides comprehensive evaluation management while seamlessly integrating with the existing AIDevGallery architecture and patterns.