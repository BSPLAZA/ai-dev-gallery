# Accessibility Audit - Required Fixes

*Based on testing feedback - June 23, 2025*

## Critical Issues to Fix

### 1. Icon-Only Buttons
All buttons with only icons need accessible names:

```xml
<!-- Wrong -->
<Button ToolTipService.ToolTip="Refresh list">
    <FontIcon Glyph="&#xE117;"/>
</Button>

<!-- Correct -->
<Button ToolTipService.ToolTip="Refresh list"
        AutomationProperties.Name="Refresh evaluation list">
    <FontIcon Glyph="&#xE117;"/>
</Button>
```

### 2. Drop Zones
Drag and drop areas need proper announcements:

```xml
<Grid x:Name="ImageDropZone"
      AutomationProperties.Name="Drop image folder here"
      AutomationProperties.HelpText="Drag and drop a folder containing images, or click Browse to select">
```

### 3. Dynamic Content
Add live regions for dynamic updates:

```xml
<!-- For search results -->
<ListView AutomationProperties.LiveSetting="Polite"
          AutomationProperties.Name="{x:Bind GetResultsAnnouncement(), Mode=OneWay}">

<!-- For loading states -->
<ProgressRing AutomationProperties.LiveSetting="Assertive"
              AutomationProperties.Name="{x:Bind LoadingMessage, Mode=OneWay}">
```

### 4. Form Fields
Ensure all inputs have associated labels:

```xml
<TextBox x:Name="EvaluationNameInput"
         Header="Evaluation Name"
         AutomationProperties.Name="Evaluation Name"
         AutomationProperties.HelpText="Enter a descriptive name for this evaluation">
```

### 5. Complex Controls
TreeViews and expandable content need state announcements:

```xml
<TreeViewItem AutomationProperties.Name="{x:Bind FolderName}"
              AutomationProperties.HelpText="{x:Bind GetFolderHelpText(), Mode=OneWay}"
              AutomationProperties.ExpandedStateText="{x:Bind GetExpandedState(), Mode=OneWay}">
```

### 6. Visual-Only Information
Provide text alternatives:

```xml
<!-- For scores -->
<TextBlock Text="{x:Bind Score}"
           AutomationProperties.Name="{x:Bind GetScoreAnnouncement(), Mode=OneWay}">

<!-- For progress -->
<ProgressBar Value="{x:Bind Progress}"
             AutomationProperties.Name="{x:Bind GetProgressAnnouncement(), Mode=OneWay}">
```

## Implementation Priority

### Phase 1: Critical (Do Now)
1. Fix all icon-only buttons
2. Add labels to all form fields
3. Fix drop zone announcements
4. Add live regions for dynamic content

### Phase 2: Important (Next Sprint)
1. Improve TreeView navigation
2. Add text alternatives for visual data
3. Fix complex control announcements
4. Improve focus management

### Phase 3: Enhancement
1. Add landmarks and proper headings
2. Improve keyboard shortcuts
3. Add skip navigation links
4. Enhance color contrast

## Testing Checklist

- [ ] Test with Narrator enabled
- [ ] Navigate using only keyboard
- [ ] Check all interactive elements have names
- [ ] Verify dynamic updates are announced
- [ ] Ensure forms can be completed with screen reader
- [ ] Test error states and validation messages
- [ ] Verify focus doesn't get lost
- [ ] Check color contrast ratios