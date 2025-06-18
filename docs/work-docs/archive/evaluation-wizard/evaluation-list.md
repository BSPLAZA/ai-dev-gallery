# Evaluation Experience UX/UI Design Plan

## Current State Analysis

### Evaluation List Page (Home)
Current fields displayed:
- **Evaluation name** - Auto-generated timestamp-based name
- **Status** - Single status badge (Succeeded/Failed/Pending/Imported)
- **Progress** - Simple percentage (not very meaningful)
- **Dataset** - Just the filename
- **Rows** - Number of items
- **Criteria** - Count of criteria (not descriptive)

### Issues with Current Design
1. **Not Informative Enough**:
   - No model information shown
   - No evaluation type visible
   - No workflow type indicated
   - No date/time information
   - No metrics/scores preview

2. **Poor Information Hierarchy**:
   - All evaluations look the same
   - Can't quickly identify what was evaluated
   - No visual grouping or filtering

3. **Limited Actions**:
   - Can only delete evaluations
   - No way to view details
   - No comparison capabilities
   - No export options

## Improved Evaluation List Design

### Enhanced Table Columns
```
| Status | Evaluation | Model | Dataset | Score | Date | Actions |
|--------|------------|--------|---------|--------|------|---------|
| ✓      | Image Desc | GPT-4V | Nature/1000 | 85.2% | 2h ago | ⋮ |
| ⏳     | Translation| Claude | Docs/500 | -- | Running | ⋮ |
| ✗      | Q&A Test   | Gemini | FAQ/250 | Failed | Yesterday | ⋮ |
```

### Key Improvements
1. **Status Icons**: Visual indicators (✓ ✗ ⏳) with color coding
2. **Evaluation Type**: Show what kind of evaluation
3. **Model Info**: Which model was tested
4. **Dataset Summary**: Folder/count format
5. **Score Preview**: Overall score for completed evaluations
6. **Relative Time**: "2h ago" more intuitive than timestamps
7. **Action Menu**: Dropdown with View/Compare/Export/Delete

### Enhanced Data Model
```csharp
public class EnhancedEvaluation
{
    // Identity
    public string Id { get; set; }
    public string Name { get; set; }
    
    // Type Information
    public EvaluationType Type { get; set; }
    public EvaluationWorkflow Workflow { get; set; }
    
    // Model Information
    public string ModelProvider { get; set; }  // "OpenAI", "Azure", etc.
    public string ModelName { get; set; }      // "GPT-4V", "Claude-3.5"
    
    // Dataset Information
    public string DatasetName { get; set; }
    public string DatasetFolder { get; set; }  // Primary folder if organized
    public int ItemCount { get; set; }
    
    // Status & Progress
    public EvaluationStatus Status { get; set; }
    public double Progress { get; set; }       // 0-100
    public string? CurrentOperation { get; set; } // "Processing image 45/100"
    
    // Results Summary
    public double? OverallScore { get; set; }  // Aggregate score
    public Dictionary<string, double>? MetricScores { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public decimal? EstimatedCost { get; set; }
}
```

## Evaluation Details Page (Click to View)

### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│ ← Back to Evaluations                                       │
│                                                             │
│ Image Description Evaluation - GPT-4V                   ⋮  │
│ Completed 2 hours ago • Duration: 45 min • Cost: $2.35    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─────────────┬─────────────┬─────────────┬─────────────┐ │
│ │ Overall     │ SPICE       │ CLIPScore   │ AI Judge    │ │
│ │   85.2%     │   82.5      │   0.89      │   88.1%     │ │
│ └─────────────┴─────────────┴─────────────┴─────────────┘ │
│                                                             │
│ [Overview] [By Folder] [By Metric] [Samples] [Export]      │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │                    Tab Content Area                      │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Tab 1: Overview
- **Configuration Summary**: Model, dataset, prompts used
- **Performance Chart**: Line/bar chart of all metrics
- **Statistics**: Min/max/mean/std dev for each metric
- **Cost Breakdown**: Tokens used, API calls made

### Tab 2: By Folder (if dataset organized)
```
📁 Nature (350 images) - Avg: 87.2%
   ├─ 📁 Landscapes (120) - Avg: 92.1% ⭐
   ├─ 📁 Wildlife (150) - Avg: 85.3%
   └─ 📁 Underwater (80) - Avg: 82.4% ⚠️

📁 Urban (400 images) - Avg: 83.5%
   ├─ 📁 Architecture (200) - Avg: 86.2%
   └─ 📁 Street (200) - Avg: 80.8% ⚠️
```

