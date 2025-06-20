# Evaluation Insights Page Planning

*Last Updated: June 19, 2025*

## Overview

This document outlines the design and implementation plan for the Evaluation Insights page, which will display detailed results and visualizations when users click on an evaluation from the list.

## Current Data Flow & Constraints

### How Data is Currently Handled

#### For ImportResults Workflow (Workflow 3)
The JSONL format expects per-image data:
```json
{
  "image_path": "images/test1.jpg",
  "prompt": "Describe this image",
  "response": "A sunny beach scene",
  "criteria_scores": {
    "accuracy": {"score": 4, "range": "1-5"},
    "completeness": {"score": 3, "range": "1-5"}
  }
}
```

#### Current Implementation Gap
While the system EXPECTS and PARSES per-image criteria scores:
1. **Import process** reads all individual scores from JSONL
2. **Aggregation** calculates averages for each criterion
3. **Storage** ONLY saves the aggregated averages
4. **Individual scores are discarded** - not stored anywhere

This means we have rich per-image data available during import, but we're throwing it away!

### Proposed Data Model Enhancements

#### 1. Individual Result Storage
```csharp
public class EvaluationItemResult
{
    public string Id { get; set; }
    public string ImagePath { get; set; }
    public string RelativePath { get; set; } // Preserves folder structure
    public string Prompt { get; set; }
    public string ModelResponse { get; set; }
    public Dictionary<string, double> CriteriaScores { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string Error { get; set; } // If failed
    
    // Support for custom metadata from JSONL
    public Dictionary<string, object> CustomMetadata { get; set; }
    public bool HasCustomMetadata => CustomMetadata?.Count > 0;
}

// Add to EvaluationResult:
public List<EvaluationItemResult> ItemResults { get; set; }
public Dictionary<string, FolderStats> FolderStatistics { get; set; }
```

#### 1.1 Handling Custom JSONL Fields
When parsing JSONL files, any fields not recognized as standard fields should be preserved:
```csharp
// During JSONL parsing
var standardFields = new HashSet<string> { 
    "image_path", "prompt", "response", "criteria_scores", 
    "processing_time", "error" 
};

var customMetadata = new Dictionary<string, object>();
foreach (var property in jsonObject.Properties())
{
    if (!standardFields.Contains(property.Name))
    {
        customMetadata[property.Name] = property.Value;
    }
}

// Examples of custom fields users might include:
// - "category": "outdoor_scene"
// - "photographer": "john_doe"
// - "tags": ["sunset", "landscape", "mountains"]
// - "difficulty_level": "hard"
// - "original_resolution": "1920x1080"
// - "camera_settings": { "iso": 100, "aperture": "f/2.8" }
```

#### 2. Folder Statistics
```csharp
public class FolderStats
{
    public string FolderPath { get; set; }
    public int ItemCount { get; set; }
    public Dictionary<string, double> AverageScores { get; set; }
    public double SuccessRate { get; set; }
}
```

#### 3. Storage Strategy
- Store detailed results in separate JSON files
- Reference from main evaluation record
- Lazy load when needed
- Example: `Evaluations/{id}/details.json`

### Implementation Approach

#### Phase 1: Quick Fix - Store Individual Results
Modify `EvaluationResultsStore.ImportFromJsonlAsync` to preserve individual results:
```csharp
// Instead of discarding, store individual results
var itemResults = new List<EvaluationItemResult>();
foreach (var line in lines)
{
    // Parse line...
    itemResults.Add(new EvaluationItemResult
    {
        ImagePath = imagePath,
        Prompt = prompt,
        Response = response,
        CriteriaScores = criteriaScores,
        // ... other fields
    });
}

// Save to separate file
var detailsPath = Path.Combine(_storagePath, $"{evaluation.Id}_details.json");
await File.WriteAllTextAsync(detailsPath, JsonSerializer.Serialize(itemResults));
```

#### Phase 2: Enhanced Insights Page
With individual results available:
1. **Image Browser** - Browse individual results
2. **Folder Analysis** - Group by subfolder performance
3. **Statistical Views** - Distribution, outliers, etc.
4. **Detailed Drill-downs** - See exact scores per image

#### Phase 3: Extend to All Workflows
- For workflows 1 & 2: Store results as they're calculated
- Consistent data model across all evaluation types

## User Journey

### Entry Points
1. **Double-click** on evaluation row in list → **Full page navigation**
2. **Single-click** then "View Details" button (future) → **Full page navigation**
3. **Deep link** from external source (future)

### Navigation Flow
```
Evaluation List → Click Row → Navigate to Full Page → Insights Page
                                                         ↓
                                                 Load evaluation data
                                                         ↓
                                                 Display insights
```

