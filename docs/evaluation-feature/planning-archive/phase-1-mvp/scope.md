# AI Evaluation Wizard - MVP Scope Definition

*Created: June 18, 2025*  
*Updated: June 18, 2025 - 5:00 PM*

**Design Pivot**: After implementing and testing the card-based design, we've identified UX improvements and are pivoting to a more efficient compact list design (v3).

## MVP Philosophy
**Start with the easiest path to value** - Focus on Import Results workflow because it's the simplest to implement while providing immediate user value.

We're prioritizing the Import Results workflow (Workflow 3) where users already have evaluation data with scores from external tools. This strategic choice allows us to:
- Ship working features faster (no backend complexity)
- Validate our visualization designs with real user data
- Provide immediate value to users who have evaluation results
- Build momentum before tackling harder problems

## What's In MVP ✅

### Already Built
1. **Complete Wizard UI** - All 6 steps working with proper validation
2. **Import Results Workflow** - Users can import JSONL with evaluation scores
3. **Dataset Validation** - Processes evaluation data with criteria scores
4. **State Persistence** - Smooth navigation experience
5. **Wizard Completion** - Creates and saves evaluations properly
6. **Empty State Buttons** - Quick actions for import/new evaluation
7. **Compact List Design** - High-density information display
8. **Multi-Select System** - Checkbox selection with bulk operations

### Must Build for MVP (New Priority Order)

#### 1. **Evaluation List Redesign** ✅ COMPLETE
- ~~Rich card-based UI with visual hierarchy~~ ✅ V1 Complete (Superseded)
- **Compact list design (v3)** ✅ V3 Complete
  - ✅ Checkbox multi-select pattern
  - ✅ Workflow type indicators (📥 imports, 🧪 tests)
  - ✅ Integrated score badge with stars
  - ✅ Show criteria names only (no scores in list)
  - ✅ Floating action bar for bulk operations
  - ✅ "Compare" coming soon placeholder
  - ✅ Delete functionality working
  - ✅ 72px row height for efficiency

#### 2. **Evaluation Insights Page** 📊
- **Overview Section**
  - Overall metrics and score distributions
  - Criteria breakdown with visual charts
  - Folder-based performance analysis
  
- **Per-Image Viewer**
  - Image preview with zoom capability
  - Individual criteria scores
  - Average score per image
  - Prompt used for evaluation
  - Navigation between images

#### 3. **Comparison View** 📈
- Select multiple evaluations to compare
- Side-by-side metrics visualization
- Model performance comparison charts
- Criteria-based comparison
- Export comparison results

### Data Already Available (from JSONL)
- Image paths and folder structure
- Individual criteria scores per image
- Model name and prompts used
- All data needed for rich visualizations!

## What's NOT in MVP ❌

### Backend Execution (Deprioritized)
- ❌ Test Model workflow execution
- ❌ Evaluate Responses workflow execution
- ❌ API integrations (OpenAI, Azure, etc.)
- ❌ Real-time evaluation processing

### Advanced Features
- ❌ SPICE, CLIPScore, METEOR calculations
- ❌ AI Judge evaluation
- ❌ Cost tracking
- ❌ Export to PDF/Excel
- ❌ Batch processing >1000

### Nice-to-Have
- ❌ Real-time progress updates for running evaluations
- ❌ Advanced filtering and search
- ❌ Historical tracking beyond current session
- ❌ Team collaboration features

## MVP User Journey

1. **Import Evaluation Results**
   - Use Import Results workflow (already working!)
   - Upload JSONL with evaluation scores
   - Click "Log Evaluation"

2. **View Compact Evaluation List**
   - See efficient rows with key information
   - Workflow icons, scores, criteria names
   - Multi-select with checkboxes
   - Double-click for insights (placeholder)

3. **Explore Evaluation Insights**
   - View overall performance metrics
   - Browse individual image results
   - See criteria breakdowns and folder analysis
   - Navigate through image viewer

4. **Compare Evaluations**
   - Select multiple evaluations
   - See side-by-side comparisons
   - Understand which models perform better

## Technical Approach for MVP

### Data Flow
1. **Import JSONL** → Parse evaluation data with scores
2. **Store in Memory** → Quick access for current session
3. **Rich Visualizations** → Leverage existing UI framework

### UI Components Completed
```
EvaluationListPage (Redesign v3) ✅
├── EvaluationListRow component ✅
├── Checkbox selection column ✅
├── Workflow type icons ✅
├── Floating action bar ✅
└── Score badge with stars ✅

EvaluationInsightsPage (New)
├── OverviewSection
├── CriteriaBreakdown
├── FolderAnalysis
└── ImageViewer component

ComparisonPage (New)
├── EvaluationSelector
├── ComparisonCharts
└── MetricsTable
```

## Success Criteria for MVP

1. **Beautiful UI**: Rich, modern interface that impresses
2. **Insightful**: Users understand their evaluation performance
3. **Comparative**: Can compare multiple evaluations
4. **Fast**: All visualizations render quickly

## Development Order

### Week 1: Evaluation List Redesign ✅ COMPLETE
- ~~Create rich card components~~ ✅ Complete (Superseded)
- ✅ Convert to compact list rows (v3)
- ✅ Implement multi-select functionality
- ✅ Add floating action bar
- ✅ Create "Compare" placeholder
- ✅ All build errors resolved
- ✅ Deployed and tested

### Week 2: Evaluation Insights Page 🎯 NEXT
- Build overview dashboard
- Create image viewer with scores
- Add folder-based analysis
- Show individual criteria scores
- Add navigation between images

### Week 3: Comparison View
- Multi-select functionality
- Comparison visualizations
- Polish and refinement

## Key Design Principles

1. **Efficient Information Design**
   - Maximize information density
   - Clean, professional appearance
   - Clear workflow distinctions
   - Fast scanning and comparison

2. **Information Hierarchy**
   - Most important info visible immediately
   - Details revealed progressively
   - Clear visual groupings

3. **Intuitive Navigation**
   - Click evaluation → see insights
   - Clear back/forward flow
   - Consistent patterns throughout

## Remember
We're building a visualization tool for evaluation data that already exists. No backend execution needed - just beautiful, insightful UI.