# Evaluation Insights Feature

*Last Updated: June 20, 2025*

## Overview

The Evaluation Insights feature provides comprehensive visualization and analysis capabilities for evaluation results. Users can explore detailed metrics, compare evaluations, and export data in multiple formats.

## Key Features

### 1. Evaluation Insights Page

Accessed by double-clicking an evaluation from the list, this page provides:

- **Header Section**: Breadcrumb navigation, evaluation metadata, overall score
- **Metric Cards**: Key statistics at a glance (Images Processed, Average Score, etc.)
- **Visualizations**: Multiple view options for analyzing results
  - Bar Chart View: Horizontal bars with color-coded performance levels
  - Table View: Sortable data grid with all metrics
  - Distribution View: Histograms showing score distributions
- **Statistical Summary**: Mean, standard deviation, min/max values
- **Individual Results Browser**: File tree navigation with detailed item view
- **Export Functionality**: CSV, JSON, and HTML report generation

### 2. Comparison View

Compare 2-5 evaluations side-by-side with:

- **Grouped Bar Charts**: Visual comparison of criteria scores
- **Model Rankings**: Overall performance ranking with medal emojis (🥇🥈🥉)
- **Key Statistics**: 
  - Consistency (lowest standard deviation)
  - Best Performer (largest performance gap)
  - Most Agreement (smallest variance)
  - Evaluation Coverage
- **Detailed Scores Table**: Expandable section with all data
- **Export Options**: Full comparison data in multiple formats

### 3. Data Model Enhancements

#### Individual Result Storage
```csharp
public class EvaluationItemResult
{
    public string Id { get; set; }
    public string ImagePath { get; set; }
    public string RelativePath { get; set; }
    public string Prompt { get; set; }
    public string ModelResponse { get; set; }
    public Dictionary<string, double> CriteriaScores { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string Error { get; set; }
    public Dictionary<string, object> CustomMetadata { get; set; }
}
```

#### Folder Statistics
```csharp
public class FolderStats
{
    public string FolderPath { get; set; }
    public int ItemCount { get; set; }
    public Dictionary<string, double> AverageScores { get; set; }
    public double SuccessRate { get; set; }
}
```

### 4. Storage Strategy

- Main evaluation data: `Evaluations/{id}.json`
- Individual results: `Evaluations/{id}/items.json`
- Folder statistics: `Evaluations/{id}/folder_stats.json`
- Lazy loading for performance
- Automatic cleanup of orphaned files

## UI Components

### Color Scheme
- Excellent (4.5-5.0): #4CAF50 (Green)
- Good (3.5-4.4): #2196F3 (Blue)
- Fair (2.5-3.4): #FFC107 (Yellow)
- Needs Improvement (1.0-2.4): #FF5722 (Orange/Red)

### Accessibility Features
- Full keyboard navigation support
- Screen reader announcements
- High contrast mode compatibility
- AutomationProperties on all controls

### Performance Optimizations
- List virtualization for large datasets
- Lazy loading of detailed results
- Efficient chart rendering
- Memory cleanup on navigation

## Export Capabilities

### CSV Export
- All evaluation data in tabular format
- Includes individual results if available
- UTF-8 encoding with proper escaping

### JSON Export
- Complete data structure preservation
- Pretty-printed for readability
- Includes custom metadata

### HTML Report
- Professional layout for printing
- Embedded charts and visualizations
- Responsive design for various screen sizes

## Implementation Details

### Key Files
- `EvaluationInsightsPage.xaml/.cs` - Main insights view
- `EvaluationInsightsViewModel.cs` - Data binding and logic
- `CompareEvaluationsPage.xaml/.cs` - Comparison view
- `IndividualResultsPanel.xaml/.cs` - Item details browser
- `FileTreeExplorer.xaml/.cs` - Folder navigation

### Navigation Flow
1. User double-clicks evaluation in list
2. Navigate to `EvaluationInsightsPage` with evaluation ID
3. Load evaluation data including individual results
4. Display visualizations and enable interactions
5. For comparison: Select 2-5 evaluations and click Compare

## Known Limitations

1. **Image Previews**: Shows file paths, not thumbnail images
2. **Real-time Updates**: Requires refresh for new data
3. **Large Datasets**: Performance may degrade beyond 10K items

## Future Enhancements

- Statistical significance testing for comparisons
- Cost/performance analysis integration
- Historical trends tracking
- AI-powered recommendations
- Subset analysis by categories

[← Back to Documentation](README.md)