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
using Windows.UI;

namespace AIDevGallery.Controls.Evaluate
{
    public sealed partial class EvaluationListRow : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(EvaluationListItemViewModel), typeof(EvaluationListRow), new PropertyMetadata(null, OnViewModelChanged));

        public EvaluationListItemViewModel ViewModel
        {
            get => (EvaluationListItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // Bindable properties from ViewModel
        public string EvaluationName => ViewModel?.Name ?? string.Empty;
        public string ModelName => ViewModel?.ModelName ?? string.Empty;
        public string DatasetName => ViewModel?.DatasetName ?? string.Empty;
        public int ItemCount => ViewModel?.ItemCount ?? 0;
        public DateTime Timestamp => ViewModel?.Timestamp ?? DateTime.Now;
        public double AverageScore => ViewModel?.AverageScore ?? 0.0;
        public EvaluationWorkflow WorkflowType => ViewModel?.WorkflowType ?? EvaluationWorkflow.ImportResults;
        public bool IsCompleted => ViewModel?.Status == EvaluationStatus.Completed || ViewModel?.Status == EvaluationStatus.Imported;
        public bool IsRunning => ViewModel?.Status == EvaluationStatus.Running;
        public double? RunningProgress => ViewModel?.RunningProgress;
        
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
        public bool IsHovered
        {
            get => _isHovered;
            private set
            {
                _isHovered = value;
                UpdateVisualState();
            }
        }

        public event EventHandler<EvaluationListItemViewModel>? ItemClicked;
        public event EventHandler<EvaluationListItemViewModel>? ItemDoubleClicked;

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
        public string GetWorkflowIcon(EvaluationWorkflow workflow)
        {
            return workflow switch
            {
                EvaluationWorkflow.ImportResults => "📥",
                _ => "🧪"
            };
        }

        public string GetWorkflowGlyph(EvaluationWorkflow workflow)
        {
            return workflow switch
            {
                EvaluationWorkflow.ImportResults => "\uE118", // Download icon
                _ => "\uEA80" // Test/Science icon
            };
        }

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

        public string GetCriteriaDisplay()
        {
            if (ViewModel?.CriteriaNames == null || !ViewModel.CriteriaNames.Any())
            {
                return "None";
            }

            var criteria = ViewModel.CriteriaNames.ToList();
            return string.Join(", ", criteria);
        }

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

        public string GetProgressText(double? progress)
        {
            if (!progress.HasValue)
            {
                return "Starting...";
            }

            return $"{progress:F0}%";
        }

        public double GetProgressValue(double? progress)
        {
            return progress ?? 0.0;
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}