# Evaluation Wizard - MVP Task List

*Last Updated: June 18, 2025*

## 🚨 Critical Path to MVP

### Priority 1: Make It Work (Execution Pipeline)
- [ ] Create `IEvaluationExecutor` interface
- [ ] Implement `BasicEvaluationExecutor` class
- [ ] Add progress reporting (0-100%)
- [ ] Handle cancellation
- [ ] Create `OpenAIImageClient` for GPT-4V
- [ ] Add retry logic (3 attempts)
- [ ] Test with 10 images first

### Priority 2: Store Results
- [ ] Create `EvaluationResult` model (simple)
- [ ] Add SQLite table for results
- [ ] Store: config, score, start/end time
- [ ] Update evaluation status in list

### Priority 3: Show Results
- [ ] Add "Score" column to evaluation list
- [ ] Show completion time
- [ ] Create minimal details page
- [ ] Display configuration and score

### Priority 4: Error Handling
- [ ] Handle API failures gracefully
- [ ] Show clear error messages
- [ ] Allow retry of failed evaluations
- [ ] Log errors for debugging

## ✅ Completed (Already Built)

### Wizard UI (100% Complete)
- ✅ All 6 wizard pages implemented
- ✅ Three workflows working
- ✅ State persistence
- ✅ Navigation and validation
- ✅ Two-part upload for workflows 2 & 3
- ✅ Optional prompt support
- ✅ Log Evaluation button fixed

### Bug Fixes (Complete)
- ✅ Dialog positioning issues
- ✅ Dataset size enforcement
- ✅ Build errors resolved
- ✅ Navigation step numbering

## 📋 Deferred to Post-MVP

### Advanced Features (DO NOT BUILD YET)
- ⏸️ SPICE, CLIPScore, METEOR metrics
- ⏸️ AI Judge evaluation
- ⏸️ Comparison view
- ⏸️ Detailed insights page
- ⏸️ Export functionality
- ⏸️ Cost tracking
- ⏸️ Multiple model providers
- ⏸️ Folder-based analysis
- ⏸️ Sample result viewing
- ⏸️ Real-time progress updates

## 📊 MVP Success Metrics

1. **Can run evaluation**: Start → Progress → Complete
2. **Shows results**: Score visible in list
3. **Handles errors**: Doesn't crash on API failure
4. **Reasonable speed**: <1 hour for 1000 images

## 🎯 Daily Focus

### Today's Goal
Implement `IEvaluationExecutor` interface and basic implementation

### Tomorrow's Goal  
Create `OpenAIImageClient` and test with 10 images

### This Week's Goal
Complete MVP - evaluations actually run and show results

## 🚧 Known Limitations (Acceptable for MVP)

1. **Single provider**: OpenAI only
2. **Basic metric**: Simple accuracy score
3. **No live updates**: Progress refreshes on navigation
4. **Limited details**: Just config and score
5. **No comparisons**: One evaluation at a time

## 💡 Remember

> "The MVP is not about impressing with features, it's about delivering value. A working evaluation with basic scoring is infinitely more valuable than a beautiful UI that does nothing."

## Quick Command Reference

```csharp
// Start evaluation
var executor = new BasicEvaluationExecutor(openAiClient);
var result = await executor.ExecuteAsync(config, progress => 
{
    // Update UI with progress
});

// Store result
await resultsStore.SaveAsync(evaluationId, result.Score);
```