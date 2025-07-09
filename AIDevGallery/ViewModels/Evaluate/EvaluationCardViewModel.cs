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
    
    /// <summary>
    /// Initializes a new instance of the EvaluationCardViewModel class
    /// </summary>
    /// <param name="evaluationResult">The evaluation result to display</param>
    public EvaluationCardViewModel(EvaluationResult evaluationResult)
    {
        _evaluationResult = evaluationResult ?? throw new ArgumentNullException(nameof(evaluationResult));
    }
    
    // Basic Properties
    /// <summary>
    /// Gets the unique identifier of the evaluation
    /// </summary>
    public string Id => _evaluationResult.Id;
    
    /// <summary>
    /// Gets the name of the evaluation
    /// </summary>
    public string Name => _evaluationResult.Name;
    
    /// <summary>
    /// Gets the name of the model that was evaluated
    /// </summary>
    public string ModelName => _evaluationResult.ModelName;
    
    /// <summary>
    /// Gets the name of the dataset used for evaluation
    /// </summary>
    public string DatasetName => _evaluationResult.DatasetName;
    
    /// <summary>
    /// Gets a formatted description of the dataset including item count
    /// </summary>
    public string DatasetDescription => $"{_evaluationResult.DatasetName} • {_evaluationResult.DatasetItemCount} items";
    
    /// <summary>
    /// Gets the current status of the evaluation
    /// </summary>
    public EvaluationStatus Status => _evaluationResult.Status;
    
    /// <summary>
    /// Gets the type of evaluation workflow used
    /// </summary>
    public EvaluationWorkflow WorkflowType => _evaluationResult.WorkflowType;
    
    /// <summary>
    /// Gets the progress percentage as an integer
    /// </summary>
    public int Progress => (int)(_evaluationResult.ProgressPercentage ?? (Status == EvaluationStatus.Completed ? 100 : 0));
    
    // Score Display
    /// <summary>
    /// Gets the average score of the evaluation
    /// </summary>
    public double AverageScore => _evaluationResult.AverageScore;
    
    /// <summary>
    /// Gets the formatted average score for display
    /// </summary>
    public string ScoreDisplay => AverageScore.ToString("F1");
    
    /// <summary>
    /// Gets the star rating representation of the score
    /// </summary>
    public string StarRating => GetStarRating(AverageScore);
    
    /// <summary>
    /// Gets the progress percentage based on the score
    /// </summary>
    public double ProgressPercentage => (AverageScore / 5.0) * 100;
    
    // Criteria Scores
    /// <summary>
    /// Gets the criteria scores dictionary
    /// </summary>
    public Dictionary<string, double> CriteriaScores => _evaluationResult.CriteriaScores;
    
    /// <summary>
    /// Gets a formatted string of criteria names for display
    /// </summary>
    public string CriteriaNames => CriteriaScores.Count <= 3 
        ? string.Join(" • ", CriteriaScores.Keys.Take(3))
        : $"{string.Join(" • ", CriteriaScores.Keys.Take(2))} • +{CriteriaScores.Count - 2} more";
    
    /// <summary>
    /// Gets a formatted string of criteria scores for display
    /// </summary>
    public string CriteriaScoresDisplay => string.Join(" • ", 
        CriteriaScores.Values.Take(3).Select(s => $"{s:F1}/5"));
    
    // Status Display
    /// <summary>
    /// Gets the icon representation of the evaluation status
    /// </summary>
    public string StatusIcon => Status switch
    {
        EvaluationStatus.Completed => "✅",
        EvaluationStatus.Running => "⏳",
        EvaluationStatus.Failed => "❌",
        EvaluationStatus.Imported => "📥",
        _ => "❓"
    };
    
    /// <summary>
    /// Gets the text representation of the evaluation status
    /// </summary>
    public string StatusText => Status switch
    {
        EvaluationStatus.Completed => "Completed",
        EvaluationStatus.Running => _evaluationResult.CurrentOperation ?? "Running",
        EvaluationStatus.Failed => "Failed",
        EvaluationStatus.Imported => "Imported",
        _ => "Unknown"
    };
    
    // Time Display
    /// <summary>
    /// Gets the relative time display for when the evaluation was performed
    /// </summary>
    public string TimeDisplay => GetRelativeTimeDisplay(_evaluationResult.Timestamp);
    
    /// <summary>
    /// Gets the formatted duration of the evaluation
    /// </summary>
    public string DurationDisplay => _evaluationResult.Duration?.ToString(@"mm' min'") ?? "";
    
    // Colors and Gradients
    /// <summary>
    /// Gets the start color for score visualization gradients
    /// </summary>
    public string ScoreColorStart => GetScoreColor(AverageScore, true);
    
    /// <summary>
    /// Gets the end color for score visualization gradients
    /// </summary>
    public string ScoreColorEnd => GetScoreColor(AverageScore, false);
    
    /// <summary>
    /// Gets a solid color brush for the score color
    /// </summary>
    public SolidColorBrush ScoreColorBrush
    {
        get
        {
            var colorString = ScoreColorStart;
            if (colorString.Length >= 7 && colorString.StartsWith("#"))
            {
                try
                {
                    return new SolidColorBrush(Color.FromArgb(255,
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
            return new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)); // Material Blue
        }
    }
    
    // Progress Display (for running evaluations)
    /// <summary>
    /// Gets a value indicating whether progress should be shown
    /// </summary>
    public bool ShowProgress => Status == EvaluationStatus.Running && 
                               WorkflowType != EvaluationWorkflow.ImportResults;
    
    /// <summary>
    /// Gets the current running progress percentage
    /// </summary>
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