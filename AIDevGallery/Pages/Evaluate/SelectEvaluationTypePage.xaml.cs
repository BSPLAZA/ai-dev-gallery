// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AIDevGallery.Pages;

/// <summary>
/// Step 1: Choose What to Test
/// User selects which type of AI task they want to evaluate.
/// Currently only Image Description is available in this MVP.
/// </summary>
public sealed partial class SelectEvaluationTypePage : Page
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

    private EvaluationType? selectedEvaluationType;

    public SelectEvaluationTypePage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the currently selected evaluation type.
    /// </summary>
    /// <value>The selected evaluation type, or null if none selected.</value>
    internal EvaluationType? SelectedEvaluationType => selectedEvaluationType;

    /// <summary>
    /// Checks if this step has valid input (an evaluation type is selected)
    /// </summary>
    public bool IsValid => selectedEvaluationType.HasValue;

    private void EvaluationType_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radio)
        {
            // For MVP, only Image Description is available
            if (radio == ImageDescriptionRadio)
            {
                selectedEvaluationType = EvaluationType.ImageDescription;
            }

            // Notify parent dialog that validation state has changed
            ValidationChanged?.Invoke(IsValid);

            // Enable Next button in parent dialog - navigate up the visual tree
            UpdateParentDialogState();
        }
    }

    /// <summary>
    /// Updates the parent dialog's primary button state based on validation
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
    /// Called when the page is loaded to ensure initial state
    /// </summary>
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Ensure parent dialog starts with disabled Next button
        UpdateParentDialogState();
    }

    /// <summary>
    /// Gets the evaluation data for this step to pass to next steps
    /// </summary>
    /// <returns>The evaluation type data containing the selected type</returns>
    internal EvaluationTypeData GetStepData()
    {
        return new EvaluationTypeData
        {
            EvaluationType = selectedEvaluationType ?? EvaluationType.ImageDescription
        };
    }

    /// <summary>
    /// Resets the step to its initial state
    /// </summary>
    public void Reset()
    {
        selectedEvaluationType = null;
        ImageDescriptionRadio.IsChecked = false;
        ValidationChanged?.Invoke(IsValid);
    }
}

/// <summary>
/// Data model for evaluation type selection step.
/// </summary>
internal class EvaluationTypeData
{
    /// <summary>
    /// Gets or sets the selected evaluation type.
    /// </summary>
    public required EvaluationType EvaluationType { get; set; }
}