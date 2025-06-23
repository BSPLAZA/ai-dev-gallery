# Git Workflow Guide for Evaluation Feature

*Last Updated: June 23, 2025*

## Overview

This guide consolidates all git workflow strategies for contributing the evaluation feature from a personal fork to the team's staging repository.

## Current Situation

- **Development History**: 14+ branches built sequentially over several months
- **Current State**: All work consolidated in `feat/evaluation-insights-mvp` branch
- **Target**: Team's staging repository with clean, reviewable PRs
- **Challenge**: Complex history with interdependent branches and mixed commits

## Recommended Approach: Two-PR Strategy

### PR 1: Core Evaluation System (Phases 1-3)
- Evaluation page and multi-select list view
- Three-workflow wizard (Test Model, Evaluate Responses, Import Results)
- Data models and persistence
- Bug fixes and UI improvements

### PR 2: Evaluation Insights & Analytics (Phase 4)
- Insights visualization page
- Individual results browser with file tree
- Multi-evaluation comparison (2-5 evaluations)
- Export functionality (CSV, JSON, HTML)

## Step-by-Step Migration Plan

### 1. Prepare Your Repository

```bash
# Add staging remote if not already added
git remote add staging https://github.com/TEAM/ai-dev-gallery-staging

# Fetch latest from staging
git fetch staging

# Verify your current branch has everything
git checkout feat/evaluation-insights-mvp
# Test the application to ensure all features work
```

### 2. Create Clean Branches from Current State

Since branches were built sequentially, the latest branch contains all work:

#### For PR 1: Core Evaluation System
```bash
# Create branch for core features
git checkout staging/main
git checkout -b feature/evaluation-core

# Apply changes from your branch (excluding insights-specific files)
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Pages/Evaluate/EvaluatePage.xaml*
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Pages/Evaluate/EvaluationWizard/
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Models/Evaluation*
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Services/Evaluate/EvaluationResultsStore.cs
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Controls/Evaluate/
# (exclude EvaluationInsightsPage and CompareEvaluationsPage files)

git add .
git commit -m "feat: Core evaluation system with wizard and list view

- Three evaluation workflows (Test Model, Evaluate Responses, Import Results)
- Multi-step wizard with validation
- Evaluation list with multi-selection
- Status tracking and progress indicators
- Local JSON persistence"
```

#### For PR 2: Evaluation Insights
```bash
# Create branch for insights features
git checkout staging/main  
git checkout -b feature/evaluation-insights

# First apply core changes (PR 1 prerequisite)
git merge feature/evaluation-core

# Then apply insights-specific changes
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Pages/Evaluate/EvaluationInsightsPage.xaml*
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Pages/Evaluate/CompareEvaluationsPage.xaml*
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Controls/Evaluate/FileTreeExplorer.xaml*
git checkout feat/evaluation-insights-mvp -- AIDevGallery/Controls/Evaluate/ResultDetailsPanel.xaml*
# ... other insights-specific files

git add .
git commit -m "feat: Evaluation insights and comparison features

- Detailed results visualization with charts
- Individual results browser with file tree
- Multi-evaluation comparison (2-5 evaluations)
- Export to CSV/JSON/HTML
- Statistical analysis and model rankings"
```

### 3. Alternative: Soft Reset Approach

If you prefer more granular commits:

```bash
# Start from your final branch
git checkout feat/evaluation-insights-mvp
git checkout -b feature/evaluation-clean

# Soft reset to staging/main (keeps all changes unstaged)
git reset --soft staging/main

# Stage and commit in logical chunks
git add AIDevGallery/Pages/Evaluate/EvaluatePage.xaml*
git add AIDevGallery/Pages/Evaluate/EvaluationWizard/*
git commit -m "feat: Add evaluation page and wizard"

git add AIDevGallery/Models/Evaluation*
git add AIDevGallery/Services/Evaluate/*
git commit -m "feat: Add evaluation data models and services"

# Continue with other logical groupings...
```

### 4. Alternative: Single Branch Approach

If team prefers one comprehensive PR:

```bash
# Get the final working state
git checkout feat/evaluation-insights-mvp

# Create a patch of all changes
git diff staging/main > evaluation-complete.patch

# Apply to clean branch
git checkout staging/main
git checkout -b feature/evaluation-complete
git apply evaluation-complete.patch
git add .
git commit -m "feat: Complete evaluation feature implementation

- Evaluation wizard with three workflows
- Multi-select list view with status tracking  
- Evaluation insights with visualizations
- Multi-evaluation comparison
- Import/export functionality
- Bug fixes and UI improvements"
```

## PR Description Templates

### PR 1: Core Evaluation System

