using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace AIDevGallery.Controls;

/// <summary>
/// Multi-step wizard dialog that uses Frame navigation between steps
/// </summary>
internal sealed partial class WizardDialog : ContentDialog
{
    public Frame Frame => ContentFrame;

    public event EventHandler? NextClicked;
    public event EventHandler? BackClicked;

    public WizardDialog()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Updates the progress indicator and step title following existing progress patterns
    /// </summary>
    public void UpdateProgress(int currentStep, string stepTitle, int totalSteps = 5)
    {
        WizardProgressBar.Value = currentStep;
        WizardProgressBar.Maximum = totalSteps;
        StepProgressText.Text = $"Step {currentStep} of {totalSteps}";
        StepTitleText.Text = stepTitle;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        NextClicked?.Invoke(this, EventArgs.Empty);
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        args.Cancel = true;
        BackClicked?.Invoke(this, EventArgs.Empty);
    }
}