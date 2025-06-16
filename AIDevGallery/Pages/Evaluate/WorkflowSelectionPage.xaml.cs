// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

    /// <summary>
    /// Called when the page is loaded to ensure initial state.
    /// </summary>
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Since we have a default selection, the page is always valid
        UpdateParentDialogState();
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