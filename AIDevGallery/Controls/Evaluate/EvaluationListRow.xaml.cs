// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using AIDevGallery.ViewModels.Evaluate;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI;

namespace AIDevGallery.Controls.Evaluate
{
    /// <summary>
    /// A row control for displaying evaluation items in a list view
    /// </summary>
    public sealed partial class EvaluationListRow : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// Dependency property for the ViewModel
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(EvaluationListItemViewModel), typeof(EvaluationListRow), new PropertyMetadata(null, OnViewModelChanged));

        /// <summary>
        /// Gets or sets the view model for this evaluation list row
        /// </summary>
        public EvaluationListItemViewModel ViewModel
        {
            get => (EvaluationListItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // Bindable properties from ViewModel
        /// <summary>
        /// Gets the evaluation name
        /// </summary>
        public string EvaluationName => ViewModel?.Name ?? string.Empty;
        
        /// <summary>
        /// Gets the model name
        /// </summary>
        public string ModelName => ViewModel?.ModelName ?? string.Empty;
        
        /// <summary>
        /// Gets the dataset name
        /// </summary>
        public string DatasetName => ViewModel?.DatasetName ?? string.Empty;
        
        /// <summary>
        /// Gets the item count
        /// </summary>
        public int ItemCount => ViewModel?.ItemCount ?? 0;
        
        /// <summary>
        /// Gets the timestamp
        /// </summary>
        public DateTime Timestamp => ViewModel?.Timestamp ?? DateTime.Now;
        
        /// <summary>
        /// Gets the average score
        /// </summary>
        public double AverageScore => ViewModel?.AverageScore ?? 0.0;
        
        /// <summary>
        /// Gets the workflow type
        /// </summary>
        public EvaluationWorkflow WorkflowType => ViewModel?.WorkflowType ?? EvaluationWorkflow.ImportResults;
        
        /// <summary>
        /// Gets a value indicating whether the evaluation is completed
        /// </summary>
        public bool IsCompleted => ViewModel?.Status == EvaluationStatus.Completed || ViewModel?.Status == EvaluationStatus.Imported;
        
        /// <summary>
        /// Gets a value indicating whether the evaluation is running
        /// </summary>
        public bool IsRunning => ViewModel?.Status == EvaluationStatus.Running;
        
        /// <summary>
        /// Gets the running progress percentage
        /// </summary>
        public double? RunningProgress => ViewModel?.RunningProgress;
        
        /// <summary>
        /// Gets or sets a value indicating whether this row is selected
        /// </summary>
        public bool IsSelected
        {
            get => ViewModel?.IsSelected ?? false;
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.IsSelected = value;
                }
            }
        }

        private bool _isHovered;
        /// <summary>
        /// Gets a value indicating whether this row is hovered
        /// </summary>
        public bool IsHovered
        {
            get => _isHovered;
            private set
            {
                _isHovered = value;
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Event raised when the item is clicked
        /// </summary>
        public event EventHandler<EvaluationListItemViewModel>? ItemClicked;
        
        /// <summary>
        /// Event raised when the item is double-clicked
        /// </summary>
        public event EventHandler<EvaluationListItemViewModel>? ItemDoubleClicked;

        /// <summary>
        /// Initializes a new instance of the EvaluationListRow control
        /// </summary>
        public EvaluationListRow()
        {
            this.InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EvaluationListRow row)
            {
                row.UpdateBindings(e.OldValue as EvaluationListItemViewModel);
            }
        }

        private void UpdateBindings(EvaluationListItemViewModel? oldViewModel)
        {
            // Unsubscribe from old ViewModel
            if (oldViewModel != null)
            {
                oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            
            Bindings.Update();
            
            // Subscribe to new ViewModel property changes
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(EvaluationListItemViewModel.IsSelected):
                    OnPropertyChanged(nameof(IsSelected));
                    UpdateVisualState();
                    break;
                case nameof(EvaluationListItemViewModel.AverageScore):
                    OnPropertyChanged(nameof(AverageScore));
                    break;
                case nameof(EvaluationListItemViewModel.ItemCount):
                    OnPropertyChanged(nameof(ItemCount));
                    break;
            }
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            IsHovered = true;
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            IsHovered = false;
        }

        private void OnRowTapped(object sender, TappedRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"OnRowTapped called, OriginalSource: {e.OriginalSource?.GetType().Name}");
            
            // Check if the tap originated from the checkbox
            var originalSource = e.OriginalSource as DependencyObject;
            
            // Walk up the visual tree to see if we hit a checkbox
            while (originalSource != null)
            {
                if (originalSource == SelectionCheckBox)
                {
                    System.Diagnostics.Debug.WriteLine("Tap originated from checkbox, ignoring row tap");
                    return; // Don't process row tap for checkbox clicks
                }
                
                // For WinUI 3, use VisualTreeHelper
                originalSource = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(originalSource);
            }
            
            System.Diagnostics.Debug.WriteLine("Tap not from checkbox, invoking ItemClicked");
            // For non-checkbox clicks, invoke the item click (for opening details)
            if (ViewModel != null)
            {
                ItemClicked?.Invoke(this, ViewModel);
            }
        }

        private void OnRowDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ItemDoubleClicked?.Invoke(this, ViewModel);
        }

        private void OnCheckboxClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnCheckboxClick called");
            // Let the binding handle the state change
            // Don't invoke ItemClicked since that's for row clicks
            // Checkbox state is already handled by two-way binding
        }


        private void UpdateVisualState()
        {
            if (IsSelected)
            {
                VisualStateManager.GoToState(this, "Selected", true);
            }
            else if (IsHovered)
            {
                VisualStateManager.GoToState(this, "PointerOver", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        // Helper methods for x:Bind
        /// <summary>
        /// Gets the workflow icon for the specified workflow type
        /// </summary>
        /// <param name="workflow">The workflow type</param>
        /// <returns>The icon emoji for the workflow</returns>
        public string GetWorkflowIcon(EvaluationWorkflow workflow)
        {
            return workflow switch
            {
                EvaluationWorkflow.ImportResults => "📥",
                _ => "🧪"
            };
        }

        /// <summary>
        /// Gets the workflow glyph for the specified workflow type
        /// </summary>
        /// <param name="workflow">The workflow type</param>
        /// <returns>The glyph character for the workflow</returns>
        public string GetWorkflowGlyph(EvaluationWorkflow workflow)
        {
            return workflow switch
            {
                EvaluationWorkflow.ImportResults => "\uE118", // Download icon
                _ => "\uEA80" // Test/Science icon
            };
        }

        /// <summary>
        /// Gets a formatted date string for display
        /// </summary>
        /// <param name="date">The date to format</param>
        /// <returns>A human-readable relative date string</returns>
        public string GetFormattedDate(DateTime date)
        {
            var timeSpan = DateTime.Now - date;

            return timeSpan switch
            {
                { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes}m ago",
                { TotalHours: < 24 } => $"{(int)timeSpan.TotalHours}h ago",
                { TotalDays: < 7 } => $"{(int)timeSpan.TotalDays}d ago",
                _ => date.ToString("MMM d, yyyy")
            };
        }

        /// <summary>
        /// Gets the criteria display string
        /// </summary>
        /// <returns>A comma-separated list of criteria names</returns>
        public string GetCriteriaDisplay()
        {
            if (ViewModel?.CriteriaNames == null || !ViewModel.CriteriaNames.Any())
            {
                return "None";
            }

            var criteria = ViewModel.CriteriaNames.ToList();
            return string.Join(", ", criteria);
        }

        /// <summary>
        /// Gets a star rating representation for the given score
        /// </summary>
        /// <param name="score">The score to convert to stars</param>
        /// <returns>A string of star characters representing the rating</returns>
        public string GetStarRating(double score)
        {
            var fullStars = (int)Math.Floor(score);
            var hasHalfStar = score - fullStars >= 0.5;
            
            var stars = new string('★', fullStars);
            if (hasHalfStar && fullStars < 5)
            {
                stars += "☆";
                fullStars++;
            }
            
            stars += new string('☆', 5 - fullStars);
            return stars;
        }

        /// <summary>
        /// Gets the progress text for display
        /// </summary>
        /// <param name="progress">The progress value</param>
        /// <returns>A formatted progress string</returns>
        public string GetProgressText(double? progress)
        {
            if (!progress.HasValue)
            {
                return "Starting...";
            }

            return $"{progress:F0}%";
        }

        /// <summary>
        /// Gets the progress value for the progress bar
        /// </summary>
        /// <param name="progress">The nullable progress value</param>
        /// <returns>The progress value or 0.0 if null</returns>
        public double GetProgressValue(double? progress)
        {
            return progress ?? 0.0;
        }

        /// <summary>
        /// Gets the background brush for the row based on its state
        /// </summary>
        /// <param name="isHovered">Whether the row is hovered</param>
        /// <param name="isSelected">Whether the row is selected</param>
        /// <returns>The appropriate background brush</returns>
        public Brush GetRowBackground(bool isHovered, bool isSelected)
        {
            if (isSelected)
            {
                return new SolidColorBrush(Color.FromArgb(255, 235, 245, 255)); // #EBF5FF
            }
            else if (isHovered)
            {
                return new SolidColorBrush(Color.FromArgb(255, 246, 248, 250)); // #F6F8FA
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)); // Transparent
            }
        }

        /// <summary>
        /// Event for property change notifications
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Accessibility helper methods
        /// <summary>
        /// Gets the accessible name for screen readers
        /// </summary>
        /// <returns>A descriptive name for accessibility</returns>
        public string GetAccessibleName()
        {
            if (ViewModel == null) return "Evaluation item";
            
            var status = ViewModel.Status switch
            {
                EvaluationStatus.Running => "Running",
                EvaluationStatus.Completed => "Completed",
                EvaluationStatus.Failed => "Failed",
                EvaluationStatus.Imported => "Imported",
                _ => "Unknown"
            };
            
            return $"{EvaluationName}, {status}, {ItemCount} items, {ModelName}";
        }

        /// <summary>
        /// Gets the accessible description for screen readers
        /// </summary>
        /// <returns>A detailed description for accessibility</returns>
        public string GetAccessibleDescription()
        {
            if (ViewModel == null) return string.Empty;
            
            var description = $"Evaluation created {GetFormattedDate(Timestamp)}.";
            
            if (IsCompleted)
            {
                description += $" Average score: {AverageScore:F1} out of 5.";
            }
            else if (IsRunning && RunningProgress.HasValue)
            {
                description += $" Progress: {RunningProgress:F0}%.";
            }
            
            description += $" Criteria: {GetCriteriaDisplay()}.";
            description += " Press Space to select, Enter to view details.";
            
            return description;
        }

        public string GetCheckboxAccessibleName()
        {
            return $"Select {EvaluationName}";
        }

        public string GetWorkflowAccessibleName(EvaluationWorkflow workflow)
        {
            return workflow switch
            {
                EvaluationWorkflow.ImportResults => "Imported results workflow",
                EvaluationWorkflow.TestModel => "Test model workflow",
                EvaluationWorkflow.EvaluateResponses => "Evaluate responses workflow",
                _ => "Unknown workflow"
            };
        }

        // Keyboard navigation handler
        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Space:
                    // Toggle selection
                    if (ViewModel != null)
                    {
                        ViewModel.IsSelected = !ViewModel.IsSelected;
                        e.Handled = true;
                    }
                    break;
                    
                case VirtualKey.Enter:
                    // Open details
                    if (ViewModel != null)
                    {
                        ItemDoubleClicked?.Invoke(this, ViewModel);
                        e.Handled = true;
                    }
                    break;
            }
        }
    }
}