```markdown
## Overview

This PR introduces the core evaluation feature infrastructure, providing the foundation for model evaluation workflows in AI Dev Gallery.

## Implementation Details

### ✅ Core Features
- **Evaluation Workflows**: Three distinct paths for evaluation
  - Test Model: Direct model testing with prompts
  - Evaluate Responses: Assess existing model outputs
  - Import Results: Load pre-computed evaluation data
- **Multi-Step Wizard**: Guided setup with validation at each step
- **Evaluation Management**: List view with multi-selection support
- **Data Persistence**: Local JSON storage for evaluation results

### 📁 Key Components
- `EvaluatePage.xaml/cs` - Main evaluation list and management
- `EvaluationWizard/*` - Multi-step wizard implementation
- `EvaluationResultsStore.cs` - Data persistence service
- `Models/Evaluation*.cs` - Data models and enums

### 🧪 Testing
- [x] All three workflows complete successfully
- [x] Data persists across app sessions
- [x] Multi-selection works correctly
- [x] Import JSONL functionality validated

### 📝 Notes
- Backend execution not implemented (UI complete)
- Mock data generation for Test/Evaluate workflows
- Real data import works via JSONL files

## Screenshots
[Add evaluation list and wizard screenshots]
```

### PR 2: Evaluation Insights & Analytics

```markdown
## Overview

This PR builds upon the core evaluation system (PR #1) to add comprehensive insights, visualization, and comparison capabilities.

## Prerequisites
- Requires PR #1 (Core Evaluation System) to be merged first

## Implementation Details

### ✅ New Features  
- **Insights Dashboard**: Detailed evaluation results with charts
- **Individual Results Browser**: File tree navigation with result details
- **Comparison View**: Analyze 2-5 evaluations side-by-side
- **Export Functionality**: Generate reports in CSV/JSON/HTML

### 📊 Visualizations
- Score distribution charts
- Per-criterion breakdowns
- Model performance rankings
- Folder-level statistics

### 📁 Key Components
- `EvaluationInsightsPage.xaml/cs` - Main insights dashboard
- `CompareEvaluationsPage.xaml/cs` - Multi-evaluation comparison
- `FileTreeExplorer.xaml/cs` - Hierarchical result browser
- `Controls/Evaluate/*` - Reusable visualization components

### 🧪 Testing
- [x] Charts render correctly with various data
- [x] File tree handles deep hierarchies
- [x] Export produces valid files
- [x] Comparison handles edge cases (2-5 evaluations)

## Screenshots
[Add insights and comparison screenshots]
```

## Handling Existing Draft PRs

For existing PRs on your personal fork:

1. **Close with Explanation**:
   ```
   This PR has been superseded by consolidated PRs to the staging repository:
   - Core System: [staging-repo-url]/pull/[number]
   - Insights: [staging-repo-url]/pull/[number]
   
   See the new PRs for the complete, production-ready implementation.
   ```

2. **Reference in New PRs**:
   ```
   This PR consolidates development work from multiple branches:
   - Initial implementation explored in fork PRs #1-#14
   - Represents [X] months of iterative development
   - All features thoroughly tested in current state
   ```

## Post-Merge Cleanup

### After PRs are Merged

```bash
# 1. Update your fork's main branch
git checkout main
git pull staging main
git push origin main

# 2. Archive development branches
git tag archive/evaluation-development feat/evaluation-insights-mvp
git push origin archive/evaluation-development

# 3. Delete old branches locally
git branch -D feature/evaluate-page-entry
git branch -D feature/wizard-type-and-setup
# ... other branches

# 4. Delete from remote (optional - keep tagged version)
git push origin --delete feature/evaluate-page-entry
# ... other branches
```

## Branch History Reference

For historical context, the feature was developed across these branches:
1. `feature/evaluate-page-entry` - Initial page structure
2. `feature/wizard-type-and-setup` - Wizard flow design
3. `feature/configure-model-for-eval` - Model configuration
4. `feature/upload-dataset-step` - Dataset handling
5. `feature/metrics-selection-step` - Metrics configuration
6. `feature/review-configuration-step` - Final review step
7. `feature/complete-core-wizard` - Wizard completion
8. `feature/evaluation-list-with-multi-select` - List view
9. `feature/evaluation-wizard-fixes` - Bug fixes
10. `feat/evaluation-insights` - Initial insights
11. `feat/result-distribution-chart` - Chart components
12. `feat/evaluation-insights-mvp` - Complete insights feature

## Key Decisions

### Why Two PRs?
1. **Logical Separation**: Core system vs. advanced features
2. **Review Complexity**: Easier to review in chunks
3. **Risk Management**: Core features can ship independently
4. **Clear Dependencies**: PR 2 explicitly depends on PR 1

### Why Not Cherry-Pick?
1. **Interdependencies**: Commits often touched multiple features
2. **Time Investment**: Would require extensive conflict resolution
3. **Current State Works**: Latest branch is fully tested
4. **Clean Result**: Taking current state gives clean code

## Quick Command Reference

```bash
# Check branch differences
git log staging/main..feat/evaluation-insights-mvp --oneline

# See changed files
git diff staging/main --name-only

# Count lines changed
git diff staging/main --stat

# Create patch for specific files
git diff staging/main -- "AIDevGallery/Pages/Evaluate/*" > evaluation-pages.patch

# Apply patch to new branch
git apply evaluation-pages.patch
```

## Summary

The recommended approach prioritizes:
1. **Clean, reviewable code** over perfect git history
2. **Working features** over process purity  
3. **Team productivity** over individual preferences
4. **Clear documentation** of development journey

This guide consolidates learnings from the development process and provides a clear path forward for contributing substantial features from personal development to team repositories.