### Important: Full Page vs Modal Decision
- **Full Page Navigation**: YES - This will be a complete new page, not a modal
- **Reasoning**: 
  - Rich data visualization needs full screen real estate
  - Multiple views and tabs require space
  - Export functionality works better in full page
  - Consistent with app navigation patterns
- **Back Navigation**: Standard back button to return to evaluation list

## Page Structure

### 1. Header Section
```
[← Back] Evaluation Insights

[Evaluation Name]
Model: [Model Name] | Dataset: [Dataset Name] | [Timestamp]
Status: [Status Badge] | Overall Score: [★★★★☆ 4.2/5]
```

### 2. Key Metrics Summary Cards
A row of metric cards showing:
- **Images Processed**: [Count]
- **Average Score**: [4.2/5] (no trend indicators)
- **Criteria Evaluated**: [Count]
- **Duration**: [Time] (if available)
- **Status**: [Completed/Failed/Running]

### 3. Criteria Breakdown Section

#### 3.1 Visual Display Options (Tabs)
- **Chart View** (default)
  - Bar chart showing scores for each criterion
  - Color-coded by performance (green/yellow/red)
  - Interactive tooltips showing statistics
  
- **Table View**
  - Sortable table with criterion name, score, std dev, min/max
  - Export options
  
- **Distribution View** (with individual data)
  - Histogram showing score distribution per criterion
  - Box plots for statistical visualization
  - Outlier identification

#### 3.2 Statistical Summary
With individual results, we can show:
- **Average Score**: Mean across all images
- **Standard Deviation**: Measure of consistency
- **Min/Max**: Best and worst performing images
- **Median**: Middle score (less affected by outliers)
- **Percentiles**: 25th, 75th percentile ranges
- **Failed Count**: Images that scored below threshold

### 3.5 Folder-Based Pivoting
Group results by folder structure:

```xml
<Pivot>
    <PivotItem Header="Overall">
        <!-- Aggregate view -->
    </PivotItem>
    <PivotItem Header="By Folder">
        <TreeView ItemsSource="{x:Bind FolderStats}">
            <TreeView.ItemTemplate>
                <DataTemplate>
                    <TreeViewItem>
                        <StackPanel Orientation="Horizontal" Spacing="12">
                            <FontIcon Glyph="&#xE8B7;" FontSize="16"/>
                            <TextBlock Text="{x:Bind FolderName}"/>
                            <TextBlock Text="{x:Bind ItemCount}" 
                                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                            <TextBlock Text="{x:Bind AverageScore}" 
                                       FontWeight="SemiBold"/>
                        </StackPanel>
                    </TreeViewItem>
                </DataTemplate>
            </TreeView.ItemTemplate>
        </TreeView>
    </PivotItem>
    <PivotItem Header="Comparisons">
        <!-- Folder vs folder comparison -->
    </PivotItem>
</Pivot>
```

This enables insights like:
- "Images in 'products/shoes' folder score 0.5 points higher than 'products/bags'"
- "The 'outdoor' folder has the most consistent scores (lowest std dev)"
- "90% of failures come from the 'complex_scenes' folder"

### 4. Individual Results Browser
Once we store individual results:

#### 4.1 Image Gallery View
```xml
<GridView ItemsSource="{x:Bind FilteredResults}">
    <GridView.ItemTemplate>
        <DataTemplate>
            <Grid Width="200" Height="250">
                <Grid.RowDefinitions>
                    <RowDefinition Height="150"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                
                <!-- Image Thumbnail -->
                <Image Source="{x:Bind ImagePath}" 
                       Stretch="UniformToFill"/>
                
                <!-- Score Badge -->
                <Border Grid.Row="0" 
                        HorizontalAlignment="Right" 
                        VerticalAlignment="Top"
                        Margin="8"
                        Background="{x:Bind ScoreBackgroundBrush}"
                        CornerRadius="4"
                        Padding="8,4">
                    <TextBlock Text="{x:Bind AverageScore}" 
                               FontWeight="Bold"/>
                </Border>
                
                <!-- Details -->
                <StackPanel Grid.Row="1" Padding="8">
                    <TextBlock Text="{x:Bind FileName}" 
                               Style="{StaticResource CaptionTextBlockStyle}"
                               TextTrimming="CharacterEllipsis"/>
                    <ItemsRepeater ItemsSource="{x:Bind CriteriaScores}">
                        <ItemsRepeater.ItemTemplate>
                            <DataTemplate>
                                <TextBlock FontSize="11">
                                    <Run Text="{x:Bind Key}"/>: 
                                    <Run Text="{x:Bind Value}" FontWeight="SemiBold"/>
                                </TextBlock>
                            </DataTemplate>
                        </ItemsRepeater.ItemTemplate>
                    </ItemsRepeater>
                </StackPanel>
            </Grid>
        </DataTemplate>
    </GridView.ItemTemplate>
</GridView>
```

