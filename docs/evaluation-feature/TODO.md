# Evaluation Feature - Consolidated TODO

*Last Updated: June 19, 2025*

## Overview

This consolidated TODO list represents the current state of development for the Evaluation feature. Items are organized by phase and priority, with clear status indicators.

**Status Legend:**
- ✅ Complete
- 🚧 In Progress
- ⏳ Planned
- ❌ Blocked
- 🔄 Needs Review

## Phase 1: UI/UX Implementation ✅ COMPLETE

### Wizard UI (100% Complete)
- ✅ SelectEvaluationTypePage - Choose evaluation domain
- ✅ WorkflowSelectionPage - Pick workflow (Test/Evaluate/Import)
- ✅ ModelConfigurationStep - Configure API (TestModel only)
- ✅ DatasetUploadPage - Upload images/JSONL with two-part support
- ✅ MetricsSelectionPage - Select evaluation methods
- ✅ ReviewConfigurationPage - Review and start
- ✅ State persistence across navigation
- ✅ Validation at each step
- ✅ Optional prompt field support
- ✅ Auto-fill model name and prompt from JSONL

### Evaluation List V3 (100% Complete)
- ✅ Compact 72px row design (EvaluationListRow)
- ✅ Checkbox multi-selection pattern
- ✅ Floating action bar (SelectionActionBar)
- ✅ Bulk delete functionality
- ✅ Empty state with guided actions
- ✅ Workflow type indicators (📥 imports, 🧪 tests)
- ✅ Score display with 1-5 star system
- ✅ Model name on second line
- ✅ Custom criteria names display

## Phase 2: Visualization & Insights 🚧 IN PROGRESS

### Evaluation Insights Page (Priority 1)
- ⏳ Create EvaluationInsightsPage.xaml/cs
- ⏳ Navigation from list double-click
- ⏳ Overview section with aggregate scores
- ⏳ Criteria breakdown with charts
- ⏳ Dataset information display
- ⏳ Individual image results gallery
- ⏳ Export results button
- ⏳ Breadcrumb navigation

### Score Visualizations
- ⏳ Radar chart for multi-metric comparison
- ⏳ Bar charts for criteria breakdown
- ⏳ Progress indicators for running evaluations
- ⏳ Score distribution histograms
- ⏳ Trend analysis (if multiple runs)

## Phase 3: Backend Execution Pipeline ❌ NOT STARTED

### Core Infrastructure (Critical)
- ⏳ IEvaluationExecutor interface definition
- ⏳ BasicEvaluationExecutor implementation
- ⏳ Background task management
- ⏳ Progress tracking (IProgress<T>)
- ⏳ Cancellation support (CancellationToken)
- ⏳ Error handling and recovery
- ⏳ Rate limiting for API calls

### API Integration
- ⏳ IModelApiClient interface
- ⏳ OpenAIApiClient implementation
- ⏳ AzureOpenAIApiClient implementation
- ⏳ AzureAIVisionApiClient implementation
- ⏳ Image Description API client (local)
- ⏳ Credential retrieval from CredentialManager
- ⏳ Retry logic with exponential backoff

### Metrics Implementation
- ⏳ SPICE Score calculation
- ⏳ CLIP Score calculation
- ⏳ METEOR Score calculation
- ⏳ Length statistics
- ⏳ AI Judge implementation
- ⏳ Custom criteria scoring
- ⏳ Score aggregation and normalization

### Results Storage
- ⏳ Extend EvaluationResultsStore for in-progress updates
- ⏳ Stream results to disk during execution
- ⏳ Handle partial results on failure
- ⏳ Implement resume capability

## Phase 4: Import/Export Enhancement ⏳ PLANNED

### Import Improvements
- ⏳ Import preview dialog
- ⏳ Format validation with detailed errors
- ⏳ Support for multiple JSONL formats
- ⏳ CSV import support
- ⏳ Batch import UI
- ⏳ Duplicate detection

### Export Features
- ⏳ Export to CSV
- ⏳ Export to JSON/JSONL
- ⏳ Export to Excel
- ⏳ Export to PDF report
- ⏳ Customizable export templates
- ⏳ Bulk export operations

## Phase 5: Advanced Features ⏳ FUTURE

### Comparison View
- ⏳ Multi-evaluation selection
- ⏳ Side-by-side comparison
- ⏳ Diff visualization
- ⏳ Comparison reports

### Enterprise Features
- ⏳ Evaluation templates
- ⏳ Scheduled evaluations
- ⏳ Cost estimation/tracking
- ⏳ Team collaboration
- ⏳ Audit logging
- ⏳ API for external tools

### Performance & Scale
- ⏳ Support for >1000 items
- ⏳ Streaming for large datasets
- ⏳ Parallel execution
- ⏳ Cloud storage integration
- ⏳ Distributed processing

## Known Issues & Bugs

### High Priority
- 🔄 Running evaluations show progress but never complete (no backend)
- 🔄 Compare button shows "Coming Soon" (needs implementation)
- 🔄 No actual model execution happens

### Medium Priority
- ⏳ Search/filter UI exists but not functional
- ⏳ Progress bars are static mockups
- ⏳ Scores show 0.0 for new evaluations

### Low Priority
- ⏳ Timezone handling for timestamps
- ⏳ Accessibility improvements needed
- ⏳ Print styling not implemented

## Technical Debt

### Code Organization
- ⏳ Extract common UI patterns to shared controls
- ⏳ Consolidate validation logic
- ⏳ Add unit tests for ViewModels
- ⏳ Add integration tests for workflows

### Documentation
- ⏳ API documentation for backend services
- ⏳ Architecture decision records
- ⏳ Deployment guide
- ⏳ Performance benchmarks

## Notes

### Design Decisions
- Moved from card-based to row-based design for efficiency
- Chose 1-5 star rating over 0-1 scores for clarity
- Implemented local-first storage before cloud sync
- Prioritized import workflow for immediate value

### Dependencies
- Waiting on AI Toolkit team for model execution APIs
- Credential Manager integration pending security review
- Chart library selection pending performance testing

### Resources
- UI Mockups: [Internal Design System]
- API Specs: See architecture/api-integration.md
- Test Data: samples/evaluation-test-data/

---

*This TODO list is actively maintained. Update status as tasks are completed.*