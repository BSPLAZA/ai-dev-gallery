# Evaluation Insights Page - UI Mockup

## Page Type: Full Page Navigation (Not a Modal)
- Opens as a complete new page when user double-clicks evaluation
- Standard back navigation to return to evaluation list
- Full screen real estate for rich visualizations

## Page Layout Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│ [←] Evaluations > Image Quality Assessment                              │
│                                                                         │
│ Image Quality Assessment for Product Catalog                            │
│ Model: GPT-4V | Dataset: Product Images (1000) | Jun 19, 2025          │
│ Status: [✓] Completed | Overall Score: ★★★★☆ 4.2/5                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐      │
│ │   Images    │ │   Average   │ │  Criteria   │ │  Duration   │      │
│ │ Processed   │ │    Score    │ │ Evaluated   │ │             │      │
│ │   1,000     │ │   4.2/5     │ │     5       │ │  2h 15m     │      │
│ │ 100% Done   │ │             │ │             │ │             │      │
│ └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘      │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ [Chart View] [Table View] [Distribution]                                │
│                                                                         │
│ Criteria Performance                              [Copy] [Save Image]  │
│ ┌─────────────────────────────────────────────────────────────────┐  │
│ │ Average Scores by Evaluation Criteria                            │  │
│ │                                                                   │  │
│ │                           Score (1-5 scale)                       │  │
│ │         1         2         3         4         5                 │  │
│ │         |         |         |         |         |                 │  │
│ │                                                                   │  │
│ │ Accuracy        ████████████████████████████████████ 4.5         │  │
│ │                                                      Excellent   │  │
│ │                                                                   │  │
│ │ Completeness    ████████████████████████████████     4.2         │  │
│ │                                                      Good        │  │
│ │                                                                   │  │
│ │ Relevance       ████████████████████████████         4.0         │  │
│ │                                                      Good        │  │
│ │                                                                   │  │
│ │ Clarity         ███████████████████████████████████  4.4         │  │
│ │                                                      Good        │  │
│ │                                                                   │  │
│ │ Detail Level    ████████████████████████             3.8         │  │
│ │                                                      Good        │  │
│ │                                                                   │  │
│ │ ─────────────────────────────────────────────────────────────    │  │
│ │ Scale: 1-2.4 Needs Improvement | 2.5-3.4 Fair |                 │  │
│ │        3.5-4.4 Good | 4.5-5.0 Excellent                         │  │
│ └─────────────────────────────────────────────────────────────────┘  │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ Statistical Summary                                                     │
│ ┌─────────────────────────────────────────────────────────────────┐  │
│ │ Mean: 4.2 | Std Dev: 0.3 | Min: 2.1 | Max: 5.0 | Median: 4.3  │  │
│ └─────────────────────────────────────────────────────────────────┘  │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ [Overall] [By Folder] [Comparisons]                                    │
│                                                                         │
│ Folder Performance Analysis                                             │
│ ┌─────────────────────────────────────────────────────────────────┐  │
│ │ [▸] products/                                                    │  │
│ │     [▸] shoes (250 items) - Avg: 4.5 [↑]                       │  │
│ │     [▸] bags (200 items) - Avg: 4.0                            │  │
│ │     [▸] accessories (150 items) - Avg: 4.3                     │  │
│ │ [▸] lifestyle/                                                   │  │
│ │     [▸] outdoor (200 items) - Avg: 4.1                         │  │
│ │     [▸] indoor (200 items) - Avg: 4.2                          │  │
│ └─────────────────────────────────────────────────────────────────┘  │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│ Individual Results Browser                                              │
│ [Filter: All] [Sort: Score ↓] [View: Grid]                            │
│                                                                         │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐        │
│ │ [Image] │ │ [Image] │ │ [Image] │ │ [Image] │ │ [Image] │        │
│ │   4.8   │ │   4.7   │ │   4.6   │ │   4.5   │ │   4.5   │        │
│ │ shoe_01 │ │ bag_15  │ │ shoe_23 │ │ acc_07  │ │ out_12  │        │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘ └─────────┘        │
│                                                                         │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐        │
│ │ [Image] │ │ [Image] │ │ [Image] │ │ [Image] │ │ [Image] │        │
│ │   4.4   │ │   4.3   │ │   4.3   │ │   4.2   │ │   4.1   │        │
│ │ in_05   │ │ bag_22  │ │ shoe_45 │ │ acc_19  │ │ out_08  │        │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘ └─────────┘        │
│                                                                         │
│                        [Load More...]                                   │
│                                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│ [Export Data ▼] [Share] [Print Report]                                 │
└─────────────────────────────────────────────────────────────────────────┘
```

## Key UI Components

### 1. Header Section
- **Back navigation** to evaluation list
- **Evaluation name** as primary title
- **Metadata bar** with model, dataset, timestamp
- **Status and overall score** prominently displayed

### 2. Metric Cards Row
- **4 key metrics** in card layout
- **Large numbers** for quick scanning
- **Contextual indicators** (completion % only, no trend arrows)
- **Consistent card styling** with existing design

### 3. Criteria Visualization
- **Tab switcher** for different views
- **Bar chart** as default view with:
  - Horizontal bars for easy label reading
  - Color coding (green/blue/yellow/red)
  - Score labels on right
  - Performance badges (Excellent/Good/Fair)
- **Animated transitions** between views

### 4. Statistical Summary Bar
- **Compact horizontal layout**
- **Key stats** at a glance
- **Subtle background** to separate sections

### 5. Folder Analysis (When Individual Data Available)
- **Tree view** with folder hierarchy
- **Item counts** and average scores
- **Performance indicators** (arrows for above/below average)
- **Expandable folders** for drilling down

### 6. Image Browser Grid
- **Thumbnail previews** with scores
- **Filter and sort controls** in toolbar
- **Score badges** overlaid on images
- **Click interaction**: Inline expansion below selected image row
- **Detail panel shows**: Full image, prompt, response, all scores, custom metadata
- **Other images shift down smoothly** to accommodate detail view
- **Close button or clicking another image** switches view
- **Load more** pagination

### 7. Export Actions Bar
- **Sticky bottom bar**
- **Grouped actions** with icons
- **Dropdown for export formats**
- **Primary actions** easily accessible

## Design Details

### Color Scheme (Following Existing Patterns)
```
Excellent (4.5-5.0): #4CAF50 (Green)
Good (3.5-4.4): #2196F3 (Blue)  
Fair (2.5-3.4): #FFC107 (Yellow)
Needs Improvement (1.0-2.4): #FF5722 (Orange/Red)
```

### Typography
- **Title**: TitleTextBlockStyle (28px)
- **Subtitles**: SubtitleTextBlockStyle (20px)
- **Body**: BodyTextBlockStyle (14px)
- **Captions**: CaptionTextBlockStyle (12px)

### Spacing & Layout
- Page padding: 36,24,36,36
- Card spacing: 4 (SettingsCardSpacing)
- Max content width: 1000px
- Corner radius: OverlayCornerRadius

### Interactive Elements
- **Hover states** on all clickable items
- **Smooth transitions** for view changes
- **Loading states** with progress indicators
- **Empty states** with helpful messages

## Alternative Views

### Table View
```
┌─────────────────────────────────────────────────────────────┐
│ Criterion      │ Score │ Std Dev │ Min  │ Max  │ Status   │
├─────────────────────────────────────────────────────────────┤
│ Accuracy       │ 4.5   │ 0.3     │ 3.2  │ 5.0  │ [✓] Excel│
│ Completeness   │ 4.2   │ 0.4     │ 2.8  │ 5.0  │ [✓] Good │
│ Relevance      │ 4.0   │ 0.5     │ 2.1  │ 5.0  │ [✓] Good │
│ Clarity        │ 4.4   │ 0.2     │ 3.5  │ 5.0  │ [✓] Good │
│ Detail Level   │ 3.8   │ 0.6     │ 2.0  │ 5.0  │ [!] Fair │
└─────────────────────────────────────────────────────────────┘
```

### Distribution View (When Individual Data Available)
```
┌─────────────────────────────────────────────────────────────────────┐
│ Score Distribution for Accuracy                   [Copy] [Save Image]│
│                                                                      │
│ Number of Images                                                     │
│ 500 ┤                                                               │
│     │                                                               │
│ 400 ┤                              ████████████████████ (450)       │
│     │                              ████████████████████             │
│ 300 ┤                              ████████████████████             │
│     │         ████████████ (280)   ████████████████████             │
│ 200 ┤         ████████████         ████████████████████             │
│     │         ████████████         ████████████████████             │
│ 100 ┤ ██████  ████████████         ████████████████████             │
│     │ ██████  ████████████  ███    ████████████████████             │
│   0 └─────────────────────────────────────────────────────          │
│       1.0   1.5   2.0   2.5   3.0   3.5   4.0   4.5   5.0         │
│                            Score Range                              │
│                                                                      │
│ Mean: 4.5 | Median: 4.6 | Std Dev: 0.3 | Total Images: 1000       │
└─────────────────────────────────────────────────────────────────────┘
```

### Comparison View (Multiple Evaluations)
```
┌──────────────────────────────────────────────────────────────────────┐
│ Evaluation Comparison                             [Copy] [Save Image] │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐                 │
│ │ Eval #1      │ │ Eval #2      │ │ Eval #3      │                 │
│ │ GPT-4V       │ │ Claude 3     │ │ GPT-4V       │                 │
│ │ Jun 19, 2025 │ │ Jun 18, 2025 │ │ Jun 17, 2025 │                 │
│ └──────────────┘ └──────────────┘ └──────────────┘                 │
│                                                                       │
│ Criteria Performance Comparison                                       │
│                                                                       │
│                  ■ Eval #1  ■ Eval #2  ■ Eval #3                    │
│                                                                       │
│ Accuracy        ████████ 4.5  ███████ 4.2  ████████ 4.6            │
│                                                                       │
│ Completeness    ███████ 4.2   ██████ 4.0   ███████ 4.3             │
│                                                                       │
│ Relevance       ██████ 4.0    █████ 3.8    ██████ 4.1              │
│                                                                       │
│ Clarity         ███████ 4.4   ███████ 4.3  ████████ 4.5            │
│                                                                       │
│ Detail Level    █████ 3.8     ████ 3.5     ██████ 4.0              │
│                                                                       │
│ ──────────────────────────────────────────────────────────          │
│ Overall Average  4.18          3.96         4.30 [Best]            │
│                                                                       │
│ Legend: █ = 0.5 points | Scale: 1-5                                 │
└──────────────────────────────────────────────────────────────────────┘
```

## Implementation Notes

1. **Progressive Enhancement**
   - Basic view works with current aggregate data
   - Enhanced features activate when individual data available
   - Graceful degradation for missing data

2. **Performance Considerations**
   - Lazy load image thumbnails
   - Virtualize long lists
   - Cache chart renderings
   - Paginate results over 100 items

3. **Accessibility**
   - All charts have text alternatives
   - Keyboard navigation fully supported
   - High contrast mode compatible
   - Screen reader announcements for updates

4. **Export Quality**
   - Charts include title and metadata
   - White background for printing
   - High resolution (300 DPI equivalent)
   - Multiple format options

This design provides a comprehensive view of evaluation results while maintaining consistency with the existing AI Dev Gallery design language.

## Design Highlights

- **Progressive Enhancement**: Works with current aggregate data, enhanced when individual results available
- **Consistent Styling**: Uses existing color scheme (Green/Blue/Yellow/Red for performance levels)
- **Multiple Views**: Chart, Table, and Distribution views for different analysis needs
- **Comparison Mode**: Can compare up to 5 evaluations side-by-side
- **Professional Export**: Charts include title, metadata, proper formatting for presentations
- **Custom Metadata Support**: Automatically displays any additional fields from JSONL files
- **Full Page Navigation**: Provides full screen space for rich data visualization (not a modal)

The design balances information density with clarity, providing both high-level insights and detailed drill-down capabilities.

## Custom Metadata Handling

When users include additional fields in their JSONL files beyond the standard fields, the UI will:

1. **Automatically detect** custom fields during import
2. **Display in detail view** when an image is selected
3. **Support various data types**: strings, numbers, arrays, nested objects
4. **Examples of custom fields**:
   - `category`: "outdoor_scene"
   - `photographer`: "john_doe"
   - `tags`: ["sunset", "landscape", "mountains"]
   - `difficulty_level`: "hard"
   - `camera_settings`: { "iso": 100, "aperture": "f/2.8" }

This ensures users can leverage their domain-specific metadata without modifying the application.

## UI Fixes Based on User Testing (June 20, 2025)

### Header & Navigation Fixes

#### 1. Back Arrow Alignment
**Current Issue**: Arrow is misaligned with "Evaluations" text

```xml
<!-- FIXED VERSION -->
<StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="8">
    <Button x:Name="BackButton" 
            Click="BackButton_Click"
            Style="{StaticResource SubtleButtonStyle}">
        <StackPanel Orientation="Horizontal" Spacing="4">
            <!-- Ensure vertical alignment -->
            <FontIcon Glyph="&#xE72B;" 
                      FontSize="16" 
                      VerticalAlignment="Center"/>
            <TextBlock Text="Evaluations" 
                       VerticalAlignment="Center"/>
        </StackPanel>
    </Button>
    <TextBlock Text=">" 
               VerticalAlignment="Center"
               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
    <TextBlock x:Name="EvaluationNameText" 
               VerticalAlignment="Center"
               Style="{StaticResource BodyTextBlockStyle}"/>
