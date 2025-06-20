# Quick Start Guide - Evaluation Visualization MVP

## 🚀 Getting Started TODAY

### Step 1: Set Up Your Branch
```bash
# From main branch
git checkout -b feature/eval-list-cards

# Create your work structure
mkdir AIDevGallery/Controls/Evaluate
mkdir AIDevGallery/ViewModels/Evaluate
mkdir AIDevGallery/Services/Evaluate
```

### Step 2: Create Initial Files
```bash
# Create these files to start
AIDevGallery/Controls/Evaluate/EvaluationCard.xaml
AIDevGallery/Controls/Evaluate/EvaluationCard.xaml.cs
AIDevGallery/ViewModels/Evaluate/EvaluationCardViewModel.cs
AIDevGallery/Services/Evaluate/EvaluationResultsStore.cs
```

### Step 3: Start with the ViewModel
```csharp
// EvaluationCardViewModel.cs
namespace AIDevGallery.ViewModels.Evaluate;

public class EvaluationCardViewModel : INotifyPropertyChanged
{
    private string _id;
    private string _modelName;
    private string _datasetName;
    private double? _overallScore;
    private DateTime _timestamp;
    private EvaluationStatus _status;

    public string Id 
    { 
        get => _id; 
        set { _id = value; OnPropertyChanged(); } 
    }

    public string ModelName 
    { 
        get => _modelName; 
        set { _modelName = value; OnPropertyChanged(); } 
    }

    public string DatasetName 
    { 
        get => _datasetName; 
        set { _datasetName = value; OnPropertyChanged(); } 
    }

    public double? OverallScore 
    { 
        get => _overallScore; 
        set 
        { 
            _overallScore = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(ScorePercentage));
            OnPropertyChanged(nameof(ScoreColor));
        } 
    }

    public string ScorePercentage => OverallScore.HasValue 
        ? $"{OverallScore.Value * 100:F0}%" 
        : "--";

    public string ScoreColor => OverallScore switch
    {
        >= 0.9 => "#4CAF50",  // Green
        >= 0.75 => "#2196F3", // Blue
        >= 0.6 => "#FFC107",  // Yellow
        _ => "#F44336"        // Red
    };

    public string TimeDisplay => GetRelativeTime(_timestamp);

    private string GetRelativeTime(DateTime time)
    {
        var span = DateTime.Now - time;
        return span switch
        {
            { TotalMinutes: < 1 } => "Just now",
            { TotalMinutes: < 60 } => $"{(int)span.TotalMinutes}m ago",
            { TotalHours: < 24 } => $"{(int)span.TotalHours}h ago",
            { TotalDays: < 7 } => $"{(int)span.TotalDays}d ago",
            _ => time.ToString("MMM d")
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Step 4: Create the Card Control
```xml
<!-- EvaluationCard.xaml -->
<UserControl x:Class="AIDevGallery.Controls.Evaluate.EvaluationCard"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Border x:Name="CardBorder" 
            CornerRadius="8" 
            Padding="16"
            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
            BorderThickness="1"
            PointerEntered="OnPointerEntered"
            PointerExited="OnPointerExited">
        
        <Border.Shadow>
            <ThemeShadow />
        </Border.Shadow>
        
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <!-- Progress Box -->
            <Border Grid.Column="0" 
                    Width="64" 
                    Height="64" 
                    CornerRadius="8"
                    Margin="0,0,16,0">
                <Border.Background>
                    <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                        <GradientStop Color="{x:Bind ViewModel.ScoreColor}" Offset="0"/>
                        <GradientStop Color="{x:Bind ViewModel.ScoreColor}" Offset="1"/>
                    </LinearGradientBrush>
                </Border.Background>
                
                <TextBlock Text="{x:Bind ViewModel.ScorePercentage}" 
                          HorizontalAlignment="Center"
                          VerticalAlignment="Center"
                          FontSize="20"
                          FontWeight="Bold"
                          Foreground="White"/>
            </Border>
            
            <!-- Content -->
            <StackPanel Grid.Column="1" VerticalAlignment="Center">
                <TextBlock Text="{x:Bind ViewModel.ModelName}" 
                          Style="{StaticResource SubtitleTextBlockStyle}"/>
                <TextBlock Text="{x:Bind ViewModel.DatasetName}" 
                          Style="{StaticResource CaptionTextBlockStyle}"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                <TextBlock Text="{x:Bind ViewModel.TimeDisplay}" 
                          Style="{StaticResource CaptionTextBlockStyle}"
                          Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
            </StackPanel>
            
            <!-- Actions (hidden by default) -->
            <StackPanel x:Name="ActionPanel" 
                       Grid.Column="2" 
                       Orientation="Horizontal"
                       Visibility="Collapsed">
                <Button Content="👁️" 
                       Style="{StaticResource SubtleButtonStyle}"
                       ToolTipService.ToolTip="View Details"
                       Click="OnViewClick"/>
            </StackPanel>
        </Grid>
    </Border>
