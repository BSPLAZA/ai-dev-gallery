# Architecture Overview

## High-Level Architecture

The Evaluation Feature follows a modular architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        UI Layer                              │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │   Pages     │  │   Controls   │  │     Dialogs      │  │
│  │             │  │              │  │                  │  │
│  │ EvaluatePage│  │ EvaluationCard│ │  WizardDialog   │  │
│  │ InsightsPage│  │ SelectionBar  │ │ ComparisonDialog│  │
│  └─────────────┘  └──────────────┘  └──────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    ViewModel Layer                           │
│  ┌──────────────────┐  ┌────────────────┐                  │
│  │ EvaluationListVM │  │ EvaluationCardVM│                 │
│  └──────────────────┘  └────────────────┘                  │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     Service Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌────────────┐ │
│  │ResultsStore     │  │StatisticsCalc   │  │ExportService│ │
│  │                 │  │                 │  │            │ │
│  └─────────────────┘  └─────────────────┘  └────────────┘ │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                      Data Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐                  │
│  │  LocalStorage   │  │   Model Classes │                  │
│  │ ApplicationData │  │ EvaluationResult│                  │
│  └─────────────────┘  └─────────────────┘                  │
└─────────────────────────────────────────────────────────────┘
```

## Component Structure

### Pages
- `EvaluatePage.xaml/cs` - Main evaluation list view
- `EvaluationInsightsPage.xaml/cs` - Detailed analysis view
- `ComparisonView.xaml/cs` - Multi-evaluation comparison

### Controls
- `EvaluationCard.xaml/cs` - Card view component
- `EvaluationListRow.xaml/cs` - List row component
- `SelectionActionBar.xaml/cs` - Bulk operations toolbar
- `ScoreDistributionChart.xaml/cs` - Score visualization
- `MetricsRadarChart.xaml/cs` - Multi-metric chart
- `StatisticsCard.xaml/cs` - Statistical summary

### Wizard Components
- `WizardDialog.xaml/cs` - Base wizard framework
- `WorkflowSelectionPage.xaml/cs` - Step 1
- `EvaluationTypePage.xaml/cs` - Step 2
- `ModelConfigurationPage.xaml/cs` - Step 3
- `DatasetUploadPage.xaml/cs` - Step 4
- `MetricsSelectionPage.xaml/cs` - Step 5
- `ReviewAndLaunchPage.xaml/cs` - Step 6

### Models
```csharp
public class EvaluationResult
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ModelName { get; set; }
    public string DatasetName { get; set; }
    public EvaluationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public double AverageScore { get; set; }
    public int TotalItems { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public List<IndividualResult> Results { get; set; }
}

public class EvaluationWizardState
{
    public EvaluationWorkflow SelectedWorkflow { get; set; }
    public string EvaluationType { get; set; }
    public ModelConfiguration ModelConfig { get; set; }
    public DatasetConfiguration DatasetConfig { get; set; }
    public List<string> SelectedMetrics { get; set; }
}
```

### Services

#### EvaluationResultsStore
- Handles data persistence using ApplicationData.LocalFolder
- Provides CRUD operations for evaluations
- Generates sample data on first run
- Manages evaluation lifecycle

#### StatisticsCalculator
- Computes statistical metrics (mean, median, std dev)
- Generates distribution data for charts
- Performs comparison analysis
- Calculates confidence intervals

#### ExportService
- Exports evaluation data to JSON/CSV
- Handles bulk export operations
- Formats data for different use cases

## Data Flow

1. **Creation Flow**:
   - User launches wizard → Wizard collects configuration
   - Configuration saved to EvaluationWizardState
   - ResultsStore creates new EvaluationResult
   - Background process (simulated) updates status
   - UI refreshes to show new evaluation

2. **Analysis Flow**:
   - User selects evaluation → Navigate to InsightsPage
   - InsightsPage loads full evaluation data
   - StatisticsCalculator computes metrics
   - Charts render visualization
   - User can export results

3. **Comparison Flow**:
   - User selects multiple evaluations
   - ComparisonDialog loads selected data
   - ComparisonService aligns metrics
   - Side-by-side visualization rendered
   - Statistical significance calculated

## State Management

- **Navigation State**: Maintained by Frame navigation
- **Wizard State**: Preserved in WizardDialog instance
- **Selection State**: Managed by EvaluatePage
- **Data State**: Persisted in ApplicationData

## Storage Schema

```
ApplicationData.LocalFolder/
├── evaluations/
│   ├── metadata.json         # List of all evaluations
│   ├── eval_001/
│   │   ├── config.json      # Evaluation configuration
│   │   ├── results.json     # Individual results
│   │   └── stats.json       # Computed statistics
│   └── eval_002/
│       └── ...
└── settings.json            # User preferences
```

## Threading Model

- **UI Thread**: All UI updates and user interactions
- **Background Tasks**: Simulated evaluation processing
- **File I/O**: Async operations for data persistence
- **Statistics**: Computed on background thread for large datasets

## Error Handling

- Input validation at each wizard step
- Try-catch blocks around file operations
- User-friendly error messages via InfoBar
- Graceful degradation for missing data

## Performance Considerations

- Virtualized list views for large datasets
- Lazy loading of detailed results
- Caching of computed statistics
- Efficient search implementation
- Maximum 1,000 items per evaluation (current limit)