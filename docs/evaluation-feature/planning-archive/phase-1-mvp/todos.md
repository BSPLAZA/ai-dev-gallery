# Evaluation Wizard TODO Tracker (Full Feature List)
*Last Updated: June 18, 2025*

> **⚠️ NOTE**: This file contains ALL planned features. For MVP-focused tasks, see [todos-mvp.md](todos-mvp.md)

**Status Legend**: ⏳ Pending | 🔄 In Progress | ✅ Complete | ❌ Blocked | ⏸️ Post-MVP

## 🎯 Current Status (June 18, 2025)

### ✅ Just Completed (Today)
- **Two-Part Upload UI** - Implemented separate image folder + JSONL upload for workflows 2 & 3
- **Optional Prompt Field** - JSONL files no longer require prompt field, can use default prompt
- **Auto-fill Enhancement** - Model name and prompt auto-populate from JSONL data
- **State Persistence Fixed** - Model name and default prompt now persist through navigation
- **UI Reorganization** - Model/prompt fields now appear after validation for better flow
- **Log Evaluation Button** - Fixed navigation conditions for workflow 3 (TESTED ✅)
- **Test Sample Images Removed** - Eliminated ContentDialog conflicts
- **Build Errors Fixed** - All CS errors and warnings resolved

### 🔄 In Progress
- **Workflow 3 JSONL Support** - Need to improve criteria_scores field handling

### ⚠️ Current Limitations
- **Workflow 1**: ✅ Fully functional (Test Model)
- **Workflow 2**: ✅ Ready for testing (Evaluate Responses) 
- **Workflow 3**: ✅ Fully working - Log Evaluation button now functions correctly (TESTED ✅)
- **Execution**: No actual API calls or evaluation execution yet

## 🏆 Two-Part Upload Implementation (June 18, 2025)

### UI Implementation
| Task | Details | Status |
|------|---------|--------|
| Create two-part upload UI | Separate sections for images and JSONL | ✅ Complete |
| Add drag-drop support | Both image folder and JSONL sections | ✅ Complete |
| Add browse buttons | Separate buttons for each section | ✅ Complete |
| Show upload status | Success indicators with file counts | ✅ Complete |
| Add model name input | Text field for workflows 2 & 3 | ✅ Complete |
| Add default prompt input | Optional field with placeholder | ✅ Complete |

### Validation & Processing
| Task | Details | Status |
|------|---------|--------|
| Make prompt optional | No longer required in JSONL | ✅ Complete |
| Smart image matching | Flexible path resolution | ✅ Complete |
| Auto-fill from JSONL | Extract model/prompt from data | ✅ Complete |
| Validation report | Show match statistics | ✅ Complete |
| State persistence | Save model name and prompt | ✅ Complete |

### Bug Fixes
| Task | Details | Commit |
|------|---------|--------|
| Fix build errors | Required member 'Prompt', unused fields | "Fix: Resolve build errors in two-part upload" |
| Fix navigation | CurrentStep conditions for workflow 3 | "Fix: Complete fix for Log Evaluation button" |
| Remove Test Sample Images | Avoid ContentDialog conflicts | Same commit |
| Add SaveToState() | Missing call after dataset creation | Same commit |
| Fix ValidationChanged | Add missing event and handler | "Fix: Complete fix for Log Evaluation button validation" |
| Fix all warnings | CS1929, CA warnings, documentation | "Fix: Resolve all build errors and warnings" |
| Fix Log Evaluation | Step numbering mismatch | "Fix: Log Evaluation button now works - TESTED ✅" |

## 🏆 Critical Fixes Completed (June 16, 2025)

### Dialog and UI Fixes
| Task | Details | Commit |
|------|---------|--------|
| Fix dialog positioning | Removed fixed Width/Height from WizardDialog.xaml | "Fix dialog positioning and sizing issues - TESTED ✅" |
| Fix content sizing | Added MinWidth="500" MaxWidth="650" to page grids | Same commit |
| Fix dataset warning | Created inline error panel instead of nested dialog | "Enforce dataset size limit with inline error panel - TESTED ✅" |
| Enforce 1000 limit | Changed from warning to hard refusal | Same commit |