### Tab 3: By Metric
Interactive charts for each metric:
- **Distribution Histogram**: Show score distribution
- **Correlation Matrix**: How metrics relate
- **Outlier Detection**: Highlight best/worst performers

### Tab 4: Samples
Grid view of actual results:
```
┌─────────┬─────────┬─────────┬─────────┐
│ [Image] │ [Image] │ [Image] │ [Image] │
│ Score:95│ Score:89│ Score:45│ Score:22│
│ ✓ Good  │ ✓ Good  │ ⚠ Fair  │ ✗ Poor  │
└─────────┴─────────┴─────────┴─────────┘
```
Click any sample to see:
- Original image
- Generated response
- Expected response (if available)
- All metric scores
- AI Judge feedback

### Tab 5: Export
- **Download Options**:
  - Full results (CSV/JSON)
  - Summary report (PDF)
  - Best/worst samples
  - Share link generation

## Comparison View (Compare Multiple Evaluations)

### Access Pattern
1. Select multiple evaluations with checkboxes
2. Click "Compare" button
3. Opens comparison view

### Comparison Layout
```
┌─────────────────────────────────────────────────────────────┐
│ Comparing 3 Evaluations                                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Model Performance Comparison                                │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │     📊 Bar Chart: GPT-4V vs Claude vs Gemini           │ │
│ │     [Interactive chart with metric selection]           │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ Detailed Comparison Table                                   │
│ ┌─────────────┬──────────┬──────────┬──────────┐         │
│ │ Metric      │ GPT-4V   │ Claude   │ Gemini   │         │
│ ├─────────────┼──────────┼──────────┼──────────┤         │
│ │ Overall     │ 85.2% ⭐ │ 82.1%    │ 79.5%    │         │
│ │ SPICE       │ 82.5     │ 84.2 ⭐  │ 78.3     │         │
│ │ Speed       │ 45 min   │ 38 min⭐ │ 52 min   │         │
│ │ Cost        │ $2.35    │ $1.89 ⭐ │ $3.12    │         │
│ └─────────────┴──────────┴──────────┴──────────┘         │
│                                                             │
│ Insights & Recommendations                                  │
│ • GPT-4V performs best overall but costs more              │
│ • Claude offers best value (performance/cost)              │
│ • Gemini struggles with wildlife images specifically       │
└─────────────────────────────────────────────────────────────┘
```

## Dashboard View (Future Enhancement)

### Key Metrics Dashboard
```
┌─────────────────────────────────────────────────────────────┐
│ Evaluation Insights Dashboard                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─────────────┬─────────────┬─────────────┬─────────────┐ │
│ │ Total Evals │ This Month  │ Avg Score   │ Total Cost  │ │
│ │     142     │     23      │   83.7%     │   $145.20   │ │
│ └─────────────┴─────────────┴─────────────┴─────────────┘ │
│                                                             │
│ Model Performance Trends (Last 30 days)                     │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ 📈 Line chart showing score trends by model over time   │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ Best Performing Categories        Needs Improvement         │
│ • Nature scenes: 92.1% avg       • Indoor scenes: 71.2%    │
│ • Landscapes: 91.5% avg          • Low light: 68.5%        │
│ • Portraits: 89.3% avg           • Abstract art: 65.3%     │
└─────────────────────────────────────────────────────────────┘
```

## Advanced Features

### 1. Smart Filtering & Search
- Filter by: Status, Model, Date Range, Score Range
- Search in: Evaluation names, datasets, prompts
- Saved filter presets

### 2. Batch Operations
- Compare multiple evaluations
- Bulk export results
- Archive old evaluations
- Re-run with different models

### 3. Insights Engine
- Automatic insight generation
- Performance trend detection
- Cost optimization suggestions
- Dataset quality analysis

### 4. Collaboration Features
- Share evaluation results
- Add comments/notes
- Tag evaluations
- Export branded reports

## Implementation Priority

### Phase 1: Enhanced List View (1 week)
1. Update Evaluation model with new fields
2. Redesign list UI with better columns
3. Add status icons and color coding
4. Implement relative time display
5. Add action menu dropdown

### Phase 2: Basic Details Page (2 weeks)
1. Create evaluation details page
2. Implement overview tab
3. Add metric visualizations
4. Enable sample viewing
5. Basic export functionality