#### 4.2 Filtering & Sorting
- **Score Range Filter**: Show only images scoring 3-4
- **Criterion Filter**: Show images where "accuracy" < 3
- **Folder Filter**: Show only images from specific subfolder
- **Sort Options**: By score, name, folder, processing order

#### 4.3 Enhanced Image Selection UX
When a user clicks/selects an image in the grid:

**Option A: Inline Expansion (Recommended)**
```xml
<!-- Selected image expands inline below its row -->
<Grid x:Name="SelectedImageDetails" 
      Grid.Row="2" 
      Grid.ColumnSpan="5"
      Visibility="Collapsed"
      Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
      BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
      BorderThickness="1"
      CornerRadius="{StaticResource OverlayCornerRadius}"
      Padding="16"
      Margin="0,8,0,8">
    
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="400"/> <!-- Image -->
        <ColumnDefinition Width="*"/>    <!-- Details -->
    </Grid.ColumnDefinitions>
    
    <!-- Full Image Preview -->
    <Image Grid.Column="0" 
           Source="{x:Bind SelectedImage.Path}"
           Stretch="Uniform"
           MaxHeight="400"/>
    
    <!-- Details Panel -->
    <ScrollViewer Grid.Column="1" 
                  Margin="16,0,0,0">
        <StackPanel Spacing="12">
            <!-- Close Button -->
            <Button HorizontalAlignment="Right"
                    Click="CloseDetails_Click">
                <FontIcon Glyph="&#xE8BB;" FontSize="16"/>
            </Button>
            
            <!-- File Info -->
            <TextBlock Text="{x:Bind SelectedImage.FileName}"
                       Style="{StaticResource SubtitleTextBlockStyle}"/>
            
            <!-- Prompt -->
            <Grid Style="{StaticResource CardGridStyle}">
                <StackPanel Spacing="4">
                    <TextBlock Text="Prompt" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <TextBlock Text="{x:Bind SelectedImage.Prompt}"
                               TextWrapping="Wrap"
                               Style="{StaticResource BodyTextBlockStyle}"/>
                </StackPanel>
            </Grid>
            
            <!-- Model Response -->
            <Grid Style="{StaticResource CardGridStyle}">
                <StackPanel Spacing="4">
                    <TextBlock Text="Model Response" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <TextBlock Text="{x:Bind SelectedImage.Response}"
                               TextWrapping="Wrap"
                               Style="{StaticResource BodyTextBlockStyle}"/>
                </StackPanel>
            </Grid>
            
            <!-- Criteria Scores -->
            <Grid Style="{StaticResource CardGridStyle}">
                <StackPanel Spacing="8">
                    <TextBlock Text="Criteria Scores" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <ItemsRepeater ItemsSource="{x:Bind SelectedImage.CriteriaScores}">
                        <ItemsRepeater.ItemTemplate>
                            <DataTemplate>
                                <Grid Margin="0,4">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>
                                    <TextBlock Text="{x:Bind Key}"/>
                                    <TextBlock Grid.Column="1" 
                                               Text="{x:Bind Value}"
                                               FontWeight="SemiBold"/>
                                </Grid>
                            </DataTemplate>
                        </ItemsRepeater.ItemTemplate>
                    </ItemsRepeater>
                </StackPanel>
            </Grid>
            
            <!-- Custom Metadata (Dynamic) -->
            <Grid Style="{StaticResource CardGridStyle}"
                  Visibility="{x:Bind SelectedImage.HasCustomMetadata}">
                <StackPanel Spacing="8">
                    <TextBlock Text="Additional Metadata" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <ItemsRepeater ItemsSource="{x:Bind SelectedImage.CustomMetadata}">
                        <ItemsRepeater.ItemTemplate>
                            <DataTemplate>
                                <Grid Margin="0,4">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto" MinWidth="150"/>
                                        <ColumnDefinition Width="*"/>
                                    </Grid.ColumnDefinitions>
                                    <TextBlock Text="{x:Bind Key}"
                                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                                    <TextBlock Grid.Column="1" 
                                               Text="{x:Bind Value}"
                                               TextWrapping="Wrap"/>
                                </Grid>
                            </DataTemplate>
                        </ItemsRepeater.ItemTemplate>
                    </ItemsRepeater>
                </StackPanel>
            </Grid>
            
            <!-- Processing Info -->
            <Grid Style="{StaticResource CardGridStyle}">
                <StackPanel Spacing="4">
                    <TextBlock Text="Processing Details" 
                               Style="{StaticResource BodyStrongTextBlockStyle}"/>
                    <TextBlock>
                        <Run Text="Processing Time:"/>
                        <Run Text="{x:Bind SelectedImage.ProcessingTime}" FontWeight="SemiBold"/>
                    </TextBlock>
                    <TextBlock Visibility="{x:Bind SelectedImage.HasError}">
                        <Run Text="Error:"/>
                        <Run Text="{x:Bind SelectedImage.Error}" 
                             Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
                    </TextBlock>
                </StackPanel>
            </Grid>
        </StackPanel>
    </ScrollViewer>
</Grid>
```

