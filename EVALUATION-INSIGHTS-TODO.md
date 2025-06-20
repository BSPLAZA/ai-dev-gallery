# Evaluation Insights MVP - Implementation Tasks

## Phase 1A: Data Model Enhancement (Priority: Critical)

### 1. Update Data Models
- [ ] Create `EvaluationItemResult` class in Models/
  - [ ] Add all properties (Id, ImagePath, RelativePath, Prompt, ModelResponse, etc.)
  - [ ] Add CustomMetadata dictionary for dynamic fields
  - [ ] Add HasCustomMetadata computed property
- [ ] Create `FolderStats` class in Models/
  - [ ] Add properties (FolderPath, ItemCount, AverageScores, SuccessRate)
- [ ] Update `EvaluationResult` model
  - [ ] Add `List<EvaluationItemResult> ItemResults` property
  - [ ] Add `Dictionary<string, FolderStats> FolderStatistics` property
  - [ ] Ensure backward compatibility with existing data

### 2. Update Data Storage
- [ ] Modify `EvaluationResultsStore.cs`
  - [ ] Update `ImportFromJsonlAsync` to preserve individual results
  - [ ] Extract custom metadata fields during parsing
  - [ ] Calculate folder statistics during import
  - [ ] Implement separate JSON file storage for details
  - [ ] Add lazy loading mechanism for detail files
- [ ] Create storage structure: `Evaluations/{id}/details.json`
- [ ] Add cleanup mechanism for orphaned detail files
- [ ] Add migration path for existing evaluations

### 3. Update JSONL Parser
- [ ] Identify standard vs custom fields
- [ ] Preserve all custom metadata in dictionary
- [ ] Handle nested objects and arrays in custom fields
- [ ] Add validation for custom field types

## Phase 1B: Basic Insights Page UI (Priority: High)

### 4. Create Page Infrastructure
- [ ] Create `EvaluationInsightsPage.xaml` in Pages/Evaluate/
- [ ] Create `EvaluationInsightsPage.xaml.cs` code-behind
- [ ] Create `EvaluationInsightsViewModel.cs` in ViewModels/
- [ ] Add navigation from evaluation list (double-click handler)
- [ ] Pass evaluation ID as navigation parameter
- [ ] Implement back navigation with breadcrumb

### 5. Implement Page Layout Structure
- [ ] Create main Grid with 3 rows (header, content, actions)
- [ ] Add ScrollViewer for content area
- [ ] Set max-width constraint (1000px)
- [ ] Apply proper padding (36,24,36,36)
- [ ] Add loading state with ProgressRing

### 6. Build Header Section
- [ ] Add breadcrumb navigation ([←] Evaluations > [Name])
- [ ] Display evaluation name as title
- [ ] Add metadata bar (Model | Dataset | Timestamp)
- [ ] Add status indicator with FontIcon (not emoji)
- [ ] Display overall score with star rating

### 7. Create Metric Cards
- [ ] Build reusable MetricCard UserControl
- [ ] Implement 4 cards layout in horizontal StackPanel
  - [ ] Images Processed (with completion %)
  - [ ] Average Score (no trend indicators)
  - [ ] Criteria Evaluated count
  - [ ] Duration (if available)
- [ ] Apply card styling from existing patterns
- [ ] Add hover effects with ThemeShadow

### 8. Implement Criteria Visualization - Chart View
- [ ] Create tab control for view switching
- [ ] Build bar chart visualization
  - [ ] Add chart title "Average Scores by Evaluation Criteria"
  - [ ] Add X-axis labels (1-5 scale with gridlines)
  - [ ] Add Y-axis criterion names
  - [ ] Show score values on bars
  - [ ] Color-code bars by performance level
  - [ ] Add performance badges (Excellent/Good/Fair)
- [ ] Add chart-specific controls (Copy/Save buttons)
- [ ] Implement scale legend at bottom

### 9. Implement Table View
- [ ] Create sortable DataGrid
- [ ] Add columns: Criterion, Score, Std Dev, Min, Max, Status
- [ ] Use FontIcons for status indicators
- [ ] Enable column sorting
- [ ] Apply alternating row colors

### 10. Add Statistical Summary
- [ ] Create summary card below visualizations
- [ ] Display Mean, Median, Std Dev, Min, Max
- [ ] Use card grid layout
- [ ] Only show when individual data available

### 11. Build Export Actions Bar
- [ ] Create sticky bottom bar
- [ ] Add Export Data button with flyout
  - [ ] CSV export option
  - [ ] JSON export option
  - [ ] Excel export option (future)
