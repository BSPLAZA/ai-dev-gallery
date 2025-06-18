// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AIDevGallery.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace AIDevGallery.ViewModels.Evaluate;

/// <summary>
/// ViewModel for evaluation card display
/// </summary>
public partial class EvaluationCardViewModel : ObservableObject
{
    private readonly EvaluationResult _evaluationResult;
    
    public EvaluationCardViewModel(EvaluationResult evaluationResult)
    {
        _evaluationResult = evaluationResult ?? throw new ArgumentNullException(nameof(evaluationResult));
    }
    
    // Basic Properties
    public string Id => _evaluationResult.Id;
    public string Name => _evaluationResult.Name;
    public string ModelName => _evaluationResult.ModelName;
    public string DatasetName => _evaluationResult.DatasetName;
    public string DatasetDescription => $"{_evaluationResult.DatasetName} • {_evaluationResult.DatasetItemCount} items";
    public EvaluationStatus Status => _evaluationResult.Status;
    public EvaluationWorkflow WorkflowType => _evaluationResult.WorkflowType;
    public int Progress => (int)(_evaluationResult.ProgressPercentage ?? (Status == EvaluationStatus.Completed ? 100 : 0));
    
    // Score Display
    public double AverageScore => _evaluationResult.AverageScore;
    public string ScoreDisplay => AverageScore.ToString("F1");
    public string StarRating => GetStarRating(AverageScore);
    public double ProgressPercentage => (AverageScore / 5.0) * 100;
    
    // Criteria Scores
    public Dictionary<string, double> CriteriaScores => _evaluationResult.CriteriaScores;
    public string CriteriaNames => CriteriaScores.Count <= 3 
        ? string.Join(" • ", CriteriaScores.Keys.Take(3))
        : $"{string.Join(" • ", CriteriaScores.Keys.Take(2))} • +{CriteriaScores.Count - 2} more";
    public string CriteriaScoresDisplay => string.Join(" • ", 
        CriteriaScores.Values.Take(3).Select(s => $"{s:F1}/5"));
    
    // Status Display
    public string StatusIcon => Status switch
    {
        EvaluationStatus.Completed => "✅",
        EvaluationStatus.Running => "⏳",
        EvaluationStatus.Failed => "❌",
        EvaluationStatus.Imported => "📥",
        _ => "❓"
    };
    
    public string StatusText => Status switch
    {
        EvaluationStatus.Completed => "Completed",
        EvaluationStatus.Running => _evaluationResult.CurrentOperation ?? "Running",
        EvaluationStatus.Failed => "Failed",
        EvaluationStatus.Imported => "Imported",
        _ => "Unknown"
    };
    
    // Time Display
    public string TimeDisplay => GetRelativeTimeDisplay(_evaluationResult.Timestamp);
    public string DurationDisplay => _evaluationResult.Duration?.ToString(@"mm' min'") ?? "";
    
    // Colors and Gradients
    public string ScoreColorStart => GetScoreColor(AverageScore, true);
    public string ScoreColorEnd => GetScoreColor(AverageScore, false);
    public SolidColorBrush ScoreColorBrush
    {
        get
        {
            var colorString = ScoreColorStart;
            if (colorString.Length >= 7 && colorString.StartsWith("#"))
            {
                try
                {
                    return new SolidColorBrush(ColorHelper.FromArgb(255,
                        Convert.ToByte(colorString.Substring(1, 2), 16),
                        Convert.ToByte(colorString.Substring(3, 2), 16),
                        Convert.ToByte(colorString.Substring(5, 2), 16)));
                }
                catch
                {
                    // Fall back to accent color if parsing fails
                }
            }
            // Default fallback color
            return new SolidColorBrush(ColorHelper.FromArgb(255, 33, 150, 243)); // Material Blue
        }
    }
    
    // Progress Display (for running evaluations)
    public bool ShowProgress => Status == EvaluationStatus.Running && 
                               WorkflowType != EvaluationWorkflow.ImportResults;
    public double? RunningProgress => _evaluationResult.ProgressPercentage;
    
    // Interaction States
    [ObservableProperty]
    private bool _isHovered;
    
    [ObservableProperty]
    private bool _isSelected;
    
    // Helper Methods
    private static string GetStarRating(double score)
    {
        var fullStars = (int)Math.Floor(score);
        var hasHalfStar = score - fullStars >= 0.25 && score - fullStars < 0.75;
        var emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);
        
        var stars = new string('★', fullStars);
        if (hasHalfStar) stars += "☆";
        stars += new string('☆', emptyStars);
        
        return stars;
    }
    
    private static string GetScoreColor(double score, bool isStart)
    {
        return score switch
        {
            >= 4.5 => isStart ? "#4CAF50" : "#66BB6A",    // Green
            >= 3.75 => isStart ? "#2196F3" : "#42A5F5",   // Blue
            >= 3.0 => isStart ? "#FFC107" : "#FFCA28",    // Yellow
            _ => isStart ? "#FF5722" : "#FF7043"          // Orange/Red
        };
    }
    
    private static string GetRelativeTimeDisplay(DateTime timestamp)
    {
        var timeSpan = DateTime.Now - timestamp;
        
        return timeSpan switch
        {
            { TotalMinutes: < 1 } => "Just now",
            { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes}m ago",
            { TotalHours: < 24 } => $"{(int)timeSpan.TotalHours}h ago",
            { TotalDays: < 7 } => $"{(int)timeSpan.TotalDays}d ago",
            { TotalDays: < 30 } => $"{(int)(timeSpan.TotalDays / 7)}w ago",
            _ => timestamp.ToString("MMM d")
        };
    }
}