**Interaction Flow:**
1. User clicks image thumbnail in grid
2. Row expands smoothly with animation
3. Detail panel appears inline below the selected image's row
4. Other images shift down to accommodate
5. Clicking close button or another image collapses current and shows new
6. ESC key also closes detail view

**Benefits:**
- Context preserved - user can see surrounding images
- Smooth navigation between images
- No modal overlay blocking other content
- Responsive to different screen sizes

### 5. Export & Actions Bar
Sticky bottom bar with actions:
- **Export Data** → CSV/JSON/Excel (exports raw data)
- **Share** → Generate shareable link (future)
- **Print Report** → Print-friendly full report

Note: Chart-specific actions (Copy/Save) are placed inline with each visualization for better context and discoverability.

## Visual Design Principles

### 1. Information Hierarchy
- **Primary**: Overall score and status (TitleTextBlockStyle)
- **Secondary**: Individual criterion scores (SubtitleTextBlockStyle)
- **Tertiary**: Metadata and actions (CaptionTextBlockStyle)

### 2. Color Coding (Following Existing Patterns)
```xml
<!-- From EvaluationCardStyles.xaml -->
Excellent (4.5-5.0): #4CAF50 (Green gradient)
Good (3.5-4.4): #2196F3 (Blue gradient)  
Fair (2.5-3.4): #FFC107 (Yellow gradient)
Needs Improvement (1.0-2.4): #FF5722 (Orange/Red gradient)
```

### 3. Layout Standards
- Page padding: `36,24,36,36`
- Card spacing: `4` (SettingsCardSpacing)
- Max content width: `1000`
- Corner radius: `OverlayCornerRadius`
- Border thickness: `1`

### 4. Component Patterns
- Use `CardBackgroundFillColorDefaultBrush` for card backgrounds
- Use `CardStrokeColorDefaultBrush` for borders
- FontIcon size: 16-24 for inline, 48 for features
- Button spacing in groups: `8`

## Technical Implementation

### 1. Data Loading
```csharp
// In EvaluationInsightsPage.xaml.cs
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    if (e.Parameter is string evaluationId)
    {
        var evaluation = await _evaluationStore.GetByIdAsync(evaluationId);
        if (evaluation != null)
        {
            ViewModel = new EvaluationInsightsViewModel(evaluation);
        }
    }
}
```

### 2. Chart Implementation Options

#### Option A: WinUI Community Toolkit Charts
```xml
<toolkit:RadialGauge Value="{x:Bind ViewModel.AverageScore}" />
<toolkit:Chart>
    <toolkit:ColumnSeries ItemsSource="{x:Bind ViewModel.CriteriaScores}" />
</toolkit:Chart>
```

#### Option B: Custom Visual Controls
- Create custom UserControls for charts
- Use Canvas/Path for drawing
- More control over appearance

#### Option C: OxyPlot.WinUI
- Full-featured charting library
- Better for complex visualizations
- Adds dependency

### 3. Export Functionality

#### CSV Export
```csharp
private async Task ExportToCsvAsync()
{
    var savePicker = new FileSavePicker();
    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
    savePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
    savePicker.SuggestedFileName = $"{ViewModel.Name}_results";
    
    var file = await savePicker.PickSaveFileAsync();
    if (file != null)
    {
        // Write CSV content
        await FileIO.WriteTextAsync(file, GenerateCsvContent());
    }
}
```

#### Chart-Specific Controls
Charts should have their own inline controls, placed near the chart title:
- **Copy Chart**: Copies the current visualization to clipboard
- **Save as Image**: Exports the chart as PNG/JPEG with proper formatting

```xml
<!-- Chart header with controls -->
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <TextBlock Grid.Column="0" 
               Text="Average Scores by Evaluation Criteria"
               Style="{StaticResource BodyStrongTextBlockStyle}"/>
    
    <StackPanel Grid.Column="1" 
                Orientation="Horizontal" 
                Spacing="4">
        <Button Click="CopyChart_Click"
                ToolTipService.ToolTip="Copy chart to clipboard">
            <FontIcon Glyph="&#xE8C8;" FontSize="14"/>
        </Button>
        <Button Click="SaveChartAsImage_Click"
                ToolTipService.ToolTip="Save chart as image">
            <FontIcon Glyph="&#xE838;" FontSize="14"/>
        </Button>
    </StackPanel>
</Grid>
```

