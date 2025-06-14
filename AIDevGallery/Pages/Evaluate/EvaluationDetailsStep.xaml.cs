// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AIDevGallery.Pages.Evaluate
{
    /// <summary>
    /// Step 2: Project Setup
    /// Collects evaluation project details and configures the AI model to test.
    /// Handles secure API key storage using Windows Credential Manager.
    /// </summary>
    public sealed partial class EvaluationDetailsStep : Page
    {
        public delegate void ValidationChangedEventHandler(bool isValid);
        public event ValidationChangedEventHandler? ValidationChanged;

        private string? selectedModelId;
        private string? selectedModelName;

        public EvaluationDetailsStep()
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
            if (sender == EvaluationNameTextBox)
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

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Ensure parent dialog starts with disabled Next button
            UpdateParentDialogState();
        }

        /// <summary>
        /// Saves the API key securely and gets the evaluation data for this step
        /// </summary>
        public EvaluationDetailsData GetStepData()
        {
            // Save API key securely if provided
            if (!string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password) && selectedModelId != null)
            {
                string credentialKey = GetCredentialKey();
                CredentialManager.WriteCredential(credentialKey, ApiKeyPasswordBox.Password);
            }

            return new EvaluationDetailsData
            {
                EvaluationName = EvaluationNameTextBox.Text.Trim(),
                FinalEvaluationName = GetFinalEvaluationName(),
                ProjectGoal = ProjectGoalTextBox.Text.Trim(),
                SelectedModelId = selectedModelId ?? string.Empty,
                SelectedModelName = selectedModelName ?? string.Empty,
                ApiEndpoint = ApiEndpointTextBox.Text.Trim(),
                BaselinePrompt = BaselinePromptTextBox.Text.Trim()
            };
        }

        /// <summary>
        /// Resets the step to its initial state
        /// </summary>
        public void Reset()
        {
            EvaluationNameTextBox.Text = string.Empty;
            ProjectGoalTextBox.Text = string.Empty;
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
    /// Data collected from the Project Setup step
    /// </summary>
    public class EvaluationDetailsData
    {
        public required string EvaluationName { get; set; }
        public required string FinalEvaluationName { get; set; }
        public string ProjectGoal { get; set; } = string.Empty;
        public required string SelectedModelId { get; set; }
        public required string SelectedModelName { get; set; }
        public required string ApiEndpoint { get; set; }
        public required string BaselinePrompt { get; set; }
    }
}