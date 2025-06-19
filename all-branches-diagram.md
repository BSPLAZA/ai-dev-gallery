# Complete Git Branch Diagram - All Branches

## Summary
- **Total branches**: 31 (including remotes)
- **Unique branches**: 15
- **Current branch**: feature/evaluation-wizard-fixes

## All Unique Branches

### Core Branches
1. **main** - The main production branch
2. **docs-recovery** - Documentation recovery branch

### Feature Branches - Wizard Development
3. **feature/evaluate-page-entry** - Initial evaluate page
4. **feature/wizard-type-and-setup** - Wizard type selection step
5. **feature/dataset-upload** - Dataset upload functionality
6. **feature/metrics-selection** - Metrics/criteria selection
7. **feature/review-configuration** - Review and configuration page
8. **feature/complete-core-wizard** - Complete wizard integration

### Feature Branches - Enhancements
9. **feature/eval-wizard-two-part-upload** - Two-part upload for Import Results
10. **feature/eval-list-cards-v2** - Card-based evaluation list view
11. **feature/eval-list-compact** - Compact list view design

### Feature Branches - Current Work
12. **feature/evaluation-wizard-fixes** ⭐ (CURRENT BRANCH) - Collecting all fixes

### Fix Branches
13. **fix/eval-wizard-navigation** - Fixed navigation timing issue
14. **fix/eval-dataset-ui-refresh** - Fixed dataset UI refresh
15. **fix/eval-checkbox-selection** - Added checkbox selection functionality

## Visual Branch Tree

```
Repository Structure:
├── main (production)
├── docs-recovery
│
├── Feature Development (Wizard Core)
│   ├── feature/evaluate-page-entry
│   ├── feature/wizard-type-and-setup
│   ├── feature/dataset-upload
│   ├── feature/metrics-selection
│   ├── feature/review-configuration
│   └── feature/complete-core-wizard
│
├── Feature Development (Enhancements)
│   ├── feature/eval-wizard-two-part-upload
│   ├── feature/eval-list-cards-v2
│   └── feature/eval-list-compact
│
├── Feature Development (Bug Fixes)
│   └── feature/evaluation-wizard-fixes ⭐ (CURRENT)
│
└── Fix Branches (Merged into feature/evaluation-wizard-fixes)
    ├── fix/eval-wizard-navigation
    ├── fix/eval-dataset-ui-refresh
    └── fix/eval-checkbox-selection
```

## Remote Tracking

Each local branch has a corresponding remote on origin:
- `remotes/origin/main`
- `remotes/origin/docs-recovery`
- `remotes/origin/feature/*`
- `remotes/origin/fix/*`

## Branch Merge Status

### Into Main
- ✅ Likely merged: Initial wizard features
- ❓ Unknown: Enhancement features
- ⏳ Pending: Current fixes branch

### Into feature/evaluation-wizard-fixes
- ✅ fix/eval-wizard-navigation
- ✅ fix/eval-dataset-ui-refresh  
- ✅ fix/eval-checkbox-selection

## Full Mermaid Diagram

```mermaid
gitGraph
    commit id: "Initial commit"
    
    branch main
    commit id: "Main branch"
    
    branch docs-recovery
    commit id: "Docs recovery"
    
    checkout main
    
    branch feature/evaluate-page-entry
    commit id: "Evaluate page"
    
    branch feature/wizard-type-and-setup
    commit id: "Type selection"
    
    branch feature/dataset-upload
    commit id: "Dataset upload"
    
    branch feature/metrics-selection
    commit id: "Metrics selection"
    
    branch feature/review-configuration
    commit id: "Review config"
    
    branch feature/complete-core-wizard
    commit id: "Complete wizard"
    
    checkout main
    
    branch feature/eval-wizard-two-part-upload
    commit id: "Two-part upload"
    
    branch feature/eval-list-cards-v2
    commit id: "Cards v2"
    
    branch feature/eval-list-compact
    commit id: "Compact list"
    
    checkout main
    
    branch feature/evaluation-wizard-fixes
    commit id: "Start fixes"
    
    branch fix/eval-wizard-navigation
    commit id: "Nav fix"
    
    checkout feature/evaluation-wizard-fixes
    branch fix/eval-dataset-ui-refresh
    commit id: "UI refresh fix"
    merge fix/eval-wizard-navigation
    
    checkout feature/evaluation-wizard-fixes
    branch fix/eval-checkbox-selection
    commit id: "Checkbox fix"
    merge fix/eval-dataset-ui-refresh
    
    checkout feature/evaluation-wizard-fixes
    merge fix/eval-checkbox-selection tag: "CURRENT"
```

## Notes

1. The `git branch -a` command shows 31 entries because it includes:
   - 15 local branches
   - 15 remote tracking branches (origin/*)
   - 1 HEAD pointer (origin/HEAD -> origin/main)

2. All branches exist both locally and on the remote (origin)

3. The current branch `feature/evaluation-wizard-fixes` contains the merged fixes from:
   - fix/eval-wizard-navigation
   - fix/eval-dataset-ui-refresh
   - fix/eval-checkbox-selection

4. There's also a `docs-recovery` branch that appears to be separate from the evaluation wizard work