### State Persistence Implementation
| Task | Details | Commit |
|------|---------|--------|
| Create wizard state class | EvaluationWizardState with all wizard data | "Implement proper state passing in wizard navigation (Fix 4) - TESTED ✅" |
| Update navigation | All Frame.Navigate calls now pass state | Same commit |
| Implement OnNavigatedTo | All 6 pages restore state on navigation | Same commit |
| Fix control names | ImageDescriptionRadio, CriteriaContainer, etc. | "Fix: Resolve build errors in state persistence implementation" |
| Add SetValues to CriterionControl | Enables custom criteria restoration | Same commit |

### Build Error Fixes
| Error | Fix Applied |
|-------|-------------|
| CS0246 NavigationEventArgs | Added using Microsoft.UI.Xaml.Navigation |
| Control name mismatches | Updated to match XAML x:Name values |
| SA1203 field ordering | Moved constants above static fields |
| Empty string literals | Replaced "" with string.Empty |
| Culture-specific ToUpper | Changed to ToUpperInvariant |

## 🚧 Critical Missing Infrastructure (Next Priority)

### Execution Pipeline Implementation
| Task | Priority | Details |
|------|----------|---------|
| Create EvaluationExecutor service | HIGH | Core service to run evaluations |
| Implement IEvaluationTask interface | HIGH | Define evaluation execution contract |
| Add background task management | HIGH | Handle long-running evaluations |
| Create progress reporting system | HIGH | Update UI during execution |
| Add cancellation support | MEDIUM | Allow users to stop evaluations |
| Implement error recovery | MEDIUM | Handle API failures gracefully |

### API Integration
| Task | Priority | Details |
|------|----------|---------|
| Define IModelApiClient interface | HIGH | Common interface for all models |
| Implement OpenAIApiClient | HIGH | For GPT-4o support |
| Implement AzureOpenAIApiClient | HIGH | For Azure-hosted models |
| Implement AzureAIVisionApiClient | HIGH | For Azure AI Vision |
| Add credential retrieval | HIGH | Load API keys from CredentialManager |
| Implement rate limiting | MEDIUM | Respect API quotas |
| Add retry logic | MEDIUM | Handle transient failures |

### Results and Storage
| Task | Priority | Details |
|------|----------|---------|
| Design results database schema | HIGH | Store evaluation outputs |
| Implement metrics calculation | HIGH | SPICE, CLIPScore, METEOR |
| Create AI Judge scoring system | HIGH | Custom criteria evaluation |
| Add results serialization | HIGH | Save to local database |
| Implement results UI | MEDIUM | Display in EvaluatePage |
| Add export functionality | LOW | Export results to CSV/JSON |

## Phase 2: Conditional Navigation & ModelConfigurationStep Rename

| # | Task | Status | Notes |
|---|------|--------|--------|
| 26 | Analyze current ModelConfigurationStep visibility logic | ✅ Complete | Always shown, needs conditional logic |
| 27 | Plan conditional navigation based on workflow selection | ✅ Complete | See Phase 2 Analysis above |
| 28 | Create detailed flow diagram for each workflow path | ✅ Complete | See navigation flows above |
| 29 | Rename EvaluationDetailsStep to ModelConfigurationStep | ✅ Complete | Better naming clarity |
| 30 | Update all references to EvaluationDetailsStep in codebase | ✅ Complete | EvaluatePage.xaml.cs updated |
| 31 | Update namespace and class documentation | ✅ Complete | Comments updated to Step 3 |
| 32 | Test rename doesn't break existing functionality | ⏳ Pending | Need user to test |
| 33 | Implement conditional navigation in EvaluatePage | ✅ Complete | Skip model config for other workflows |
| 34 | Update step numbering based on workflow | ✅ Complete | TestModel: 6 steps, Others: 5 steps |
| 35 | Test navigation for TestModel workflow (all steps) | ⏳ Pending | |
| 36 | Test navigation for EvaluateResponses workflow (skip model config) | ⏳ Pending | |
| 37 | Test navigation for ImportResults workflow (minimal steps) | ⏳ Pending | |
| 38 | Ensure back navigation works correctly for all paths | ⏳ Pending | |
| 39 | Update progress bar to show correct total steps | ⏳ Pending | |
| 40 | Create commit for conditional navigation implementation | ⏳ Pending | |

