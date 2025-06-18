// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AIDevGallery.Controls;
using AIDevGallery.Controls.Evaluate;
using AIDevGallery.Models;
using AIDevGallery.Pages.Evaluate;
using AIDevGallery.Services.Evaluate;
using AIDevGallery.Utils;
using AIDevGallery.ViewModels.Evaluate;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AIDevGallery.Pages;

internal sealed partial class EvaluatePage : Page, INotifyPropertyChanged
{
    private readonly IEvaluationResultsStore _evaluationStore;
    private ContentDialog? _activeDialog;
    
    public ObservableCollection<EvaluationListItemViewModel> AllEvaluations { get; } = new();
    public ObservableCollection<EvaluationListItemViewModel> FilteredEvaluations { get; } = new();

    private string _searchQuery = string.Empty;
    private bool _isLoading;

    public EvaluatePage()
    {
        this.InitializeComponent();
        
        // Create the evaluation store service
        _evaluationStore = new EvaluationResultsStore();
        
        _ = LoadEvaluationsAsync();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _ = LoadEvaluationsAsync();
    }

    private async Task LoadEvaluationsAsync()
    {
        try
        {
            SetLoadingState(true);
            
            var evaluations = await _evaluationStore.GetAllEvaluationsAsync();
            
            AllEvaluations.Clear();
            foreach (var eval in evaluations)
            {
                AllEvaluations.Add(new EvaluationListItemViewModel(eval));
            }
            
            ApplyFilter();
            UpdateEmptyState();
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error loading evaluations: {ex}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        _isLoading = isLoading;
        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        EvaluationsList.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateEmptyState()
    {
        var hasEvaluations = FilteredEvaluations.Count > 0;
        EmptyState.Visibility = hasEvaluations ? Visibility.Collapsed : Visibility.Visible;
        EvaluationsList.Visibility = hasEvaluations ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyFilter()
    {
        FilteredEvaluations.Clear();
        
        var filtered = string.IsNullOrWhiteSpace(_searchQuery)
            ? AllEvaluations
            : AllEvaluations.Where(e => 
                e.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                e.ModelName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                e.DatasetName.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
        
        foreach (var eval in filtered)
        {
            FilteredEvaluations.Add(eval);
        }
        
        UpdateEmptyState();
    }

    // Event Handlers
    private async void NewEvaluationButton_Click(object sender, RoutedEventArgs e)
    {
        // Prevent multiple dialogs
        if (_activeDialog != null) return;
        
        // Open evaluation wizard
        var dialog = new WizardDialog();
        dialog.XamlRoot = this.XamlRoot;
        _activeDialog = dialog;
        
        // Initialize the wizard with the first page
        var wizardState = new EvaluationWizardState();
        dialog.Frame.Navigate(typeof(SelectEvaluationTypePage), wizardState);
        dialog.UpdateProgress(1, "Select Evaluation Type", 6);
        
        // Disable back button on first step
        dialog.IsSecondaryButtonEnabled = false;
        
        // Set up navigation handlers
        SetupWizardNavigation(dialog, wizardState);
        
        try
        {
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await LoadEvaluationsAsync();
            }
        }
        finally
        {
            _activeDialog = null;
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadEvaluationsAsync();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchQuery = SearchBox.Text;
        ApplyFilter();
    }

    private void EvaluationsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is EvaluationListItemViewModel evaluation)
        {
            NavigateToEvaluationDetails(evaluation);
        }
    }

    private void NavigateToEvaluationDetails(EvaluationListItemViewModel evaluation)
    {
        // TODO: Navigate to evaluation insights page when implemented
        // Frame.Navigate(typeof(EvaluationInsightsPage), evaluation.Id);
        
        // For now, show a placeholder dialog
        _ = ShowPlaceholderDialog($"Evaluation insights for '{evaluation.Name}' will be shown here.");
    }

    // Empty State Event Handlers
    private async void EmptyState_ImportResultsClicked(object sender, EventArgs e)
    {
        // Prevent multiple dialogs
        if (_activeDialog != null) return;
        
        // Open wizard with Import Results workflow
        var dialog = new WizardDialog();
        dialog.XamlRoot = this.XamlRoot;
        _activeDialog = dialog;
        
        // Initialize the wizard - skip to workflow selection with Import Results pre-selected
        var wizardState = new EvaluationWizardState();
        wizardState.SelectedEvaluationType = EvaluationType.ImageDescription;
        wizardState.SelectedWorkflow = EvaluationWorkflow.ImportResults;
        
        // Navigate directly to workflow selection page
        dialog.Frame.Navigate(typeof(WorkflowSelectionPage), wizardState);
        dialog.UpdateProgress(2, "Choose Workflow", 5);
        
        // Enable back button since we're on step 2
        dialog.IsSecondaryButtonEnabled = true;
        
        // Set up navigation handlers
        SetupWizardNavigation(dialog, wizardState);
        
        try
        {
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await LoadEvaluationsAsync();
            }
        }
        finally
        {
            _activeDialog = null;
        }
    }

    private async void EmptyState_TestModelClicked(object sender, EventArgs e)
    {
        // Just call the same handler as the header button
        NewEvaluationButton_Click(sender, new RoutedEventArgs());
    }

    private void EmptyState_LearnMoreClicked(object sender, EventArgs e)
    {
        // TODO: Open documentation or tutorial
        _ = ShowPlaceholderDialog("Learn more documentation will be available soon.");
    }

    // Multi-select support (for future)
    private string GetSelectionText(int count)
    {
        return count == 1 ? "1 evaluation selected" : $"{count} evaluations selected";
    }

    private bool CanCompare(int count)
    {
        return count >= 2;
    }

    private void CompareButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement comparison view
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement export functionality
    }

