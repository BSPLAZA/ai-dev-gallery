// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AIDevGallery.Pages.Evaluate
{
    /// <summary>
    /// Step 5: Select Evaluation Methods
    /// Allows users to choose automated metrics and/or AI Judge criteria.
    /// </summary>
    public sealed partial class MetricsSelectionPage : Page, INotifyPropertyChanged
    {
        public delegate void ValidationChangedEventHandler(bool isValid);
        public event ValidationChangedEventHandler? ValidationChanged;

        private const int MaxCriteria = 5;
        private readonly List<CriterionControl> _criteriaControls = new();
        private int _criteriaCount = 0;

        public MetricsSelectionPage()
        {
            this.InitializeComponent();
            
            // Don't add default criterion here - wait for AI Judge to be checked
        }

        /// <summary>
        /// Checks if this step has valid input
        /// </summary>
        public bool IsValid
        {
            get
            {
                // Guard against null controls during initialization
                if (SpiceCheckBox == null || ClipScoreCheckBox == null || 
                    MeteorCheckBox == null || LengthStatsCheckBox == null || 
                    AIJudgeCheckBox == null)
                {
                    return false;
                }

                // At least one metric must be selected
                bool hasAutomatedMetric = SpiceCheckBox.IsChecked == true ||
                                         ClipScoreCheckBox.IsChecked == true ||
                                         MeteorCheckBox.IsChecked == true ||
                                         LengthStatsCheckBox.IsChecked == true;

                bool hasValidAIJudge = false;
                if (AIJudgeCheckBox.IsChecked == true)
                {
                    // AI Judge requires at least one valid criterion
                    hasValidAIJudge = _criteriaControls.Any(c => c.IsValid);
                }

                return hasAutomatedMetric || hasValidAIJudge;
            }
        }

        /// <summary>
        /// Gets the metrics configuration for this step
        /// </summary>
        internal EvaluationMetrics GetStepData()
        {
            var metrics = new EvaluationMetrics
            {
                UseSpice = SpiceCheckBox.IsChecked == true,
                UseClipScore = ClipScoreCheckBox.IsChecked == true,
                UseMeteor = MeteorCheckBox.IsChecked == true,
                UseLengthStats = LengthStatsCheckBox.IsChecked == true,
                UseAIJudge = AIJudgeCheckBox.IsChecked == true
            };

            if (metrics.UseAIJudge)
            {
                metrics.CustomCriteria = _criteriaControls
                    .Where(c => c.IsValid)
                    .Select((c, index) => new CustomCriterion
                    {
                        Id = index + 1,
                        Name = c.CriterionName,
                        Description = c.CriterionDescription,
                        IsEnabled = true
                    })
                    .ToList();
            }

            return metrics;
        }

        #region Event Handlers

        private void Metric_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateValidationState();
        }

        private void AIJudge_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Show/hide AI Judge criteria panel
            if (AIJudgeCheckBox.IsChecked == true)
            {
                AIJudgeCriteriaPanel.Visibility = Visibility.Visible;
                
                // Add first criterion if none exist
                if (_criteriaControls.Count == 0)
                {
                    AddCriterion();
                }
            }
            else
            {
                AIJudgeCriteriaPanel.Visibility = Visibility.Collapsed;
            }

            UpdateValidationState();
        }

        private void AddCriterionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_criteriaCount < MaxCriteria)
            {
                AddCriterion();
            }
        }

        #endregion

        #region Private Methods

        private void AddCriterion()
        {
            if (_criteriaCount >= MaxCriteria) return;

            _criteriaCount++;
            
            var criterionControl = new CriterionControl(_criteriaCount);
            criterionControl.RemoveRequested += OnCriterionRemoveRequested;
            criterionControl.ValidationChanged += OnCriterionValidationChanged;
            
            _criteriaControls.Add(criterionControl);
            CriteriaContainer.Children.Add(criterionControl);
            
            UpdateAddButtonState();
            UpdateValidationState();
        }

        private void OnCriterionRemoveRequested(CriterionControl control)
        {
            _criteriaControls.Remove(control);
            CriteriaContainer.Children.Remove(control);
            
            // Re-number remaining criteria
            for (int i = 0; i < _criteriaControls.Count; i++)
            {
                _criteriaControls[i].UpdateNumber(i + 1);
            }
            
            _criteriaCount = _criteriaControls.Count;
            UpdateAddButtonState();
            UpdateValidationState();
        }

        private void OnCriterionValidationChanged()
        {
            UpdateValidationState();
        }

        private void UpdateAddButtonState()
        {
            int remaining = MaxCriteria - _criteriaCount;
            AddCriterionButton.IsEnabled = remaining > 0;
            AddCriterionButtonText.Text = remaining > 0 
                ? $"Add another criterion ({remaining} remaining)" 
                : "Maximum criteria reached";
        }

        private void UpdateValidationState()
        {
            bool isValid = IsValid;
            if (ValidationInfoBar != null)
            {
                ValidationInfoBar.IsOpen = !isValid;
            }
            ValidationChanged?.Invoke(isValid);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Custom control for each AI Judge criterion
    /// </summary>
    internal class CriterionControl : StackPanel
    {
        public delegate void RemoveRequestedEventHandler(CriterionControl control);
        public event RemoveRequestedEventHandler? RemoveRequested;
        public event Action? ValidationChanged;

        private readonly TextBox _nameTextBox;
        private readonly TextBox _descriptionTextBox;
        private readonly TextBlock _numberText;
        private int _number;

        public string CriterionName => _nameTextBox.Text.Trim();
        public string CriterionDescription => _descriptionTextBox.Text.Trim();
        public bool IsValid => !string.IsNullOrWhiteSpace(CriterionName) && 
                              !string.IsNullOrWhiteSpace(CriterionDescription);

        public CriterionControl(int number)
        {
            _number = number;
            Spacing = 12;

            // Header with remove button
            var headerPanel = new Grid();
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _numberText = new TextBlock
            {
                Text = $"Criterion {number}",
                Style = Application.Current.Resources["BodyStrongTextBlockStyle"] as Style
            };
            headerPanel.Children.Add(_numberText);

            if (number > 1) // Don't allow removing the first criterion
            {
                var removeButton = new Button
                {
                    Content = new FontIcon { Glyph = "\xE74D", FontSize = 12 },
                    Style = Application.Current.Resources["AlternateCloseButtonStyle"] as Style,
                    VerticalAlignment = VerticalAlignment.Center
                };
                removeButton.Click += (s, e) => RemoveRequested?.Invoke(this);
                Grid.SetColumn(removeButton, 1);
                headerPanel.Children.Add(removeButton);
            }

            Children.Add(headerPanel);

            // Name field
            _nameTextBox = new TextBox
            {
                PlaceholderText = "e.g., Accuracy, Completeness, Relevance",
                Header = "Name"
            };
            AutomationProperties.SetName(_nameTextBox, $"Criterion {number} name");
            _nameTextBox.TextChanged += (s, e) => ValidationChanged?.Invoke();
            Children.Add(_nameTextBox);

            // Description field
            _descriptionTextBox = new TextBox
            {
                PlaceholderText = "What should the AI look for? e.g., \"Does the description accurately identify all visible objects and their relationships?\"",
                Header = "Description",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 80
            };
            ScrollViewer.SetVerticalScrollBarVisibility(_descriptionTextBox, ScrollBarVisibility.Auto);
            AutomationProperties.SetName(_descriptionTextBox, $"Criterion {number} description");
            _descriptionTextBox.TextChanged += (s, e) => ValidationChanged?.Invoke();
            Children.Add(_descriptionTextBox);
        }

        public void UpdateNumber(int newNumber)
        {
            _number = newNumber;
            _numberText.Text = $"Criterion {newNumber}";
            AutomationProperties.SetName(_nameTextBox, $"Criterion {newNumber} name");
            AutomationProperties.SetName(_descriptionTextBox, $"Criterion {newNumber} description");
        }
    }
}