### Phase 3: Comparison View (1 week)
1. Multi-select in list view
2. Comparison page layout
3. Interactive charts
4. Comparison insights

### Phase 4: Advanced Features (2 weeks)
1. Filtering and search
2. Dashboard view
3. Batch operations
4. Insights engine

## Technical Considerations

### Performance
- Lazy load evaluation details
- Virtualized lists for large datasets
- Cache computed metrics
- Progressive image loading

### Data Storage
- Store aggregate metrics in main table
- Detailed results in separate files
- Index by model, date, score
- Compress old evaluation data

### Accessibility
- Keyboard navigation for all views
- Screen reader announcements
- High contrast mode support
- Alternative text for all visuals

## Success Metrics
1. **Time to Insight**: < 3 clicks to understand evaluation performance
2. **Comparison Time**: < 30 seconds to compare 3 evaluations
3. **Export Usage**: 50% of users export results
4. **Return Rate**: Users check results multiple times# Evaluation List Page Redesign

## Current vs Improved Design

### Current Design Issues
```
┌─────────────────────────────────────────────────────────────────┐
│ Evaluation name | Status    | Progress | Dataset  | Rows | Criteria│
├─────────────────────────────────────────────────────────────────┤
│ gpt4o eval      | Succeeded | 65       | gpt4o eval| 1001 | 1      │
└─────────────────────────────────────────────────────────────────┘
```

**Problems**:
- Generic names don't convey what was evaluated
- No model information
- "Progress" is meaningless for completed evaluations
- Dataset name is redundant
- Criteria count doesn't indicate what was measured

### Improved Design
```
┌─────────────────────────────────────────────────────────────────────────┐
│ 🏠 Evaluations                                     [+ New] [⟳] [Filter ▼]│
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│ 🔍 Search evaluations, models, or datasets...                    [Clear]│
│                                                                          │
│ ┌──┬────────────────────┬─────────┬───────────┬────────┬────────┬────┐│
│ │□ │ Evaluation         │ Model   │ Dataset   │ Score  │ When   │    ││
│ ├──┼────────────────────┼─────────┼───────────┼────────┼────────┼────┤│
│ │□ │ ✅ Image Captions  │ GPT-4V  │ Nature    │ 92.5%  │ 2h ago │ ⋮  ││
│ │  │    Wildlife photos │ OpenAI  │ 850 items │ ████▌  │        │    ││
│ ├──┼────────────────────┼─────────┼───────────┼────────┼────────┼────┤│
│ │□ │ ⏳ Translation Test│ Claude  │ TechDocs  │ 45%    │ Running│ ⋮  ││
│ │  │    English→Spanish │ Anthropic│ 1200/2500│ ██▌    │ 15 min │    ││
│ ├──┼────────────────────┼─────────┼───────────┼────────┼────────┼────┤│
│ │□ │ ❌ Q&A Evaluation  │ Gemini  │ Support   │ Failed │ 1d ago │ ⋮  ││
│ │  │    Customer FAQs  │ Google  │ 325 items │ Error  │        │    ││
│ ├──┼────────────────────┼─────────┼───────────┼────────┼────────┼────┤│
│ │□ │ 📥 Imported Results│ Multiple│ Mixed     │ 87.3%  │ 3d ago │ ⋮  ││
│ │  │    Team benchmark │ Various │ 5K items  │ ████▎  │        │    ││
│ └──┴────────────────────┴─────────┴───────────┴────────┴────────┴────┘│
│                                                                          │
│ Showing 4 of 23 evaluations                          [10 ▼] per page < > │
└─────────────────────────────────────────────────────────────────────────┘
```

## Key Design Elements

### 1. Status Icons
- ✅ **Completed** (green) - Successfully finished
- ⏳ **Running** (amber) - In progress with live updates
- ❌ **Failed** (red) - Error occurred
- 📥 **Imported** (blue) - External results
- ⏸️ **Paused** (gray) - Manually paused
- 🔄 **Queued** (purple) - Waiting to start

### 2. Two-Line Format
```
Primary:   ✅ Image Captions     | GPT-4V
Secondary:    Wildlife photos    | OpenAI
```
- **Line 1**: Status icon + Evaluation type + Model name
- **Line 2**: Specific description + Provider

### 3. Smart Dataset Display
Instead of just filename:
```
Nature         (Single folder with count)
850 items

Mixed/5 folders (Multiple folders)
2,340 items
```