#### Chart Image Export
```csharp
private async Task ExportChartAsImageAsync()
{
    // Chart already includes title and proper labeling
    // Just render the chart container with its existing design
    
    var renderBitmap = new RenderTargetBitmap();
    await renderBitmap.RenderAsync(ChartContainer);
    
    // Save dialog
    var savePicker = new FileSavePicker();
    savePicker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
    savePicker.FileTypeChoices.Add("JPEG Image", new List<string>() { ".jpg", ".jpeg" });
    savePicker.SuggestedFileName = $"{ViewModel.EvaluationName}_criteria_scores_{DateTime.Now:yyyyMMdd}";
    
    var file = await savePicker.PickSaveFileAsync();
    if (file != null)
    {
        await SaveBitmapToFileAsync(renderBitmap, file);
    }
}

private async Task CopyChartToClipboardAsync()
{
    // Similar rendering as above
    var renderBitmap = await RenderChartWithMetadataAsync();
    
    // Convert to stream
    var stream = new InMemoryRandomAccessStream();
    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
    
    var pixels = await renderBitmap.GetPixelsAsync();
    encoder.SetPixelData(
        BitmapPixelFormat.Bgra8, 
        BitmapAlphaMode.Premultiplied,
        (uint)renderBitmap.PixelWidth,
        (uint)renderBitmap.PixelHeight,
        96.0, 96.0,
        pixels.ToArray());
    
    await encoder.FlushAsync();
    
    // Copy to clipboard
    var dataPackage = new DataPackage();
    dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
    dataPackage.Properties.Title = $"{ViewModel.EvaluationName} Evaluation Results";
    Clipboard.SetContent(dataPackage);
    
    // Show confirmation
    ShowCopyConfirmation();
}
```

#### Visualization Best Practices
1. **Proper Chart Anatomy**:
   - Clear, descriptive title
   - Labeled axes with units
   - Visible gridlines for reference
   - Scale indicators (1-5 range)
   - Legend when multiple series
   - Data labels on bars/points

2. **Context and Metadata**:
   - Chart title describes what's being shown
   - Axes clearly labeled (e.g., "Score (1-5 scale)", "Number of Images")
   - Statistical summary where relevant (mean, median, std dev)
   - Total count indicators

3. **Visual Hierarchy**:
   - Title is prominent but not overwhelming
   - Data is the focus, not decorations
   - Sufficient contrast for readability
   - Consistent color coding across views

