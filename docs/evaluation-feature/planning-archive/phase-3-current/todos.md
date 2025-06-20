# AI Evaluation Wizard - Active TODO Tracker
*Last Updated: June 18, 2025 - 5:15 PM*

## 🎯 Current Sprint: Evaluation Insights Page

### ✅ Completed This Sprint (June 18)

#### V3 Compact List Implementation
- [x] Convert cards to 72px compact rows
- [x] Add checkbox multi-select pattern
- [x] Implement floating action bar
- [x] Add workflow type icons (📥 imports, 🧪 tests)
- [x] Create "Compare" coming soon dialog
- [x] Fix wizard completion to save evaluations
- [x] Fix empty state button handlers
- [x] Implement delete functionality
- [x] Add PropertyChanged for selection tracking
- [x] Resolve all build errors

#### Documentation Updates
- [x] Update current-status.md with v3 completion
- [x] Update todos-unified.md progress
- [x] Update tasks-list-redesign.md
- [x] Update MVP-SCOPE.md
- [x] Update index.md
- [x] Create new master plan
- [x] Create active TODO tracker

### 🚀 Immediate Next Tasks

#### Evaluation Insights Page (Priority 1)
- [ ] Create EvaluationInsightsPage.xaml/cs
- [ ] Design page layout with tabs/sections
- [ ] Add navigation from list double-click
- [ ] Create overview section with aggregate scores
- [ ] Show criteria breakdown with visualizations
- [ ] Display dataset information
- [ ] Add individual image results viewer
- [ ] Implement breadcrumb navigation

#### Score Visualizations (Priority 2)
- [ ] Research charting libraries (Win2D vs OxyPlot)
- [ ] Create score distribution chart component
- [ ] Add criteria comparison bars
- [ ] Implement star rating display
- [ ] Create progress indicators for running evals
- [ ] Add tooltips with detailed information

#### Navigation & State (Priority 3)
- [ ] Add proper page navigation parameters
- [ ] Implement back navigation from insights
- [ ] Preserve list selection state
- [ ] Add loading states for data fetch
- [ ] Handle missing evaluation scenarios

### 📋 Backlog - Organized by Phase

#### Phase 1: Complete MVP Visualization
- [ ] Image gallery component for insights page
- [ ] Export functionality (JSON, CSV)
- [ ] Print-friendly evaluation report
- [ ] Keyboard shortcuts (Ctrl+A, Delete)
- [ ] Advanced search/filter in list
- [ ] Sort by name, date, score
- [ ] Pagination for large result sets

#### Phase 2: Execution Backend (Critical)
- [ ] Design EvaluationExecutor service interface
- [ ] Create OpenAI API client implementation
- [ ] Add Azure OpenAI support
- [ ] Implement local model execution (Phi-4)
- [ ] Add progress tracking during execution
- [ ] Handle API errors gracefully
- [ ] Support cancellation of running evals
- [ ] Queue management for multiple evaluations

#### Phase 3: Enhanced Import
- [ ] Create import preview dialog
- [ ] Support multiple JSONL formats
- [ ] Add CSV import option
- [ ] Validate scores on import (1-5 range)
- [ ] Handle malformed data gracefully
- [ ] Support streaming for large files
- [ ] Add import history

#### Phase 4: Comparison Feature
- [ ] Design comparison page layout
- [ ] Implement model selection for compare
- [ ] Create side-by-side visualizations
- [ ] Add difference highlighting
- [ ] Export comparison reports
- [ ] Statistical significance testing

#### Phase 5: Advanced Features
- [ ] Custom criteria definition
- [ ] Evaluation templates
- [ ] Batch evaluation scheduling
- [ ] Cost estimation before execution
- [ ] Integration with Git for versioning
- [ ] Team collaboration features
- [ ] API for external tools

### 🐛 Bug Fixes & Polish

#### Known Issues
- [ ] Performance test with 200+ evaluations
- [ ] Theme consistency in dark mode
- [ ] Accessibility testing with Narrator
- [ ] Memory usage optimization
- [ ] Handle very long evaluation names

#### UI Polish
- [ ] Smooth all animations
- [ ] Perfect spacing and alignment
- [ ] Consistent icon sizes
- [ ] Loading skeleton states
- [ ] Empty state illustrations
- [ ] Success/error notifications

### 📊 Technical Debt

#### Code Quality
- [ ] Split large files (DatasetUploadPage)
- [ ] Add XML documentation
- [ ] Create unit tests for ViewModels
- [ ] Implement proper DI for services
- [ ] Add logging infrastructure

#### Performance
- [ ] Implement proper caching
- [ ] Add database for persistence
- [ ] Optimize JSONL parsing
- [ ] Lazy load evaluation details
- [ ] Virtual scrolling for lists

### 🎯 Success Criteria for Current Sprint

1. **Insights Page Functional**
   - [ ] Users can click evaluation → see details
   - [ ] All scores clearly visualized
   - [ ] Navigation works both ways
   - [ ] Page loads in < 1 second

2. **Visualization Quality**
   - [ ] Charts are clear and readable
   - [ ] Information hierarchy is obvious
   - [ ] Mobile-friendly responsive design
   - [ ] Consistent with app theme

3. **User Experience**
   - [ ] Zero to insights in 3 clicks
   - [ ] No confusing UI elements
   - [ ] Clear next actions
   - [ ] Helpful empty states

### 📅 Sprint Timeline

#### This Week (June 18-21)
- Monday PM: ✅ V3 list implementation
- Tuesday: ✅ Documentation updates
- Wednesday: Insights page design
- Thursday: Basic implementation
- Friday: Polish and test

#### Next Week
- Visualization components
- API client design
- Performance testing
- User feedback integration

### 💡 Ideas Parking Lot

- Real-time evaluation streaming
- LLM-powered insights generation
- Evaluation recommendation engine
- Integration with VS Code
- Mobile companion app
- Web dashboard view
- Evaluation marketplace

---

## Quick Links
- [Master Plan](./EVALUATION-WIZARD-MASTER-PLAN.md)
- [Current Status](./docs/work-docs/evaluation-wizard/current-status.md)
- [MVP Scope](./docs/work-docs/MVP-SCOPE.md)
- [UX Design](./docs/work-docs/ux-design/ux-design-unified.md)