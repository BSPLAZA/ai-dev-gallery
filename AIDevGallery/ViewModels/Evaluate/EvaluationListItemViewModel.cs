// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIDevGallery.ViewModels.Evaluate
{
    public partial class EvaluationListItemViewModel : ObservableObject
    {
        private readonly EvaluationResult _evaluation;

        [ObservableProperty]
        private bool _isSelected;

        public EvaluationListItemViewModel(EvaluationResult evaluation)
        {
            _evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
        }

        // Properties from the evaluation model
        public string Id => _evaluation.Id;
        public string Name => _evaluation.Name;
        public string ModelName => _evaluation.ModelName;
        public string DatasetName => _evaluation.DatasetName;
        public int ItemCount => _evaluation.ItemCount;
        public DateTime Timestamp => _evaluation.Timestamp;
        public double AverageScore => _evaluation.AverageScore;
        public EvaluationStatus Status => _evaluation.Status;
        public EvaluationWorkflow WorkflowType => _evaluation.WorkflowType;
        public double? RunningProgress => _evaluation.RunningProgress;

        // Computed properties for display
        public IEnumerable<string> CriteriaNames => _evaluation.CriteriaScores?.Keys ?? Enumerable.Empty<string>();

        public bool IsCompleted => Status == EvaluationStatus.Completed || Status == EvaluationStatus.Imported;
        public bool IsRunning => Status == EvaluationStatus.Running;

        // Update progress for running evaluations
        public void UpdateProgress(double progress)
        {
            if (_evaluation.Status == EvaluationStatus.Running)
            {
                _evaluation.RunningProgress = progress;
                OnPropertyChanged(nameof(RunningProgress));
            }
        }

        // Update status
        public void UpdateStatus(EvaluationStatus newStatus)
        {
            _evaluation.Status = newStatus;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsRunning));
        }
    }
}