4. **Export Quality**:
   - Charts are self-contained (don't need external context)
   - White background for printing
   - High resolution output
   - No UI chrome in exports (just the visualization)

5. **Control Placement**:
   - Chart-specific actions (Copy/Save) placed inline with chart header
   - Global actions (Export Data/Print) in bottom action bar
   - Tooltips explain what each action does

## Comparison View Planning

### Multi-Evaluation Comparison (Up to 5)

#### Selection Flow
1. User selects 2-5 evaluations from list
2. Clicks "Compare" in action bar
3. Navigates to comparison view with evaluation IDs

#### Comparison Layout
```xml
<Grid>
    <!-- Header with selected evaluations -->
    <Grid Grid.Row="0">
        <ItemsRepeater ItemsSource="{x:Bind SelectedEvaluations}">
            <ItemsRepeater.Layout>
                <UniformGridLayout Orientation="Horizontal" 
                                   MaximumRowsOrColumns="5"
                                   ItemsStretch="Fill"/>
            </ItemsRepeater.Layout>
            <ItemsRepeater.ItemTemplate>
                <DataTemplate>
                    <Grid Style="{StaticResource CardGridStyle}">
                        <StackPanel Spacing="4">
                            <TextBlock Text="{x:Bind Name}" 
                                       Style="{StaticResource BodyStrongTextBlockStyle}"
                                       TextTrimming="CharacterEllipsis"/>
                            <TextBlock Text="{x:Bind ModelName}" 
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                            <TextBlock Text="{x:Bind Timestamp}" 
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                        </StackPanel>
                    </Grid>
                </DataTemplate>
            </ItemsRepeater.ItemTemplate>
        </ItemsRepeater>
    </Grid>
    
    <!-- Comparison Visualizations -->
    <Pivot Grid.Row="1">
        <PivotItem Header="Side by Side">
            <!-- Grouped bar chart -->
        </PivotItem>
        <PivotItem Header="Radar View">
            <!-- Spider/radar chart -->
        </PivotItem>
        <PivotItem Header="Table View">
            <!-- Comparison matrix -->
        </PivotItem>
        <PivotItem Header="Trends">
            <!-- Line charts over time -->
        </PivotItem>
    </Pivot>
</Grid>
```

#### Comparison Metrics
- **Delta indicators**: +/- changes between evaluations
- **Best performer**: Highlight highest score for each criterion
- **Consistency**: Standard deviation across evaluations
- **Improvement**: Trend direction if timestamps differ

### Visual Treatments
```csharp
// Color assignment for comparisons
private readonly string[] ComparisonColors = new[]
{
    "#2196F3", // Blue
    "#4CAF50", // Green
    "#FF9800", // Orange
    "#9C27B0", // Purple
    "#F44336"  // Red
};

// Delta indicator
public string GetDeltaIndicator(double current, double previous)
{
    var delta = current - previous;
    if (delta > 0) return $"↑ +{delta:F1}";
    if (delta < 0) return $"↓ {delta:F1}";
    return "→ 0.0";
}

## Future Enhancements

### When Individual Results Are Available
1. **Image Browser**
   - Thumbnail grid with scores
   - Click for full details
   - Filter by score/criterion

2. **Folder Analysis**
   - Performance by subfolder
   - Identify problem areas
   - Drill-down navigation

3. **Advanced Statistics**
   - Distribution curves
   - Outlier detection
   - Confidence intervals

### Integration Features
1. **Export to Power BI**
2. **API for external analysis**
3. **Scheduled reports**
4. **Team annotations**

## Implementation Phases

### Phase 1: Basic Insights (MVP)
- [ ] Create EvaluationInsightsPage
- [ ] Display key metrics
- [ ] Show criteria scores (table)
- [ ] Basic export (CSV)

### Phase 2: Visualizations
- [ ] Add charting library
- [ ] Implement bar charts
- [ ] Add interactive features
- [ ] Chart export functionality

### Phase 3: Enhanced Features
- [ ] Comparison view
- [ ] Advanced statistics
- [ ] Print-friendly reports
- [ ] Share functionality

### Phase 4: Individual Results (Future)
- [ ] Modify data model
- [ ] Image result browser
- [ ] Folder-based analysis
- [ ] Detailed drill-downs

## Open Questions

1. **Charting Library**: Which one to use?
   - WinUI Community Toolkit (simple, integrated)
   - OxyPlot (powerful, more complex)
   - Custom implementation (full control)

2. **Export Formats**: Priority order?
   - CSV (easiest)
   - Excel (requires library)
   - PDF (requires library)
   - JSON (developer-friendly)

3. **Real-time Updates**: For running evaluations?
   - Poll for updates
   - Use events/notifications
   - Static snapshot

4. **Comparison Limits**: Why 5 evaluations max?
   - UI space constraints
   - Performance considerations
   - User comprehension

5. **Data Model Enhancement**: Should we modify to store individual results?
   - Storage implications
   - Performance impact
   - Migration strategy

## Detailed Component Specifications

### Page Layout Structure
```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/> <!-- Header -->
        <RowDefinition Height="*"/>    <!-- Content -->
        <RowDefinition Height="Auto"/> <!-- Actions Bar -->
    </Grid.RowDefinitions>
    
    <!-- Header -->
    <Grid Grid.Row="0" Padding="36,24,36,0">
        <TextBlock Text="Evaluation Insights" Style="{StaticResource TitleTextBlockStyle}"/>
    </Grid>
    
    <!-- Scrollable Content -->
    <ScrollViewer Grid.Row="1">
        <Grid Padding="36,24,36,36">
            <StackPanel MaxWidth="1000" Spacing="{StaticResource SettingsCardSpacing}">
                <!-- Content sections here -->
            </StackPanel>
        </Grid>
    </ScrollViewer>
    
    <!-- Sticky Actions Bar -->
    <Grid Grid.Row="2" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
          BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
          BorderThickness="0,1,0,0">
        <!-- Export actions -->
    </Grid>
</Grid>
```

### Metric Cards Component
```xml
<Grid Style="{StaticResource CardGridStyle}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <TextBlock Grid.Row="0" 
               Text="Images Processed" 
               Style="{StaticResource CaptionTextBlockStyle}"
               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
    
    <TextBlock Grid.Row="1" 
               Text="1,000" 
               Style="{StaticResource SubtitleTextBlockStyle}"
               FontWeight="SemiBold"/>
    
    <TextBlock Grid.Row="2" 
               Text="100% Complete" 
               Style="{StaticResource CaptionTextBlockStyle}"
               Foreground="{ThemeResource SystemFillColorSuccessBrush}"/>
</Grid>
```

### Score Visualization Component
```xml
<!-- Individual Criterion Score Card -->
<Grid Style="{StaticResource CardGridStyle}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    
    <StackPanel Grid.Column="0" Spacing="4">
        <TextBlock Text="{x:Bind CriterionName}" 
                   Style="{StaticResource BodyStrongTextBlockStyle}"/>
        
        <ProgressBar Value="{x:Bind Score}" 
                     Maximum="5"
                     Height="8"
                     Foreground="{x:Bind ScoreColorBrush}"/>
        
        <TextBlock Style="{StaticResource CaptionTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}">
            <Run Text="{x:Bind Score}"/><Run Text=" / 5.0"/>
        </TextBlock>
    </StackPanel>
    
    <Border Grid.Column="1" 
            Background="{x:Bind RatingBackgroundBrush}"
            CornerRadius="4"
            Padding="8,4">
        <TextBlock Text="{x:Bind RatingText}" 
                   FontSize="12"
                   FontWeight="SemiBold"/>
    </Border>
</Grid>
```

### View Toggle Component
```xml
<StackPanel Orientation="Horizontal" Spacing="4">
    <ToggleButton x:Name="ChartViewToggle" 
                  IsChecked="True"
                  Click="ViewToggle_Click">
        <StackPanel Orientation="Horizontal" Spacing="8">
            <FontIcon Glyph="&#xE8A1;" FontSize="16"/>
            <TextBlock Text="Chart"/>
        </StackPanel>
    </ToggleButton>
    
    <ToggleButton x:Name="TableViewToggle" 
                  Click="ViewToggle_Click">
        <StackPanel Orientation="Horizontal" Spacing="8">
            <FontIcon Glyph="&#xE8A5;" FontSize="16"/>
            <TextBlock Text="Table"/>
        </StackPanel>
    </ToggleButton>
    
    <ToggleButton x:Name="CardViewToggle" 
                  Click="ViewToggle_Click">
        <StackPanel Orientation="Horizontal" Spacing="8">
            <FontIcon Glyph="&#xE8A9;" FontSize="16"/>
            <TextBlock Text="Cards"/>
        </StackPanel>
    </ToggleButton>
</StackPanel>
```

### Export Actions Bar
```xml
<Grid Padding="24,12">
    <StackPanel Orientation="Horizontal" 
                HorizontalAlignment="Right" 
                Spacing="8">
        
        <Button Click="ExportData_Click">
            <Button.Flyout>
                <MenuFlyout>
                    <MenuFlyoutItem Text="Export as CSV" Click="ExportCsv_Click">
                        <MenuFlyoutItem.Icon>
                            <FontIcon Glyph="&#xE8B7;"/>
                        </MenuFlyoutItem.Icon>
                    </MenuFlyoutItem>
                    <MenuFlyoutItem Text="Export as JSON" Click="ExportJson_Click">
                        <MenuFlyoutItem.Icon>
                            <FontIcon Glyph="&#xE943;"/>
                        </MenuFlyoutItem.Icon>
                    </MenuFlyoutItem>
                    <MenuFlyoutItem Text="Export as Excel" Click="ExportExcel_Click">
                        <MenuFlyoutItem.Icon>
                            <FontIcon Glyph="&#xE8B7;"/>
                        </MenuFlyoutItem.Icon>
                    </MenuFlyoutItem>
                </MenuFlyout>
            </Button.Flyout>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <FontIcon Glyph="&#xE896;" FontSize="16"/>
                <TextBlock Text="Export Data"/>
            </StackPanel>
        </Button>
        
        <Button Click="CopyChart_Click"
                ToolTipService.ToolTip="Copy visualization to clipboard">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <FontIcon Glyph="&#xE8C8;" FontSize="16"/>
                <TextBlock Text="Copy Chart"/>
            </StackPanel>
        </Button>
        
        <Button Click="SaveVisualization_Click">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <FontIcon Glyph="&#xE838;" FontSize="16"/>
                <TextBlock Text="Save as Image"/>
            </StackPanel>
        </Button>
    </StackPanel>
</Grid>
```

### Empty State for Missing Data
```xml
<Grid HorizontalAlignment="Center" 
      VerticalAlignment="Center"
      MaxWidth="400">
    <StackPanel Spacing="12">
        <FontIcon Glyph="&#xE9CE;" 
                  FontSize="48"
                  Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                  HorizontalAlignment="Center"/>
        
        <TextBlock Text="No evaluation data available"
                   Style="{StaticResource SubtitleTextBlockStyle}"
                   HorizontalAlignment="Center"/>
        
        <TextBlock Text="This evaluation doesn't have any results to display."
                   Style="{StaticResource BodyTextBlockStyle}"
                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                   TextAlignment="Center"
                   TextWrapping="Wrap"/>
    </StackPanel>
</Grid>
```

### Chart Container with Loading State
```xml
<Grid x:Name="ChartContainer" MinHeight="400">
    <!-- Loading State -->
    <Grid x:Name="LoadingPanel" Visibility="Visible">
        <StackPanel HorizontalAlignment="Center" 
                    VerticalAlignment="Center"
                    Spacing="12">
            <ProgressRing IsActive="True" Width="48" Height="48"/>
            <TextBlock Text="Loading visualization..."
                       Style="{StaticResource CaptionTextBlockStyle}"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        </StackPanel>
    </Grid>
    
    <!-- Chart Content -->
    <Grid x:Name="ChartContent" Visibility="Collapsed">
        <!-- Chart implementation here -->
    </Grid>
</Grid>
```

## Animation Specifications

### Page Transitions
```csharp
// Entrance animation for metric cards
private void AnimateMetricCards()
{
    var staggerDelay = 50; // milliseconds
    var cards = MetricCardsPanel.Children;
    
    for (int i = 0; i < cards.Count; i++)
    {
        var card = cards[i] as FrameworkElement;
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            BeginTime = TimeSpan.FromMilliseconds(i * staggerDelay),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        
        Storyboard.SetTarget(animation, card);
        Storyboard.SetTargetProperty(animation, "Opacity");
    }
}
```

### Chart Animations
- Bars grow from bottom on load
- Smooth transitions when switching views
- Hover states with subtle scale transforms

## Accessibility Considerations

1. **Screen Reader Support**
   - All charts have text alternatives
   - Proper heading hierarchy
   - Descriptive labels for all metrics

2. **Keyboard Navigation**
   - Tab order follows visual hierarchy
   - All interactive elements keyboard accessible
   - Clear focus indicators

3. **High Contrast Mode**
   - Charts adapt to high contrast themes
   - Sufficient color contrast ratios
   - Patterns in addition to colors

## Success Metrics

1. **User Understanding**: Can users quickly grasp evaluation performance?
2. **Export Usage**: Do users successfully export data?
3. **Time to Insight**: How quickly do users find what they need?
4. **Feature Adoption**: Which views are most used?

## Summary & Recommendations

### Critical First Step: Preserve Individual Results
**The JSONL files already contain per-image criteria scores** - we just need to store them instead of discarding them!

### Revised Implementation Plan

#### Phase 1A: Data Model Enhancement (1-2 days)
1. **Modify EvaluationResultsStore** to save individual results
2. **Add details.json** file alongside each evaluation
3. **Preserve folder structure** from image paths
4. **No UI changes needed** - just data storage

#### Phase 1B: Basic Insights Page (3-5 days)
With aggregate data only:
1. **Key Metrics Dashboard** - Overview cards
2. **Criteria Breakdown** - Bar chart and table views
3. **Basic Export** - CSV with aggregate scores
4. **Chart Export** - PNG/JPEG with proper formatting

#### Phase 2: Enhanced Insights (1 week)
With individual results now available:
1. **Image Browser** - Grid view with filtering
2. **Statistical Analysis** - Std dev, percentiles, distributions
3. **Folder Pivoting** - Performance by subfolder
4. **Detail Drill-down** - Click image to see all details

#### Phase 3: Advanced Features (2 weeks)
1. **Comparison View** - Up to 5 evaluations
2. **Custom Visualizations** - Radar charts, heatmaps
3. **Advanced Filters** - Complex queries
4. **Batch Export** - Multiple formats

### Key Insights from Analysis

1. **We're throwing away valuable data** - Individual scores are parsed but not stored
2. **Small code change, big impact** - Just need to save what we already parse
3. **Rich insights possible** - Folder analysis, outlier detection, consistency metrics
4. **User requested features achievable** - All the pivoting, browsing, and statistics are possible

### Technical Decisions

1. **Storage Strategy**
   - Main evaluation file: Keep as-is for backward compatibility
   - Details file: New JSON file with array of individual results
   - Lazy loading: Load details only when needed

2. **Charting Library**
   - Start with WinUI Community Toolkit
   - Add advanced charts (like radar) with OxyPlot if needed

3. **Export Formats**
   - Priority: PNG/JPEG (for reports), CSV (for analysis)
   - Include proper titles and metadata in all exports

### Design Excellence

1. **Progressive Enhancement**
   - Start with what works today (aggregates)
   - Add richness as data becomes available
   - Never break existing functionality

2. **Delightful Details**
   - Smooth animations on data load
   - Intuitive filtering with instant feedback
   - Smart defaults (e.g., show worst performing items first)

3. **Professional Output**
   - Charts include all context (title, model, date)
   - Export quality suitable for presentations
   - Consistent visual language

### Success Metrics

1. **Time to Insight** < 5 seconds
2. **Export Success Rate** > 95%
3. **Feature Discovery** - Users find advanced features
4. **Data Completeness** - No more discarded scores

This approach delivers immediate value while building toward the full vision of rich, interactive evaluation insights.

---

*This document will be continuously updated as we refine the design and gather feedback.*