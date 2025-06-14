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
    public delegate void ValidationChangedEventHandler(bool isValid);
    public event ValidationChangedEventHandler? ValidationChanged;

    private EvaluationType? selectedEvaluationType;

    public SelectEvaluationTypePage()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Gets the currently selected evaluation type
    /// </summary>
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
    public EvaluationTypeData GetStepData()
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
/// Data model for evaluation type selection step
/// </summary>
internal class EvaluationTypeData
{
    public required EvaluationType EvaluationType { get; set; }
}