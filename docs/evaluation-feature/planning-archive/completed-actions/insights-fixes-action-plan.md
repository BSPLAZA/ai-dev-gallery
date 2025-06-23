# Evaluation Insights - Critical Fixes Action Plan

## Priority 1: Accessibility Fixes (MUST DO)

### 1. Add AutomationProperties.Name to all buttons

```xml
<!-- BackButton (Line 31) -->
<Button x:Name="BackButton" 
        AutomationProperties.Name="Navigate back to evaluations list"
        Click="BackButton_Click"
        Style="{StaticResource SubtleButtonStyle}"
        VerticalAlignment="Center">

<!-- StatusInfoBadge (Line 101) -->
<Button x:Name="StatusInfoBadge"
        AutomationProperties.Name="View evaluation status details"
        Style="{StaticResource SubtleButtonStyle}">

<!-- View Toggle Buttons (Line 190-214) -->
<ToggleButton x:Name="ChartViewToggle" 
              AutomationProperties.Name="Switch to chart view"
              IsChecked="True"
              Click="ViewToggle_Click">

<ToggleButton x:Name="TableViewToggle"
              AutomationProperties.Name="Switch to table view" 
              Click="ViewToggle_Click">

<ToggleButton x:Name="FolderViewToggle"
              AutomationProperties.Name="Switch to folder view"
              Click="ViewToggle_Click">

<!-- Chart action buttons (Line 245-256) -->
<Button x:Name="CopyChartButton"
        AutomationProperties.Name="Copy chart to clipboard"
        Click="CopyChart_Click">

<Button x:Name="SaveChartButton"
        AutomationProperties.Name="Save chart as image"
        Click="SaveChartAsImage_Click">

<!-- Export button (Line 937) -->
<Button x:Name="ExportDataButton" 
        AutomationProperties.Name="Export evaluation data"
        Click="ExportData_Click">

<!-- Print button (Line 958) -->
<Button x:Name="PrintButton"
        AutomationProperties.Name="Print evaluation report" 
        Click="PrintReport_Click">
```

### 2. Add AutomationProperties to input controls

```xml
<!-- Search box (Line 626) -->
<TextBox x:Name="ImageSearchBox"
         AutomationProperties.Name="Search images by filename"
         PlaceholderText="Search images..."
         TextChanged="ImageSearchBox_TextChanged"/>

<!-- Filter toggles (Line 632-646) -->
<ToggleButton x:Name="ErrorFilterToggle"
              AutomationProperties.Name="Filter to show only errors"
              Click="ErrorFilterToggle_Click">

<ToggleButton x:Name="HighScoreFilterToggle"
              AutomationProperties.Name="Filter to show highest scoring images"
              Click="HighScoreFilterToggle_Click">
```

### 3. Add keyboard navigation support

```csharp
// In constructor after InitializeComponent()
this.PreviewKeyDown += OnPreviewKeyDown;

private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
{
    // Handle Escape key to go back
    if (e.Key == Windows.System.VirtualKey.Escape)
    {
        e.Handled = true;
        Frame.GoBack();
    }
    
    // Handle Ctrl+S to save chart
    if (e.Key == Windows.System.VirtualKey.S && 
        (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control) & 
         Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down)
    {
        e.Handled = true;
        SaveChartAsImage_Click(null, null);
    }
}
```

## Priority 2: Memory Leak Fixes (MUST DO)

### 1. Implement OnNavigatedFrom cleanup

```csharp
protected override void OnNavigatedFrom(NavigationEventArgs e)
{
    base.OnNavigatedFrom(e);
    
    // Unsubscribe from page events
    this.PreviewKeyDown -= OnPreviewKeyDown;
    
    // Unsubscribe from control events
    if (ImageFileTreeView != null)
    {
        ImageFileTreeView.Expanding -= ImageFileTreeView_Expanding;
        ImageFileTreeView.ItemInvoked -= ImageFileTreeView_ItemInvoked;
    }
    
    if (ImageSearchBox != null)
    {
        ImageSearchBox.TextChanged -= ImageSearchBox_TextChanged;
    }
    
    // Clear collections
    _allTreeItems?.Clear();
    _allTreeItems = null;
    
    // Clear chart
    if (ChartContentGrid != null)
    {
        ChartContentGrid.Children.Clear();
    }
    
    // Dispose view model
    _viewModel = null;
    _evaluationStore = null;
}
```

### 2. Fix resource disposal in chart rendering

