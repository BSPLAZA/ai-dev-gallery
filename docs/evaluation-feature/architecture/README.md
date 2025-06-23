# Architecture Documentation

Technical documentation for the Evaluation feature implementation.

## Available Documentation

- [Data Model](data-model.md) - Core data structures and relationships
- [Overview](overview.md) - System architecture overview
- [Data Flow](data-flow.md) - How data moves through the system
- [State Management](state-management.md) - Application state handling

## Key Architectural Decisions

### MVVM Pattern
- ViewModels for reactive data binding
- Separation of UI logic from business logic
- INotifyPropertyChanged for state updates

### Storage Architecture
- Local JSON persistence in ApplicationData
- Separate storage for evaluation metadata and detailed results
- Lazy loading for performance with large datasets

### Component Structure
- **Pages**: Main navigation endpoints (`EvaluatePage`, `EvaluationInsightsPage`, etc.)
- **Controls**: Reusable UI components (`FileTreeExplorer`, `ResultDetailsPanel`, etc.)
- **Services**: Business logic (`EvaluationResultsStore`, model runners)
- **Models**: Data structures (`EvaluationResult`, `EvaluationItemResult`, etc.)

### Navigation Flow
- Frame-based navigation between pages
- Dialog-based wizard with state preservation
- Parameter passing via navigation arguments

## Code Organization

```
AIDevGallery/
├── Pages/Evaluate/          # Main pages
├── Controls/Evaluate/       # Reusable controls
├── Services/Evaluate/       # Business logic
├── Models/                  # Data models
└── ViewModels/Evaluate/     # View models
```

[← Back to Main Documentation](../README.md)
