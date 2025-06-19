# Evaluation Card Redesign Specification

## Current Layout Issues
- Too much redundant information
- Item count is inaccurate
- Score/rating takes up too much space
- Workflow icon is not essential

## New Simplified Layout

### Visual Design
```
┌─────────────────────────────────────────────────────────┐
│ □  GPT-4 Vision - pet-images (1,247 items) • 2h ago    │
│    Criteria: Accuracy, Relevance, Clarity               │
└─────────────────────────────────────────────────────────┘
```

### Implementation

```xml
<Grid Height="56" Padding="16,10">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="32"/>  <!-- Checkbox -->
        <ColumnDefinition Width="*"/>   <!-- Content -->
    </Grid.ColumnDefinitions>
    
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
    
    <!-- Checkbox spans both rows -->
    <CheckBox Grid.Column="0" 
              Grid.RowSpan="2"
              VerticalAlignment="Center"
              IsChecked="{x:Bind ViewModel.IsSelected, Mode=TwoWay}"/>
    
    <!-- Line 1: Model - Dataset (count) • Time -->
    <TextBlock Grid.Column="1" 
               Grid.Row="0"
               VerticalAlignment="Center">
        <Run Text="{x:Bind ViewModel.ModelName}" 
             FontWeight="SemiBold"/>
        <Run Text=" - " 
             Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <Run Text="{x:Bind ViewModel.DatasetName}"/>
        <Run Text=" (" 
             Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <Run Text="{x:Bind ViewModel.ItemCount}"/>
        <Run Text=" items) • " 
             Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
        <Run Text="{x:Bind GetFormattedDate(ViewModel.Timestamp), Mode=OneWay}"
             Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
    </TextBlock>
    
    <!-- Line 2: Criteria list -->
    <TextBlock Grid.Column="1" 
               Grid.Row="1"
               FontSize="12"
               Foreground="{ThemeResource TextFillColorSecondaryBrush}">
        <Run Text="Criteria:" 
             FontWeight="Bold"/>
        <Run Text=" "/>
        <Run Text="{x:Bind GetCriteriaList(ViewModel.CriteriaNames), Mode=OneWay}"/>
    </TextBlock>
</Grid>
```

### Code Changes

```csharp
// In EvaluationListRow.xaml.cs
public string GetCriteriaList(IEnumerable<string> criteriaNames)
{
    if (criteriaNames == null || !criteriaNames.Any())
    {
        return "No criteria defined";
    }
    
    return string.Join(", ", criteriaNames);
}

// Remove these methods (no longer needed):
- GetWorkflowIcon()
- GetStarRating()
- GetProgressText()
- GetProgressValue()
```

### Removed Elements
1. **Workflow Icon** - Not essential information
2. **Star Rating** - Takes too much space, not always relevant
3. **Progress Bar** - Running evaluations can show status in text
4. **Third line** - Condensed to two lines

### Benefits
1. **Cleaner**: Less visual clutter
2. **Scannable**: Key info (model, dataset, count) on first line
3. **Compact**: Reduced from 72px to 56px height
4. **Focused**: Shows only essential information

## Accurate Item Count Fix

The issue with item count needs to be traced through:

1. **Import Process**
```csharp
// In EvaluationResultsStore.cs
evaluation.DatasetItemCount = imagePaths.Count; // This should be accurate
```

2. **Wizard Save**
```csharp
// In EvaluatePage.xaml.cs - SaveEvaluationFromWizard
// Make sure we're not overriding with wizard dataset count
DatasetItemCount = wizardState.Dataset?.ValidEntries ?? 0; // This might be wrong
```

3. **Fix**: Trust the import count
```csharp
if (wizardState.Workflow == EvaluationWorkflow.ImportResults)
{
    var imported = await _evaluationStore.ImportFromJsonlAsync(...);
    // Don't override DatasetItemCount - trust what import found
}
```

## Migration Path

1. Create new `EvaluationListRowCompact.xaml`
2. Test side-by-side with existing row
3. Replace old row once validated
4. Remove unused code and methods