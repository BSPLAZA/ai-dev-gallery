// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace AIDevGallery.Controls.Evaluate;

/// <summary>
/// Empty state control for when no evaluations exist
/// </summary>
public sealed partial class EvaluationEmptyState : UserControl
{
    public event EventHandler? ImportResultsClicked;
    public event EventHandler? TestModelClicked;
    public event EventHandler? LearnMoreClicked;

    public EvaluationEmptyState()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Start the gradient animation
        var storyboard = Resources["GradientAnimation"] as Storyboard;
        storyboard?.Begin();
    }

    private void OnImportResultsClick(object sender, RoutedEventArgs e)
    {
        ImportResultsClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnTestModelClick(object sender, RoutedEventArgs e)
    {
        TestModelClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnLearnMoreClick(object sender, RoutedEventArgs e)
    {
        LearnMoreClicked?.Invoke(this, EventArgs.Empty);
    }
}