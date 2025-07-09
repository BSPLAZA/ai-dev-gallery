// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;
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
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Microsoft.UI.Text;

namespace AIDevGallery.Pages;

internal sealed partial class EvaluatePage : Page, INotifyPropertyChanged
{
    private readonly IEvaluationResultsStore _evaluationStore;
    private ContentDialog? _activeDialog;
    
    public ObservableCollection<EvaluationListItemViewModel> AllEvaluations { get; } = new();
    public ObservableCollection<EvaluationListItemViewModel> FilteredEvaluations { get; } = new();

    private string _searchQuery = string.Empty;
    private bool _isLoading;

    // Multi-selection support
    private bool _isMultiSelectMode;
    public bool IsMultiSelectMode
    {
        get => _isMultiSelectMode;
        set
        {
            if (_isMultiSelectMode != value)
            {
                _isMultiSelectMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedCount));
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }
    
    public int SelectedCount => AllEvaluations.Count(e => e.IsSelected);
    public bool HasSelection => SelectedCount > 0;

    public EvaluatePage()
    {
        this.InitializeComponent();
        this.DataContext = this;
        
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
                var viewModel = new EvaluationListItemViewModel(eval);
                // Subscribe to selection changes
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(EvaluationListItemViewModel.IsSelected))
                    {
                        UpdateSelectionState();
                    }
                };
                AllEvaluations.Add(viewModel);
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

    private void UpdateSelectionState()
    {
        IsMultiSelectMode = AllEvaluations.Any(e => e.IsSelected);
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
        
        // Show/hide the selection action bar
        if (SelectionActionBar != null)
        {
            SelectionActionBar.Visibility = IsMultiSelectMode ? Visibility.Visible : Visibility.Collapsed;
        }
        
        // Update select all checkbox state
        if (SelectAllCheckBox != null && FilteredEvaluations.Count > 0)
        {
            var allSelected = FilteredEvaluations.All(e => e.IsSelected);
            var someSelected = FilteredEvaluations.Any(e => e.IsSelected);
            
            if (allSelected)
            {
                SelectAllCheckBox.IsChecked = true;
            }
            else if (someSelected)
            {
                SelectAllCheckBox.IsChecked = null; // Indeterminate state
            }
            else
            {
                SelectAllCheckBox.IsChecked = false;
            }
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
        // Simulate progress for running evaluations
        await _evaluationStore.SimulateProgressAsync();
        
        // Reload the list
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
        // Navigate to evaluation insights page
        Frame.Navigate(typeof(EvaluationInsightsPage), evaluation.Id);
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
        wizardState.EvaluationType = EvaluationType.ImageDescription;
        wizardState.Workflow = EvaluationWorkflow.ImportResults;
        
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

    private void EmptyState_TestModelClicked(object sender, EventArgs e)
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

    private async void CompareButton_Click(object sender, RoutedEventArgs e)
    {
        // Get selected evaluation IDs
        var selectedEvaluations = AllEvaluations.Where(e => e.IsSelected).ToList();
        
        if (selectedEvaluations.Count >= 2 && selectedEvaluations.Count <= 5)
        {
            // Navigate to comparison page with selected evaluation IDs
            var evaluationIds = selectedEvaluations.Select(e => e.Id).ToList();
            Frame.Navigate(typeof(CompareEvaluationsPage), evaluationIds);
        }
        else if (selectedEvaluations.Count < 2)
        {
            // Show error dialog
            var dialog = new ContentDialog
            {
                Title = "Selection Required",
                Content = "Please select at least 2 evaluations to compare.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        else
        {
            // Show error dialog for too many selections
            var dialog = new ContentDialog
            {
                Title = "Too Many Selections",
                Content = "You can compare up to 5 evaluations at a time. Please deselect some evaluations.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
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
    
    private async Task ShowEnhancedCompareDialog()
    {
        // Prevent multiple dialogs
        if (_activeDialog != null) return;
        
        var selectedCount = AllEvaluations.Count(e => e.IsSelected);
        
        // Create rich content for the dialog
        var contentPanel = new StackPanel
        {
            Spacing = 16,
            MaxWidth = 400
        };
        
        // Header section with icon
        var headerPanel = new Grid();
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        
        var compareIcon = new FontIcon 
        { 
            Glyph = "\uE9D5", // Compare icon
            FontSize = 48,
            Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)) // Windows blue
        };
        Grid.SetColumn(compareIcon, 0);
        headerPanel.Children.Add(compareIcon);
        
        var titlePanel = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };
        titlePanel.Children.Add(new TextBlock 
        { 
            Text = "Evaluation Comparison",
            Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"]
        });
        titlePanel.Children.Add(new TextBlock 
        { 
            Text = "Coming in the next update!",
            Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
        });
        Grid.SetColumn(titlePanel, 1);
        headerPanel.Children.Add(titlePanel);
        
        contentPanel.Children.Add(headerPanel);
        
        // Add a separator
        contentPanel.Children.Add(new Border 
        { 
            Height = 1, 
            Background = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"],
            Margin = new Thickness(0, 8, 0, 8)
        });
        
        // Feature highlights
        var featuresPanel = new StackPanel { Spacing = 12 };
        
        featuresPanel.Children.Add(new TextBlock 
        { 
            Text = "What you'll be able to do:",
            Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
        });
        
        // Feature list
        var features = new[]
        {
            ("\uE73E", "Side-by-side comparison", "View multiple evaluations in synchronized panels"),
            ("\uE9D2", "Performance metrics", "Compare scores, accuracy, and timing across models"),
            ("\uEA37", "Visual differences", "Spot trends and outliers with comparative charts"),
            ("\uE7C1", "Export comparisons", "Save comparison reports for presentations")
        };
        
        foreach (var (icon, title, description) in features)
        {
            var featurePanel = new Grid();
            featurePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            featurePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            var featureIcon = new FontIcon 
            { 
                Glyph = icon,
                FontSize = 20,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)), // Green
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(featureIcon, 0);
            featurePanel.Children.Add(featureIcon);
            
            var textPanel = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };
            textPanel.Children.Add(new TextBlock 
            { 
                Text = title,
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });
            textPanel.Children.Add(new TextBlock 
            { 
                Text = description,
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap
            });
            Grid.SetColumn(textPanel, 1);
            featurePanel.Children.Add(textPanel);
            
            featuresPanel.Children.Add(featurePanel);
        }
        
        contentPanel.Children.Add(featuresPanel);
        
        // Current selection info
        if (selectedCount > 0)
        {
            contentPanel.Children.Add(new Border 
            { 
                Background = (Brush)Application.Current.Resources["SystemFillColorAttentionBackgroundBrush"],
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 12, 0, 0),
                Child = new TextBlock
                {
                    Text = $"You have {selectedCount} evaluation{(selectedCount > 1 ? "s" : "")} selected and ready to compare!",
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    TextWrapping = TextWrapping.Wrap
                }
            });
        }
        
        // Create the dialog
        var dialog = new ContentDialog
        {
            Title = "Compare Evaluations - Preview",
            Content = contentPanel,
            PrimaryButtonText = "Notify Me",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };
        _activeDialog = dialog;
        
        try
        {
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Show notification setup dialog
                await ShowNotificationDialog();
            }
        }
        finally
        {
            _activeDialog = null;
        }
    }
    
    private async Task ShowNotificationDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Feature Notification",
            Content = new TextBlock
            {
                Text = "We'll notify you when the comparison feature is available! Check the app's release notes for updates.",
                TextWrapping = TextWrapping.Wrap
            },
            CloseButtonText = "Got it",
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
        // Single click - currently no action (selection is handled by checkbox)
        // Could be used for future single-click actions like quick preview
    }

    private void EvaluationRow_ItemDoubleClicked(object sender, EvaluationListItemViewModel e)
    {
        // Double click opens details
        NavigateToEvaluationDetails(e);
    }


    // Action bar event handlers
    private void ActionBar_CompareClicked(object sender, EventArgs e)
    {
        CompareButton_Click(sender, new RoutedEventArgs());
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
            UpdateSelectionState();
        }
    }

    private void ActionBar_CancelClicked(object sender, EventArgs e)
    {
        // Clear all selections
        foreach (var item in AllEvaluations)
        {
            item.IsSelected = false;
        }
        OnPropertyChanged(nameof(SelectedCount));
        
        // Uncheck the select all checkbox
        if (SelectAllCheckBox != null)
        {
            SelectAllCheckBox.IsChecked = false;
        }
    }

    // Empty state handlers for new design
    private void ImportResults_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("ImportResults_Click called");
        // Call the import results handler directly
        EmptyState_ImportResultsClicked(sender, EventArgs.Empty);
    }

    private void NewEvaluation_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("NewEvaluation_Click called");
        // Call the new evaluation handler directly
        NewEvaluationButton_Click(sender, e);
    }
    
    // Helper for visibility binding
    public Visibility GetListHeaderVisibility(int count)
    {
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    
    // Select All checkbox handler
    private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox)
        {
            var isChecked = checkBox.IsChecked ?? false;
            foreach (var eval in FilteredEvaluations)
            {
                eval.IsSelected = isChecked;
            }
        }
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
                // Complete wizard - save the evaluation
                if (currentPage is ReviewConfigurationPage reviewPage && reviewPage.IsReadyToExecute)
                {
                    await SaveEvaluationFromWizard(wizardState);
                    dialog.Hide();
                }
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
        var currentPageType = currentPage.GetType().Name;
        var workflow = state.Workflow;
        
        System.Diagnostics.Debug.WriteLine($"GetNextPage: Current={currentPageType}, Workflow={workflow}");
        
        var nextPage = currentPage switch
        {
            SelectEvaluationTypePage => typeof(WorkflowSelectionPage),
            WorkflowSelectionPage => state.Workflow == EvaluationWorkflow.TestModel 
                ? typeof(ModelConfigurationStep) 
                : typeof(DatasetUploadPage),
            ModelConfigurationStep => typeof(DatasetUploadPage),
            DatasetUploadPage => state.Workflow == EvaluationWorkflow.ImportResults 
                ? typeof(ReviewConfigurationPage)
                : typeof(MetricsSelectionPage),
            MetricsSelectionPage => typeof(ReviewConfigurationPage),
            ReviewConfigurationPage => null,
            _ => null
        };
        
        System.Diagnostics.Debug.WriteLine($"GetNextPage: Next={nextPage?.Name ?? "null"}");
        
        return nextPage;
    }

    private void UpdateWizardProgress(WizardDialog dialog, EvaluationWizardState state)
    {
        var currentPage = dialog.Frame.Content;
        var (step, title) = currentPage switch
        {
            SelectEvaluationTypePage => (1, "Select Evaluation Type"),
            WorkflowSelectionPage => (2, "Choose Workflow"),
            ModelConfigurationStep => (3, "Configure Model"),
            DatasetUploadPage => (state.Workflow == EvaluationWorkflow.TestModel ? 4 : 3, "Upload Dataset"),
            MetricsSelectionPage => (state.Workflow == EvaluationWorkflow.TestModel ? 5 : 4, "Select Metrics"),
            ReviewConfigurationPage => state.Workflow switch
            {
                EvaluationWorkflow.TestModel => (6, "Review & Start"),
                EvaluationWorkflow.ImportResults => (4, "Review & Import"),
                _ => (5, "Review & Start")
            },
            _ => (1, "Unknown Step")
        };

        var totalSteps = state.Workflow switch
        {
            EvaluationWorkflow.TestModel => 6,
            EvaluationWorkflow.ImportResults => 4,
            _ => 5
        };
        dialog.UpdateProgress(step, title, totalSteps);
        
        // Enable/disable back button
        dialog.IsSecondaryButtonEnabled = step > 1;
        
        // Update primary button text
        dialog.PrimaryButtonText = currentPage is ReviewConfigurationPage 
            ? (state.Workflow == EvaluationWorkflow.ImportResults ? "Import Results" : "Start Evaluation") 
            : "Next";
    }

    private string GetEvaluationName(EvaluationWizardState wizardState)
    {
        // If we have a name from model config, use it
        if (!string.IsNullOrEmpty(wizardState.ModelConfig?.EvaluationName))
        {
            return wizardState.ModelConfig.EvaluationName;
        }

        // For Import Results, use the evaluation name from the dataset upload page
        if (wizardState.Workflow == EvaluationWorkflow.ImportResults && !string.IsNullOrEmpty(wizardState.EvaluationName))
        {
            return wizardState.EvaluationName;
        }

        // Default fallback
        return $"Evaluation {DateTime.Now:yyyy-MM-dd HH:mm}";
    }

    private string GetDatasetFolderName(DatasetConfiguration dataset)
    {
        if (!string.IsNullOrEmpty(dataset.BaseDirectory))
        {
            // Get the last folder name from the path
            return System.IO.Path.GetFileName(dataset.BaseDirectory.TrimEnd('\\', '/')) ?? "Dataset";
        }
        else if (!string.IsNullOrEmpty(dataset.FilePath))
        {
            // Use JSONL filename without extension
            return System.IO.Path.GetFileNameWithoutExtension(dataset.FilePath) ?? "Dataset";
        }
        return "Dataset";
    }

    private async Task SaveEvaluationFromWizard(EvaluationWizardState wizardState)
    {
        try
        {
            // Create evaluation result based on workflow
            var evaluation = new EvaluationResult
            {
                Id = Guid.NewGuid().ToString(),
                Name = GetEvaluationName(wizardState),
                ModelName = wizardState.ModelName ?? wizardState.ModelConfig?.SelectedModelName ?? "Unknown Model",
                DatasetName = wizardState.Dataset != null ? GetDatasetFolderName(wizardState.Dataset) : "Unknown Dataset",
                DatasetItemCount = wizardState.Dataset?.ValidEntries ?? 0,
                DatasetBasePath = wizardState.Dataset?.BaseDirectory, // Store the base directory for image path resolution
                Timestamp = DateTime.Now,
                WorkflowType = wizardState.Workflow ?? EvaluationWorkflow.ImportResults,
                Status = wizardState.Workflow == EvaluationWorkflow.ImportResults ? EvaluationStatus.Imported : EvaluationStatus.Running,
                CriteriaScores = new Dictionary<string, double>()
            };

            // For Import Results, we should parse the JSONL and extract scores
            if (wizardState.Workflow == EvaluationWorkflow.ImportResults && !string.IsNullOrEmpty(wizardState.Dataset?.FilePath))
            {
                // ImportFromJsonlAsync returns a new evaluation with all data populated
                evaluation = await _evaluationStore.ImportFromJsonlAsync(wizardState.Dataset.FilePath, evaluation.Name);
                // Preserve the dataset base path from the wizard
                evaluation.DatasetBasePath = wizardState.Dataset?.BaseDirectory;
                // Only update dataset name if it would be more meaningful
                if (wizardState.Dataset != null)
                {
                    var folderName = GetDatasetFolderName(wizardState.Dataset);
                    if (folderName != "Dataset" && !string.IsNullOrEmpty(folderName))
                    {
                        evaluation.DatasetName = folderName;
                    }
                }
                // The import already set the correct DatasetItemCount from counting actual entries
            }
            else
            {
                // For other workflows, start with placeholder scores
                if (wizardState.Metrics?.UseAIJudge == true && wizardState.Metrics.CustomCriteria != null)
                {
                    // Add each custom criterion
                    foreach (var criterion in wizardState.Metrics.CustomCriteria)
                    {
                        evaluation.CriteriaScores[criterion.Name] = 0.0;
                    }
                }
                if (wizardState.Metrics?.UseClipScore == true)
                {
                    evaluation.CriteriaScores["CLIP Score"] = 0.0;
                }
                if (wizardState.Metrics?.UseSpice == true)
                {
                    evaluation.CriteriaScores["SPICE Score"] = 0.0;
                }
                if (wizardState.Metrics?.UseMeteor == true)
                {
                    evaluation.CriteriaScores["METEOR Score"] = 0.0;
                }
                if (wizardState.Metrics?.UseLengthStats == true)
                {
                    evaluation.CriteriaScores["Length Stats"] = 0.0;
                }
                
                // Save the evaluation
                await _evaluationStore.SaveEvaluationAsync(evaluation);
            }

            // Reload the list
            await LoadEvaluationsAsync();
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error saving evaluation: {ex}");
        }
    }
}