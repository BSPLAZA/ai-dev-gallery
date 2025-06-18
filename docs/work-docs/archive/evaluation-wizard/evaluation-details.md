# Evaluation Details Page Design

## Navigation Flow
1. User clicks on evaluation row in list
2. Navigate to `/evaluate/{evaluationId}`
3. Show loading state while fetching full details
4. Display comprehensive evaluation results

## Page Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Back to Evaluations                                                   │
│                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Wildlife Image Captions                                          ⋮  │ │
│ │ GPT-4V (OpenAI) • Completed 2 hours ago                            │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ ┌───────────────┬───────────────┬───────────────┬───────────────────┐ │
│ │ Overall Score │ Items Evaluated│ Total Duration│ API Cost         │ │
│ │    92.5%      │     850        │    45 min     │   $2.35          │ │
│ │    ████████▌  │  Nature photos │               │ ~1,250k tokens   │ │
│ └───────────────┴───────────────┴───────────────┴───────────────────┘ │
│                                                                         │
│ [📊 Overview] [📁 By Folder] [📈 Metrics] [🖼️ Samples] [⚙️ Config]    │
│ ─────────────────────────────────────────────────────────────────────── │
│                                                                         │
│                         [Tab Content Area]                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 1: Overview (Default)

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Performance Summary                                                     │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │                      Radar Chart                                     │ │
│ │                   SPICE ────●──── 85.2                              │ │
│ │                    /          \                                      │ │
│ │         METEOR ●───            ───● CLIPScore                       │ │
│ │          78.5  \               /    89.7                            │ │
│ │                 \            /                                       │ │
│ │                  ●──────────●                                        │ │
│ │              AI Judge    Length Stats                                │ │
│ │                91.3        88.0                                      │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ Key Insights                                                            │
│ • Strongest performance: AI Judge criteria (91.3%)                      │
│ • Weakest metric: METEOR score (78.5%)                                 │
│ • Consistent high scores across visual metrics (SPICE, CLIPScore)      │
│                                                                         │
│ Distribution Analysis                                                   │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Score Distribution (850 items)                                       │ │
│ │                                                                      │ │
│ │ 40 ┤  ██                                                            │ │
│ │ 30 ┤  ██ ██                                                         │ │
│ │ 20 ┤  ██ ██ ██ ██                                                   │ │
│ │ 10 ┤  ██ ██ ██ ██ ██ ██ ██                                         │ │
│ │  0 └──┴──┴──┴──┴──┴──┴──┴──┴──┴──┴──                            │ │
│ │     0  10 20 30 40 50 60 70 80 90 100                              │ │
│ │                                                                      │ │
│ │ Mean: 92.5 | Median: 94.2 | Std Dev: 8.3                           │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 2: By Folder

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Folder Performance Breakdown                                    [Export]│
│                                                                         │
│ 📁 Wildlife (350 images)                               Overall: 94.2%  │
│ ├─────────────────────────────────────────────────────────────────────┤ │
│ │ Mammals (120) ████████████████████████████████████████▌ 96.5% ⭐  │ │
│ │ Birds (150)   ████████████████████████████████████▌     93.8%     │ │
│ │ Reptiles (80) ████████████████████████████████▌         91.2%     │ │
│                                                                         │
│ 📁 Landscapes (300 images)                             Overall: 92.8%  │
│ ├─────────────────────────────────────────────────────────────────────┤ │
│ │ Mountains (100) ████████████████████████████████████▌   95.1% ⭐  │ │
│ │ Forests (120)   ████████████████████████████████▌       92.7%     │ │
│ │ Deserts (80)    ████████████████████████████▌           88.9% ⚠️  │ │
│                                                                         │
│ 📁 Underwater (200 images)                             Overall: 89.1%  │
│ ├─────────────────────────────────────────────────────────────────────┤ │
│ │ Coral (100) ████████████████████████████████▌            90.3%     │ │
│ │ Fish (100)  ████████████████████████████▌                87.9% ⚠️  │ │
│                                                                         │
│ 💡 Insights:                                                            │
│ • Best performing: Wildlife/Mammals (96.5%)                            │
│ • Needs improvement: Underwater/Fish (87.9%)                           │
│ • Desert landscapes showing lower accuracy than other terrains         │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 3: Metrics Deep Dive

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Metric Performance Analysis                                             │
│                                                                         │
│ SPICE Score (Semantic Propositional Image Caption Evaluation)          │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Average: 85.2 | Min: 45.3 | Max: 98.7                              │ │
│ │                                                                      │ │
│ │ Trend over items: [Line chart showing SPICE scores across dataset] │ │
│ │                                                                      │ │
│ │ Component Breakdown:                                                 │ │
│ │ • Object Detection: 88.5%                                           │ │
│ │ • Attribute Recognition: 84.2%                                      │ │
│ │ • Relationship Understanding: 82.9%                                 │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ CLIPScore (Vision-Language Alignment)                                   │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Average: 0.897 | Min: 0.612 | Max: 0.983                           │ │
│ │                                                                      │ │
│ │ [Scatter plot: CLIPScore vs Image Complexity]                      │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ AI Judge Custom Criteria                                                │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Criterion 1: Descriptive Accuracy (Weight: 40%)          Score: 93%│ │
│ │ "How accurately does the caption describe visible elements?"        │ │
│ │                                                                      │ │
│ │ Criterion 2: Contextual Understanding (Weight: 30%)      Score: 90%│ │
│ │ "Does the caption understand the scene context and relationships?"  │ │
│ │                                                                      │ │
│ │ Criterion 3: Natural Language Quality (Weight: 30%)      Score: 91%│ │
│ │ "Is the caption grammatically correct and naturally written?"       │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 4: Sample Results

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Sample Results                     [Best ▼] [Grid View] [🔍 Search]    │
│                                                                         │
│ ┌─────────────────────┬─────────────────────┬─────────────────────┐   │
│ │ Rank #1 (Score: 98.7)                                           │   │
│ │ ┌─────────────────┐ │ Generated:          │ Metrics:            │   │
│ │ │                 │ │ "A majestic lion    │ • SPICE: 97.2       │   │
│ │ │  [Lion Image]   │ │ resting on a rock   │ • CLIP: 0.983       │   │
│ │ │                 │ │ overlooking the     │ • AI Judge: 99%     │   │
│ │ └─────────────────┘ │ African savanna at  │                     │   │
│ │                     │ golden hour"        │ Expected:           │   │
│ │ wildlife/lion_23.jpg│                     │ "Lion on rock at    │   │
│ │                     │                     │ sunset in savanna"  │   │
│ ├─────────────────────┴─────────────────────┴─────────────────────┤   │
│ │ Rank #2 (Score: 97.9)                                           │   │
│ │ ┌─────────────────┐ │ Generated:          │ Metrics:            │   │
│ │ │                 │ │ "Colorful coral     │ • SPICE: 96.5       │   │
│ │ │ [Coral Image]   │ │ reef teeming with   │ • CLIP: 0.971       │   │
│ │ │                 │ │ tropical fish in    │ • AI Judge: 98%     │   │
│ │ └─────────────────┘ │ crystal clear water"│                     │   │
│ └─────────────────────┴─────────────────────┴─────────────────────┘   │
│                                                                         │
│ [View More] [Export Top 100] [Export Bottom 100]                       │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 5: Configuration

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Evaluation Configuration                               [Copy] [Re-run] │
│                                                                         │
│ Basic Information                                                       │
│ ├─ Evaluation Type: Image Description                                   │
│ ├─ Workflow: Test Model (Full Pipeline)                                 │
│ └─ Created: June 18, 2025 2:47 PM                                      │
│                                                                         │
│ Model Configuration                                                     │
│ ├─ Provider: OpenAI                                                     │
│ ├─ Model: gpt-4-vision-preview                                         │
│ ├─ API Endpoint: https://api.openai.com/v1/                           │
│ └─ Base Prompt:                                                         │
│     "Describe this image in 1-2 sentences, focusing on the main       │
│      subject and scene context."                                       │
│                                                                         │
│ Dataset                                                                 │
│ ├─ Source: /Users/name/datasets/nature_photos/                         │
│ ├─ Total Items: 850                                                    │
│ ├─ Image Formats: JPG (780), PNG (70)                                  │
│ └─ Folder Structure:                                                    │
│     ├─ Wildlife/ (350 images)                                          │
│     ├─ Landscapes/ (300 images)                                        │
│     └─ Underwater/ (200 images)                                        │
│                                                                         │
│ Evaluation Methods                                                      │
│ ├─ Automated Metrics:                                                   │
│ │   ☑ SPICE Score                                                      │
│ │   ☑ CLIPScore                                                        │
│ │   ☑ METEOR                                                           │
│ │   ☑ Length Statistics                                                │
│ └─ AI Judge Criteria: 3 custom criteria (see Metrics tab)              │
│                                                                         │
│ Execution Details                                                       │
│ ├─ Start Time: June 18, 2025 2:47 PM                                   │
│ ├─ End Time: June 18, 2025 3:32 PM                                     │
│ ├─ Total Duration: 45 minutes                                           │
│ ├─ API Calls: 850                                                      │
│ ├─ Tokens Used: ~1,250,000                                             │
│ └─ Estimated Cost: $2.35                                                │
└─────────────────────────────────────────────────────────────────────────┘
```

## Action Menu (⋮) Options

1. **Export Results**
   - Full dataset (CSV with all metrics)
   - Summary report (PDF)
   - Best/worst samples
   - Raw JSON data

2. **Share**
   - Generate shareable link
   - Copy summary to clipboard
   - Export for presentation

3. **Compare**
   - Add to comparison set
   - Compare with previous runs
   - Compare with team benchmarks

4. **Re-run**
   - With same configuration
   - With different model
   - With subset of data

5. **Archive/Delete**
   - Archive (keep summary, remove details)
   - Delete permanently

## Interactive Features

### Hover Effects
- **Score bars**: Show exact value and rank
- **Folder names**: Show full path
- **Metric values**: Show calculation details
- **Sample images**: Show larger preview

### Click Actions
- **Folder names**: Filter samples by that folder
- **Metric names**: Jump to detailed analysis
- **Sample images**: Open full-screen viewer
- **Copy buttons**: Copy to clipboard with confirmation

### Data Export Options
```
Export Results
├─ Format:        [CSV ▼]
├─ Include:       ☑ Scores ☑ Metadata ☑ Paths
├─ Samples:       ○ All ● Top 100 ○ Bottom 100
└─ [Download]     [Copy Link]
```

## Mobile Responsive Design

### Tablet (768-1200px)
- Stack metric cards 2x2
- Collapse folder tree to accordion
- Reduce sample preview to 2 columns

### Mobile (<768px)
- Single column layout
- Swipeable tabs
- Bottom sheet for exports
- Tap to expand sections

## Performance Considerations

1. **Lazy Loading**
   - Load overview first
   - Load other tabs on demand
   - Paginate sample results

2. **Caching**
   - Cache computed aggregates
   - Store chart data
   - Preload adjacent samples

3. **Progressive Enhancement**
   - Show basic data immediately
   - Load visualizations async
   - Stream large exports