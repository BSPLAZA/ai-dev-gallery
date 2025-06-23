// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AIDevGallery.Models
{
    /// <summary>
    /// Represents the result of evaluating a single item (e.g., an image) in an evaluation run.
    /// </summary>
    public class EvaluationItemResult
    {
        /// <summary>
        /// Gets or sets the unique identifier for this item result.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the full path to the image file.
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relative path that preserves folder structure.
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the prompt used for this evaluation item.
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model's response for this item.
        /// </summary>
        public string ModelResponse { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the criteria scores for this item.
        /// Key: criterion name, Value: score value.
        /// </summary>
        public Dictionary<string, double> CriteriaScores { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Gets or sets the processing time for this item.
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// Gets or sets the error message if evaluation failed for this item.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets custom metadata fields from JSONL that don't map to standard properties.
        /// </summary>
        public Dictionary<string, object>? CustomMetadata { get; set; }

        /// <summary>
        /// Gets a value indicating whether this item has custom metadata.
        /// </summary>
        public bool HasCustomMetadata => CustomMetadata?.Count > 0;

        /// <summary>
        /// Gets the average score across all criteria.
        /// </summary>
        public double AverageScore
        {
            get
            {
                if (CriteriaScores == null || CriteriaScores.Count == 0)
                    return 0;

                double sum = 0;
                foreach (var score in CriteriaScores.Values)
                {
                    sum += score;
                }
                return sum / CriteriaScores.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the evaluation succeeded for this item.
        /// </summary>
        public bool IsSuccess => string.IsNullOrEmpty(Error);

        /// <summary>
        /// Gets the file name from the image path.
        /// </summary>
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePath))
                    return string.Empty;

                return System.IO.Path.GetFileName(ImagePath);
            }
        }

        /// <summary>
        /// Gets the folder path from the relative path.
        /// </summary>
        public string FolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(RelativePath))
                    return string.Empty;

                var dir = System.IO.Path.GetDirectoryName(RelativePath);
                return string.IsNullOrEmpty(dir) ? "/" : dir.Replace('\\', '/');
            }
        }
    }
}