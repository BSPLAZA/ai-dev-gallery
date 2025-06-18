# Unified TODO Tracker - AI Evaluation Feature

*Last Updated: June 18, 2025 - 5:00 PM*

## Overview

This document consolidates all evaluation feature tasks. We're currently focused on the **Visualization MVP** for Import Results workflow as the easiest path to deliver value.

### Recent Design Pivot (June 18)
After building and testing the card-based design, we've identified UX improvements needed:
- Moving from cards to a more efficient **list view**
- Adding **multi-select with compare functionality**
- Better **workflow type indicators** (import vs test)
- Improved **information density** and scanning

---

## ✅ Phase 1: Wizard UI (COMPLETE - 95%)

### What's Built
- ✅ Complete 6-step wizard with navigation
- ✅ Three workflows: Test Model, Evaluate Responses, Import Results
- ✅ Two-part upload (JSONL + image folder)
- ✅ Dataset validation (up to 1,000 items)
- ✅ Optional prompt field support
- ✅ Model/prompt auto-fill from JSONL
- ✅ State persistence across steps
- ✅ Validation and error handling
- ✅ Log Evaluation button for workflow 3

### Remaining Polish
- [x] Fixed "Log Evaluation" button functionality
- [x] Added proper wizard completion handling
- [x] Implemented evaluation saving from wizard
- [ ] Add dataset preview before proceeding
- [ ] Enhance folder structure visualization

---

## 🎯 Phase 2: Visualization MVP (CURRENT FOCUS)

### Why This First?
- No backend required - just parse and display JSONL
- Users already have evaluation data with scores
- Immediate value delivery
- Validates UX before building execution

