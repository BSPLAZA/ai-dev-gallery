// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace AIDevGallery.Pages.Evaluate
{
    /// <summary>
    /// Step 6: Review Configuration
    /// Final review page showing all evaluation settings before execution.
    /// </summary>
    public sealed partial class ReviewConfigurationPage : Page
    {
        // Configuration data
        private EvaluationType _evaluationType;
        private EvaluationWorkflow _workflow;
        private ModelConfigurationData? _modelConfig;
        private DatasetConfiguration? _dataset;
        private EvaluationMetrics? _metrics;
        
        // For navigation back to specific steps
        private Frame? _wizardFrame;
        
        public ReviewConfigurationPage()
        {
            this.InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check if we have state from navigation
            if (e.Parameter is EvaluationWizardState state)
            {
                _evaluationType = state.EvaluationType ?? EvaluationType.ImageDescription;
                _workflow = state.Workflow ?? EvaluationWorkflow.TestModel;
                _modelConfig = state.ModelConfig;
                _dataset = state.Dataset;
                _metrics = state.Metrics;
                
                // Update display with state data
                UpdateDisplay();
            }
        }
        
        /// <summary>
        /// Sets all configuration data from the wizard
        /// </summary>
        internal void SetConfigurationData(
            EvaluationType evaluationType,
            EvaluationWorkflow workflow,
            ModelConfigurationData? modelConfig,
            DatasetConfiguration? dataset,
            EvaluationMetrics? metrics,
            Frame wizardFrame)
        {
            _evaluationType = evaluationType;
            _workflow = workflow;
            _modelConfig = modelConfig;
            _dataset = dataset;
            _metrics = metrics;
            _wizardFrame = wizardFrame;
            
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            // Update Evaluation Overview
            EvaluationTypeText.Text = GetEvaluationTypeDisplayName(_evaluationType);
            WorkflowText.Text = $"Workflow: {GetWorkflowDisplayName(_workflow)}";
            
            // Update Model Configuration (TestModel only)
            if (_workflow == EvaluationWorkflow.TestModel && _modelConfig != null)
            {
                ModelConfigSection.Visibility = Visibility.Visible;
                ProviderText.Text = GetProviderName(_modelConfig.ApiEndpoint);
                ModelText.Text = _modelConfig.SelectedModelName;
                ApiKeyText.Text = MaskApiKey(_modelConfig.ApiEndpoint);
                PromptText.Text = _modelConfig.BaselinePrompt;
            }
            else
            {
                ModelConfigSection.Visibility = Visibility.Collapsed;
            }
            
            // Update Dataset
            if (_dataset != null)
            {
                DatasetNameText.Text = System.IO.Path.GetFileName(_dataset.SourcePath);
                DatasetCountText.Text = $"{_dataset.ValidEntries} valid {GetDatasetItemType()}";
                
                // Show folder organization if applicable
                if (_dataset.FolderStructure != null && _dataset.FolderStructure.Count > 1)
                {
                    FolderOrgPanel.Visibility = Visibility.Visible;
                    var folders = _dataset.FolderStructure
                        .OrderBy(f => f.Key)
                        .Select(f => $"• {f.Key}/ ({f.Value} images)")
                        .ToList();
                    FoldersList.ItemsSource = folders;
                }
                else
                {
                    FolderOrgPanel.Visibility = Visibility.Collapsed;
                }
            }
            
            // Update Evaluation Methods (not for ImportResults)
            if (_workflow != EvaluationWorkflow.ImportResults && _metrics != null)
            {
                MetricsSection.Visibility = Visibility.Visible;
                UpdateMetricsDisplay();
            }
            else
            {
                MetricsSection.Visibility = Visibility.Collapsed;
            }
            
            // Update time estimate
            UpdateTimeEstimate();
        }
        
        private void UpdateMetricsDisplay()
        {
            if (_metrics == null) return;
            
            var automatedMetrics = new List<MetricDisplayItem>();
            
            if (_metrics.UseSpice)
                automatedMetrics.Add(new MetricDisplayItem { Name = "SPICE Score", Icon = "\uE8E3", IconColor = GetGreenBrush() });
            if (_metrics.UseClipScore)
                automatedMetrics.Add(new MetricDisplayItem { Name = "CLIPScore", Icon = "\uE8E3", IconColor = GetGreenBrush() });
            if (_metrics.UseMeteor)
                automatedMetrics.Add(new MetricDisplayItem { Name = "METEOR", Icon = "\uE8E3", IconColor = GetGreenBrush() });
            if (_metrics.UseLengthStats)
                automatedMetrics.Add(new MetricDisplayItem { Name = "Length Statistics", Icon = "\uE8E3", IconColor = GetGreenBrush() });
                
            if (!automatedMetrics.Any())
            {
                automatedMetrics.Add(new MetricDisplayItem { Name = "None selected", Icon = "\uE711", IconColor = GetRedBrush() });
            }
            
            AutomatedMetricsList.ItemsSource = automatedMetrics;
            
            // AI Judge criteria
            if (_metrics.UseAIJudge && _metrics.CustomCriteria.Any())
            {
                AIJudgePanel.Visibility = Visibility.Visible;
                AIJudgeHeaderText.Text = $"AI Judge Criteria ({_metrics.CustomCriteria.Count}):";
                CriteriaList.ItemsSource = _metrics.CustomCriteria
                    .Select(c => new CriterionDisplayItem { Name = c.Name, Description = c.Description })
                    .ToList();
            }
            else
            {
                AIJudgePanel.Visibility = Visibility.Collapsed;
            }
        }
        
        private void UpdateTimeEstimate()
        {
            if (_dataset == null) return;
            
            int imageCount = _dataset.ValidEntries;
            int estimatedSeconds = CalculateEstimatedSeconds(imageCount);
            
            string timeText = FormatTimeEstimate(estimatedSeconds);
            TimeEstimateText.Text = $"Estimated time: {timeText}";
            
            // Update details
            string details = $"Based on {imageCount} {GetDatasetItemType()}";
            if (_metrics != null)
            {
                int metricCount = CountSelectedMetrics();
                if (metricCount > 0)
                {
                    details += $" with {metricCount} metrics";
                    if (_metrics.UseAIJudge)
                    {
                        details += " + AI Judge";
                    }
                }
            }
            TimeEstimateDetailsText.Text = details;
            
            // Show warning for large datasets
            LargeDatasetWarning.IsOpen = imageCount > 500 || estimatedSeconds > 1800;
        }
        
        private int CalculateEstimatedSeconds(int itemCount)
        {
            // Base estimation: 2 seconds per image for generation (TestModel only)
            int seconds = 0;
            if (_workflow == EvaluationWorkflow.TestModel)
            {
                seconds += itemCount * 2;
            }
            
            // Add time for metrics
            if (_metrics != null)
            {
                if (_metrics.UseSpice || _metrics.UseClipScore)
                    seconds += itemCount * 1; // 1 second per image for automated metrics
                    
                if (_metrics.UseAIJudge)
                    seconds += itemCount * 3; // 3 seconds per image for AI judge
            }
            
            return seconds;
        }
        
        private string FormatTimeEstimate(int seconds)
        {
            if (seconds < 60)
                return $"{seconds} seconds";
            else if (seconds < 3600)
                return $"~{seconds / 60} minutes";
            else
                return $"~{seconds / 3600} hours {(seconds % 3600) / 60} minutes";
        }
        
        private int CountSelectedMetrics()
        {
            if (_metrics == null) return 0;
            
            int count = 0;
            if (_metrics.UseSpice) count++;
            if (_metrics.UseClipScore) count++;
            if (_metrics.UseMeteor) count++;
            if (_metrics.UseLengthStats) count++;
            return count;
        }
        
        private string GetDatasetItemType()
        {
            return _workflow switch
            {
                EvaluationWorkflow.TestModel => "images",
                EvaluationWorkflow.EvaluateResponses => "responses",
                EvaluationWorkflow.ImportResults => "results",
                _ => "items"
            };
        }
        
        private string GetEvaluationTypeDisplayName(EvaluationType type)
        {
            return type switch
            {
                EvaluationType.ImageDescription => "Image Description Quality",
                EvaluationType.TextSummarization => "Text Summarization",
                EvaluationType.Translation => "Translation Quality",
                EvaluationType.QuestionAnswering => "Question Answering",
                EvaluationType.CustomTask => "Custom Task",
                _ => type.ToString()
            };
        }
        
        private string GetWorkflowDisplayName(EvaluationWorkflow workflow)
        {
            return workflow switch
            {
                EvaluationWorkflow.TestModel => "Test Model (Full Pipeline)",
                EvaluationWorkflow.EvaluateResponses => "Evaluate Responses",
                EvaluationWorkflow.ImportResults => "Import Results",
                _ => workflow.ToString()
            };
        }
        
        private string GetProviderName(string apiEndpoint)
        {
            if (apiEndpoint.Contains("openai", StringComparison.OrdinalIgnoreCase))
                return "OpenAI";
            else if (apiEndpoint.Contains("azure", StringComparison.OrdinalIgnoreCase))
                return "Azure OpenAI";
            else if (apiEndpoint.Contains("anthropic", StringComparison.OrdinalIgnoreCase))
                return "Anthropic";
            else
                return "Custom";
        }
        
        private string MaskApiKey(string apiEndpoint)
        {
            // For now, just show a masked version
            // In real implementation, would extract last 4 chars from credential manager
            return "••••••••••1234";
        }
        
        private SolidColorBrush GetGreenBrush()
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 16));
        }
        
        private SolidColorBrush GetRedBrush()
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 196, 49, 75));
        }
        
        #region Edit Navigation Handlers
        
        private void EditEvaluationType_Click(object sender, RoutedEventArgs e)
        {
            // Create state to pass during navigation
            var state = new EvaluationWizardState
            {
                EvaluationType = _evaluationType,
                Workflow = _workflow,
                ModelConfig = _modelConfig,
                Dataset = _dataset,
                Metrics = _metrics,
                CurrentStep = 1
            };
            _wizardFrame?.Navigate(typeof(SelectEvaluationTypePage), state);
        }
        
        private void EditModelConfig_Click(object sender, RoutedEventArgs e)
        {
            // Create state to pass during navigation
            var state = new EvaluationWizardState
            {
                EvaluationType = _evaluationType,
                Workflow = _workflow,
                ModelConfig = _modelConfig,
                Dataset = _dataset,
                Metrics = _metrics,
                CurrentStep = 3
            };
            _wizardFrame?.Navigate(typeof(ModelConfigurationStep), state);
        }
        
        private void EditDataset_Click(object sender, RoutedEventArgs e)
        {
            // Create state to pass during navigation
            var state = new EvaluationWizardState
            {
                EvaluationType = _evaluationType,
                Workflow = _workflow,
                ModelConfig = _modelConfig,
                Dataset = _dataset,
                Metrics = _metrics,
                CurrentStep = 4
            };
            _wizardFrame?.Navigate(typeof(DatasetUploadPage), state);
        }
        
        private void EditMetrics_Click(object sender, RoutedEventArgs e)
        {
            // Create state to pass during navigation
            var state = new EvaluationWizardState
            {
                EvaluationType = _evaluationType,
                Workflow = _workflow,
                ModelConfig = _modelConfig,
                Dataset = _dataset,
                Metrics = _metrics,
                CurrentStep = 5
            };
            _wizardFrame?.Navigate(typeof(MetricsSelectionPage), state);
        }
        
        #endregion
        
        /// <summary>
        /// Checks if the configuration is ready for execution
        /// </summary>
        public bool IsReadyToExecute
        {
            get
            {
                return _workflow switch
                {
                    EvaluationWorkflow.TestModel => 
                        _modelConfig != null && _dataset != null && _metrics != null,
                    EvaluationWorkflow.EvaluateResponses => 
                        _dataset != null && _metrics != null,
                    EvaluationWorkflow.ImportResults => 
                        _dataset != null,
                    _ => false
                };
            }
        }
        
        /// <summary>
        /// Builds the final evaluation configuration for execution
        /// </summary>
        internal EvaluationConfiguration BuildFinalConfiguration()
        {
            return new EvaluationConfiguration
            {
                Id = Guid.NewGuid().ToString(),
                Name = GenerateEvaluationName(),
                Type = _evaluationType,
                Workflow = _workflow,
                Goal = _modelConfig?.EvaluationGoal,
                Created = DateTime.UtcNow,
                SelectedModelId = _modelConfig?.SelectedModelId ?? "",
                SelectedModelName = _modelConfig?.SelectedModelName ?? "",
                ApiEndpoint = _modelConfig?.ApiEndpoint ?? "",
                BaselinePrompt = _modelConfig?.BaselinePrompt ?? "",
                Dataset = _dataset,
                Metrics = _metrics,
                Status = EvaluationStatus.Ready
            };
        }
        
        private string GenerateEvaluationName()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            var workflowShort = _workflow switch
            {
                EvaluationWorkflow.TestModel => "Test",
                EvaluationWorkflow.EvaluateResponses => "Eval",
                EvaluationWorkflow.ImportResults => "Import",
                _ => "Unknown"
            };
            
            return $"{_evaluationType} {workflowShort} - {timestamp}";
        }
    }
    
    // Helper classes for display
    internal class MetricDisplayItem
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "";
        public SolidColorBrush IconColor { get; set; } = new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }
    
    internal class CriterionDisplayItem
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}