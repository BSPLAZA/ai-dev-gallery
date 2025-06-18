// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AIDevGallery.ViewModels.Evaluate;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;

namespace AIDevGallery.Controls.Evaluate;

/// <summary>
/// Control for displaying evaluation results in a card format
/// </summary>
public sealed partial class EvaluationCard : UserControl
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(EvaluationCardViewModel), 
            typeof(EvaluationCard), new PropertyMetadata(null, OnViewModelChanged));

    public EvaluationCardViewModel? ViewModel
    {
        get => (EvaluationCardViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public event EventHandler<EvaluationCardViewModel>? ViewClicked;
    public event EventHandler<EvaluationCardViewModel>? DeleteClicked;
    public event EventHandler<EvaluationCardViewModel>? CardClicked;

    private Storyboard? _hoverEnterStoryboard;
    private Storyboard? _hoverExitStoryboard;

    public EvaluationCard()
    {
        this.InitializeComponent();
        SetupAnimations();
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EvaluationCard card && e.NewValue is EvaluationCardViewModel viewModel)
        {
            card.UpdateScoreColors(viewModel.AverageScore);
        }
    }

    private void SetupAnimations()
    {
        // Create hover animations
        _hoverEnterStoryboard = new Storyboard();
        var translateUp = new DoubleAnimation
        {
            To = -4,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(translateUp, CardBorder);
        Storyboard.SetTargetProperty(translateUp, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
        _hoverEnterStoryboard.Children.Add(translateUp);

        var fadeIn = new DoubleAnimation
        {
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };
        Storyboard.SetTarget(fadeIn, ActionButtons);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        _hoverEnterStoryboard.Children.Add(fadeIn);

        _hoverExitStoryboard = new Storyboard();
        var translateDown = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(translateDown, CardBorder);
        Storyboard.SetTargetProperty(translateDown, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
        _hoverExitStoryboard.Children.Add(translateDown);

        var fadeOut = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(100))
        };
        Storyboard.SetTarget(fadeOut, ActionButtons);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");
        _hoverExitStoryboard.Children.Add(fadeOut);
    }

    private void UpdateScoreColors(double score)
    {
        var (startColor, endColor) = GetScoreColors(score);
        GradientStart.Color = startColor;
        GradientEnd.Color = endColor;
    }

    private (Color start, Color end) GetScoreColors(double score)
    {
        return score switch
        {
            >= 4.5 => (ColorHelper.FromArgb(255, 76, 175, 80), ColorHelper.FromArgb(255, 102, 187, 106)),    // Green
            >= 3.75 => (ColorHelper.FromArgb(255, 33, 150, 243), ColorHelper.FromArgb(255, 66, 165, 245)),   // Blue
            >= 3.0 => (ColorHelper.FromArgb(255, 255, 193, 7), ColorHelper.FromArgb(255, 255, 202, 40)),     // Yellow
            _ => (ColorHelper.FromArgb(255, 255, 87, 34), ColorHelper.FromArgb(255, 255, 112, 67))           // Orange
        };
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsHovered = true;
            _hoverEnterStoryboard?.Begin();
            
            // Enhance shadow
            CardBorder.Translation = new System.Numerics.Vector3(0, 0, 8);
        }
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.IsHovered = false;
            _hoverExitStoryboard?.Begin();
            
            // Reset shadow
            CardBorder.Translation = new System.Numerics.Vector3(0, 0, 0);
        }
    }

    private void OnCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            CardClicked?.Invoke(this, ViewModel);
        }
    }

    private void OnViewClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewClicked?.Invoke(this, ViewModel);
        }
        e.Handled = true;
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            DeleteClicked?.Invoke(this, ViewModel);
        }
        e.Handled = true;
    }

    public string FormatCriteriaLabel(string criteria)
    {
        return string.IsNullOrEmpty(criteria) ? "" : $"Criteria: {criteria}";
    }

    public string FormatScoresLabel(string scores)
    {
        return string.IsNullOrEmpty(scores) ? "" : $"Scores:   {scores}";
    }

    public bool IsProgressIndeterminate(double? progress)
    {
        return !progress.HasValue || progress.Value == 0;
    }
}