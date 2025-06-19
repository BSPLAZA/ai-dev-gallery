# Evaluation Feature - Issues and Fix Plan

## Current Issues Summary

### 1. Wizard Navigation Issues
**Problem**: Navigation flow is broken - clicking "Next" from workflow selection skips Model Configuration step (Step 3) and goes directly to Dataset Upload (Step 4).
**Workaround**: User must go back and click workflow selection again to see proper navigation.
**Affects**: All workflows have similar navigation issues.

### 2. Dataset Upload UI Refresh
**Problem**: Folder grouping options don't appear immediately after dataset upload.
**Workaround**: Must navigate away and back to see the options.
**Root Cause**: UI not refreshing after async data load.

### 3. Selection Functionality (Critical)
**Problem**: Checkbox selection is fundamentally broken
- Hard to click checkboxes
- Selection state not persisting properly
- Dynamic action bar (Compare, Delete) not appearing on selection
**Previous Attempts**: Multiple fixes attempted, but core issue remains.

### 4. Delete Button
**Problem**: Delete button functionality needs verification.
**Status**: Unclear if working properly.

### 5. Card Display Issues
**Problems**:
- Item count is inaccurate
- Too much repetitive information
- Layout needs redesign

**Proposed New Layout**:
```
Line 1: [Model Name] - [Dataset Name] (X items) • [Time]
Line 2: Criteria: [criterion1, criterion2, criterion3]
```

## Grouped Fix Strategy

### Phase 1: Core Functionality (Branch: `fix/evaluation-core`)
1. **Wizard Navigation Fix**
   - Fix state management in wizard navigation
   - Ensure proper step sequencing for all workflows
   - Add navigation state debugging

2. **Delete Functionality**
   - Verify delete button works
   - Ensure proper cleanup of selected items
   - Add confirmation dialog if missing

### Phase 2: Selection System Overhaul (Branch: `fix/selection-system`)
1. **Complete Selection Rewrite**
   - Remove all previous selection logic
   - Implement clean MVVM selection pattern
   - Use proper command binding instead of event handlers
   - Ensure action bar appears/disappears correctly

2. **Checkbox Interaction**
   - Larger hit targets for checkboxes
   - Clear visual feedback on hover/selection
   - Prevent event bubbling issues

### Phase 3: UI Polish (Branch: `fix/ui-refresh`)
1. **Dataset Upload Refresh**
   - Force UI update after async operations
   - Add loading states during data processing
   
2. **Card Layout Redesign**
   - Implement new compact layout
   - Fix item count accuracy
   - Remove redundant information

## Git Strategy

```bash
# Create feature branch for all fixes
git checkout -b feature/evaluation-fixes

# Create sub-branches for each phase
git checkout -b fix/evaluation-core
# ... implement Phase 1 fixes
git commit -m "Fix: Wizard navigation and delete functionality"

git checkout feature/evaluation-fixes
git merge fix/evaluation-core

git checkout -b fix/selection-system
# ... implement Phase 2 fixes
git commit -m "Fix: Complete overhaul of selection system"

git checkout feature/evaluation-fixes
git merge fix/selection-system

git checkout -b fix/ui-refresh
# ... implement Phase 3 fixes
git commit -m "Fix: UI refresh and card layout improvements"

git checkout feature/evaluation-fixes
git merge fix/ui-refresh
```

## Technical Approach

### 1. Wizard Navigation Fix
```csharp
// Problem: State not properly maintained between navigation
// Solution: Ensure wizardState is properly passed and workflow is set
private Type? GetNextPage(object currentPage, EvaluationWizardState state)
{
    // Add logging to debug navigation
    System.Diagnostics.Debug.WriteLine($"Current: {currentPage.GetType().Name}, Workflow: {state.Workflow}");
    
    return currentPage switch
    {
        WorkflowSelectionPage => state.Workflow switch
        {
            EvaluationWorkflow.TestModel => typeof(ModelConfigurationStep),
            _ => typeof(DatasetUploadPage)
        },
        // ... rest of navigation
    };
}
```

### 2. Selection System Overhaul
```csharp
// New approach: Use Commands instead of events
public class EvaluationListItemViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;
    
    partial void OnIsSelectedChanged(bool value)
    {
        // Notify parent view model
        SelectionChanged?.Invoke(this, value);
    }
    
    public event EventHandler<bool>? SelectionChanged;
}

// In EvaluatePage
private void SetupSelectionHandling()
{
    foreach (var item in AllEvaluations)
    {
        item.SelectionChanged += OnItemSelectionChanged;
    }
}

private void OnItemSelectionChanged(object? sender, bool isSelected)
{
    UpdateActionBarVisibility();
}
```

### 3. Card Layout Redesign
```xml
<!-- New compact layout -->
<Grid Height="60" Padding="16,12">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Line 1: Model - Dataset (count) • Time -->
    <TextBlock Grid.Row="0">
        <Run Text="{x:Bind ModelName}" FontWeight="SemiBold"/>
        <Run Text=" - "/>
        <Run Text="{x:Bind DatasetName}"/>
        <Run Text=" ("/>
        <Run Text="{x:Bind ItemCount}"/>
        <Run Text=" items) • "/>
        <Run Text="{x:Bind GetFormattedDate(Timestamp)}"/>
    </TextBlock>
    
    <!-- Line 2: Criteria list -->
    <TextBlock Grid.Row="1" FontSize="12" Opacity="0.8">
        <Run Text="Criteria:" FontWeight="Bold"/>
        <Run Text=" "/>
        <Run Text="{x:Bind GetCriteriaList()}"/>
    </TextBlock>
</Grid>
```

## Priority Order

1. **Wizard Navigation** (High) - Core functionality broken
2. **Selection System** (High) - Major UX issue
3. **Delete Button** (Medium) - Basic functionality
4. **Dataset UI Refresh** (Medium) - Has workaround
5. **Card Redesign** (Low) - Visual polish

## Testing Checklist

- [ ] Wizard navigation works for all 3 workflows
- [ ] Checkboxes are easily clickable
- [ ] Selection state persists correctly
- [ ] Action bar appears/disappears on selection
- [ ] Delete button removes selected items
- [ ] Dataset upload shows options immediately
- [ ] Card shows accurate item count
- [ ] New card layout displays correctly