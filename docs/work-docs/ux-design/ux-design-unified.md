# Unified UX Design - Evaluation Visualization

*Last Updated: June 18, 2025 - Pivoted to Compact List Design*

## 🎨 Design Philosophy

**Rich, intuitive visualizations for evaluation data** - Transform raw JSONL scores into actionable insights through modern UI patterns.

---

## 📱 Evaluation List - Compact List Design (V3)

### Visual Design
```
┌─────────────────────────────────────────────────────────────────────┐
│ AI Evaluations                                    [+ New Evaluation] │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│ □ 📥 GPT-4 Customer Support Evaluation                    4.3 ★★★★☆ │
│      GPT-4 • customer_support_qa.jsonl (1,000 items) • Dec 11      │
│      Criteria: Accuracy, Helpfulness, Clarity                       │
│ ─────────────────────────────────────────────────────────────────── │
│ □ 🧪 Claude 3 Medical Q&A Test                           4.6 ★★★★★ │
│      Claude 3 • medical_qa_dataset.jsonl (500 items) • Dec 14      │
│      Criteria: Medical Accuracy, Safety, Completeness               │
│ ─────────────────────────────────────────────────────────────────── │
│ □ 🧪 Llama 2 Code Generation       [▓▓▓▓░░░░░░ 65%]      3.5 ★★★☆☆ │
│      Llama 2 70B • code_tasks.jsonl (250 items) • Running...       │
│      Criteria: Code Correctness, Code Quality, Performance          │
└─────────────────────────────────────────────────────────────────────┘
```

### Key Elements
1. **Checkbox Column** - Multi-select functionality
2. **Workflow Icons** - 📥 for imports, 🧪 for tests
3. **Information Density** - 3 lines per row, compact layout
4. **Integrated Score** - Badge format with stars on right
5. **Criteria Display** - Names only, no scores in list
6. **Selection States** - Hover (gray bg), Selected (blue bg)

### List Design Rationale
- **Information Density** - 3-4x more evaluations visible
- **Scannable** - Easy to compare multiple items
- **Clear Hierarchy** - Most important info prominent
- **Multi-select Ready** - Built for comparison workflow

### Selection Behavior
- **Single Click** - Toggle checkbox selection
- **Double Click** - View evaluation details
- **Ctrl+A** - Select all visible items
- **Escape** - Clear all selections

### Floating Action Bar
```
┌──────────────────────────────────────────────────┐
│  2 evaluations selected                          │
│  [Compare] [Delete] [Cancel]                     │
└──────────────────────────────────────────────────┘
```
Appears when 2+ items selected, centered at bottom

---

## 📊 Evaluation Insights Page

### Page Structure
```
┌─────────────────────────────────────────────────────────────────────────┐
│ ← Back     GPT-4V Image Captions Evaluation                        ⋮   │
│            Wildlife & Nature • 850 images • Completed 2 hours ago      │
├─────────────────────────────────────────────────────────────────────────┤
│ ┌─────────────┬─────────────┬─────────────┬─────────────┐             │
│ │ Overall     │ Avg Score   │ Best Score  │ Completion  │             │
│ │   92.5%     │   4.62/5    │   5.0/5     │   100%      │             │
│ └─────────────┴─────────────┴─────────────┴─────────────┘             │
│                                                                          │
│ [Overview] [By Criteria] [By Folder] [Image Viewer] [Export]           │
│ ├────────────────────────────────────────────────────────────────────┤ │
│ │                         Active Tab Content                          │ │
│ └────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────┘
```

### Tab 1: Overview
- **Score Distribution** - Bar chart showing score ranges
- **Key Metrics** - Statistical summary (mean, median, std dev)
- **Quick Insights** - Auto-generated performance summary

### Tab 2: By Criteria
```
Criteria Performance
├── Accuracy        ████████████████████ 94.2%
├── Relevance       ███████████████████  92.8%
├── Completeness    █████████████████    89.5%
└── Clarity         ████████████████████ 93.1%
```
- Click any criteria for detailed breakdown
- Identify low-scoring images per criterion