</UserControl>
```

### Step 5: Quick Test in EvaluatePage
```csharp
// In EvaluatePage.xaml.cs constructor or OnNavigatedTo
private void LoadSampleData()
{
    var sampleEvaluations = new List<EvaluationCardViewModel>
    {
        new() 
        {
            Id = "1",
            ModelName = "GPT-4 Vision",
            DatasetName = "Wildlife Photos • 850 images",
            OverallScore = 0.925,
            Timestamp = DateTime.Now.AddHours(-2),
            Status = EvaluationStatus.Completed
        },
        new() 
        {
            Id = "2",
            ModelName = "Claude 3.5",
            DatasetName = "Technical Docs • 2,500 items",
            OverallScore = 0.783,
            Timestamp = DateTime.Now.AddDays(-1),
            Status = EvaluationStatus.Completed
        }
    };

    // Bind to your ListView
    EvaluationsListView.ItemsSource = sampleEvaluations;
}
```

---

## 📝 Today's Checklist

### Morning (Design & Setup)
- [ ] Create feature branch
- [ ] Set up folder structure
- [ ] Create initial design mockup (even rough sketch)
- [ ] List design decisions to discuss
- [ ] Create ViewModel with sample data

### Afternoon (Implementation)
- [ ] Create EvaluationCard control
- [ ] Style with gradients and shadows
- [ ] Add hover animations
- [ ] Test with sample data
- [ ] Take screenshot for review

### End of Day
- [ ] Commit progress (mark as WIP if incomplete)
- [ ] Document any blockers
- [ ] Plan tomorrow's tasks
- [ ] Share screenshot in team chat

---

## 🎨 Design Decisions to Make

1. **Card Height**: Fixed or variable?
2. **Progress Display**: Percentage, progress bar, or both?
3. **Color Thresholds**: Current (90/75/60) good?
4. **Action Buttons**: Always visible or on hover?
5. **Selection**: Checkbox position?
6. **Empty State**: What to show when no evaluations?

---

## 🧪 Test Scenarios

1. **No evaluations** - Empty state
2. **Single evaluation** - Card displays correctly
3. **Many evaluations** - Scrolling performance
4. **Long names** - Text truncation
5. **No score** - Imported but not evaluated
6. **Failed evaluation** - Error state

---

## 🔗 Resources

- [WinUI Gallery](https://aka.ms/winui3gallery) - For control examples
- [Current Table Implementation](../../../AIDevGallery/Pages/Evaluate/EvaluatePage.xaml) - What we're replacing
- [Design Mockups](../ux-design/evaluation-list-mvp.md) - Visual reference
- [Color System](./implementation-quick-ref.md#-design-system) - Score colors

---

## 💬 Need Help?

1. **Design Questions**: Check design docs or ask in design channel
2. **Technical Blockers**: Check existing patterns in codebase
3. **Git Issues**: See git strategy in detailed todos
4. **Testing**: Use sample JSONL from test data

---

## 🎯 Success Criteria for Today

- [ ] Card control created and styled
- [ ] Sample data displaying in list
- [ ] Basic hover interaction working
- [ ] Screenshot shared with team
- [ ] At least one design decision documented

**Remember**: Perfect is the enemy of good. Get something working, then iterate!