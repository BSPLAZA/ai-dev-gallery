# Migration Plan: From Personal Fork to Team Staging Repo

*Last Updated: June 20, 2025*
*Scenario: Moving 14+ branches from personal fork to team staging repo*

## Current Situation

- **Your Setup**: Personal fork with multiple feature branches and existing draft PRs
- **Target**: Team's staging repository where you now have access
- **Challenge**: Clean up messy history and present organized changes

## Step-by-Step Migration Plan

### Phase 1: Assess Current State

```bash
# 1. Check your remotes
git remote -v
# Should show:
# origin    https://github.com/YOUR_USERNAME/ai-dev-gallery (your fork)
# upstream  https://github.com/TEAM/ai-dev-gallery (original repo)

# 2. Add staging remote if not already added
git remote add staging https://github.com/TEAM/ai-dev-gallery-staging

# 3. Fetch latest from staging
git fetch staging
```

### Phase 2: Create Clean Consolidated Branch

Since you have draft PRs on your fork, let's create a fresh start:

```bash
# 1. Start from staging's main branch
git checkout staging/main
git checkout -b feature/evaluation-complete-clean

# 2. Create a list of all your changes
# First, see what's unique to your branches
git log staging/main..feature/evaluation-wizard-fixes --oneline > wizard-fixes-commits.txt
git log staging/main..feat/evaluation-insights-mvp --oneline > insights-commits.txt

# 3. Cherry-pick the essential changes
# This gives you control over what to include
```

### Phase 3: Smart Consolidation Strategy

Instead of merging all branches, selectively bring in working code:

#### A. Core Evaluation Features
```bash
# If these aren't already in staging/main, cherry-pick them
git cherry-pick <commit-hash> # From feature/evaluate-page-entry
git cherry-pick <commit-hash> # From wizard implementation branches
```

#### B. Bug Fixes and Improvements
```bash
# Bring in all fixes from the consolidated fixes branch
git cherry-pick staging/main..feature/evaluation-wizard-fixes
```

#### C. Insights Feature
```bash
# Bring in the complete insights implementation
git cherry-pick staging/main..feat/evaluation-insights-mvp
```

### Phase 4: Clean Up the History

```bash
# Interactive rebase to organize commits logically
git rebase -i staging/main

# Organize commits into logical groups:
# 1. Core evaluation infrastructure
# 2. Wizard implementation
# 3. List view and UI
# 4. Bug fixes
# 5. Insights feature
# 6. Comparison view
```

Example rebase plan:
```
pick abc1234 feat: Add evaluation page structure
squash def5678 fix: Update evaluation page layout
pick ghi9012 feat: Implement evaluation wizard
squash jkl3456 fix: Wizard validation
squash mno7890 fix: Navigation timing
pick pqr1234 feat: Add evaluation insights
squash stu5678 feat: Add chart visualizations
squash vwx9012 feat: Add export functionality
pick yz12345 feat: Add comparison view
```

### Phase 5: Handle Existing Draft PRs

For your existing draft PRs on your personal fork:

```bash
# 1. Close them with a comment
"This PR has been superseded by a consolidated PR to the staging repository.
See [link to new PR] for the complete implementation."

# 2. Reference them in your new PR for history
"This PR consolidates work from the following development branches:
- #123 (Initial wizard implementation)
- #456 (Bug fixes)
- #789 (Insights feature)"
```

### Phase 6: Prepare the Staging PR

#### A. Push to Staging
```bash
# Push your clean branch to staging
git push staging feature/evaluation-complete-clean
```

#### B. Create Comprehensive PR

**PR Title**: 
`feat: Complete Evaluation Feature Implementation`

