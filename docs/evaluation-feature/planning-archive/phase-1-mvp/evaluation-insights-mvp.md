# Evaluation Insights Page - MVP Design

## Overview

When a user clicks on an evaluation from the list, they see comprehensive insights about that evaluation's performance. This page provides rich visualizations of the imported JSONL data.

## Page Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Back to Evaluations                                                   │
│                                                                          │
│ GPT-4V Image Captions Evaluation                                   ⋮   │
│ Wildlife & Nature Photos • 850 images • Completed 2 hours ago          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│ ┌─────────────┬─────────────┬─────────────┬─────────────┐             │
│ │ Overall     │ Avg. Score  │ Best Score  │ Completion  │             │
│ │   92.5%     │   4.62/5    │   5.0/5     │   100%      │             │
│ │ ████████▌   │ ⭐⭐⭐⭐⭐    │ Perfect: 423│ 850/850     │             │
│ └─────────────┴─────────────┴─────────────┴─────────────┘             │
│                                                                          │
│ [Overview] [By Criteria] [By Folder] [Image Viewer] [Export]           │
│                                                                          │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │                         Tab Content Area                             │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 1: Overview

### Score Distribution Chart
```
Score Distribution (850 images)
┌────────────────────────────────────────────────────────┐
│ 5.0 ████████████████████████████████ 423 (49.8%)      │
│ 4.5 ██████████████████ 234 (27.5%)                    │
│ 4.0 ████████ 112 (13.2%)                               │
│ 3.5 ███ 45 (5.3%)                                      │
│ 3.0 █ 23 (2.7%)                                        │
│ <3  █ 13 (1.5%)                                        │
└────────────────────────────────────────────────────────┘
```

### Key Metrics Grid
```
┌─────────────────┬─────────────────┬─────────────────┐
│ 📊 Statistics   │ 🎯 Performance  │ 📁 Coverage     │
├─────────────────┼─────────────────┼─────────────────┤
│ Mean: 4.62      │ Perfect: 49.8%  │ Folders: 5      │
│ Median: 5.0     │ Good: 77.3%     │ Images: 850     │
│ Std Dev: 0.43   │ Fair: 18.9%     │ Complete: 100%  │
│ Min: 2.0        │ Poor: 3.8%      │ Model: GPT-4V   │
└─────────────────┴─────────────────┴─────────────────┘
```

## Tab 2: By Criteria

### Criteria Performance
```
Individual Criteria Scores
┌──────────────────────────────────────────────────────────┐
│ Accuracy        ████████████████████████ 94.2%          │
│ Relevance       ███████████████████████ 92.8%           │
│ Completeness    █████████████████████ 89.5%             │
│ Clarity         ████████████████████████ 93.1%          │
│ Detail Level    ██████████████████ 87.3%                │
└──────────────────────────────────────────────────────────┘

Click any criteria to see score distribution →
```

### Criteria Deep Dive (on click)
```
Accuracy Score Distribution
┌────────────────────────────────────────────────────────┐
│ 100% ████████████████████████████████████ 512         │
│  90% ████████████ 156                                  │
│  80% ██████ 89                                         │
│  70% ███ 45                                            │
│  <70% ██ 48                                            │
└────────────────────────────────────────────────────────┘

Images with low accuracy scores:
• IMG_2341.jpg - Score: 45% - "Misidentified animal species"
• IMG_7823.jpg - Score: 52% - "Incomplete description"
[View all 48 images with accuracy < 70%]
```

## Tab 3: By Folder

### Folder Performance Tree
```
📁 Dataset Root (850 images, avg: 92.5%)
├── 📁 Wildlife (350 images, avg: 94.2%) ⭐ Best
│   ├── 📁 Birds (120 images, avg: 95.8%)
│   ├── 📁 Mammals (150 images, avg: 93.5%)
│   └── 📁 Reptiles (80 images, avg: 92.1%)
│
├── 📁 Nature (300 images, avg: 91.8%)
│   ├── 📁 Landscapes (100 images, avg: 93.2%)
│   ├── 📁 Flora (120 images, avg: 92.1%)
│   └── 📁 Weather (80 images, avg: 88.9%) ⚠️
│
└── 📁 Underwater (200 images, avg: 90.1%) ⚠️ Needs Work
    ├── 📁 Fish (100 images, avg: 91.2%)
    └── 📁 Coral (100 images, avg: 89.0%)
```

### Folder Comparison Chart
```
Average Score by Folder
Wildlife    ████████████████████████ 94.2%
Nature      ███████████████████████ 91.8%
Underwater  ██████████████████████ 90.1%
            │         │         │         │
            80%      85%       90%       95%
```

## Tab 4: Image Viewer