```csharp
private async Task<SoftwareBitmap> RenderChartToBitmapAsync()
{
    RenderTargetBitmap renderBitmap = null;
    SoftwareBitmap softwareBitmap = null;
    
    try
    {
        renderBitmap = new RenderTargetBitmap();
        await renderBitmap.RenderAsync(ChartContentGrid);
        
        var pixelBuffer = await renderBitmap.GetPixelsAsync();
        softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            pixelBuffer,
            BitmapPixelFormat.Bgra8,
            renderBitmap.PixelWidth,
            renderBitmap.PixelHeight);
            
        return softwareBitmap;
    }
    catch
    {
        softwareBitmap?.Dispose();
        throw;
    }
}
```

## Priority 3: Error Handling Fixes (SHOULD DO)

### 1. Create error dialog helper

```csharp
private async Task ShowErrorDialogAsync(string title, string message)
{
    var dialog = new ContentDialog
    {
        Title = title,
        Content = message,
        CloseButtonText = "OK",
        XamlRoot = this.XamlRoot
    };
    
    await dialog.ShowAsync();
}
```

### 2. Fix async void methods

```csharp
private async void ExportCsv_Click(object sender, RoutedEventArgs e)
{
    try
    {
        await ExportCsvAsync();
    }
    catch (Exception ex)
    {
        await ShowErrorDialogAsync(
            "Export Failed", 
            $"Unable to export CSV file: {ex.Message}");
    }
}

private async void SaveChartAsImage_Click(object sender, RoutedEventArgs e)
{
    try
    {
        await SaveChartAsImageAsync();
    }
    catch (Exception ex)
    {
        await ShowErrorDialogAsync(
            "Save Failed", 
            $"Unable to save chart image: {ex.Message}");
    }
}
```

### 3. Add loading states

```csharp
private async Task LoadEvaluationAsync(string evaluationId)
{
    try
    {
        // Show loading
        LoadingPanel.Visibility = Visibility.Visible;
        ContentScrollViewer.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Collapsed;
        
        var evaluation = await _evaluationStore.GetEvaluationByIdAsync(evaluationId);
        
        if (evaluation == null)
        {
            ShowEmptyState();
            return;
        }
        
        // ... rest of method
    }
    catch (Exception ex)
    {
        await ShowErrorDialogAsync(
            "Loading Failed",
            $"Unable to load evaluation details: {ex.Message}");
        ShowEmptyState();
    }
    finally
    {
        LoadingPanel.Visibility = Visibility.Collapsed;
    }
}
```

## Priority 4: Performance Fixes (SHOULD DO)

### 1. Virtualize large lists

```xml
<!-- In ScrollViewer for table view (Line 325) -->
<ScrollViewer Grid.Row="1" 
              HorizontalScrollBarVisibility="Disabled"
              VerticalScrollBarVisibility="Auto">
    <ItemsRepeater x:Name="CriteriaTableRepeater"
                   VirtualizingLayout="{x:Bind GetVirtualizingLayout()}">
```

### 2. Implement lazy loading for tree view

```csharp
private void ImageFileTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
{
    var treeViewItem = args.Item as FileTreeItem;
    if (treeViewItem?.IsFolder == true && !treeViewItem.ChildrenLoaded)
    {
        args.Cancel = true; // Cancel default expansion
        _ = LoadChildrenAsync(treeViewItem, args.Node);
    }
}

private async Task LoadChildrenAsync(FileTreeItem folderItem, TreeViewNode node)
{
    folderItem.IsLoading = true;
    
    try
    {
        await Task.Run(() =>
        {
            // Load children on background thread
            var children = LoadFolderChildren(folderItem.FullPath);
            folderItem.Children.Clear();
            folderItem.Children.AddRange(children);
        });
        
        folderItem.ChildrenLoaded = true;
        node.IsExpanded = true;
    }
    finally
    {
        folderItem.IsLoading = false;
    }
}
```

## Testing Checklist

### Accessibility Testing
- [ ] Test with Windows Narrator
- [ ] Verify all buttons are announced correctly
- [ ] Test keyboard-only navigation
- [ ] Verify tab order is logical
- [ ] Test with high contrast theme

### Memory Testing
- [ ] Navigate to/from page multiple times
- [ ] Monitor memory usage in Task Manager
- [ ] Verify event handlers are cleaned up
- [ ] Check for disposed object access

### Error Handling Testing
- [ ] Test with missing evaluation data
- [ ] Test export with no write permissions
- [ ] Test with corrupted data
- [ ] Verify all error dialogs appear correctly

### Performance Testing
- [ ] Test with 1000+ image results
- [ ] Test with deep folder hierarchies
- [ ] Measure chart render time
- [ ] Verify UI remains responsive