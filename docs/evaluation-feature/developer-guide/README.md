# Developer Guide

Documentation for developers working on the Evaluation feature.

## Quick Start

1. **Clone the repository** and open in Visual Studio 2022
2. **Build the solution** - all dependencies are included
3. **Navigate to** `AIDevGallery/Pages/Evaluate/` to find evaluation code
4. **Review** the [Architecture Overview](../architecture/README.md) for system design

## Key Development Areas

### Adding New Features
- **New Workflows**: Extend `EvaluationWorkflow` enum and wizard pages
- **New Metrics**: Add to `MetricsSelectionPage` and scoring logic
- **New Visualizations**: Add controls to `AIDevGallery/Controls/Evaluate/`
- **New Export Formats**: Extend export methods in insights/comparison pages

### Important Files to Know
```
# Core Components
EvaluatePage.xaml.cs              # Main list view and entry point
EvaluationWizard/*.xaml.cs        # Multi-step wizard pages
EvaluationResultsStore.cs         # Data persistence service
EvaluationInsightsPage.xaml.cs    # Detailed results view
CompareEvaluationsPage.xaml.cs    # Multi-evaluation comparison

# Data Models
Models/EvaluationResult.cs        # Main evaluation data structure
Models/EvaluationItemResult.cs    # Individual result items
Models/DatasetConfiguration.cs    # Dataset settings

# Key Controls
FileTreeExplorer.xaml.cs          # Hierarchical result browser
ResultDetailsPanel.xaml.cs        # Individual result display
```

### Development Patterns

#### MVVM Implementation
```csharp
public class EvaluationListItemViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}
```

#### Navigation
```csharp
// Navigate with parameters
Frame.Navigate(typeof(EvaluationInsightsPage), evaluationId);

// Dialog navigation
var dialog = new WizardDialog();
dialog.Frame.Navigate(typeof(SelectEvaluationTypePage), wizardState);
```

#### Data Persistence
```csharp
// Save evaluation
await _evaluationStore.SaveEvaluationAsync(evaluation);

// Import from JSONL
var evaluation = await _evaluationStore.ImportFromJsonlAsync(path, name);
```

## Testing Approach

1. **UI Testing**: Manual testing of all workflows
2. **Sample Data**: Use `SimulateProgressAsync()` for testing running evaluations
3. **Import Testing**: Test JSONL files in various formats
4. **Edge Cases**: Test with 0, 1, 100+ evaluations

## Common Tasks

### Fix Build Errors
- Check for missing `using` statements
- Ensure all enum values are defined
- Verify internal/public accessibility

### Debug Data Issues
- Check `ApplicationData.Current.LocalFolder` for JSON files
- Use Debug.WriteLine() for tracing
- Verify JSONL format matches expected schema

### UI Debugging
- Use Live Visual Tree in Visual Studio
- Check XAML bindings in Output window
- Verify event handlers are connected

## Resources

- [Architecture Documentation](../architecture/README.md)
- [Data Model Reference](../architecture/data-model.md)
- [Current Implementation Status](../current-state.md)
- [Git Workflow Guide](../git-workflow.md)

[← Back to Main Documentation](../README.md)
