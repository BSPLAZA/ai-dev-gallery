# Evaluation Comparison View Design

## Access Patterns

### Method 1: From List View
1. Select checkboxes next to 2-5 evaluations
2. "Compare" button appears in toolbar
3. Click to open comparison view

### Method 2: From Details Page
1. Click "Add to Comparison" in action menu
2. Banner shows "1 evaluation selected for comparison"
3. Navigate back and select more
4. Click "View Comparison (3)"

### Method 3: Quick Compare
1. Right-click evaluation in list
2. Select "Compare with..."
3. Modal shows recent evaluations
4. Select one or more to compare

## Comparison Page Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Back to Evaluations                                    [+ Add More]   │
│                                                                         │
│ Comparing 3 Evaluations                                                 │
│ ─────────────────────────────────────────────────────────────────────── │
│                                                                         │
│ ┌─────────────┬─────────────┬─────────────┐                           │
│ │   GPT-4V    │  Claude 3.5 │ Gemini Pro  │        [Remove ✕]         │
│ │   Wildlife  │  Wildlife   │  Wildlife   │                           │
│ │   850 items │  850 items  │  850 items  │                           │
│ │   2 hrs ago │  Yesterday  │  3 days ago │                           │
│ └─────────────┴─────────────┴─────────────┘                           │
│                                                                         │
│ [📊 Overview] [📈 Metrics] [💰 Cost] [⚡ Performance] [🎯 Insights]    │
│ ─────────────────────────────────────────────────────────────────────── │
│                                                                         │
│                         [Tab Content Area]                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 1: Overview Comparison

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Overall Performance Comparison                                          │
│                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │     100% ┤                                                           │ │
│ │          │    ██ 92.5%        ██ 89.3%         █ 86.7%             │ │
│ │      80% ┤    ██               ██              ██                  │ │
│ │          │    ██               ██              ██                  │ │
│ │      60% ┤    ██               ██              ██                  │ │
│ │          │    ██               ██              ██                  │ │
│ │      40% ┤    ██               ██              ██                  │ │
│ │          │    ██               ██              ██                  │ │
│ │      20% ┤    ██               ██              ██                  │ │
│ │          │    ██               ██              ██                  │ │
│ │       0% └────┴────────────────┴───────────────┴──────────────────│ │
│ │              GPT-4V          Claude 3.5      Gemini Pro            │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ Winner: GPT-4V 🏆 (+3.2% vs Claude, +5.8% vs Gemini)                  │
│                                                                         │
│ Quick Stats Comparison                                                  │
│ ┌───────────────────┬──────────────┬──────────────┬─────────────────┐ │
│ │                   │ GPT-4V ⭐    │ Claude 3.5   │ Gemini Pro      │ │
│ ├───────────────────┼──────────────┼──────────────┼─────────────────┤ │
│ │ Overall Score     │ 92.5%        │ 89.3%        │ 86.7%           │ │
│ │ Best Category     │ Mammals 96%  │ Birds 91%    │ Landscape 88%   │ │
│ │ Worst Category    │ Desert 88%   │ Desert 84%   │ Fish 79%        │ │
│ │ Consistency (σ)   │ 8.3 ⭐       │ 10.2         │ 12.5            │ │
│ └───────────────────┴──────────────┴──────────────┴─────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 2: Metrics Comparison

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Detailed Metrics Comparison                              [Export CSV]   │
│                                                                         │
│ Select Metric: [SPICE Score ▼]                                         │
│                                                                         │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ SPICE Score Distribution                                            │ │
│ │                                                                      │ │
│ │     GPT-4V    ━━━━━━━━━━━━━━━━━━━━━━━●━━━━━━  85.2               │ │
│ │               ├─────┼─────┼─────┼─────┼─────┤                     │ │
│ │               60    70    80    90    100                          │ │
│ │                                                                      │ │
│ │     Claude    ━━━━━━━━━━━━━━━━━●━━━━━━━━━━━  82.7                │ │
│ │                                                                      │ │
│ │     Gemini    ━━━━━━━━━━━━●━━━━━━━━━━━━━━━━  79.1                │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ Metric Breakdown Table                                                  │
│ ┌─────────────────┬──────────┬──────────┬──────────┬────────────────┐ │
│ │ Metric          │ GPT-4V   │ Claude   │ Gemini   │ Best           │ │
│ ├─────────────────┼──────────┼──────────┼──────────┼────────────────┤ │
│ │ SPICE           │ 85.2     │ 82.7     │ 79.1     │ GPT-4V ⭐      │ │
│ │ CLIPScore       │ 0.897    │ 0.912 ⭐ │ 0.875    │ Claude ⭐      │ │
│ │ METEOR          │ 78.5     │ 81.2 ⭐  │ 77.9     │ Claude ⭐      │ │
│ │ Length Accuracy │ 88.0 ⭐  │ 85.3     │ 86.1     │ GPT-4V ⭐      │ │
│ │ AI Judge        │ 91.3 ⭐  │ 88.7     │ 84.2     │ GPT-4V ⭐      │ │
│ └─────────────────┴──────────┴──────────┴──────────┴────────────────┘ │
│                                                                         │
│ 💡 Insights:                                                            │
│ • GPT-4V leads in 3 of 5 metrics                                       │
│ • Claude performs best on vision-language alignment (CLIPScore)        │
│ • All models struggle with METEOR scores                               │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 3: Cost Analysis

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Cost & Efficiency Comparison                                            │
│                                                                         │
│ Total Cost Breakdown                                                    │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │     $4.00 ┤                                                          │ │
│ │           │    ██ $3.85                                              │ │
│ │     $3.00 ┤    ██                ██ $2.92                           │ │
│ │           │    ██                ██               ██ $2.35 💰       │ │
│ │     $2.00 ┤    ██                ██               ██                │ │
│ │           │    ██                ██               ██                │ │
│ │     $1.00 ┤    ██                ██               ██                │ │
│ │           │    ██                ██               ██                │ │
│ │      $.00 └────┴─────────────────┴────────────────┴───────────────│ │
│ │               Gemini            Claude           GPT-4V             │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ Cost Efficiency Analysis                                                │
│ ┌─────────────────┬──────────┬──────────┬──────────┬────────────────┐ │
│ │                 │ GPT-4V   │ Claude   │ Gemini   │ Best Value     │ │
│ ├─────────────────┼──────────┼──────────┼──────────┼────────────────┤ │
│ │ Total Cost      │ $2.35 💰 │ $2.92    │ $3.85    │ GPT-4V         │ │
│ │ Cost per Item   │ $0.0028  │ $0.0034  │ $0.0045  │ GPT-4V         │ │
│ │ Score per $     │ 39.4 ⭐  │ 30.6     │ 22.5     │ GPT-4V         │ │
│ │ Tokens Used     │ 1.25M    │ 1.18M ⭐ │ 1.41M    │ Claude         │ │
│ └─────────────────┴──────────┴──────────┴──────────┴────────────────┘ │
│                                                                         │
│ 💡 Value Insights:                                                      │
│ • GPT-4V offers best performance AND lowest cost                       │
│ • Claude uses fewest tokens but has higher per-token cost             │
│ • Gemini is most expensive with lowest performance                     │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 4: Performance Analysis

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Speed & Reliability Comparison                                          │
│                                                                         │
│ Processing Speed                                                        │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ Items per Minute                                                     │ │
│ │                                                                      │ │
│ │ Claude    ████████████████████████████████████  22.4 items/min ⚡  │ │
│ │ GPT-4V   ██████████████████████████  18.9 items/min               │ │
│ │ Gemini   ████████████████  16.3 items/min                          │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
│                                                                         │
│ Reliability Metrics                                                     │
│ ┌─────────────────┬──────────┬──────────┬──────────┬────────────────┐ │
│ │                 │ GPT-4V   │ Claude   │ Gemini   │ Best           │ │
│ ├─────────────────┼──────────┼──────────┼──────────┼────────────────┤ │
│ │ Success Rate    │ 100% ⭐  │ 100% ⭐  │ 98.2%    │ GPT/Claude     │ │
│ │ Retry Count     │ 0        │ 2        │ 15       │ GPT-4V ⭐      │ │
│ │ Avg Latency     │ 3.2s     │ 2.7s ⚡  │ 3.7s     │ Claude ⚡      │ │
│ │ P95 Latency     │ 4.8s     │ 4.1s ⚡  │ 8.2s     │ Claude ⚡      │ │
│ └─────────────────┴──────────┴──────────┴──────────┴────────────────┘ │
│                                                                         │
│ Processing Timeline                                                     │
│ ┌─────────────────────────────────────────────────────────────────────┐ │
│ │ 0 min                    25 min                   50 min            │ │
│ │ │                        │                        │                 │ │
│ │ GPT-4V  ████████████████████████████████████████ 45 min          │ │
│ │ Claude  ████████████████████████████████  38 min ⚡               │ │
│ │ Gemini  ██████████████████████████████████████████████ 52 min    │ │
│ └─────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