## Phase 3: DatasetUploadPage Implementation (Step 4)

| # | Task | Status | Notes |
|---|------|--------|--------|
| 41 | Review technical plan for dataset upload requirements | ✅ Complete | Step 4 in wizard |
| 42 | Study existing file upload patterns (FileHelper) | ✅ Complete | EvaluationFileHelper reviewed |
| 43 | Create mockup for DatasetUploadPage UI | ✅ Complete | See design above |
| 44 | Design file validation feedback UI | ✅ Complete | Real-time validation |
| 45 | Get user approval on DatasetUploadPage design | ✅ Complete | User approved |
| 46 | Create DatasetUploadPage.xaml | ✅ Complete | Full UI implemented |
| 47 | Create DatasetUploadPage.xaml.cs | ✅ Complete | All handlers implemented |
| 48 | Implement file picker integration | ✅ Complete | File and folder pickers |
| 49 | Create JSONL parser for each workflow type | ✅ Complete | Workflow-specific parsing |
| 50 | Implement local image path validation | ✅ Complete | Absolute/relative paths |
| 51 | Add preview functionality (first 5 rows) | ✅ Complete | Preview expander |
| 52 | Implement real-time validation feedback | ✅ Complete | InfoBar with details |
| 53 | Create helpful error messages | ✅ Complete | Clear error messages |
| 54 | Add file format examples/templates | ✅ Complete | Download/copy examples |
| 55 | Test with valid JSONL files | ⏳ Pending | Need user testing |
| 56 | Test with invalid files | ⏳ Pending | Need user testing |
| 57 | Test with large files | ⏳ Pending | Need user testing |
| 58 | Integrate into wizard navigation (Step 4) | ✅ Complete | All workflows integrated |
| 59 | Ensure accessibility compliance | ✅ Complete | AutomationProperties set |
| 60 | Create commit for DatasetUploadPage | ⏳ Pending | Ready to commit |

## Phase 4: MetricsSelectionPage Implementation (Step 5)

| # | Task | Status | Notes |
|---|------|--------|--------|
| 61 | Review metrics selection design in technical plan | ✅ Complete | Automated + AI Judge |
| 62 | Study existing checkbox/form patterns | ✅ Complete | |
| 63 | Create MetricsSelectionPage.xaml | ✅ Complete | Full UI implemented |
| 64 | Create MetricsSelectionPage.xaml.cs | ⏳ Pending | |
| 65 | Implement automated metrics checkboxes | ⏳ Pending | SPICE, CLIPScore, etc. |
| 66 | Create AI Judge checkbox and criteria section | ⏳ Pending | |
| 67 | Implement dynamic criterion addition UI | ⏳ Pending | Up to 5 criteria |
| 68 | Add criterion text fields with placeholders | ⏳ Pending | Name + Description |
| 69 | Implement "Add another criterion" button | ⏳ Pending | With remaining count |
| 70 | Add validation logic | ⏳ Pending | At least one metric selected |
| 71 | Create GetStepData() for metrics | ⏳ Pending | |
| 72 | Style according to existing patterns | ⏳ Pending | |
| 73 | Test progressive disclosure of AI Judge | ⏳ Pending | |
| 74 | Test criterion addition/removal | ⏳ Pending | |
| 75 | Test validation states | ⏳ Pending | |
| 76 | Integrate into wizard navigation (Step 5) | ⏳ Pending | |
| 77 | Update navigation for all workflows | ⏳ Pending | Skip for ImportResults |
| 78 | Ensure accessibility compliance | ⏳ Pending | |
| 79 | Run code style checks | ⏳ Pending | |
| 80 | Create commit for MetricsSelectionPage | ✅ Complete | TESTED ✅ |

