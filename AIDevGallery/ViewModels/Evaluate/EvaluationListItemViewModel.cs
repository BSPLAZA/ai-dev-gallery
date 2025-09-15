// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIDevGallery.ViewModels.Evaluate
{
    /// <summary>
    /// ViewModel for evaluation list item display
    /// </summary>
    public partial class EvaluationListItemViewModel : ObservableObject
    {
        private readonly EvaluationResult _evaluation;

        [ObservableProperty]
        private bool _isSelected;

        /// <summary>
        /// Initializes a new instance of the EvaluationListItemViewModel class
        /// </summary>
        /// <param name="evaluation">The evaluation result to display</param>
        public EvaluationListItemViewModel(EvaluationResult evaluation)
        {
            _evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
        }

        // Properties from the evaluation model
        /// <summary>
        /// Gets the unique identifier of the evaluation
        /// </summary>
        public string Id => _evaluation.Id;
        
        /// <summary>
        /// Gets the name of the evaluation
        /// </summary>
        public string Name => _evaluation.Name;
        
        /// <summary>
        /// Gets the name of the model that was evaluated
        /// </summary>
        public string ModelName => _evaluation.ModelName;
        
        /// <summary>
        /// Gets the name of the dataset used for evaluation
        /// </summary>
        public string DatasetName => _evaluation.DatasetName;
        
        /// <summary>
        /// Gets the number of items in the evaluation
        /// </summary>
        public int ItemCount => _evaluation.ItemCount;
        
        /// <summary>
        /// Gets the timestamp when the evaluation was performed
        /// </summary>
        public DateTime Timestamp => _evaluation.Timestamp;
        
        /// <summary>
        /// Gets the average score of the evaluation
        /// </summary>
        public double AverageScore => _evaluation.AverageScore;
        
        /// <summary>
        /// Gets the current status of the evaluation
        /// </summary>
        public EvaluationStatus Status => _evaluation.Status;
        
        /// <summary>
        /// Gets the type of evaluation workflow used
        /// </summary>
        public EvaluationWorkflow WorkflowType => _evaluation.WorkflowType;
        
        /// <summary>
        /// Gets the current running progress percentage
        /// </summary>
        public double? RunningProgress => _evaluation.RunningProgress;

        // Computed properties for display
        /// <summary>
        /// Gets the criteria names used in the evaluation
        /// </summary>
        public IEnumerable<string> CriteriaNames => _evaluation.CriteriaScores?.Keys ?? Enumerable.Empty<string>();

        /// <summary>
        /// Gets a value indicating whether the evaluation is completed
        /// </summary>
        public bool IsCompleted => Status == EvaluationStatus.Completed || Status == EvaluationStatus.Imported;
        
        /// <summary>
        /// Gets a value indicating whether the evaluation is currently running
        /// </summary>
        public bool IsRunning => Status == EvaluationStatus.Running;

        // Update progress for running evaluations
        /// <summary>
        /// Updates the progress of a running evaluation
        /// </summary>
        /// <param name="progress">The new progress value</param>
        public void UpdateProgress(double progress)
        {
            if (_evaluation.Status == EvaluationStatus.Running)
            {
                _evaluation.ProgressPercentage = progress;
                OnPropertyChanged(nameof(RunningProgress));
            }
        }

        // Update status
        /// <summary>
        /// Updates the status of the evaluation
        /// </summary>
        /// <param name="newStatus">The new status</param>
        public void UpdateStatus(EvaluationStatus newStatus)
        {
            _evaluation.Status = newStatus;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsRunning));
        }
    }
}