# Evaluation List Redesign - MVP Focus

## Priority 1: Rich Visual Design

### Card-Based Layout
Transform the plain table into rich cards with visual hierarchy:

```
┌─────────────────────────────────────────────────────────────────────┐
│ AI Evaluations                                    [+ New Evaluation] │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ ┌──────────────────────────────────────────────────────────────┐   │
│ │ ┌────┐                                                        │   │
│ │ │ 92%│  GPT-4V Image Captions                           ⋮    │   │
│ │ │████│  Wildlife & Nature Photos • 850 images               │   │
│ │ └────┘  ✅ Completed 2 hours ago • 45 min duration          │   │
│ │         Average: 92.5% • SPICE: 89.2 • CLIPScore: 0.91      │   │
│ └──────────────────────────────────────────────────────────────┘   │
│                                                                      │
│ ┌──────────────────────────────────────────────────────────────┐   │
│ │ ┌────┐                                                        │   │
│ │ │ -- │  Multi-Model Comparison                          ⋮    │   │
│ │ │░░░░│  Benchmark Dataset • 5,000 items                      │   │
│ │ └────┘  📥 Imported 3 days ago from benchmark_results.jsonl  │   │
│ │         4 models compared • Best: GPT-4V (87.3%)             │   │
│ └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

### Visual Elements

#### 1. Progress Indicator Box
- Large percentage display (92%)
- Visual progress bar with gradient fill
- Color coding:
  - Green (90-100%): Excellent
  - Blue (75-89%): Good
  - Yellow (60-74%): Fair
  - Red (<60%): Poor

#### 2. Rich Information Hierarchy
- **Primary**: Model name + evaluation type
- **Secondary**: Dataset description with bullet separators
- **Status Line**: Icon + status + timing info
- **Metrics Preview**: Key scores visible without clicking

#### 3. Modern UI Patterns
```css
/* Card styling */
.evaluation-card {
  background: linear-gradient(to right, #f8f9fa, #ffffff);
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  padding: 20px;
  margin-bottom: 16px;
  transition: all 0.2s ease;
}

.evaluation-card:hover {
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
  transform: translateY(-2px);
}

/* Progress box */
.progress-box {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #4CAF50, #45a049);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
}
```

## Priority 2: Information Design

### Status Indicators
- **Completed**: ✅ with green accent
- **Running**: ⏳ with animated pulse
- **Failed**: ❌ with red accent  
- **Imported**: 📥 with blue accent

### Time Display
- "Just now" (< 1 min)
- "X minutes ago" (< 60 min)
- "X hours ago" (< 24 hours)
- "X days ago" (< 7 days)
- Exact date for older

### Running Evaluation Updates
For evaluations in progress, show live status:
```
⏳ Running 45% • Processing item 1,125/2,500
   ████████░░░░░░░░░░░░ 
   Est. 15 min remaining • Current: 78.3% accuracy
```

## Implementation Approach

### 1. Data Model Enhancement
```csharp
public class EvaluationCardViewModel
{
    public string Id { get; set; }
    public string ModelName { get; set; }
    public string EvaluationType { get; set; }
    public string DatasetDescription { get; set; }
    public int ItemCount { get; set; }
    
    public EvaluationStatus Status { get; set; }
    public double Progress { get; set; }
    public string StatusMessage { get; set; }
    
    public double? OverallScore { get; set; }
    public Dictionary<string, double> MetricPreviews { get; set; }
    
    public DateTime Timestamp { get; set; }
    public TimeSpan? Duration { get; set; }
    
    public string ProgressColorStart => GetProgressGradient().start;
    public string ProgressColorEnd => GetProgressGradient().end;
    
    private (string start, string end) GetProgressGradient() => OverallScore switch
    {
        >= 90 => ("#4CAF50", "#45a049"), // Green
        >= 75 => ("#2196F3", "#1976D2"), // Blue  
        >= 60 => ("#FFC107", "#FF9800"), // Yellow
        _ => ("#F44336", "#D32F2F")      // Red
    };
}
```

### 2. XAML Structure
```xml
<ListView ItemsSource="{x:Bind Evaluations}" 
          SelectionMode="None"
          IsItemClickEnabled="True"
          ItemClick="OnEvaluationClick">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:EvaluationCardViewModel">
            <Border Style="{StaticResource EvaluationCardStyle}"
                    PointerEntered="OnCardPointerEntered"
                    PointerExited="OnCardPointerExited">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    
                    <!-- Progress Box -->
                    <Border Grid.Column="0" Width="80" Height="80" CornerRadius="8">
                        <Border.Background>
                            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                                <GradientStop Color="{x:Bind ProgressColorStart}" Offset="0"/>
                                <GradientStop Color="{x:Bind ProgressColorEnd}" Offset="1"/>
                            </LinearGradientBrush>
                        </Border.Background>
                        <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                            <TextBlock Text="{x:Bind ProgressText}" 
                                      FontSize="24" 
                                      FontWeight="Bold"
                                      Foreground="White"/>
                            <ProgressBar Value="{x:Bind Progress}" 
                                        Width="60" 
                                        Height="4"
                                        Foreground="White"
                                        Background="#40FFFFFF"/>
                        </StackPanel>
                    </Border>
                    
                    <!-- Content -->
                    <StackPanel Grid.Column="1" Margin="16,0">
                        <TextBlock FontSize="16" FontWeight="SemiBold">
                            <Run Text="{x:Bind ModelName}"/>
                            <Run Text="{x:Bind EvaluationType}" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        </TextBlock>
                        <TextBlock Text="{x:Bind DatasetDescription}" 
                                  Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                  FontSize="14"/>
                        <TextBlock Margin="0,4,0,0" FontSize="13">
                            <Run Text="{x:Bind StatusIcon}"/>
                            <Run Text="{x:Bind StatusText}"/>
                            <Run Text="•" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                            <Run Text="{x:Bind TimeDisplay}" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                        </TextBlock>
                        <TextBlock Text="{x:Bind MetricsDisplay}" 
                                  FontSize="12"
                                  Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                  Margin="0,2,0,0"/>
                    </StackPanel>
                    
                    <!-- Actions (hidden by default) -->
                    <StackPanel x:Name="ActionButtons" 
                               Grid.Column="2" 
                               Orientation="Horizontal"
                               Visibility="Collapsed">
                        <Button Content="👁️" ToolTipService.ToolTip="View Details"/>
                        <Button Content="📊" ToolTipService.ToolTip="Compare" Margin="4,0"/>
                        <Button Content="⋮" ToolTipService.ToolTip="More Options"/>
                    </StackPanel>
                </Grid>
            </Border>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### 3. Code-Behind for Interactions
```csharp
private void OnCardPointerEntered(object sender, PointerRoutedEventArgs e)
{
    if (sender is Border border)
    {
        // Show action buttons
        var actionPanel = border.FindName("ActionButtons") as StackPanel;
        if (actionPanel != null)
        {
            actionPanel.Visibility = Visibility.Visible;
        }
        
        // Enhance shadow
        border.Translation = new Vector3(0, -2, 0);
    }
}

private void OnCardPointerExited(object sender, PointerRoutedEventArgs e)
{
    if (sender is Border border)
    {
        // Hide action buttons
        var actionPanel = border.FindName("ActionButtons") as StackPanel;
        if (actionPanel != null)
        {
            actionPanel.Visibility = Visibility.Collapsed;
        }
        
        // Reset shadow
        border.Translation = new Vector3(0, 0, 0);
    }
}

private void OnEvaluationClick(object sender, ItemClickEventArgs e)
{
    if (e.ClickedItem is EvaluationCardViewModel evaluation)
    {
        // Navigate to evaluation insights
        Frame.Navigate(typeof(EvaluationInsightsPage), evaluation.Id);
    }
}
```

## Success Metrics

1. **Visual Impact**: Users immediately understand evaluation status
2. **Information Density**: All key info visible without scrolling
3. **Interaction Speed**: < 200ms to show hover actions
4. **User Delight**: Modern, professional appearance that impresses