## Phase 5: ReviewConfigurationPage Implementation (IN PROGRESS)

| # | Task | Status | Notes |
|---|------|--------|--------|
| 81 | Design ReviewConfigurationPage layout with complete configuration summary | ✅ Complete | |
| 82 | Create detailed UI mockup for ReviewConfigurationPage with all sections | ✅ Complete | |
| 83 | Design section headers with consistent styling and edit button placement | ✅ Complete | |
| 84 | Plan workflow-specific content display logic for each section | ✅ Complete | |
| 85 | Create ReviewConfigurationPage.xaml with ScrollViewer and main layout | ✅ Complete | |
| 86 | Implement Evaluation Type section with workflow name display | ✅ Complete | |
| 87 | Implement Model Configuration section (conditional for TestModel) | ✅ Complete | |
| 88 | Implement Dataset section with file count and organization summary | ✅ Complete | |
| 89 | Implement Evaluation Methods section with metrics and criteria display | ✅ Complete | |
| 90 | Add Edit buttons with proper navigation for each section | ✅ Complete | |
| 91 | Create ReviewConfigurationPage.xaml.cs with data binding | ✅ Complete | |
| 92 | Implement SetConfigurationData method to receive wizard state | ✅ Complete | |
| 93 | Add workflow-aware visibility logic for Model Configuration section | ✅ Complete | |
| 94 | Implement edit navigation handlers for jumping to specific steps | ✅ Complete | |
| 95 | Calculate and display estimated processing time based on dataset size | ✅ Complete | |
| 96 | Add warning InfoBar for large datasets or long processing times | ✅ Complete | |
| 97 | Implement final validation to ensure all required data is present | ✅ Complete | |
| 98 | Create IsReadyToExecute property for enabling Start Evaluation button | ✅ Complete | |
| 99 | Style all sections according to existing CardBackgroundFillColorDefaultBrush patterns | ✅ Complete | |
| 100 | Add proper spacing and padding following design system | ✅ Complete | |
| 101 | Implement collapsible sections for better mobile/small screen support | ⏳ Pending | Low priority |
| 102 | Add tooltips for complex configuration items | ⏳ Pending | Low priority |
| 103 | Create configuration summary text for accessibility/screen readers | ✅ Complete | |
| 104 | Integrate ReviewConfigurationPage into EvaluatePage navigation logic | ✅ Complete | |
| 105 | Update wizard step count and progress for ReviewConfigurationPage | ✅ Complete | |
| 106 | Update NextClicked handler to navigate to ReviewConfigurationPage | ✅ Complete | |
| 107 | Update BackClicked handler to return from ReviewConfigurationPage | ✅ Complete | |
| 108 | Change primary button text to 'Start Evaluation' on final step | ✅ Complete | |
| 109 | Test ReviewConfigurationPage with TestModel workflow (6 steps) | ⏳ Pending | Ready for testing |
| 110 | Test ReviewConfigurationPage with EvaluateResponses workflow (5 steps) | ⏳ Pending | Ready for testing |
| 111 | Test ReviewConfigurationPage with ImportResults workflow (4 steps) | ⏳ Pending | Ready for testing |
| 112 | Test edit navigation from each section back to correct step | ⏳ Pending | Ready for testing |
| 113 | Test that edited data persists when returning to review | ⏳ Pending | Ready for testing |
| 114 | Verify accessibility with Narrator/screen reader | ⏳ Pending | Ready for testing |
| 115 | Test keyboard navigation through all interactive elements | ⏳ Pending | Ready for testing |
| 116 | Create EvaluationExecutor interface in Models folder | ⏳ Pending | |
| 117 | Define ExecuteAsync method signature with progress and cancellation | ⏳ Pending | |
| 118 | Create EvaluationProgress class for tracking execution state | ⏳ Pending | |
| 119 | Create EvaluationResults base class for storing outcomes | ⏳ Pending | |
| 120 | Add execution preparation logic to Start Evaluation button handler | ⏳ Pending | |
| 121 | Implement configuration serialization for execution handoff | ⏳ Pending | |
| 122 | Add loading state UI for when evaluation starts | ⏳ Pending | Low priority |
| 123 | Run complete wizard flow for all workflows with different datasets | ⏳ Pending | |
| 124 | Test cancellation at each step returns to evaluation list | ⏳ Pending | |
| 125 | Verify all data persists correctly through navigation | ⏳ Pending | |
| 126 | Performance test with maximum dataset size (1000 images) | ⏳ Pending | |
| 127 | Test error scenarios (missing data, invalid configuration) | ⏳ Pending | |
| 128 | Fix any styling inconsistencies found during testing | ⏳ Pending | |
| 129 | Update any error messages to be user-friendly | ⏳ Pending | |
| 130 | Run StyleCop and fix any code style issues | 🔄 In Progress | |
| 131 | Add XML documentation to all public methods | ⏳ Pending | Low priority |
| 132 | Create comprehensive commit message for ReviewConfigurationPage | ⏳ Pending | |
| 133 | Update technical plan with implementation insights | ⏳ Pending | |
| 134 | Document any architectural decisions made | ⏳ Pending | Low priority |
| 135 | Plan detailed tasks for Evaluation Execution Engine phase | ⏳ Pending | Low priority |