### Tab 3: By Folder
```
📁 Dataset Root (850 images, 92.5%)
├── 📁 Wildlife (350, 94.2%) ⭐
│   ├── 📁 Birds (120, 95.8%)
│   └── 📁 Mammals (230, 93.5%)
└── 📁 Nature (500, 91.2%)
    ├── 📁 Landscapes (200, 93.2%)
    └── 📁 Flora (300, 89.9%) ⚠️
```

### Tab 4: Image Viewer
- **Gallery Grid** - Thumbnail view with scores
- **Filtering** - By score range, folder, criteria
- **Detail Modal** - Full image, all scores, model response

### Tab 5: Export
- Multiple format options (JSON, CSV, PDF)
- Selective export (full data vs summary)
- Visual report generation

---

## 🔄 Comparison View (Coming Soon)

### Multi-Select Mode
When 2+ evaluations selected, floating action bar appears:
```
┌──────────────────────────────────────────────────┐
│  3 evaluations selected                          │
│  [Compare] [Delete] [Cancel]                     │
└──────────────────────────────────────────────────┘
```

### Temporary Dialog
Clicking "Compare" shows:
```
┌─────────────────────────────────────┐
│ Coming Soon                         │
│                                     │
│ Comparison view will allow you to   │
│ analyze multiple evaluations        │
│ side-by-side.                      │
│                                     │
│                        [OK]         │
└─────────────────────────────────────┘
```

### Comparison Layout
```
Model Performance Comparison
┌─────────────────────────────────────────────────┐
│         GPT-4V    Claude-3.5    Gemini-Pro     │
│ Overall   92.5% ⭐   87.3%        84.1%        │
│ Speed     45min      38min ⭐     52min        │
│ Cost      $2.35      $1.89 ⭐     $3.12        │
└─────────────────────────────────────────────────┘
```

---

## 🎯 Interaction Patterns

### Navigation Flow
```
List → Click Card → Insights Page
     ↓              ↓
     Multi-Select   Tab Navigation
     ↓              ↓
     Compare View   Image Details
```

### Loading States
- Skeleton rows for initial load
- Simple shimmer animation
- Minimal visual complexity

### Empty State (Simplified)
```
        No evaluations yet
        
Import existing results or start a new evaluation
        
[📥 Import Results]    [🧪 New Evaluation]
```

### Error Handling
- Inline error messages
- Retry options
- Graceful degradation

---

## 📐 Responsive Design

### Desktop (>1200px)
- Full card layout with all information
- Side-by-side comparisons
- Multi-column image grid

### Tablet (768-1200px)
- Condensed cards
- Stacked comparisons
- 3-column image grid

### Mobile (<768px)
- Single column cards
- Vertical tab layout
- 2-column image grid

---

## ♿ Accessibility

### Keyboard Navigation
- Tab through all interactive elements
- Enter/Space to activate
- Escape to close modals
- Arrow keys for image navigation

### Screen Reader Support
- Descriptive labels for all elements
- Score announcements
- Status descriptions
- Chart summaries

### Visual Accessibility
- High contrast mode support
- Focus indicators
- Color-blind friendly palettes
- Text alternatives for icons

---

## 🚀 Implementation Notes

### Performance Targets
- List render: < 1 second
- Tab switch: < 200ms
- Image load: < 2 seconds
- Chart render: < 500ms

### Technical Considerations
- Use virtualization for large lists
- Lazy load images with placeholders
- Cache calculated metrics
- Progressive enhancement

### Design Tokens
```xml
<!-- Row Dimensions -->
<x:Double x:Key="EvalRowHeight">72</x:Double>
<x:Double x:Key="EvalRowPadding">16,12</x:Double>

<!-- Colors -->
<Color x:Key="EvalHoverBackground">#F6F8FA</Color>
<Color x:Key="EvalSelectedBackground">#EBF5FF</Color>
<Color x:Key="EvalSelectedBorder">#2196F3</Color>

<!-- Animation -->
<x:Double x:Key="EvalTransitionDuration">150</x:Double>
```

---

## 📋 Design Checklist

- [x] Compact list layout defined
- [x] Multi-select pattern designed
- [x] Workflow icons selected (📥 🧪)
- [ ] All states designed (empty, loading, error)
- [ ] Hover/focus states defined
- [ ] Selection animations specified
- [ ] Floating action bar positioned
- [ ] Keyboard shortcuts documented
- [ ] Performance targets set (200+ items)