- [ ] Add Share button (future)
- [ ] Add Print Report button
- [ ] Apply proper styling and spacing

## Phase 2: Enhanced Features (Priority: Medium)

### 12. Implement Export Functionality
- [ ] Create CSV export method
  - [ ] Include headers
  - [ ] Format data properly
  - [ ] Use FileSavePicker
- [ ] Create JSON export method
  - [ ] Pretty-print JSON
  - [ ] Include all metadata
- [ ] Implement chart copy to clipboard
  - [ ] Render chart to bitmap
  - [ ] Copy to clipboard with DataPackage
  - [ ] Show confirmation toast
- [ ] Implement save chart as image
  - [ ] Support PNG and JPEG
  - [ ] Include proper filename
  - [ ] Use FileSavePicker

### 13. Add Loading and Error States
- [ ] Create loading skeleton for charts
- [ ] Add error state UI with retry button
- [ ] Implement empty state when no data
- [ ] Add timeout handling for data loading

### 14. Implement View Animations
- [ ] Add entrance animations for metric cards
- [ ] Implement smooth tab switching transitions
- [ ] Add chart bar growth animations
- [ ] Include hover state animations

### 15. Add Accessibility Features
- [ ] Set AutomationProperties.Name on all controls
- [ ] Add keyboard navigation support
- [ ] Implement high contrast mode support
- [ ] Add screen reader announcements
- [ ] Ensure proper tab order

## Phase 3: Individual Results Features (Priority: Low - After MVP)

### 16. Build Folder Analysis View
- [ ] Create TreeView for folder hierarchy
- [ ] Display folder statistics
- [ ] Add expand/collapse functionality
- [ ] Show performance indicators with FontIcons
- [ ] Enable folder-based filtering

### 17. Implement Image Browser
- [ ] Create GridView for image thumbnails
- [ ] Add virtualization for performance
- [ ] Display score badges on images
- [ ] Add filter controls toolbar
- [ ] Implement sort options dropdown
- [ ] Add pagination or "Load More" button

### 18. Build Image Detail View
- [ ] Implement inline expansion on image click
- [ ] Create two-column layout (image + details)
- [ ] Display all image metadata
- [ ] Show custom fields dynamically
- [ ] Add close button functionality
- [ ] Implement smooth expand/collapse animations

### 19. Add Distribution View
- [ ] Create histogram visualization
- [ ] Add proper axes labels
- [ ] Display statistical information
- [ ] Implement per-criterion distribution
- [ ] Add interactive tooltips

## Testing & Quality Assurance

### 20. Unit Tests
- [ ] Test data model serialization
- [ ] Test JSONL parsing with custom fields
- [ ] Test export functionality
- [ ] Test view model calculations

### 21. Integration Tests
- [ ] Test navigation flow
- [ ] Test data loading scenarios
- [ ] Test export file generation
- [ ] Test large dataset performance

### 22. UI Tests
- [ ] Test all view states
- [ ] Test responsive behavior
- [ ] Test accessibility features
- [ ] Test theme switching

## Documentation

### 23. Update Documentation
- [ ] Add insights page to user guide
- [ ] Document custom metadata support
- [ ] Create developer documentation
- [ ] Add code examples

## Performance Optimization

### 24. Optimize for Large Datasets
- [ ] Implement data virtualization
- [ ] Add lazy loading for images
- [ ] Optimize chart rendering
- [ ] Add caching layer

## Bug Fixes & Polish

### 25. Final Polish
- [ ] Fix any styling inconsistencies
- [ ] Ensure all FontIcons are correct
- [ ] Verify theme support
- [ ] Test on different screen sizes

---

## Execution Order

### Week 1: Foundation
1. Start with Phase 1A (Data Model Enhancement) - Critical foundation
2. Begin Phase 1B (Basic UI) in parallel once models are ready

### Week 2: Core Features  
3. Complete basic visualization views
4. Implement export functionality
5. Add polish and animations

### Week 3: Testing & Enhancement
6. Comprehensive testing
7. Bug fixes and optimization
8. Documentation

### Future: Advanced Features
9. Individual results browser
10. Folder analysis
11. Distribution views

---

## Success Criteria
- [ ] Data persistence works correctly
- [ ] All visualizations render properly
- [ ] Export functions work reliably
- [ ] Performance is acceptable (< 2s load time)
- [ ] Accessibility standards are met
- [ ] No regressions in existing functionality