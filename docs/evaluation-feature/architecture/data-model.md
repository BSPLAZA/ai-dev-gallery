# Evaluation Data Model - ARCHIVED

**Status: ARCHIVED - June 18, 2025**  
**Note: This document represents an early design that has evolved. See EvaluationModels.cs for current implementation.**

## Why This Document Was Archived

This data model design was created during initial planning but the implementation has evolved to better support our three-workflow approach (TestModel, EvaluateResponses, ImportResults). The current focus has shifted to leveraging the Import Results workflow for immediate visualization value.

## Key Differences from Current Implementation

1. **Simpler Model Configuration**: Instead of complex ModelConfiguration, we use simple fields
2. **Two-Part Upload**: Current implementation supports JSONL + image folder uploads
3. **Three Workflows**: Clear separation between TestModel, EvaluateResponses, and ImportResults
4. **Validation Focus**: Comprehensive validation for imported data with detailed error reporting

## For Current Data Models See:
- `AIDevGallery/Models/EvaluationModels.cs` - Actual implementation
- `AIDevGallery/Models/EvaluationWizardState.cs` - Wizard state management

---

# Original Design Document (Archived)

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
    public Dictionary<string, object>? Summary { get; set; }   // Summary statistics
    public List<string>? ErrorLog { get; set; }                // Any errors during execution
}
```

### 3. Enumerations

```csharp
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
    Draft,          // Being configured
    Ready,          // Ready to run
    Running,        // Currently executing
    Completed,      // Finished successfully
    Failed,         // Execution failed
    Cancelled       // Stopped by user
}

internal enum CriteriaType
{
    Numeric,        // 0.0 - 1.0 score
    Boolean,        // Pass/Fail
    Text,           // Descriptive feedback
    Scale           // 1-5 rating scale
}

internal enum DatasetType
{
    Local,          // Files on disk
    Remote,         // HTTP/HTTPS URLs
    Generated       // Created by the app
}

internal enum DatasetFormat
{
    JSONL,          // JSON Lines format
    CSV,            // Comma-separated values
    Images,         // Folder of images
    JSON,           // Single JSON file
    Custom          // User-defined format
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

## Data Persistence

### File System Structure
```
%LOCALAPPDATA%\AI Dev Gallery\
├── settings.json                          # Existing app settings
├── evaluations/
│   ├── configurations/
│   │   ├── {id}.json                     # Individual evaluation configs
│   │   └── index.json                    # Quick lookup index
│   ├── runs/
│   │   ├── {evaluation-id}/
│   │   │   ├── {run-id}/
│   │   │   │   ├── config.json          # Run configuration
│   │   │   │   ├── results.jsonl        # Detailed results
│   │   │   │   ├── summary.json         # Aggregate results
│   │   │   │   └── logs/               # Execution logs
│   └── datasets/
│       ├── uploaded/                     # User-uploaded datasets
│       └── cache/                        # Cached remote datasets
```

### Settings.json Integration
```json
{
  "ModelCachePath": "...",
  "UsageHistoryV2": [...],
  "SavedEvaluations": {
    "{id}": {
      "Id": "...",
      "Name": "...",
      "LastRun": "2024-01-20T10:30:00Z",
      "Status": "Completed"
    }
  },
  "EvaluationHistory": [
    {
      "Id": "...",
      "EvaluationId": "...",
      "Started": "2024-01-20T10:30:00Z",
      "Status": "Completed"
    }
  ],
  "DefaultEvaluationsPath": "%LOCALAPPDATA%\\AI Dev Gallery\\evaluations"
}
```

## JSON Schemas

### Evaluation Configuration Schema
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "GPT-4 Image Description Quality",
  "type": "ImageDescription",
  "goal": "Evaluate GPT-4's ability to describe complex scenes",
  "created": "2024-01-20T10:00:00Z",
  "lastModified": "2024-01-20T10:30:00Z",
  "modelConfig": {
    "modelId": "gpt-4o",
    "modelName": "GPT-4 with Vision",
    "apiEndpoint": "https://api.openai.com/v1/chat/completions",
    "baselinePrompt": "Describe this image in detail, including...",
    "parameters": {
      "temperature": 0.7,
      "max_tokens": 500
    }
  },
  "criteria": [
    {
      "id": "accuracy",
      "name": "Accuracy",
      "description": "How accurately the description matches the image",
      "type": "Numeric",
      "weight": 2.0,
      "isRequired": true
    }
  ],
  "dataset": {
    "id": "coco-val-subset",
    "name": "COCO Validation Subset",
    "type": "Local",
    "path": "./datasets/coco-val-subset.jsonl",
    "format": "JSONL",
    "itemCount": 500,
    "lastValidated": "2024-01-19T15:00:00Z"
  },
  "status": "Ready"
}
```

### Evaluation Results Schema (JSONL format)
```jsonl
{"id":"item-001","input":{"image":"./images/cat.jpg","prompt":"Describe this image"},"output":{"response":"A tabby cat sitting on a windowsill...","responseTime":1.23},"scores":{"accuracy":0.92,"relevance":0.88,"completeness":0.85},"metadata":{"timestamp":"2024-01-20T10:31:00Z"}}
{"id":"item-002","input":{"image":"./images/landscape.jpg","prompt":"Describe this image"},"output":{"response":"A mountain landscape at sunset...","responseTime":1.45},"scores":{"accuracy":0.95,"relevance":0.91,"completeness":0.93},"metadata":{"timestamp":"2024-01-20T10:31:02Z"}}
```

## Integration Points

### 1. MainWindow Navigation
```csharp
// Add to navigation items
new NavigationViewItem
{
    Content = "Evaluate",
    Icon = new FontIcon { Glyph = "\uE9D9" },
    Tag = "evaluate"
}
```

### 2. Settings Management
```csharp
// In AppData.cs
public async Task SaveEvaluationAsync(EvaluationConfiguration config)
{
    // Save to configurations folder
    // Update SavedEvaluations dictionary
    // Persist settings.json
}

public async Task<List<EvaluationConfiguration>> LoadEvaluationsAsync()
{
    // Load from configurations folder
    // Return sorted list
}
```

### 3. Usage History Integration
```csharp
// Track evaluation runs like model usage
public class EvaluationUsageHistory : UsageHistory
{
    public string EvaluationId { get; set; }
    public string RunId { get; set; }
    public int ItemsProcessed { get; set; }
    public Dictionary<string, double> Scores { get; set; }
}
```

## API Integration

### Model Provider Interface
```csharp
public interface IEvaluationModelProvider
{
    Task<string> GenerateResponseAsync(
        string prompt, 
        string imagePath, 
        Dictionary<string, object> parameters);
        
    Task<bool> ValidateCredentialsAsync();
}

// Implementations for each provider
public class OpenAIEvaluationProvider : IEvaluationModelProvider { }
public class AzureOpenAIEvaluationProvider : IEvaluationModelProvider { }
public class LocalModelEvaluationProvider : IEvaluationModelProvider { }
```

## Future Considerations

1. **Batch Processing**: Queue system for large evaluations
2. **Comparison View**: Side-by-side evaluation comparisons
3. **Export Formats**: CSV, Excel, PDF reports
4. **Scheduling**: Run evaluations on schedule
5. **Team Features**: Share evaluations and results
6. **Cloud Sync**: Backup and sync evaluations
7. **Custom Metrics**: User-defined evaluation criteria
8. **A/B Testing**: Compare multiple models simultaneously