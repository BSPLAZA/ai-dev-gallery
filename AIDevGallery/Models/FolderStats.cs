// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AIDevGallery.Models
{
    /// <summary>
    /// Represents statistics for a folder of evaluated items.
    /// </summary>
    public class FolderStats
    {
        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        public string FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of items in this folder.
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Gets or sets the average scores for each criterion in this folder.
        /// Key: criterion name, Value: average score.
        /// </summary>
        public Dictionary<string, double> AverageScores { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the success rate (percentage of items without errors).
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets the overall average score across all criteria.
        /// </summary>
        public double OverallAverageScore
        {
            get
            {
                if (AverageScores == null || AverageScores.Count == 0)
                    return 0;

                double sum = 0;
                foreach (var score in AverageScores.Values)
                {
                    sum += score;
                }
                return sum / AverageScores.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this folder's performance is above average.
        /// </summary>
        /// <param name="overallAverage">The overall average to compare against.</param>
        /// <returns>True if this folder's average is above the overall average.</returns>
        public bool IsAboveAverage(double overallAverage)
        {
            return OverallAverageScore > overallAverage;
        }

        /// <summary>
        /// Gets the folder name (last part of the path).
        /// </summary>
        public string FolderName
        {
            get
            {
                if (string.IsNullOrEmpty(FolderPath))
                    return string.Empty;

                if (FolderPath == "/" || FolderPath == "\\")
                    return "Root";

                var parts = FolderPath.Split(new[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[parts.Length - 1] : FolderPath;
            }
        }

        /// <summary>
        /// Gets the parent folder path.
        /// </summary>
        public string ParentPath
        {
            get
            {
                if (string.IsNullOrEmpty(FolderPath) || FolderPath == "/" || FolderPath == "\\")
                    return string.Empty;

                var lastSeparator = FolderPath.LastIndexOfAny(new[] { '/', '\\' });
                if (lastSeparator <= 0)
                    return "/";

                return FolderPath.Substring(0, lastSeparator);
            }
        }
    }
}