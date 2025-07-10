# Developer Guide

## Project Structure

```
AIDevGallery/
├── Controls/
│   └── Evaluate/
│       ├── EvaluationCard.xaml/cs
│       ├── EvaluationListRow.xaml/cs
│       ├── SelectionActionBar.xaml/cs
│       ├── ScoreDistributionChart.xaml/cs
│       ├── MetricsRadarChart.xaml/cs
│       └── StatisticsCard.xaml/cs
├── Dialogs/
│   ├── WizardDialog.xaml/cs
│   └── ComparisonDialog.xaml/cs
├── Models/
│   ├── EvaluationResult.cs
│   ├── EvaluationWorkflow.cs
│   ├── EvaluationStatus.cs
│   ├── DatasetConfiguration.cs
│   └── ModelConfiguration.cs
├── Pages/
│   └── Evaluate/
│       ├── EvaluatePage.xaml/cs
│       ├── EvaluationInsightsPage.xaml/cs
│       └── Wizard/
│           ├── WorkflowSelectionPage.xaml/cs
│           ├── EvaluationTypePage.xaml/cs
│           ├── ModelConfigurationPage.xaml/cs
│           ├── DatasetUploadPage.xaml/cs
│           ├── MetricsSelectionPage.xaml/cs
│           └── ReviewAndLaunchPage.xaml/cs
├── Services/
│   └── Evaluate/
│       ├── EvaluationResultsStore.cs
│       ├── StatisticsCalculator.cs
│       ├── ComparisonService.cs
│       └── ExportService.cs
└── ViewModels/
    ├── EvaluationListItemViewModel.cs
    └── EvaluationCardViewModel.cs
```

## Key Components

### WizardDialog

The base wizard framework that manages navigation and validation:

```csharp
public sealed partial class WizardDialog : ContentDialog
{
    private readonly Stack<Page> _navigationStack = new();
    
    public void NavigateToStep(Type pageType, object parameter = null)
    {
        // Navigation logic with validation
    }
    
    private async Task<bool> ValidateCurrentStep()
    {
        // Validation framework
    }
}
```

### EvaluationResultsStore

Handles all data persistence operations:

```csharp
public class EvaluationResultsStore
{
    private readonly string _storageFolder;
    
    public async Task<List<EvaluationResult>> GetAllAsync()
    {
        // Load from ApplicationData.LocalFolder
    }
    
    public async Task SaveAsync(EvaluationResult evaluation)
    {
        // Persist to local storage
    }
}
```

### Data Models

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
}

public enum EvaluationWorkflow
{
    TestModel,
    EvaluateResponses,
    ImportResults
}

public enum EvaluationStatus
{
    NotStarted,
    Running,
    Completed,
    Failed,
    Imported
}
```

## Adding New Features

### Creating a New Wizard Step

1. Create a new Page in `Pages/Evaluate/Wizard/`:
```csharp
public sealed partial class NewStepPage : Page
{
    public EvaluationWizardState WizardState { get; set; }
    
    public NewStepPage()
    {
        this.InitializeComponent();
    }
    
    public bool Validate()
    {
        // Return true if step is valid
        return !string.IsNullOrEmpty(SomeRequiredField.Text);
    }
}
```

2. Update the wizard flow in `WizardDialog`:
```csharp
private Type GetNextStepType(Type currentStepType)
{
    // Add navigation logic for new step
}
```

### Adding a New Metric

1. Define the metric in the model:
```csharp
public class EvaluationMetric
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<List<IndividualResult>, double> Calculate { get; set; }
}
```

2. Register in `MetricsSelectionPage`:
```csharp
private void LoadAvailableMetrics()
{
    _metrics.Add(new EvaluationMetric
    {
        Name = "Custom Metric",
        Description = "My custom metric",
        Calculate = results => CalculateCustomMetric(results)
    });
}
```

### Creating a New Visualization

1. Create a new UserControl:
```xml
<UserControl x:Class="AIDevGallery.Controls.Evaluate.NewChart">
    <Grid>
        <!-- Chart implementation -->
    </Grid>
</UserControl>
```

2. Add to InsightsPage:
```xml
<controls:NewChart x:Name="MyChart" 
                   Data="{x:Bind ViewModel.ChartData}" />