## Tab 5: AI-Generated Insights

```
┌─────────────────────────────────────────────────────────────────────────┐
│ Comparative Analysis & Recommendations                  [Regenerate]    │
│                                                                         │
│ 🏆 Overall Winner: GPT-4V                                              │
│ ─────────────────────────────────────────────────────────────────────── │
│                                                                         │
│ Key Findings:                                                           │
│                                                                         │
│ 1. Performance Leadership                                               │
│    • GPT-4V achieves highest overall score (92.5%)                     │
│    • Particularly strong on complex scenes (mammals, landscapes)       │
│    • Most consistent performance across all categories (σ=8.3)         │
│                                                                         │
│ 2. Cost Efficiency Surprise                                            │
│    • Despite superior performance, GPT-4V is least expensive           │
│    • 36% cheaper than Claude, 64% cheaper than Gemini                  │
│    • Best performance-per-dollar ratio (39.4 score/$)                  │
│                                                                         │
│ 3. Speed vs Quality Trade-off                                          │
│    • Claude processes 18% faster than GPT-4V                           │
│    • But sacrifices 3.2% in accuracy                                   │
│    • Consider Claude for time-sensitive, high-volume tasks             │
│                                                                         │
│ 4. Category-Specific Strengths                                         │
│    • GPT-4V: Best for wildlife and detailed scenes                    │
│    • Claude: Strong on artistic/abstract images                        │
│    • Gemini: Adequate for basic scenes, struggles with complexity      │
│                                                                         │
│ 📊 Recommendations by Use Case:                                        │
│                                                                         │
│ ┌─────────────────────┬─────────────────────────────────────────────┐ │
│ │ Use Case            │ Recommended Model                           │ │
│ ├─────────────────────┼─────────────────────────────────────────────┤ │
│ │ Quality Critical    │ GPT-4V - Highest accuracy, best consistency │ │
│ │ Budget Conscious    │ GPT-4V - Lowest cost per image              │ │
│ │ Speed Critical      │ Claude - Fastest processing time            │ │
│ │ Balanced Approach   │ Claude - Good quality at reasonable speed   │ │
│ │ Learning/Testing    │ Gemini - Acceptable for experimentation     │ │
│ └─────────────────────┴─────────────────────────────────────────────┘ │
│                                                                         │
│ 💡 Strategic Insights:                                                  │
│                                                                         │
│ • Consider A/B testing with GPT-4V and Claude for your specific       │
│   dataset characteristics                                               │
│ • GPT-4V's consistency makes it ideal for production deployments       │
│ • Monitor Gemini updates - current version underperforms               │
└─────────────────────────────────────────────────────────────────────────┘
```

