// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AIDevGallery.Models;

namespace AIDevGallery.ViewModels
{
    /// <summary>
    /// View model for the evaluation insights page.
    /// </summary>
    public class EvaluationInsightsViewModel
    {
        private readonly EvaluationResult _evaluation;
        
        public EvaluationInsightsViewModel(EvaluationResult evaluation)
        {
            _evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
        }
        
        /// <summary>
        /// Gets the evaluation ID.
        /// </summary>
        public string Id => _evaluation.Id;
        
        /// <summary>
        /// Gets the evaluation name.
        /// </summary>
        public string Name => _evaluation.Name;
        
        /// <summary>
        /// Gets the model name.
        /// </summary>
        public string ModelName => _evaluation.ModelName;
        
        /// <summary>
        /// Gets the dataset name.
        /// </summary>
        public string DatasetName => _evaluation.DatasetName;
        
        /// <summary>
        /// Gets the number of items in the dataset.
        /// </summary>
        public int ItemCount => _evaluation.DatasetItemCount;
        
        /// <summary>
        /// Gets the evaluation status.
        /// </summary>
        public EvaluationStatus Status => _evaluation.Status;
        
        /// <summary>
        /// Gets the evaluation timestamp.
        /// </summary>
        public DateTime Timestamp => _evaluation.Timestamp;
        
        /// <summary>
        /// Gets the evaluation duration.
        /// </summary>
        public TimeSpan? Duration => _evaluation.Duration;
        
        /// <summary>
        /// Gets the progress percentage for running evaluations.
        /// </summary>
        public double? ProgressPercentage => _evaluation.ProgressPercentage;
        
        /// <summary>
        /// Gets the criteria scores.
        /// </summary>
        public Dictionary<string, double> CriteriaScores => _evaluation.CriteriaScores;
        
        /// <summary>
        /// Gets the average score across all criteria.
        /// </summary>
        public double AverageScore => _evaluation.AverageScore;
        
        /// <summary>
        /// Gets the score rating category.
        /// </summary>
        public ScoreRating Rating => _evaluation.Rating;
        
        /// <summary>
        /// Gets the star rating display string.
        /// </summary>
        public string StarRating => GetStarRating(AverageScore);
        
        /// <summary>
        /// Gets a value indicating whether detailed results are available.
        /// </summary>
        public bool HasDetailedResults => _evaluation.HasDetailedResults;
        
        /// <summary>
        /// Gets a value indicating whether folder statistics are available.
        /// </summary>
        public bool HasFolderStatistics => _evaluation.HasFolderStatistics;
        
        /// <summary>
        /// Gets the individual item results if available.
        /// </summary>
        public List<EvaluationItemResult>? ItemResults => _evaluation.ItemResults;
        
        /// <summary>
        /// Gets the folder statistics if available.
        /// </summary>
        public Dictionary<string, FolderStats>? FolderStatistics => _evaluation.FolderStatistics;
        
        /// <summary>
        /// Gets the workflow type.
        /// </summary>
        public EvaluationWorkflow WorkflowType => _evaluation.WorkflowType;
        
        /// <summary>
        /// Gets the source file path for imported evaluations.
        /// </summary>
        public string? SourceFilePath => _evaluation.SourceFilePath;
        
        /// <summary>
        /// Gets the formatted duration string.
        /// </summary>
        public string DurationText
        {
            get
            {
                if (!Duration.HasValue)
                    return "N/A";
                
                var duration = Duration.Value;
                if (duration.TotalHours >= 1)
                    return $"{(int)duration.TotalHours}h {duration.Minutes}m";
                else if (duration.TotalMinutes >= 1)
                    return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";
                else
                    return $"{duration.Seconds}s";
            }
        }
        
        /// <summary>
        /// Gets statistical summary if individual results are available.
        /// </summary>
        public StatisticalSummary? GetStatisticalSummary()
        {
            if (!HasDetailedResults || ItemResults == null || ItemResults.Count == 0)
                return null;
            
            var allScores = ItemResults
                .Where(r => r.IsSuccess)
                .Select(r => r.AverageScore)
                .ToList();
            
            if (allScores.Count == 0)
                return null;
            
            return new StatisticalSummary
            {
                Mean = Math.Round(allScores.Average(), 2),
                Median = Math.Round(GetMedian(allScores), 2),
                StandardDeviation = Math.Round(GetStandardDeviation(allScores), 2),
                Min = Math.Round(allScores.Min(), 2),
                Max = Math.Round(allScores.Max(), 2),
                Count = allScores.Count
            };
        }
        
        /// <summary>
        /// Gets statistical summary for a specific criterion.
        /// </summary>
        public StatisticalSummary? GetCriterionStatistics(string criterionName)
        {
            if (!HasDetailedResults || ItemResults == null || ItemResults.Count == 0)
                return null;
            
            var scores = ItemResults
                .Where(r => r.IsSuccess && r.CriteriaScores.ContainsKey(criterionName))
                .Select(r => r.CriteriaScores[criterionName])
                .ToList();
            
            if (scores.Count == 0)
                return null;
            
            return new StatisticalSummary
            {
                Mean = Math.Round(scores.Average(), 2),
                Median = Math.Round(GetMedian(scores), 2),
                StandardDeviation = Math.Round(GetStandardDeviation(scores), 2),
                Min = Math.Round(scores.Min(), 2),
                Max = Math.Round(scores.Max(), 2),
                Count = scores.Count
            };
        }
        
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
        
        private static double GetMedian(List<double> values)
        {
            if (values.Count == 0)
                return 0;
            
            var sorted = values.OrderBy(v => v).ToList();
            var mid = sorted.Count / 2;
            
            if (sorted.Count % 2 == 0)
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            else
                return sorted[mid];
        }
        
        private static double GetStandardDeviation(List<double> values)
        {
            if (values.Count <= 1)
                return 0;
            
            var mean = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }
    }
    
    /// <summary>
    /// Statistical summary for scores.
    /// </summary>
    public class StatisticalSummary
    {
        public double Mean { get; set; }
        public double Median { get; set; }
        public double StandardDeviation { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int Count { get; set; }
    }
}