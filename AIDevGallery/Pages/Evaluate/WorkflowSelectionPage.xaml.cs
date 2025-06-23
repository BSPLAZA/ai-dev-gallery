// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AIDevGallery.Pages.Evaluate;

/// <summary>
/// Step 2: Choose Your Workflow
/// User selects how they want to proceed with the evaluation.
/// Three workflows: Test Model (full pipeline), Evaluate Responses, Import Results.
/// </summary>
public sealed partial class WorkflowSelectionPage : Page
{
    /// <summary>
    /// Delegate for validation state change events.
    /// </summary>
    /// <param name="isValid">Whether the current state is valid.</param>
    public delegate void ValidationChangedEventHandler(bool isValid);

    /// <summary>
    /// Event fired when validation state changes.
    /// </summary>
    public event ValidationChangedEventHandler? ValidationChanged;

    private EvaluationWorkflow selectedWorkflow = EvaluationWorkflow.TestModel; // Default selection

    public WorkflowSelectionPage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the currently selected evaluation workflow.
    /// </summary>
    internal EvaluationWorkflow SelectedWorkflow => selectedWorkflow;

    /// <summary>
    /// Checks if this step has valid input (a workflow is always selected by default).
    /// </summary>
    public bool IsValid => true; // Always valid since we have a default selection

    private void WorkflowType_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radio)
        {
            // Update the selected workflow based on which radio button was checked
            if (radio == TestModelRadio)
            {
                selectedWorkflow = EvaluationWorkflow.TestModel;
            }
            else if (radio == EvaluateResponsesRadio)
            {
                selectedWorkflow = EvaluationWorkflow.EvaluateResponses;
            }
            else if (radio == ImportResultsRadio)
            {
                selectedWorkflow = EvaluationWorkflow.ImportResults;
            }

            // Save state immediately when selection changes to fix navigation timing issue
            SaveToState();
            
            // Debug logging to verify state is saved
            System.Diagnostics.Debug.WriteLine($"WorkflowType_Checked: Selected {selectedWorkflow}, WizardState.Workflow = {_wizardState?.Workflow}");

            // Notify parent dialog that validation state has changed (though it's always valid)
            ValidationChanged?.Invoke(IsValid);

            // Update parent dialog state
            UpdateParentDialogState();
        }
    }

    /// <summary>
    /// Updates the parent dialog's primary button state based on validation.
    /// </summary>
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

    /// <summary>
    /// Called when the page is loaded to ensure initial state.
    /// </summary>
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Check if we have state to restore
        if (e.Parameter is EvaluationWizardState state)
        {
            _wizardState = state;
            
            // If no workflow is set yet, set the default
            if (_wizardState.Workflow == null)
            {
                _wizardState.Workflow = selectedWorkflow;
            }
            else
            {
                RestoreFromState();
            }
        }

        // Since we have a default selection, the page is always valid
        UpdateParentDialogState();
    }

    protected override void OnNavigatingFrom(Microsoft.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        SaveToState();
    }

    private void RestoreFromState()
    {
        if (_wizardState?.Workflow == null) return;

        // Restore the selected workflow
        switch (_wizardState.Workflow)
        {
            case EvaluationWorkflow.TestModel:
                TestModelRadio.IsChecked = true;
                break;
            case EvaluationWorkflow.EvaluateResponses:
                EvaluateResponsesRadio.IsChecked = true;
                break;
            case EvaluationWorkflow.ImportResults:
                ImportResultsRadio.IsChecked = true;
                break;
        }
    }

    private void SaveToState()
    {
        if (_wizardState == null) return;

        // Save current selection to state
        var stepData = GetStepData();
        if (stepData != null)
        {
            _wizardState.Workflow = stepData.Workflow;
        }
    }

    /// <summary>
    /// Gets the workflow data for this step to pass to next steps.
    /// </summary>
    /// <returns>The workflow selection data containing the selected workflow type.</returns>
    internal WorkflowSelectionData GetStepData()
    {
        return new WorkflowSelectionData
        {
            Workflow = selectedWorkflow
        };
    }

    /// <summary>
    /// Resets the step to its initial state.
    /// </summary>
    public void Reset()
    {
        selectedWorkflow = EvaluationWorkflow.TestModel;
        TestModelRadio.IsChecked = true;
        EvaluateResponsesRadio.IsChecked = false;
        ImportResultsRadio.IsChecked = false;
        ValidationChanged?.Invoke(IsValid);
    }
}

/// <summary>
/// Data model for workflow selection step.
/// </summary>
internal class WorkflowSelectionData
{
    /// <summary>
    /// Gets or sets the selected evaluation workflow.
    /// </summary>
    public required EvaluationWorkflow Workflow { get; set; }
}