## Interactive Features

### Model Cards (Top Section)
- **Hover**: Show mini stats (score, time, cost)
- **Click**: Highlight that model in all charts
- **Remove**: Remove from comparison with animation
- **Reorder**: Drag to reorder comparison

### Charts & Visualizations
- **Interactive**: Hover for exact values
- **Clickable**: Click to filter/highlight
- **Exportable**: Right-click to save as image
- **Responsive**: Adapt to number of models

### Data Export
```
Export Comparison Results
├─ Format:     [PDF Report ▼]
├─ Include:    ☑ Charts ☑ Tables ☑ Insights
├─ Layout:     ○ Portrait ● Landscape
└─ [Generate]  [Preview]
```

## Comparison Modes

### Side-by-Side (Default)
- Equal column width
- All models visible
- Best for 2-4 models

### Baseline Comparison
- Select one as baseline
- Show others as +/- difference
- Highlight improvements/regressions

### Ranked View
- Sort by selected metric
- Show rank changes
- Best for 5+ models

## Advanced Comparison Features

### 1. Statistical Significance
```
Difference Analysis:
GPT-4V vs Claude: +3.2% (p < 0.01) ✓ Significant
GPT-4V vs Gemini: +5.8% (p < 0.001) ✓ Highly Significant
```

### 2. Subset Analysis
- Compare performance on specific folders
- Filter by score range
- Analyze error patterns

### 3. Historical Comparison
- Include previous runs of same model
- Show improvement over time
- Track model version changes

### 4. Custom Weights
- Adjust importance of each metric
- Recalculate overall scores
- Find best model for specific needs

## Mobile/Responsive Design

### Tablet (768-1200px)
- Stack model cards vertically
- Swipeable chart carousel
- Collapsible sections

### Mobile (<768px)
- Single model focus
- Swipe between models
- Simplified charts
- Bottom navigation

## Save & Share Comparison

### Save Options
- Save comparison set
- Name: "Wildlife Model Comparison June 2025"
- Quick access from dashboard
- Update with new runs

### Share Options
- Generate shareable link
- Export as presentation
- Create comparison template
- Email summary