## Phase 6: Integration Testing & Polish

| # | Task | Status | Notes |
|---|------|--------|--------|
| 100 | Test complete TestModel workflow (6 steps) | ⏳ Pending | |
| 101 | Test complete EvaluateResponses workflow (5 steps) | ⏳ Pending | |
| 102 | Test complete ImportResults workflow (4 steps) | ⏳ Pending | |
| 103 | Test all back navigation paths | ⏳ Pending | |
| 104 | Test cancel functionality at each step | ⏳ Pending | |
| 105 | Test data persistence between steps | ⏳ Pending | |
| 106 | Test validation edge cases | ⏳ Pending | |
| 107 | Performance test with large datasets | ⏳ Pending | |
| 108 | Accessibility audit with screen reader | ⏳ Pending | |
| 109 | Fix any discovered issues | ⏳ Pending | |
| 110 | Update technical plan with implementation insights | ⏳ Pending | |
| 111 | Create comprehensive test documentation | ⏳ Pending | |
| 112 | Final code style and cleanup pass | ⏳ Pending | |
| 113 | Create PR to merge feature/complete-core-wizard | ⏳ Pending | |
| 114 | Document any remaining work for future phases | ⏳ Pending | |
| 115 | Celebrate successful implementation! 🎉 | ⏳ Pending | |

## Additional TODOs from Current Issues

### Critical Fixes (Added 2025-06-16)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 136 | Remove Width="700" from WizardDialog.xaml | ✅ Complete | Dialog positioning fixed |
| 137 | Fix dataset warning not showing for > 1000 items | ✅ Complete | Now refuses > 1000 |
| 138 | Debug IsReadyToExecute in ReviewConfigurationPage | 🔄 In Progress | Start button disabled |
| 139 | Implement OnNavigatedTo in all wizard pages | ⏳ Pending | State management |
| 140 | Test dialog centering after width fix | ✅ Complete | |
| 141 | Verify dataset warning shows immediately | ✅ Complete | |
| 142 | Add state persistence across navigation | ⏳ Pending | |