</StackPanel>
```

#### 2. Status Tooltip Fix
**Current Issue**: Tooltip indicator shows but has no content

```xml
<!-- OPTION 1: Remove tooltip entirely -->
<TextBlock x:Name="StatusText" Text="Imported"/>

<!-- OPTION 2: Add meaningful tooltip content AFTER text -->
<TextBlock>
    <Run x:Name="StatusText" Text="Imported"/>
    <Run Text=" ℹ" 
         FontFamily="Segoe UI Symbol"
         Foreground="{ThemeResource TextFillColorSecondaryBrush}"
         ToolTipService.ToolTip="This evaluation was imported from JSONL file on June 19, 2025"/>
</TextBlock>
```

### Metric Cards Fix

#### 3. Images Processed Count
**Current Issue**: Shows 0 instead of actual count

```csharp
// In UpdateUI() method:
// OLD: ImagesProcessedText.Text = _viewModel.ItemCount.ToString("N0");
// NEW:
ImagesProcessedText.Text = _viewModel.DatasetItemCount > 0 
    ? _viewModel.DatasetItemCount.ToString("N0") 
    : _viewModel.ItemCount.ToString("N0");
```

### Chart Visualization Fixes

#### 4. Chart Layout Improvements
**Current Issue**: Bar covers "Excellent" label, no clear max indicator

```xml
<!-- Updated Chart Design -->
┌──────────────────────────────────────────────────────────────────────┐
│ Average Scores by Evaluation Criteria              [Copy] [Save Image]│
│                                                                       │
│                     Score (1-5 scale)                                 │
│      1         2         3         4         5                        │
│      |         |         |         |         |                        │
│                                              ┊ ← Max line             │
│ Accuracy     ███████████████████████████     ┊  4.5                  │
│                                              ┊     [Excellent]       │
│                                              ┊                       │
│ Completeness ████████████████████████        ┊  4.2                  │
│                                              ┊     [Good]           │
│                                              ┊                       │
│ Relevance    ███████████████████████         ┊  4.0                  │
│                                              ┊     [Good]           │
│                                              ┊                       │
│ Clarity      ██████████████████████████      ┊  4.4                  │
│                                              ┊     [Good]           │
│                                              ┊                       │
│ Detail Level ████████████████████            ┊  3.8                  │
│                                              ┊     [Good]           │
│                                              ┊                       │
│ ─────────────────────────────────────────────┴────────────          │
│ Scale: 1-2.4 Needs Improvement | 2.5-3.4 Fair |                     │
│        3.5-4.4 Good | 4.5-5.0 Excellent                             │
└──────────────────────────────────────────────────────────────────────┘
```

**Key Changes**:
- Reduce bar width to 80% of available space
- Add dotted vertical line at score 5.0
- Move performance badges to separate line below score
- Increase spacing between bars

### Missing Features to Implement

#### 5. Folder Statistics Tab
**Issue**: Data loaded but not displayed

```xml
<!-- Add to existing tab control -->
<PivotItem Header="By Folder">
    <TreeView ItemsSource="{x:Bind ViewModel.FolderStatistics}">
        <TreeView.ItemTemplate>
            <DataTemplate>
                <TreeViewItem>
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        
                        <FontIcon Grid.Column="0" 
                                  Glyph="&#xE8B7;" 
                                  FontSize="16"
                                  Margin="0,0,8,0"/>
                        <TextBlock Grid.Column="1" 
                                   Text="{x:Bind FolderPath}"/>
                        <TextBlock Grid.Column="2" 
                                   Text="{x:Bind ItemCount}" 
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                   Margin="8,0"/>
                        <Border Grid.Column="3" 
                                Background="{x:Bind ScoreBackgroundBrush}"
                                CornerRadius="4"
                                Padding="8,4">
                            <TextBlock Text="{x:Bind AverageScoreText}" 
                                       FontWeight="SemiBold"/>
                        </Border>
                    </Grid>
                </TreeViewItem>
            </DataTemplate>
        </TreeView.ItemTemplate>
    </TreeView>