### 2.1 Foundation & Data Layer
- [x] See detailed tasks in [tasks-list-redesign.md](tasks-list-redesign.md#phase-2-data-layer-implementation-day-1-2)
- [x] Create `EvaluationResultsStore` service
- [x] Implement `EvaluationCardViewModel` with score calculations
- [x] Create data models for 1-5 scale scores
- [x] Add JSONL parsing for criteria scores
- [x] Implement average score aggregation
- [ ] Add caching mechanism

### 2.2 Evaluation List Redesign (V1 - COMPLETE BUT SUPERSEDED)
**Branch**: `feature/eval-list-cards-v2`
- [x] Create `EvaluationCard.xaml` with card design
- [x] Implement score display (4.6/5 format with stars)
- [x] Show all criteria scores on card
- [x] Add hover animations (lift + show actions)
- [x] Create beautiful empty state with onboarding
- [x] Replace table with card-based ListView
- [x] Register services in DI container
- [x] Add sample data generation
- [x] Build and test implementation

**Note**: V1 card design was completed but user feedback indicated poor information density. Pivoted to V3 compact design.

### 2.3 Evaluation List Redesign (V3 - COMPACT DESIGN COMPLETE)
**Branch**: `feature/eval-list-compact`
- [x] Convert from cards to efficient list rows (72px height)
- [x] Add checkbox selection column
- [x] Implement multi-select with floating action bar
- [x] Add workflow type icons (📥 for imports, 🧪 for tests)
- [x] Show integrated score badge in title line
- [x] Display only criteria names (no scores in list)
- [x] Improve date formatting (relative for recent, absolute for older)
- [x] Add simple loading state with progress ring
- [x] Implement "Compare" placeholder (Coming Soon dialog)
- [x] Add bulk operations (delete selected)
- [x] Ensure proper hover and selection states
- [x] Optimize for information density
- [x] Fix wizard completion to save evaluations
- [x] Fix empty state button handlers
- [x] Implement PropertyChanged for selection count

### 2.4 Evaluation Insights Page
- [ ] Create `EvaluationInsightsPage.xaml`
- [ ] Implement tab navigation (Overview, By Criteria, By Folder, Images, Export)
- [ ] **Overview Tab**: Score distribution chart, key metrics grid
- [ ] **By Criteria Tab**: Criteria performance bars, drill-down
- [ ] **By Folder Tab**: Tree view with folder metrics
- [ ] **Image Viewer Tab**: Gallery with lazy loading, detail modal
- [ ] **Export Tab**: JSON, CSV, PDF options
- [ ] Add loading and error states

### 2.5 Charts & Visualizations
- [ ] Evaluate charting libraries (Win2D vs OxyPlot)
- [ ] Create reusable chart components
- [ ] Implement bar charts for scores
- [ ] Add distribution histograms
- [ ] Create interactive tooltips
- [ ] Test performance with large datasets

### 2.6 Polish & Performance
- [ ] Optimize image loading and caching
- [ ] Implement list virtualization
- [ ] Add smooth animations
- [ ] Ensure theme consistency
- [ ] Accessibility testing
- [ ] Performance profiling

---

## 🔮 Phase 3: Execution Pipeline (FUTURE)

### Critical Infrastructure Needed
- [ ] Create `EvaluationExecutor` service
- [ ] Implement `ModelProviderFactory`
- [ ] Add API integration (start with OpenAI)
- [ ] Build progress tracking system
- [ ] Create results storage mechanism
- [ ] Implement retry and error handling
- [ ] Add cancellation support
- [ ] Create background job queue

### Execution Flow
- [ ] Generate responses for Test Model workflow
- [ ] Evaluate existing responses for workflow 2
- [ ] Calculate automated metrics (SPICE, CLIPScore)
- [ ] Implement AI Judge evaluation
- [ ] Store results in JSONL format
- [ ] Update UI with real-time progress

---

## 🚀 Phase 4: Advanced Features (POST-MVP)

### Comparison View
- [x] Multi-select in evaluation list (added to V2 redesign)
- [ ] Create comparison page
- [ ] Side-by-side visualizations
- [ ] Difference highlighting
- [ ] Insights generation

### Enhanced Analytics
- [ ] Trend analysis over time
- [ ] Model performance tracking
- [ ] Cost analysis
- [ ] Custom metric creation
- [ ] Advanced filtering

### Enterprise Features
- [ ] Batch processing (>1000 items)
- [ ] Multiple model providers
- [ ] Team collaboration
- [ ] Scheduled evaluations
- [ ] API access

---

## 📊 Progress Summary

| Phase | Status | Progress | Priority |
|-------|--------|----------|----------|
| Wizard UI | Complete | 100% | ✅ Done |
| Visualization MVP | In Progress | 85% | 🎯 Current |
| - List V1 (Cards) | Complete (Superseded) | 100% | ✅ Done |
| - List V3 (Compact) | Complete | 100% | ✅ Done |
| - Insights Page | Not Started | 0% | 🎯 Next |
| Execution Pipeline | Not Started | 0% | 📅 Future |
| Advanced Features | Not Started | 0% | 🔮 Future |

---

## 🎯 Next Steps

1. **Immediate**: Evaluation Insights Page
   - Create navigation from list to details
   - Design overview tab with charts
   - Show all criteria scores
   - Display dataset information
   - Add score breakdowns

2. **Phase 1**: Execution Infrastructure
   - Create EvaluationExecutor service
   - Implement API clients (OpenAI first)
   - Add progress tracking during execution
   - Handle errors and retries gracefully

3. **Phase 2**: Import Enhancement
   - Parse JSONL scores properly (currently basic)
   - Support multiple score formats
   - Add import preview dialog
   - Handle large files with streaming

---

## Git Strategy

### Branch Structure
```
main
└── feature/evaluation-viz-mvp
    ├── feature/eval-list-cards
    ├── feature/eval-insights-page
    └── feature/eval-charts
```

### Commit Format
```
[Component]: Brief description

- Detailed changes
- Reference issues
- Mark WIP if incomplete
```

---

## Success Criteria

- [ ] Import Results workflow shows rich visualizations
- [ ] List loads in < 1 second
- [ ] Insights page provides actionable information
- [ ] Users can understand evaluation performance at a glance
- [ ] All interactions feel smooth and polished