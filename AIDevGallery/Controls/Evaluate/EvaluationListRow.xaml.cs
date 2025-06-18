// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace AIDevGallery.Controls.Evaluate
{
    public sealed partial class EvaluationListRow : UserControl
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
        public event EventHandler<EvaluationListItemViewModel>? SelectionChanged;

        public EvaluationListRow()
        {
            this.InitializeComponent();
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EvaluationListRow row)
            {
                row.UpdateBindings();
            }
        }

        private void UpdateBindings()
        {
            Bindings.Update();
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
            // Don't toggle selection if checkbox was clicked
            if (e.OriginalSource is CheckBox)
            {
                return;
            }

            IsSelected = !IsSelected;
            SelectionChanged?.Invoke(this, ViewModel);
            ItemClicked?.Invoke(this, ViewModel);
        }

        private void OnRowDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ItemDoubleClicked?.Invoke(this, ViewModel);
        }

        private void OnCheckboxClick(object sender, RoutedEventArgs e)
        {
            SelectionChanged?.Invoke(this, ViewModel);
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
                return "No criteria";
            }

            var criteria = ViewModel.CriteriaNames.ToList();
            return $"Criteria: {string.Join(", ", criteria)}";
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

        public Brush GetRowBackground(bool isHovered, bool isSelected)
        {
            if (isSelected)
            {
                return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 235, 245, 255)); // #EBF5FF
            }
            else if (isHovered)
            {
                return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 246, 248, 250)); // #F6F8FA
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0)); // Transparent
            }
        }
    }
}