    private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement bulk delete
    }

    private void CancelSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Clear selection and hide toolbar
    }

    private async Task ShowPlaceholderDialog(string message)
    {
        // Prevent multiple dialogs
        if (_activeDialog != null) return;
        
        var dialog = new ContentDialog
        {
            Title = "Coming Soon",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        _activeDialog = dialog;
        
        try
        {
            await dialog.ShowAsync();
        }
        finally
        {
            _activeDialog = null;
        }
    }

    // New event handlers for list rows
    private void EvaluationRow_ItemClicked(object sender, EvaluationListItemViewModel e)
    {
        // Single click toggles selection (handled in the control)
    }

    private void EvaluationRow_ItemDoubleClicked(object sender, EvaluationListItemViewModel e)
    {
        // Double click opens details
        NavigateToEvaluationDetails(e);
    }

    private void EvaluationRow_SelectionChanged(object sender, EvaluationListItemViewModel e)
    {
        // Update selection count for action bar
        OnPropertyChanged(nameof(GetSelectedCount));
    }

    // Action bar event handlers
    private async void ActionBar_CompareClicked(object sender, EventArgs e)
    {
        await ShowPlaceholderDialog("Comparison view will allow you to analyze multiple evaluations side-by-side.");
    }

    private async void ActionBar_DeleteClicked(object sender, EventArgs e)
    {
        var selectedItems = AllEvaluations.Where(x => x.IsSelected).ToList();
        if (selectedItems.Count == 0) return;

        var dialog = new ContentDialog
        {
            Title = "Delete Evaluations",
            Content = $"Are you sure you want to delete {selectedItems.Count} evaluation(s)? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            foreach (var item in selectedItems)
            {
                await _evaluationStore.DeleteEvaluationAsync(item.Id);
                AllEvaluations.Remove(item);
            }
            ApplyFilter();
            UpdateEmptyState();
        }
    }

    private void ActionBar_CancelClicked(object sender, EventArgs e)
    {
        // Clear all selections
        foreach (var item in AllEvaluations)
        {
            item.IsSelected = false;
        }
        OnPropertyChanged(nameof(GetSelectedCount));
    }

    // Empty state handlers for new design
    private void ImportResults_Click(object sender, RoutedEventArgs e)
    {
        EmptyState_ImportResultsClicked(sender, EventArgs.Empty);
    }

    private void NewEvaluation_Click(object sender, RoutedEventArgs e)
    {
        EmptyState_TestModelClicked(sender, EventArgs.Empty);
    }

    // Helper to get selected count for binding
    public int GetSelectedCount()
    {
        return AllEvaluations.Count(x => x.IsSelected);
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetupWizardNavigation(WizardDialog dialog, EvaluationWizardState wizardState)
    {
        dialog.NextClicked += async (s, args) =>
        {
            var currentPage = dialog.Frame.Content;
            
            // Validate current page
            bool canNavigate = currentPage switch
            {
                SelectEvaluationTypePage page => page.IsValid,
                WorkflowSelectionPage page => page.IsValid,
                ModelConfigurationStep page => page.IsValid,
                DatasetUploadPage page => page.IsValid,
                MetricsSelectionPage page => page.IsValid,
                ReviewConfigurationPage page => page.IsReadyToExecute,
                _ => false
            };

            if (!canNavigate) return;

            // Determine next page
            Type? nextPageType = GetNextPage(currentPage, wizardState);
            if (nextPageType != null)
            {
                dialog.Frame.Navigate(nextPageType, wizardState);
                UpdateWizardProgress(dialog, wizardState);
            }
            else
            {
                // Complete wizard
                dialog.Hide();
            }
        };

        dialog.BackClicked += (s, args) =>
        {
            if (dialog.Frame.CanGoBack)
            {
                dialog.Frame.GoBack();
                UpdateWizardProgress(dialog, wizardState);
            }
        };
    }

    private Type? GetNextPage(object currentPage, EvaluationWizardState state)
    {
        return currentPage switch
        {
            SelectEvaluationTypePage => typeof(WorkflowSelectionPage),
            WorkflowSelectionPage => state.SelectedWorkflow == EvaluationWorkflow.TestModel 
                ? typeof(ModelConfigurationStep) 
                : typeof(DatasetUploadPage),
            ModelConfigurationStep => typeof(DatasetUploadPage),
            DatasetUploadPage => typeof(MetricsSelectionPage),
            MetricsSelectionPage => typeof(ReviewConfigurationPage),
            ReviewConfigurationPage => null,
            _ => null
        };
    }

    private void UpdateWizardProgress(WizardDialog dialog, EvaluationWizardState state)
    {
        var currentPage = dialog.Frame.Content;
        var (step, title) = currentPage switch
        {
            SelectEvaluationTypePage => (1, "Select Evaluation Type"),
            WorkflowSelectionPage => (2, "Choose Workflow"),
            ModelConfigurationStep => (3, "Configure Model"),
            DatasetUploadPage => (state.SelectedWorkflow == EvaluationWorkflow.TestModel ? 4 : 3, "Upload Dataset"),
            MetricsSelectionPage => (state.SelectedWorkflow == EvaluationWorkflow.TestModel ? 5 : 4, "Select Metrics"),
            ReviewConfigurationPage => (state.SelectedWorkflow == EvaluationWorkflow.TestModel ? 6 : 5, "Review & Start"),
            _ => (1, "Unknown Step")
        };

        var totalSteps = state.SelectedWorkflow == EvaluationWorkflow.TestModel ? 6 : 5;
        dialog.UpdateProgress(step, title, totalSteps);
        
        // Enable/disable back button
        dialog.IsSecondaryButtonEnabled = step > 1;
        
        // Update primary button text
        dialog.PrimaryButtonText = currentPage is ReviewConfigurationPage ? "Start Evaluation" : "Next";
    }
}