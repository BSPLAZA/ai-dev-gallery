// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AIDevGallery.Controls.Evaluate
{
    public sealed partial class SelectionActionBar : UserControl
    {
        public static readonly DependencyProperty SelectedCountProperty =
            DependencyProperty.Register(nameof(SelectedCount), typeof(int), typeof(SelectionActionBar), 
                new PropertyMetadata(0, OnSelectedCountChanged));

        public int SelectedCount
        {
            get => (int)GetValue(SelectedCountProperty);
            set => SetValue(SelectedCountProperty, value);
        }

        public event EventHandler? CompareClicked;
        public event EventHandler? DeleteClicked;
        public event EventHandler? CancelClicked;

        private bool _isVisible;

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

        public void Show()
        {
            if (_isVisible) return;

            _isVisible = true;
            this.Visibility = Visibility.Visible;
            ShowAnimation.Begin();
        }

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
        public string GetSelectionText(int count)
        {
            return count == 1 ? " evaluation selected" : " evaluations selected";
        }

        public bool CanCompare(int count)
        {
            // Need at least 2 items to compare
            return count >= 2;
        }
    }
}