</PivotItem>
```

#### 6. Statistical Summary Enhancement
**Issue**: No std dev, min/max shown

```xml
<!-- Enhanced Statistical Summary -->
<Grid x:Name="StatisticalSummaryCard" 
      Style="{StaticResource CardGridStyle}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="*"/>
    </Grid.ColumnDefinitions>
    
    <!-- Mean -->
    <StackPanel Grid.Column="0" Spacing="4">
        <TextBlock Text="Mean" 
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.MeanScore}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
    </StackPanel>
    
    <!-- Std Dev -->
    <StackPanel Grid.Column="1" Spacing="4">
        <TextBlock Text="Std Dev" 
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.StandardDeviation}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
    </StackPanel>
    
    <!-- Min -->
    <StackPanel Grid.Column="2" Spacing="4">
        <TextBlock Text="Min" 
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.MinScore}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
    </StackPanel>
    
    <!-- Max -->
    <StackPanel Grid.Column="3" Spacing="4">
        <TextBlock Text="Max" 
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.MaxScore}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
    </StackPanel>
    
    <!-- Median -->
    <StackPanel Grid.Column="4" Spacing="4">
        <TextBlock Text="Median" 
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.MedianScore}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
    </StackPanel>
    
    <!-- Total Items -->
    <StackPanel Grid.Column="5" Spacing="4">
        <TextBlock Text="Total Items" 
                   Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <TextBlock Text="{x:Bind ViewModel.TotalItems}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
    </StackPanel>