```

## Storage Implementation

### File Structure
```
ApplicationData.LocalFolder/
├── evaluations/
│   ├── metadata.json         # Index of all evaluations
│   └── {evaluation-id}/
│       ├── config.json       # Evaluation configuration
│       ├── results.json      # Individual results
│       └── statistics.json   # Computed statistics
```

### Metadata Format
```json
{
  "evaluations": [
    {
      "id": "eval_001",
      "name": "GPT-4 Classification Test",
      "createdAt": "2023-12-01T10:00:00Z",
      "status": "Completed",
      "summary": {
        "averageScore": 0.89,
        "totalItems": 100
      }
    }
  ]
}
```

## API Integration (Future)

### Placeholder for Backend Integration

Current implementation uses mock data. To integrate with real backend:

1. Update `EvaluationResultsStore`:
```csharp
public async Task<EvaluationResult> RunEvaluationAsync(
    EvaluationWizardState config)
{
    // TODO: Replace with actual API call
    var response = await _httpClient.PostAsJsonAsync(
        "/api/evaluations", config);
    return await response.Content.ReadFromJsonAsync<EvaluationResult>();
}
```

2. Implement progress tracking:
```csharp
public async IAsyncEnumerable<EvaluationProgress> GetProgressAsync(
    string evaluationId)
{
    // Stream progress updates from backend
}
```

## Testing

### Unit Test Structure
```csharp
[TestClass]
public class EvaluationResultsStoreTests
{
    [TestMethod]
    public async Task SaveAsync_ValidEvaluation_PersistsToStorage()
    {
        // Arrange
        var store = new EvaluationResultsStore(testFolder);
        var evaluation = CreateTestEvaluation();
        
        // Act
        await store.SaveAsync(evaluation);
        
        // Assert
        var loaded = await store.GetByIdAsync(evaluation.Id);
        Assert.IsNotNull(loaded);
        Assert.AreEqual(evaluation.Name, loaded.Name);
    }
}
```

### UI Testing Considerations

- Use `AutomationProperties.Name` for test identification
- Ensure all interactive elements have unique identifiers
- Test keyboard navigation paths
- Verify screen reader announcements

## Performance Guidelines

### Large Dataset Handling

1. Use virtualization for lists:
```xml
<ListView ItemsSource="{x:Bind Items}"
          VirtualizingStackPanel.VirtualizationMode="Recycling">
```

2. Implement pagination for results:
```csharp
public async Task<PagedResult<IndividualResult>> GetResultsPageAsync(
    int page, int pageSize)
{
    // Return paginated results
}
```

3. Cache computed statistics:
```csharp
private readonly Dictionary<string, Statistics> _statsCache = new();
```

### Memory Management

- Dispose chart controls properly
- Clear large collections when navigating away
- Use weak references for event handlers
- Implement IDisposable where appropriate

## Debugging Tips

### Common Issues

1. **Wizard State Lost**
   - Check navigation parameter passing
   - Verify state object is passed by reference

2. **Charts Not Rendering**
   - Ensure data binding is correct
   - Check for null/empty data
   - Verify control is in visual tree

3. **Storage Errors**
   - Check ApplicationData permissions
   - Verify JSON serialization settings
   - Handle concurrent access

### Logging

Add debug logging for troubleshooting:
```csharp
private static readonly ILogger _logger = 
    LoggerFactory.Create(builder => builder.AddDebug())
                 .CreateLogger<EvaluationResultsStore>();

_logger.LogInformation("Loading evaluation {Id}", evaluationId);
```

## Code Style Guidelines

### Naming Conventions
- Pages: `{Feature}Page` (e.g., `EvaluatePage`)
- Controls: `{Feature}Control` (e.g., `EvaluationCard`)
- ViewModels: `{Feature}ViewModel`
- Services: `{Feature}Service` or `{Feature}Store`

### XAML Guidelines
- Use `x:Bind` over `Binding` for performance
- Prefer `Grid` over `StackPanel` for layouts
- Set `AutomationProperties` on all interactive elements
- Use resource dictionaries for repeated styles

### C# Guidelines
- Use `async/await` for all I/O operations
- Implement `INotifyPropertyChanged` for ViewModels
- Use dependency injection where possible
- Keep methods under 20 lines when practical

## Accessibility Implementation

### Required Properties
```xml
<Button AutomationProperties.Name="Delete evaluation"
        AutomationProperties.HelpText="Permanently remove this evaluation">
```

### Keyboard Navigation
```csharp
protected override void OnKeyDown(KeyRoutedEventArgs e)
{
    if (e.Key == VirtualKey.Space && SelectionCheckBox.IsEnabled)
    {
        ToggleSelection();
        e.Handled = true;
    }
    base.OnKeyDown(e);
}
```

### Screen Reader Announcements
```csharp
private void AnnounceStatus(string message)
{
    AutomationPeer.RaiseAutomationEvent(
        AutomationEvents.LiveRegionChanged);
}
```