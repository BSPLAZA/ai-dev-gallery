// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Pages.Evaluate;
using System.Collections.Generic;

namespace AIDevGallery.Models
{
    /// <summary>
    /// Maintains the complete state of the evaluation wizard across navigation
    /// </summary>
    internal class EvaluationWizardState
    {
        // Step 1: Evaluation Type
        public EvaluationType? EvaluationType { get; set; }
        
        // Step 2: Workflow Selection
        public EvaluationWorkflow? Workflow { get; set; }
        
        // Step 3: Model Configuration (TestModel workflow only)
        public ModelConfigurationData? ModelConfig { get; set; }
        
        // Step 4: Dataset
        public DatasetConfiguration? Dataset { get; set; }
        
        // Step 5: Metrics (not for ImportResults workflow)
        public EvaluationMetrics? Metrics { get; set; }
        
        // Navigation state
        public int CurrentStep { get; set; } = 1;
        public Stack<int> NavigationHistory { get; set; } = new Stack<int>();
        
        // Workflow-specific total steps
        public int TotalSteps
        {
            get
            {
                return Workflow switch
                {
                    EvaluationWorkflow.TestModel => 6,          // All steps
                    EvaluationWorkflow.EvaluateResponses => 5,  // Skip model config
                    EvaluationWorkflow.ImportResults => 4,      // Skip model config and metrics
                    _ => 6
                };
            }
        }
        
        /// <summary>
        /// Checks if all required data is present for the current workflow
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (!EvaluationType.HasValue || !Workflow.HasValue || Dataset == null)
                    return false;
                
                return Workflow switch
                {
                    EvaluationWorkflow.TestModel => ModelConfig != null && Metrics != null,
                    EvaluationWorkflow.EvaluateResponses => Metrics != null,
                    EvaluationWorkflow.ImportResults => true,
                    _ => false
                };
            }
        }
        
        /// <summary>
        /// Resets state to start over
        /// </summary>
        public void Reset()
        {
            EvaluationType = null;
            Workflow = null;
            ModelConfig = null;
            Dataset = null;
            Metrics = null;
            CurrentStep = 1;
            NavigationHistory.Clear();
        }
        
        /// <summary>
        /// Creates a copy of the current state for navigation
        /// </summary>
        public EvaluationWizardState Clone()
        {
            return new EvaluationWizardState
            {
                EvaluationType = this.EvaluationType,
                Workflow = this.Workflow,
                ModelConfig = this.ModelConfig,
                Dataset = this.Dataset,
                Metrics = this.Metrics,
                CurrentStep = this.CurrentStep,
                NavigationHistory = new Stack<int>(new Stack<int>(this.NavigationHistory))
            };
        }
    }
}