### 4. Visual Score Bar
```
92.5% ████▌   (Filled based on score)
45%   ██▌     (Partial for running)
Error -----   (Dashed for failed)
```

### 5. Action Menu (⋮)
Click to reveal:
- 👁️ View Details
- 📊 Compare
- 📥 Download Results  
- 🔄 Re-run
- 📋 Copy Configuration
- 🗑️ Delete

### 6. Multi-Select Actions
When checkboxes selected:
```
[2 selected]  [Compare] [Export All] [Delete]
```

## Interactive Elements

### Hover States
- **Row Hover**: Highlight with subtle background
- **Score Hover**: Show tooltip with metric breakdown
- **Status Hover**: Show detailed status/error message
- **Time Hover**: Show exact timestamp

### Click Actions
- **Row Click**: Navigate to evaluation details
- **Model Name**: Filter by that model
- **Dataset Name**: Show dataset preview
- **Score Bar**: Show score distribution

### Live Updates (Running Evaluations)
```
⏳ Translation Test    | Claude    | TechDocs   | 45%    | Running
   Processing item 1200 of 2500 (48%)              ██▌      15 min
```
- Progress percentage updates every few seconds
- Current operation shown in subtitle
- Estimated time remaining

## Filter Panel (Collapsed by Default)
```
[Filter ▼] clicked expands to:

├─ Status          ├─ Model Type      ├─ Date Range
│  ☑ Completed     │  ☑ OpenAI        │  ○ Today
│  ☑ Running       │  ☑ Anthropic     │  ○ This Week
│  ☐ Failed        │  ☑ Google        │  ● This Month
│  ☑ Imported      │  ☐ Azure         │  ○ All Time

├─ Score Range     ├─ Dataset Size    ├─ Sort By
│  [0] to [100]    │  ○ < 100         │  ● Recent First
│  ━━●━━━━━━●━━    │  ○ 100-1000      │  ○ Score (High→Low)
│   25      90     │  ● > 1000        │  ○ Score (Low→High)
                                      │  ○ Name (A→Z)
```

## Responsive Behavior

### Desktop (>1200px)
- All columns visible
- Multi-line cells for better readability
- Hover interactions enabled

### Tablet (768-1200px)
- Hide provider name (show on hover)
- Combine score and progress bar
- Stack action buttons vertically

### Mobile (<768px)
- Card-based layout
- Stack all information vertically
- Swipe actions for delete/view
- Bottom sheet for filters

## Implementation Notes

### Enhanced Evaluation Model
```csharp
public class Evaluation
{
    // Display
    public string Id { get; set; }
    public string Name { get; set; }          // "Image Captions"
    public string Description { get; set; }   // "Wildlife photos"
    
    // Status
    public EvaluationStatus Status { get; set; }
    public double Progress { get; set; }      // 0-100
    public string? CurrentOperation { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Model
    public string ModelName { get; set; }     // "GPT-4V"
    public string ModelProvider { get; set; } // "OpenAI"
    
    // Dataset
    public string DatasetName { get; set; }   // "Nature"
    public int ItemCount { get; set; }        // 850
    public int? FolderCount { get; set; }     // null or count
    
    // Results
    public double? OverallScore { get; set; } // 92.5
    public Dictionary<string, double>? Metrics { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}
```

### Status Icon Mapping
```csharp
public static string GetStatusIcon(EvaluationStatus status) => status switch
{
    EvaluationStatus.Completed => "✅",
    EvaluationStatus.Running => "⏳",
    EvaluationStatus.Failed => "❌",
    EvaluationStatus.Imported => "📥",
    EvaluationStatus.Paused => "⏸️",
    EvaluationStatus.Queued => "🔄",
    _ => "❓"
};
```

### Time Display Helper
```csharp
public static string GetRelativeTime(DateTime dateTime)
{
    var span = DateTime.Now - dateTime;
    return span switch
    {
        { TotalMinutes: < 1 } => "Just now",
        { TotalMinutes: < 60 } => $"{(int)span.TotalMinutes}m ago",
        { TotalHours: < 24 } => $"{(int)span.TotalHours}h ago",
        { TotalDays: < 7 } => $"{(int)span.TotalDays}d ago",
        { TotalDays: < 30 } => $"{(int)(span.TotalDays / 7)}w ago",
        _ => dateTime.ToString("MMM d")
    };
}
```