### Critical Missing Infrastructure (Added 2025-06-16)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 151 | Implement EvaluationExecutor service | ⏳ Pending | Core execution engine |
| 152 | Implement ResponseGenerator for API calls | ⏳ Pending | Multimodal API integration |
| 153 | Implement MetricsCalculator service | ⏳ Pending | SPICE, CLIPScore, etc. |
| 154 | Implement AIJudgeService | ⏳ Pending | LLM-based evaluation |
| 155 | Add progress tracking infrastructure | ⏳ Pending | Real-time UI updates |
| 156 | Create background task architecture | ⏳ Pending | Non-blocking execution |
| 157 | Add error handling and retry logic | ⏳ Pending | API failure recovery |
| 158 | Implement results storage service | ⏳ Pending | Save evaluation results |
| 159 | Add results visualization UI | ⏳ Pending | Display evaluation outcomes |
| 160 | Implement cost tracking | ⏳ Pending | Track API usage costs |
| 161 | Add execution cancellation support | ⏳ Pending | Stop running evaluations |
| 162 | Implement partial results on failure | ⏳ Pending | Save progress on error |

### Future Enhancements
| # | Task | Status | Notes |
|---|------|--------|--------|
| 143 | Add JudgeConfigurationPage | ⏳ Pending | Select judge model |
| 144 | Implement cost estimation | ⏳ Pending | Per model/dataset size |
| 145 | Add batch processing for > 1000 items | ⏳ Pending | |
| 146 | Implement resume capability | ⏳ Pending | For interrupted runs |
| 147 | Add progress tracking during execution | ⏳ Pending | |
| 148 | Create results visualization | ⏳ Pending | |
| 149 | Add export functionality | ⏳ Pending | CSV/Excel export |
| 150 | Implement evaluation comparison | ⏳ Pending | Compare multiple runs |

## Summary Statistics
- **Total Tasks**: 276 (162 + 76 new workflow tasks + 38 infrastructure)
- **Completed**: 73 (26.4%) - Added 5 from Phase 1
- **In Progress**: 0 (0%)
- **Pending**: 203 (73.6%)
- **Blocked**: 0 (0%)

## Priority Order for Pending Tasks
1. **Immediate Fixes** (Tasks 138-139, 142) - Start button, state management
2. **Critical Infrastructure** (Tasks 151-162) - Execution pipeline (BLOCKING!)
3. **Testing** (Tasks 32, 35-40, 55-57, 109-115, 123-129) - Validate implementation
4. **Documentation** (Tasks 131, 133-134) - Update docs
5. **Future Enhancements** (Tasks 143-150) - Additional features

**WARNING**: Without the execution infrastructure (tasks 151-162), the wizard creates configurations but cannot actually run evaluations!

## Workflow 2 & 3 Improvements Implementation Plan (Added June 17, 2025)

### Phase 1: Foundation & Quick Fixes (COMPLETED - June 17, 2025) ✅
| # | Task | Status | Git Branch | Notes |
|---|------|--------|------------|--------|
| 200 | Create feature branch for workflow improvements | ⏳ Next | feature/eval-wizard-two-part-upload | Ready to create |
| 201 | Debug Start Evaluation button disabled issue | ✅ Complete | | Found validation but no property update |
| 202 | Fix Start Evaluation button enablement | ✅ Complete | | Added button state update after data set |
| 203 | Test button fix across all workflows | ✅ Complete | | Tested workflow 1 only (others not ready) |
| 204 | Commit button fix | ✅ Complete | | Committed with TESTED ✅ marker |

### Phase 2: DatasetUploadPage Refactor (Day 1 - 4 hours)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 205 | Backup current DatasetUploadPage implementation | ⏳ Pending | Create .backup files |
| 206 | Design two-part upload UI layout in XAML | ⏳ Pending | Image section + JSONL section |
| 207 | Add model name input field UI | ⏳ Pending | Show only for workflows 2 & 3 |
| 208 | Add validation results panel UI | ⏳ Pending | InfoBar with action buttons |
| 209 | Add matching options expander UI | ⏳ Pending | Smart/Exact/Fuzzy options |
| 210 | Update code-behind for two-part upload | ⏳ Pending | Split validation phases |
| 211 | Implement phased validation logic | ⏳ Pending | Images → JSONL → Match → Report |
| 212 | Test UI renders correctly | ⏳ Pending | All workflows |
| 213 | Commit UI changes | ⏳ Pending | "WIP: Add two-part upload UI layout for workflows 2 & 3" |

