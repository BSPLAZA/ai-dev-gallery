# Visualization Implementation Quick Reference

## 🎯 Current Focus
**Import Results Workflow** - Users have JSONL files with evaluation scores that we need to visualize beautifully.

## 📁 Key Files to Modify/Create

### 1. New Pages
- `AIDevGallery/Pages/Evaluate/EvaluationInsightsPage.xaml(.cs)` - NEW
- `AIDevGallery/Pages/Evaluate/EvaluationComparisonPage.xaml(.cs)` - NEW

### 2. Modified Pages
- `AIDevGallery/Pages/Evaluate/EvaluatePage.xaml(.cs)` - Update list to cards

### 3. New Controls
- `AIDevGallery/Controls/Evaluate/EvaluationCard.xaml(.cs)` - NEW
- `AIDevGallery/Controls/Evaluate/ScoreDistributionChart.xaml(.cs)` - NEW
- `AIDevGallery/Controls/Evaluate/ImageResultGallery.xaml(.cs)` - NEW

### 4. New ViewModels
- `AIDevGallery/ViewModels/EvaluationCardViewModel.cs` - NEW
- `AIDevGallery/ViewModels/EvaluationInsightsViewModel.cs` - NEW
- `AIDevGallery/ViewModels/ImageResultViewModel.cs` - NEW

### 5. Services
- `AIDevGallery/Services/EvaluationResultsStore.cs` - NEW
- `AIDevGallery/Services/ChartingService.cs` - NEW

## 🎨 Design System

### Colors (Score-based)
```xml
<!-- Excellent (90-100%) -->
<Color x:Key="ScoreExcellent">#4CAF50</Color>
<Color x:Key="ScoreExcellentLight">#66BB6A</Color>

<!-- Good (75-89%) -->
<Color x:Key="ScoreGood">#2196F3</Color>
<Color x:Key="ScoreGoodLight">#42A5F5</Color>

<!-- Fair (60-74%) -->
<Color x:Key="ScoreFair">#FFC107</Color>
<Color x:Key="ScoreFairLight">#FFCA28</Color>

<!-- Poor (<60%) -->
<Color x:Key="ScorePoor">#F44336</Color>
<Color x:Key="ScorePoorLight">#EF5350</Color>
```

### Card Shadows
```xml
<Style x:Key="EvaluationCardShadow">
    <Setter Property="Translation" Value="0,0,8" />
    <Setter Property="Shadow">
        <Setter.Value>
            <ThemeShadow />
        </Setter.Value>
    </Setter>
</Style>
```

### Animations
```csharp
// Card hover animation
private void AnimateCardHover(UIElement card)
{
    var animation = compositor.CreateVector3KeyFrameAnimation();
    animation.InsertKeyFrame(1.0f, new Vector3(0, -4, 16));
    animation.Duration = TimeSpan.FromMilliseconds(200);
    card.StartAnimation("Translation", animation);
}
```

## 📊 Sample Data Structure

### Imported JSONL Entry
```json
{
  "image": "wildlife/birds/eagle_01.jpg",
  "prompt": "Describe this image",
  "model": "gpt-4-vision",
  "response": "A majestic bald eagle...",
  "scores": {
    "accuracy": 0.95,
    "relevance": 0.92,
    "completeness": 0.88,
    "clarity": 0.90,
    "detail": 0.87
  }
}
```

### Aggregated Data
```csharp
public class EvaluationSummary
{
    public double OverallScore => Scores.Values.Average();
    public Dictionary<string, double> Scores { get; set; }
    public Dictionary<string, FolderStats> FolderBreakdown { get; set; }
    public int TotalImages { get; set; }
    public DateTime ImportedAt { get; set; }
}
```

## 🚀 Quick Start Commands

### Git Setup for List Redesign
```bash
# From feature/evaluation-viz-mvp branch
git checkout -b feature/eval-list-cards-v2

# Create folder structure
mkdir AIDevGallery/Controls/Evaluate
mkdir AIDevGallery/ViewModels/Evaluate
mkdir AIDevGallery/Services/Evaluate
mkdir AIDevGallery/Styles/Evaluate
```

### Add Page Navigation
```csharp
// In EvaluatePage.xaml.cs
private void OnEvaluationClick(object sender, ItemClickEventArgs e)
{
    if (e.ClickedItem is EvaluationCardViewModel eval)
    {
        Frame.Navigate(typeof(EvaluationInsightsPage), eval.Id);
    }
}
```

### Register New Page
```csharp
// In App.xaml.cs or navigation setup
PageService.Register<EvaluationInsightsPage>();
```

## ⚡ Performance Tips

1. **Image Loading**: Use `BitmapImage` with `DecodePixelWidth` for thumbnails
2. **List Virtualization**: Use `ItemsStackPanel` with `VirtualizingStackPanel.VirtualizationMode="Recycling"`
3. **Chart Rendering**: Consider Win2D for smooth animations
4. **Data Caching**: Cache calculated metrics in memory

## 🧪 Test Scenarios

1. **Empty State**: No evaluations imported yet
2. **Single Evaluation**: One JSONL file imported
3. **Large Dataset**: 1000+ images in evaluation
4. **Multiple Folders**: Dataset with 5+ folder hierarchy
5. **Missing Images**: JSONL references non-existent images
6. **Comparison**: 3+ evaluations selected for comparison

## 📝 Code Snippets

### Card Click Handler
```csharp
private async void EvaluationCard_Click(object sender, RoutedEventArgs e)
{
    var card = sender as FrameworkElement;
    var evaluation = card?.DataContext as EvaluationCardViewModel;
    if (evaluation != null)
    {
        await NavigationService.NavigateAsync<EvaluationInsightsPage>(evaluation.Id);
    }
}
```

### Score Color Converter
```csharp
public class ScoreToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double score)
        {
            return score switch
            {
                >= 0.9 => new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)),
                >= 0.75 => new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)),
                >= 0.6 => new SolidColorBrush(Color.FromArgb(255, 255, 193, 7)),
                _ => new SolidColorBrush(Color.FromArgb(255, 244, 67, 54))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }
}
```

## 🔗 Navigation Flow

```
EvaluatePage (List)
    ↓ Click Card
EvaluationInsightsPage
    ├── Overview Tab (default)
    ├── By Criteria Tab
    ├── By Folder Tab
    ├── Image Viewer Tab
    └── Export Tab

EvaluatePage (Multi-select)
    ↓ Click Compare
EvaluationComparisonPage
```