### Gallery View with Scores
```
┌─────────────────────────────────────────────────────────────────┐
│ Showing 1-12 of 850 images     [Search...] [Filter ▼] [Sort ▼] │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────┬─────────┬─────────┬─────────┐                     │
│ │ [Image] │ [Image] │ [Image] │ [Image] │                     │
│ │ 🦁      │ 🦅      │ 🌺      │ 🐠      │                     │
│ │ 5.0 ⭐  │ 4.8 ⭐  │ 3.2 ⚠️  │ 4.5 ⭐  │                     │
│ └─────────┴─────────┴─────────┴─────────┘                     │
│ ┌─────────┬─────────┬─────────┬─────────┐                     │
│ │ [Image] │ [Image] │ [Image] │ [Image] │                     │
│ │ 🏔️      │ 🦜      │ 🪸      │ 🌅      │                     │
│ │ 4.9 ⭐  │ 5.0 ⭐  │ 2.8 ❌  │ 4.7 ⭐  │                     │
│ └─────────┴─────────┴─────────┴─────────┘                     │
│                                                                 │
│ Page 1 of 71                              [◀] [1] [2] [3] [▶] │
└─────────────────────────────────────────────────────────────────┘
```

### Individual Image Details (on click)
```
┌─────────────────────────────────────────────────────────────────┐
│ Image Details                                              [×] │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────────┬─────────────────────────────────────────┐ │
│ │                 │ Wildlife/Birds/eagle_sunrise_042.jpg    │ │
│ │   [Large        │                                         │ │
│ │    Image        │ Overall Score: 4.8/5.0 ⭐⭐⭐⭐⭐         │ │
│ │   Preview]      │                                         │ │
│ │                 │ Criteria Scores:                        │ │
│ │      🦅         │ • Accuracy: 5.0                         │ │
│ │                 │ • Relevance: 4.8                        │ │
│ │                 │ • Completeness: 4.5                     │ │
│ │                 │ • Clarity: 5.0                          │ │
│ │                 │ • Detail: 4.7                           │ │
│ └─────────────────┴─────────────────────────────────────────┘ │
│                                                                 │
│ Model Response:                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ "A majestic bald eagle perched on a weathered branch       │ │
│ │ during golden hour sunrise. The bird's distinctive white   │ │
│ │ head feathers and yellow beak are prominently displayed..."│ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ Prompt Used:                                                    │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ "Describe this image in detail, focusing on the main      │ │
│ │ subject, setting, lighting, and any notable features."     │ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ [◀ Previous] [Next ▶]                    [Copy] [Export JSON] │
└─────────────────────────────────────────────────────────────────┘
```

## Tab 5: Export

### Export Options
```
Export Evaluation Results

Format:
○ Complete Results (JSON) - Full evaluation data with all scores
○ Summary Report (CSV) - Aggregate statistics and folder breakdown  
● Visual Report (PDF) - Charts, statistics, and sample images
○ Problem Images Only - Images scoring below threshold

Options:
☑ Include visualizations
☑ Include sample images (top 10 & bottom 10)
☐ Include all individual scores
☑ Include folder analysis

[Export] [Cancel]
```

## Implementation Details

### Data Model
```csharp
public class EvaluationInsightsViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ModelName { get; set; }
    public string DatasetPath { get; set; }
    public int TotalImages { get; set; }
    public DateTime CompletedAt { get; set; }
    
    // Aggregate metrics
    public double OverallScore { get; set; }
    public double AverageScore { get; set; }
    public double MedianScore { get; set; }
    public double StandardDeviation { get; set; }
    
    // Score distribution
    public Dictionary<string, int> ScoreDistribution { get; set; }
    
    // Criteria breakdown
    public Dictionary<string, double> CriteriaScores { get; set; }
    
    // Folder analysis
    public List<FolderMetrics> FolderBreakdown { get; set; }
    
    // Individual results
    public List<ImageResult> Results { get; set; }
}

public class ImageResult
{
    public string ImagePath { get; set; }
    public string FolderPath { get; set; }
    public double OverallScore { get; set; }
    public Dictionary<string, double> CriteriaScores { get; set; }
    public string ModelResponse { get; set; }
    public string PromptUsed { get; set; }
    public byte[] ImageThumbnail { get; set; }
}

public class FolderMetrics
{
    public string FolderPath { get; set; }
    public int ImageCount { get; set; }
    public double AverageScore { get; set; }
    public List<FolderMetrics> SubFolders { get; set; }
}
```

### Key Features

1. **Rich Visualizations**
   - Interactive charts using Win2D or OxyPlot
   - Color-coded performance indicators
   - Drill-down capabilities

2. **Image Gallery**
   - Lazy loading for performance
   - Thumbnail generation and caching
   - Quick filtering and sorting

3. **Detailed Analysis**
   - Per-criteria breakdown
   - Folder-based grouping
   - Statistical insights

4. **Export Capabilities**
   - Multiple format options
   - Customizable content
   - Professional PDF reports

## Success Metrics

1. **Time to Insight**: < 5 seconds to understand overall performance
2. **Navigation Speed**: < 1 second to switch between tabs
3. **Image Loading**: < 2 seconds to load image gallery
4. **User Satisfaction**: Clear, actionable insights