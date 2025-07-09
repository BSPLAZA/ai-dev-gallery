// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AIDevGallery.Controls.Evaluate
{
    /// <summary>
    /// Action bar control for handling selection operations in evaluation lists
    /// </summary>
    public sealed partial class SelectionActionBar : UserControl
    {
        /// <summary>
        /// Dependency property for the selected count
        /// </summary>
        public static readonly DependencyProperty SelectedCountProperty =
            DependencyProperty.Register(nameof(SelectedCount), typeof(int), typeof(SelectionActionBar), 
                new PropertyMetadata(0, OnSelectedCountChanged));

        /// <summary>
        /// Gets or sets the number of selected items
        /// </summary>
        public int SelectedCount
        {
            get => (int)GetValue(SelectedCountProperty);
            set => SetValue(SelectedCountProperty, value);
        }

        /// <summary>
        /// Event raised when the Compare button is clicked
        /// </summary>
        public event EventHandler? CompareClicked;
        
        /// <summary>
        /// Event raised when the Delete button is clicked
        /// </summary>
        public event EventHandler? DeleteClicked;
        
        /// <summary>
        /// Event raised when the Cancel button is clicked
        /// </summary>
        public event EventHandler? CancelClicked;

        private bool _isVisible;

        /// <summary>
        /// Initializes a new instance of the SelectionActionBar class
        /// </summary>
        public SelectionActionBar()
        {
            this.InitializeComponent();
            this.Visibility = Visibility.Collapsed;
        }

        private static void OnSelectedCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SelectionActionBar actionBar)
            {
                actionBar.UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            var shouldBeVisible = SelectedCount > 0;

            if (shouldBeVisible && !_isVisible)
            {
                Show();
            }
            else if (!shouldBeVisible && _isVisible)
            {
                Hide();
            }
        }

        /// <summary>
        /// Shows the selection action bar with animation
        /// </summary>
        public void Show()
        {
            if (_isVisible) return;

            _isVisible = true;
            this.Visibility = Visibility.Visible;
            ShowAnimation.Begin();
        }

        /// <summary>
        /// Hides the selection action bar with animation
        /// </summary>
        public void Hide()
        {
            if (!_isVisible) return;

            _isVisible = false;
            HideAnimation.Completed += OnHideCompleted;
            HideAnimation.Begin();
        }

        private void OnHideCompleted(object? sender, object e)
        {
            HideAnimation.Completed -= OnHideCompleted;
            this.Visibility = Visibility.Collapsed;
        }

        private void OnCompareClick(object sender, RoutedEventArgs e)
        {
            CompareClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, EventArgs.Empty);
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }

        // Helper methods for x:Bind
        /// <summary>
        /// Gets the appropriate text for the selection count
        /// </summary>
        /// <param name="count">Number of selected items</param>
        /// <returns>Formatted selection text</returns>
        public string GetSelectionText(int count)
        {
            return count == 1 ? " evaluation selected" : " evaluations selected";
        }

        /// <summary>
        /// Determines if compare functionality should be enabled
        /// </summary>
        /// <param name="count">Number of selected items</param>
        /// <returns>True if comparison is possible, false otherwise</returns>
        public bool CanCompare(int count)
        {
            // Need at least 2 items to compare
            return count >= 2;
        }
    }
}