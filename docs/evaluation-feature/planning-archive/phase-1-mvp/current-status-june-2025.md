# Evaluation Wizard - Current Status

*Last Updated: June 18, 2025 - 5:00 PM*

## Overview
The evaluation wizard is a multi-step UI for configuring AI model evaluations. It supports three workflows and is currently feature-complete for configuration but lacks execution infrastructure.

## What's Built ✅

### Core Wizard Infrastructure
- ✅ Multi-step wizard dialog with progress tracking
- ✅ State persistence across navigation
- ✅ Conditional navigation based on workflow
- ✅ Back/Next button handling
- ✅ Validation at each step

### All 6 Wizard Pages
1. ✅ **SelectEvaluationTypePage** - Choose evaluation domain
2. ✅ **WorkflowSelectionPage** - Pick workflow (Test/Evaluate/Import)
3. ✅ **ModelConfigurationStep** - Configure API (TestModel only)
4. ✅ **DatasetUploadPage** - Upload images/JSONL
5. ✅ **MetricsSelectionPage** - Select evaluation methods
6. ✅ **ReviewConfigurationPage** - Review and start

### Workflow Support
- ✅ **Workflow 1 (Test Model)** - Full pipeline with API configuration
- ✅ **Workflow 2 (Evaluate Responses)** - Two-part upload (images + JSONL)
- ✅ **Workflow 3 (Import Results)** - Import existing evaluations

### Recent Improvements (June 18)
- ✅ Two-part upload UI for workflows 2 & 3
- ✅ Optional prompt field in JSONL
- ✅ Auto-fill model name and prompt from data
- ✅ Fixed Log Evaluation button for workflow 3
- ✅ Resolved all build errors and warnings
- ✅ Implemented complete v3 compact list design
- ✅ Added multi-select with floating action bar
- ✅ Fixed wizard completion - now saves evaluations
- ✅ Fixed empty state button handlers
- ✅ Implemented delete functionality

## What's NOT Built ❌

### Execution Infrastructure
- ❌ No EvaluationExecutor service
- ❌ No API integration (OpenAI, Azure, etc.)
- ❌ No actual model calls
- ❌ No progress tracking during execution
- ❌ No cancellation support

### Results & Storage
- ❌ No results database
- ❌ No metrics calculation (SPICE, CLIPScore, etc.)
- ❌ No AI Judge implementation
- ❌ No results visualization
- ❌ No export functionality

### Evaluation List Page
- ✅ Compact list design (v3) implemented
- ✅ 72px height rows with high information density
- ✅ Multi-select with checkboxes
- ✅ Floating action bar (Compare/Delete/Cancel)
- ✅ Workflow type icons (📥 imports, 🧪 tests)
- ✅ Score badges with 1-5 star ratings
- ✅ Progress bars for running evaluations
- ✅ Simple empty state with quick actions
- ✅ Sample data with 4 test evaluations
- ✅ Search functionality
- ⚠️ Compare view shows "Coming Soon"
- ❌ No detailed evaluation view yet
- ❌ No advanced filtering/sorting
- ❌ No insights or analytics

## Current Limitations

### Technical
- Maximum 1,000 items per dataset (UI limitation)
- No batch processing for larger datasets
- No resume capability for interrupted evaluations
- No cost estimation before execution

### UX
- Generic evaluation names (timestamp-based)
- Limited information in list view
- No preview of results
- No progress visualization during execution

## Next Steps Priority

### Immediate: Evaluation Details Page 🎯
**List v3 complete - now focusing on details view!**
1. Create details page for single evaluation
2. Show all criteria scores with visualizations
3. Display dataset information
4. Add score breakdowns and insights
5. Show individual test results

### Phase 1: Execution Infrastructure 🚨
**Critical for making evaluations actually work!**
1. Create EvaluationExecutor service
2. Implement API clients (OpenAI, Azure)
3. Add progress tracking
4. Handle errors and retries
5. Support cancellation

### Phase 2: Import Results Enhancement
1. Parse JSONL scores properly
2. Support multiple score formats
3. Validate imported data
4. Show import preview
5. Handle large files efficiently

### Phase 3: Results Storage
1. Design results database schema
2. Implement metrics calculators
3. Store evaluation outputs
4. Create results API

### Phase 4: Advanced Features
1. Implement actual comparison view
2. Add advanced filtering/sorting
3. Create analytics dashboard
4. Generate insights

## Technical Debt

### Code Quality
- Some files exceed 1000 lines
- Need to split DatasetUploadPage
- Add unit tests for validation logic
- Improve error handling

### Performance
- Large JSONL files load entirely into memory
- No virtualization for preview lists
- Dataset validation could be async
- Image previews not optimized

## Success Metrics
- ✅ All 3 workflows navigate correctly
- ✅ State persists through navigation
- ✅ Validation prevents invalid configurations
- ❌ Evaluations can actually execute
- ❌ Results are stored and viewable
- ❌ Users can compare evaluations

## Summary
The wizard UI is **100% complete** and the evaluation list is **90% complete** with full v3 compact design. The backend remains **0% complete**. Users can now:
- ✅ Configure evaluations through the wizard
- ✅ Import results from JSONL files
- ✅ View evaluations in a compact list
- ✅ Multi-select and delete evaluations
- ❌ Actually run evaluations (no execution backend)
- ❌ View detailed results (no details page)

The critical gap is the execution infrastructure - without it, only the Import Results workflow is truly functional.