**PR Description Template**:
```markdown
## Overview

This PR introduces the complete Evaluation feature, consolidating several months of development work from multiple branches.

## Migration Note

This work was originally developed on a personal fork and is now being contributed to the staging repository. It consolidates the following development efforts:
- Initial evaluation wizard (branches: feature/evaluate-page-entry through feature/complete-core-wizard)
- UI improvements and bug fixes (feature/evaluation-wizard-fixes)
- Evaluation insights and visualization (feat/evaluation-insights-mvp)

## What's Implemented

### ✅ Core Features
- [ ] Three evaluation workflows (Test Model, Evaluate Responses, Import Results)
- [ ] Multi-step wizard with validation
- [ ] Evaluation list with multi-selection
- [ ] Progress tracking and status management

### ✅ Insights & Visualization
- [ ] Detailed results page with charts
- [ ] Individual results browser
- [ ] Statistical analysis
- [ ] Export to CSV/JSON/HTML

### ✅ Comparison
- [ ] Multi-evaluation comparison (2-5 evaluations)
- [ ] Model rankings and performance metrics
- [ ] Side-by-side visualizations

## Technical Implementation

- **Architecture**: MVVM with reactive ViewModels
- **Storage**: Local JSON persistence in ApplicationData
- **UI Framework**: WinUI 3 with custom controls
- **Performance**: Lazy loading, virtualization for large datasets

## Testing Checklist

- [ ] All three workflows complete successfully
- [ ] Import of JSONL files works correctly
- [ ] Export produces valid files
- [ ] UI is responsive and accessible
- [ ] No build warnings or errors

## Known Limitations

1. **No Backend Execution**: This PR implements the complete UI. Backend model execution is planned for Phase 3.
2. **Mock Data**: Test Model and Evaluate Responses workflows create mock evaluations
3. **Progress Simulation**: Progress bars simulate progress (actual execution not implemented)

## Screenshots

[Include comprehensive screenshots showing all major features]

## Migration Details

### Branches Consolidated
- `feature/evaluate-page-entry` (initial page)
- `feature/wizard-type-and-setup` (wizard flow)
- `feature/evaluation-wizard-fixes` (bug fixes)
- `feat/evaluation-insights-mvp` (insights feature)
- [List any other relevant branches]

### Commits Squashed
- ~200 commits consolidated into ~20 logical commits
- WIP and debug commits removed
- Clear commit messages following conventional commits

## Next Steps

After this PR is merged to staging:
1. Close draft PRs on personal fork
2. Delete old feature branches
3. Begin Phase 3: Backend Implementation

## References

- Original design docs: `/docs/evaluation-feature/`
- Architecture decisions: `/docs/evaluation-feature/architecture/`
- Related issues: #[issue numbers if any]
```

### Phase 7: Clean Up

After the staging PR is approved and merged:

```bash
# 1. Update your fork's main
git checkout main
git pull staging main
git push origin main

# 2. Close old PRs on your fork with explanation

# 3. Delete old branches from your fork
git push origin --delete feature/evaluate-page-entry
git push origin --delete feature/wizard-type-and-setup
# ... etc

# 4. Clean up local branches
git branch -D feature/evaluate-page-entry
git branch -D feature/wizard-type-and-setup
# ... etc
```

## Best Practices for This Scenario

### Do's ✅
- Start fresh from staging/main
- Cherry-pick or rebase to get clean history
- Reference old PRs for continuity
- Test thoroughly on the staging branch
- Document the migration in the PR

### Don'ts ❌
- Don't merge your fork's main (it may have diverged)
- Don't include WIP or debug commits
- Don't leave old PRs open without explanation
- Don't push all branches to staging

## Quick Command Reference

```bash
# See all your branches
git branch -a

# Check what's unique to your branch
git log staging/main..your-branch --oneline

# Cherry-pick a range of commits
git cherry-pick OLDEST_COMMIT^..NEWEST_COMMIT

# Squash last N commits
git reset --soft HEAD~N
git commit -m "feat: Consolidated feature"

# Force push to your fork (careful!)
git push origin branch-name --force-with-lease
```

## Summary

The key is to present a clean, organized contribution to the staging repo that:
1. Tells a clear story of what was built
2. Has logical, well-organized commits  
3. References the development history
4. Makes it easy for reviewers to understand and test

This approach respects both your development history and the team's need for clean, reviewable code.