### Phase 3: Smart Matching Implementation (Day 2 - 3 hours)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 214 | Create SmartImageMatcher class | ⏳ Pending | AIDevGallery/Utils/SmartImageMatcher.cs |
| 215 | Implement exact matching strategy | ⏳ Pending | Strict filename match |
| 216 | Implement smart matching strategy | ⏳ Pending | Ignore extensions, case |
| 217 | Implement fuzzy matching strategy | ⏳ Pending | Special chars, patterns |
| 218 | Create unit tests for matcher | ⏳ Pending | Test each strategy |
| 219 | Create ValidationReport model | ⏳ Pending | Track matches/issues |
| 220 | Integrate matcher with DatasetUploadPage | ⏳ Pending | |
| 221 | Commit matching implementation | ⏳ Pending | "Implement smart image-JSONL matching with multiple strategies" |

### Phase 4: Validation UI & Quick Fixes (Day 2 - 3 hours)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 222 | Implement validation success UI state | ⏳ Pending | Green InfoBar |
| 223 | Implement validation warning UI state | ⏳ Pending | Yellow InfoBar with issues |
| 224 | Implement validation error UI state | ⏳ Pending | Red InfoBar |
| 225 | Add View Details action button | ⏳ Pending | Shows detailed report |
| 226 | Add Fix Issues action button | ⏳ Pending | Launches quick fixes |
| 227 | Add Proceed Anyway action button | ⏳ Pending | For warnings only |
| 228 | Implement auto-fix extensions | ⏳ Pending | .jpg → .jpeg |
| 229 | Implement case mismatch fixes | ⏳ Pending | Handle Windows/Linux |
| 230 | Implement path resolution fixes | ⏳ Pending | Relative → absolute |
| 231 | Test all quick fixes | ⏳ Pending | |
| 232 | Commit validation UI | ⏳ Pending | "Add validation report UI with actionable feedback" |

### Phase 5: Workflow-Specific Handling (Day 3 - 2 hours)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 233 | Remove model field requirement for workflow 2 | ⏳ Pending | Update ParseJsonlEntry |
| 234 | Make response field required for workflow 3 | ⏳ Pending | Add validation |
| 235 | Update validation error messages | ⏳ Pending | Workflow-specific |
| 236 | Show/hide model input based on workflow | ⏳ Pending | Conditional visibility |
| 237 | Add model name to wizard state | ⏳ Pending | Persist through navigation |
| 238 | Pass model name to configuration | ⏳ Pending | Update data flow |
| 239 | Test workflow-specific validation | ⏳ Pending | All 3 workflows |
| 240 | Commit workflow updates | ⏳ Pending | "Update JSONL validation for simplified workflow requirements" |

### Phase 6: Export Templates (Day 3 - 2 hours)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 241 | Create template folder structure | ⏳ Pending | AIDevGallery/Assets/Templates/ |
| 242 | Write test-model example JSONL | ⏳ Pending | 5 sample entries |
| 243 | Write evaluate-responses example JSONL | ⏳ Pending | 5 sample entries |
| 244 | Write import-results example JSONL | ⏳ Pending | 5 sample entries |
| 245 | Add sample images (optimized) | ⏳ Pending | Small file size |
| 246 | Write README for each workflow | ⏳ Pending | Clear instructions |
| 247 | Implement download template function | ⏳ Pending | Create ZIP |
| 248 | Add download button to UI | ⏳ Pending | With flyout menu |
| 249 | Test template downloads | ⏳ Pending | |
| 250 | Commit templates | ⏳ Pending | "Add workflow template files and examples" |

