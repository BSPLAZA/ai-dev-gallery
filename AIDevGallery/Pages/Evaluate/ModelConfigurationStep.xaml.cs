// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using AIDevGallery.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AIDevGallery.Pages.Evaluate
{
    /// <summary>
    /// Step 3: Model Configuration
    /// Collects evaluation details and configures the AI model to test.
    /// Handles secure API key storage using Windows Credential Manager.
    /// Only shown for TestModel workflow.
    /// </summary>
    public sealed partial class ModelConfigurationStep : Page
    {
        public delegate void ValidationChangedEventHandler(bool isValid);
        public event ValidationChangedEventHandler? ValidationChanged;

        private string? selectedModelId;
        private string? selectedModelName;

        public ModelConfigurationStep()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Checks if this step has valid input (all required fields completed)
        /// </summary>
        public bool IsValid
        {
            get
            {
                bool hasEvaluationName = !string.IsNullOrWhiteSpace(EvaluationNameTextBox.Text);
                bool hasModelSelected = selectedModelId != null;
                bool hasApiEndpoint = !string.IsNullOrWhiteSpace(ApiEndpointTextBox.Text);
                bool hasApiKey = !string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password);
                bool hasBaselinePrompt = !string.IsNullOrWhiteSpace(BaselinePromptTextBox.Text);

                // If model is selected, API configuration becomes required
                bool modelConfigValid = !hasModelSelected || (hasApiEndpoint && hasApiKey);

                return hasEvaluationName && hasModelSelected && modelConfigValid && hasBaselinePrompt;
            }
        }

        /// <summary>
        /// Gets the final evaluation name with model appended
        /// </summary>
        public string GetFinalEvaluationName()
        {
            string baseName = EvaluationNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(selectedModelName))
            {
                return $"{baseName} - {selectedModelName}";
            }
            return baseName;
        }

        private void ModelSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelSelectionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                selectedModelId = selectedItem.Tag?.ToString();
                selectedModelName = selectedItem.Content?.ToString();

                // Show model configuration panel
                ModelConfigurationPanel.Visibility = Visibility.Visible;

                // Pre-populate API endpoint based on model
                PopulateDefaultApiEndpoint();

                // Update evaluation name preview
                UpdateEvaluationNamePreview();

                // Try to load existing API key for this model
                LoadExistingApiKey();
            }
            else
            {
                selectedModelId = null;
                selectedModelName = null;
                ModelConfigurationPanel.Visibility = Visibility.Collapsed;
                UpdateEvaluationNamePreview();
            }

            UpdateValidationState();
        }

        private void PopulateDefaultApiEndpoint()
        {
            if (selectedModelId == null) return;

            string defaultEndpoint = selectedModelId switch
            {
                "gpt-4o" => "https://api.openai.com/v1/chat/completions",
                "phi-4-multimodal" => "https://api.openai.com/v1/chat/completions", // Or Azure OpenAI endpoint
                "azure-ai-vision" => "https://<region>.api.cognitive.microsoft.com/computervision/imageanalysis:analyze?api-version=2024-02-01",
                _ => ""
            };

            if (!string.IsNullOrEmpty(defaultEndpoint) && string.IsNullOrWhiteSpace(ApiEndpointTextBox.Text))
            {
                ApiEndpointTextBox.Text = defaultEndpoint;
            }
        }

        private void LoadExistingApiKey()
        {
            if (selectedModelId == null) return;

            // Create a credential key based on model and endpoint combination
            string credentialKey = GetCredentialKey();
            string? existingKey = CredentialManager.ReadCredential(credentialKey);

            if (!string.IsNullOrEmpty(existingKey))
            {
                ApiKeyPasswordBox.Password = existingKey;
            }
        }

        private string GetCredentialKey()
        {
            // Create a unique key for this model configuration
            return $"AI_DEV_GALLERY_EVAL_{selectedModelId?.ToUpperInvariant()}_API_KEY";
        }

        private void UpdateEvaluationNamePreview()
        {
            string baseName = EvaluationNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(baseName) && !string.IsNullOrEmpty(selectedModelName))
            {
                EvaluationNamePreview.Text = $"Final name: {baseName} - {selectedModelName}";
            }
            else if (!string.IsNullOrEmpty(selectedModelName))
            {
                EvaluationNamePreview.Text = $"Final name will be: [Your Name] - {selectedModelName}";
            }
            else
            {
                EvaluationNamePreview.Text = "We'll automatically add the model name to your evaluation";
            }
        }

        private void RequiredField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ReferenceEquals(sender, EvaluationNameTextBox))
            {
                UpdateEvaluationNamePreview();
            }
            UpdateValidationState();
        }

        private void RequiredField_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateValidationState();
        }

        private void UpdateValidationState()
        {
            bool isValid = IsValid;
            
            // Show/hide validation warning
            ValidationInfoBar.IsOpen = !isValid && HasUserInput();
            
            // Update parent dialog state
            ValidationChanged?.Invoke(isValid);
            UpdateParentDialogState();
        }

        private bool HasUserInput()
        {
            return !string.IsNullOrWhiteSpace(EvaluationNameTextBox.Text) ||
                   selectedModelId != null ||
                   !string.IsNullOrWhiteSpace(ApiEndpointTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password) ||
                   !string.IsNullOrWhiteSpace(BaselinePromptTextBox.Text);
        }

        private void UpdateParentDialogState()
        {
            // Navigate up the visual tree to find the ContentDialog
            var current = Parent;
            while (current != null)
            {
                if (current is ContentDialog dialog)
                {
                    dialog.IsPrimaryButtonEnabled = IsValid;
                    break;
                }
                current = (current as FrameworkElement)?.Parent;
            }
        }

        private EvaluationWizardState? _wizardState;

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check if we have state to restore
            if (e.Parameter is EvaluationWizardState state)
            {
                _wizardState = state;
                RestoreFromState();
            }
            
            // Ensure parent dialog starts with disabled Next button
            UpdateParentDialogState();
        }

        protected override void OnNavigatingFrom(Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SaveToState();
        }

        private void RestoreFromState()
        {
            if (_wizardState?.ModelConfig == null) return;

            var config = _wizardState.ModelConfig;
            
            // Restore evaluation name and goal
            EvaluationNameTextBox.Text = config.EvaluationName;
            EvaluationGoalTextBox.Text = config.EvaluationGoal ?? "";
            
            // Restore provider selection
            var providerMap = new Dictionary<string, string>
            {
                { "https://api.openai.com", "OpenAI" },
                { "custom", "Custom Endpoint" }
            };
            
            if (config.ApiEndpoint.Contains("openai.azure.com"))
            {
                ProviderComboBox.SelectedValue = "Azure OpenAI";
            }
            else if (providerMap.TryGetValue(config.ApiEndpoint, out var provider))
            {
                ProviderComboBox.SelectedValue = provider;
            }
            
            // Restore model selection
            if (!string.IsNullOrEmpty(config.SelectedModelId))
            {
                selectedModelId = config.SelectedModelId;
                selectedModelName = config.SelectedModelName;
                ModelComboBox.SelectedValue = config.SelectedModelId;
            }
            
            // Restore prompt template
            PromptTemplateTextBox.Text = config.BaselinePrompt;
            
            // Note: API key is not restored for security reasons
            // User will need to re-enter it if they navigate back
        }

        private void SaveToState()
        {
            if (_wizardState == null) return;

            // Save current configuration to state
            _wizardState.ModelConfig = GetStepData();
        }

        /// <summary>
        /// Saves the API key securely and gets the evaluation data for this step
        /// </summary>
        public ModelConfigurationData GetStepData()
        {
            // Save API key securely if provided
            if (!string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password) && selectedModelId != null)
            {
                string credentialKey = GetCredentialKey();
                CredentialManager.WriteCredential(credentialKey, ApiKeyPasswordBox.Password);
            }

            return new ModelConfigurationData
            {
                EvaluationName = EvaluationNameTextBox.Text.Trim(),
                FinalEvaluationName = GetFinalEvaluationName(),
                EvaluationGoal = EvaluationGoalTextBox.Text.Trim(),
                SelectedModelId = selectedModelId ?? string.Empty,
                SelectedModelName = selectedModelName ?? string.Empty,
                ApiEndpoint = ApiEndpointTextBox.Text.Trim(),
                BaselinePrompt = BaselinePromptTextBox.Text.Trim()
            };
        }

        /// <summary>
        /// Creates a complete EvaluationConfiguration from the current step data
        /// Following the new comprehensive data model
        /// </summary>
        internal EvaluationConfiguration CreateEvaluationConfiguration(EvaluationType evaluationType)
        {
            // Save API key securely if provided
            if (!string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password) && selectedModelId != null)
            {
                string credentialKey = GetCredentialKey();
                CredentialManager.WriteCredential(credentialKey, ApiKeyPasswordBox.Password);
            }

            return new EvaluationConfiguration
            {
                Id = Guid.NewGuid().ToString(),
                Name = EvaluationNameTextBox.Text.Trim(),
                Type = evaluationType,
                Goal = string.IsNullOrWhiteSpace(EvaluationGoalTextBox.Text) ? null : EvaluationGoalTextBox.Text.Trim(),
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                SelectedModelId = selectedModelId ?? string.Empty,
                SelectedModelName = selectedModelName ?? string.Empty,
                ApiEndpoint = ApiEndpointTextBox.Text.Trim(),
                BaselinePrompt = BaselinePromptTextBox.Text.Trim(),
                Status = EvaluationStatus.Draft,
                Criteria = null, // Will be set in Step 4
                Dataset = null   // Will be set in Step 5
            };
        }

        /// <summary>
        /// Resets the step to its initial state
        /// </summary>
        public void Reset()
        {
            EvaluationNameTextBox.Text = string.Empty;
            EvaluationGoalTextBox.Text = string.Empty;
            ModelSelectionComboBox.SelectedItem = null;
            ApiEndpointTextBox.Text = string.Empty;
            ApiKeyPasswordBox.Password = string.Empty;
            BaselinePromptTextBox.Text = string.Empty;
            ModelConfigurationPanel.Visibility = Visibility.Collapsed;
            selectedModelId = null;
            selectedModelName = null;
            UpdateEvaluationNamePreview();
            UpdateValidationState();
        }
    }

    /// <summary>
    /// Data collected from the Model Configuration step
    /// </summary>
    public class ModelConfigurationData
    {
        public required string EvaluationName { get; set; }
        public required string FinalEvaluationName { get; set; }
        public string EvaluationGoal { get; set; } = string.Empty;
        public required string SelectedModelId { get; set; }
        public required string SelectedModelName { get; set; }
        public required string ApiEndpoint { get; set; }
        public required string BaselinePrompt { get; set; }
    }
}