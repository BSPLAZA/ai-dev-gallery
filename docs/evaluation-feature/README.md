# Evaluation Feature Documentation

## Quick Start for Developers

If you're looking to understand or work on the evaluation feature, start here:

1. **[Current State](current-state.md)** - What's built and how it works
2. **[TODO](TODO.md)** - What needs to be done next
3. **[Data Model](architecture/data-model.md)** - Core data structures

## What is the Evaluation Feature?

A tool to test and compare AI model performance through three workflows:
- **Test Model** - Configure a model and dataset for evaluation
- **Evaluate Responses** - Score existing model outputs
- **Import Results** - Load external evaluation data (JSONL)

## Other Documentation

- **[User Guide](user-guide/)** - For end users (when backend is ready)
- **[Developer Guide](developer-guide/)** - Implementation details & patterns
- **[Architecture](architecture/)** - System design & data flow
- **[Contributing](contributing/)** - How to submit PRs

## Key Files & Folders

```
AIDevGallery/
├── Pages/Evaluate/          # Main pages (EvaluatePage, wizard steps)
├── Controls/Evaluate/       # Reusable controls (list row, charts)
├── Services/Evaluate/       # Business logic (EvaluationResultsStore)
└── Models/                  # Data models (EvaluationResult, etc.)
```

## Current Status

- ✅ **UI Complete** - All screens and workflows implemented
- ✅ **Import Works** - Can import and visualize JSONL evaluation data
- ⏳ **No Backend** - Test/Evaluate workflows create mock data only
- ⏳ **Local Storage** - Data saved to `%LOCALAPPDATA%\AIDevGallery\Evaluations\`

---

*For development history, see [planning-archive/](planning-archive/)*