### Phase 7: Testing & Polish (Day 4 - 4 hours)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 251 | Test workflow 1 - folder upload | ⏳ Pending | |
| 252 | Test workflow 1 - JSONL upload | ⏳ Pending | |
| 253 | Test workflow 1 - navigation | ⏳ Pending | |
| 254 | Test workflow 2 - two-part upload | ⏳ Pending | |
| 255 | Test workflow 2 - model name input | ⏳ Pending | |
| 256 | Test workflow 2 - validation | ⏳ Pending | |
| 257 | Test workflow 3 - required fields | ⏳ Pending | |
| 258 | Test workflow 3 - criteria_scores | ⏳ Pending | |
| 259 | Test back button state preservation | ⏳ Pending | All pages |
| 260 | Test edit buttons from review page | ⏳ Pending | |
| 261 | Test memory usage with 1000 images | ⏳ Pending | < 500MB target |
| 262 | Document and fix any bugs found | ⏳ Pending | |

### Phase 8: Code Quality (Day 4 - 1 hour)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 263 | Run StyleCop analysis | ⏳ Pending | dotnet build |
| 264 | Fix SA1200 using directive violations | ⏳ Pending | |
| 265 | Fix SA1101 this prefix violations | ⏳ Pending | |
| 266 | Fix SA1309 underscore field violations | ⏳ Pending | |
| 267 | Remove all debug code | ⏳ Pending | |
| 268 | Check for TODO comments | ⏳ Pending | |
| 269 | Self-review all changes | ⏳ Pending | |

### Phase 9: Documentation & PR (Day 4 - 1 hour)
| # | Task | Status | Notes |
|---|------|--------|--------|
| 270 | Update EVALUATION-WIZARD-TODOS.md | ⏳ Pending | Mark completed tasks |
| 271 | Add implementation notes to master plan | ⏳ Pending | Any learnings |
| 272 | Commit documentation updates | ⏳ Pending | "Update documentation with implementation notes" |
| 273 | Push feature branch | ⏳ Pending | git push -u origin feature/workflow-2-3-improvements |
| 274 | Create pull request | ⏳ Pending | Use PR template |
| 275 | Add screenshots to PR | ⏳ Pending | Show UI changes |
| 276 | Request code review | ⏳ Pending | |

### Git Strategy Summary
```
main
  └── feature/evaluation-wizard-fixes (current - has dialog fixes)
      ├── feature/eval-wizard-two-part-upload
      │   └── Handles: Images + JSONL upload, smart matching, validation UI
      ├── feature/eval-wizard-import-results-flow
      │   └── Handles: Import existing evaluations with scores
      ├── feature/eval-wizard-export-templates
      │   └── Handles: Downloadable JSONL examples for each workflow
      └── feature/eval-wizard-smart-validation
          └── Handles: Folder detection, quick fixes, auto-matching
```

### Branch Creation Commands
```bash
# For two-part upload improvements (affects workflows 2 & 3)
git checkout feature/evaluation-wizard-fixes
git checkout -b feature/eval-wizard-two-part-upload

# For import results specific features
git checkout feature/eval-wizard-two-part-upload
git checkout -b feature/eval-wizard-import-results-flow

# For template system
git checkout -b feature/eval-wizard-export-templates

# For advanced validation features
git checkout -b feature/eval-wizard-smart-validation
```

### Alternative Naming Options
```bash
# More specific names:
feature/eval-wizard-dataset-matching      # Focus on image-JSONL matching
feature/eval-wizard-validation-report     # Focus on validation UI
feature/eval-wizard-simplified-jsonl      # Focus on removing model field
feature/eval-wizard-folder-organization   # Focus on folder detection
```

### Commit Message Standards
- **WIP**: Work in progress commits during development
- **Fix**: Bug fixes (Start button, validation issues)
- **Implement**: New features (two-part upload, smart matching)
- **Add**: New files or UI elements (templates, validation panel)
- **Update**: Modifications to existing code (JSONL validation)
- **Test**: Test-related commits
- Always end with ` - TESTED ✅` when functionality is verified on Windows

---
*This file tracks all TODO items for the Evaluation Wizard implementation. Update status as tasks are completed.*