</Grid>
```

#### 7. Prompt Display
**Issue**: Prompt used for evaluation not shown

```xml
<!-- Add to header section after metadata bar -->
<Expander Header="Evaluation Prompt" 
          HorizontalAlignment="Stretch"
          Margin="0,8,0,0">
    <Grid Style="{StaticResource CardGridStyle}" 
          Padding="16">
        <TextBlock Text="{x:Bind ViewModel.EvaluationPrompt}"
                   TextWrapping="Wrap"
                   IsTextSelectionEnabled="True"
                   Style="{StaticResource BodyTextBlockStyle}"/>
    </Grid>
</Expander>
```

### Button Icon Fixes

#### 8. Save as Image Icon
**Issue**: Folder icon is confusing

```xml
<!-- OLD -->
<FontIcon Glyph="&#xE838;" FontSize="14"/> <!-- Folder icon -->

<!-- NEW - Use photo/image icon -->
<FontIcon Glyph="&#xEB9F;" FontSize="14"/> <!-- Photo icon -->
<!-- OR -->
<FontIcon Glyph="&#xE91B;" FontSize="14"/> <!-- SaveLocal icon -->
```

### Navigation Fix

#### 9. Double-Click Navigation Issue
**Issue**: Requires two clicks to go back

```csharp
// Check for duplicate navigation in EvaluatePage.xaml.cs
private bool _isNavigating = false;

private void NavigateToEvaluationDetails(EvaluationListItemViewModel evaluation)
{
    if (_isNavigating) return;
    _isNavigating = true;
    
    Frame.Navigate(typeof(EvaluationInsightsPage), evaluation.Id);
    
    // Reset flag after navigation
    DispatcherQueue.TryEnqueue(() => _isNavigating = false);
}
```

### Implementation Priority for Fixes

**Immediate (Fix in current branch)**:
1. ✓ Images Processed count display
2. ✓ Chart layout with max line and spacing
3. ✓ Back arrow alignment
4. ✓ Remove/fix status tooltip
5. ✓ Change Save as Image icon

**High Priority (Next iteration)**:
1. Display folder statistics tab
2. Add statistical summary with std dev
3. Show evaluation prompt
4. Fix navigation double-click issue
5. Implement all export functions

**Medium Priority**:
1. Image preview browser
2. Enhanced tooltips
3. Better loading states

These fixes address all the issues identified during user testing while maintaining consistency with the existing design system.