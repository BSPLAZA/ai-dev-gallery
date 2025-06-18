using AIDevGallery.Models;
using AIDevGallery.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using AIDevGallery.Pages.Evaluate;
using System.IO;


namespace AIDevGallery.Pages
{
    public class Evaluation
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string Dataset { get; set; } = string.Empty;
        public int Rows { get; set; }
        public int Criteria { get; set; }
    }
    internal sealed partial class EvaluatePage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<Evaluation> AllEvaluations { get; } = new();
        public ObservableCollection<Evaluation> FilteredEvaluations { get; } = new();

        private Evaluation? selectedEvaluation;
        public Evaluation? SelectedEvaluation
        {
            get => selectedEvaluation;
            set
            {
                if (selectedEvaluation != value)
                {
                    selectedEvaluation = value;
                    OnPropertyChanged(nameof(SelectedEvaluation));
                    DeleteButton.IsEnabled = selectedEvaluation != null;
                }
            }
        }
        private EvaluationType? selectedEvaluationType;
        public EvaluationType? SelectedEvaluationType
        {
            get => selectedEvaluationType;
            set
            {
                if (selectedEvaluationType != value)
                {
                    selectedEvaluationType = value;
                    OnPropertyChanged(nameof(SelectedEvaluationType));
                }
            }
        }

        public EvaluatePage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LoadEvaluations();
            FilteredEvaluations.CollectionChanged += (_, __) => { /* For future use if needed */ };
            // Set ItemsSource for the table
            EvaluationsTable.ItemsSource = FilteredEvaluations;
            // Wire up event handlers if not done in XAML
            SearchBox.TextChanged += SearchBox_TextChanged;
            DeleteButton.Click += DeleteButton_Click;
            RefreshButton.Click += RefreshButton_Click;
            NewEvaluationButton.Click += NewEvaluationButton_Click;
            PrevPageButton.Click += PrevPageButton_Click;
            NextPageButton.Click += NextPageButton_Click;
            ItemsPerPageComboBox.SelectionChanged += ItemsPerPageComboBox_SelectionChanged;
            // If you want to support row selection, add a handler for ItemsControl selection (if using ListView, etc.)
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Optionally reload evaluations here
        }

        private void LoadEvaluations()
        {
            AllEvaluations.Clear();
            
            // Load evaluations asynchronously but don't block UI
            _ = Task.Run(async () =>
            {
                try
                {
                    var appData = await AppData.GetForApp();
                    
                    // Load evaluations from the data model
                    if (appData.EvaluationHistory != null)
                    {
                        _ = DispatcherQueue.TryEnqueue(() =>
                        {
                            foreach (var config in appData.EvaluationHistory)
                            {
                                var evaluation = new Evaluation
                                {
                                    Id = config.Id,
                                    Name = config.Name,
                                    Status = config.Status.ToString(),
                                    Progress = config.Status == EvaluationStatus.Completed ? 100 : 
                                              config.Status == EvaluationStatus.Running ? 50 : 0,
                                    Dataset = config.Dataset?.Name ?? "Not configured",
                                    Rows = config.Dataset?.EstimatedItemCount ?? 0,
                                    Criteria = config.Criteria?.Count ?? 0
                                };
                                AllEvaluations.Add(evaluation);
                            }
                        });
                    }
                    
                    // Add example data if no evaluations exist
                    if (appData.EvaluationHistory?.Count == 0)
                    {
                        _ = DispatcherQueue.TryEnqueue(() =>
                        {
                            AllEvaluations.Add(new Evaluation
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "gpt4o eval",
                                Status = "Succeeded",
                                Progress = 65,
                                Dataset = "gpt4o eval",
                                Rows = 1001,
                                Criteria = 1
                            });
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading evaluations: {ex.Message}");
                }
                
                _ = DispatcherQueue.TryEnqueue(() => ApplySearchFilter(SearchBox.Text));
            });
        }

        private void ApplySearchFilter(string? searchText)
        {
            FilteredEvaluations.Clear();
            var query = (searchText ?? string.Empty).Trim().ToLowerInvariant();
            foreach (var eval in AllEvaluations)
            {
                if (string.IsNullOrEmpty(query) || eval.Name.ToLowerInvariant().Contains(query))
                {
                    FilteredEvaluations.Add(eval);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter(SearchBox.Text);
        }
        private async void NewEvaluationButton_Click(object sender, RoutedEventArgs e)
        {
            // Prevent multiple dialogs
            var clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.IsEnabled = false;
            }
            
            try
            {
                var dialog = new Controls.WizardDialog()
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Create New Evaluation"
                };
                
                // Track wizard progress and data
                int currentStep = 0;
                int totalSteps = 6; // Default to TestModel workflow (6 steps)
                EvaluationTypeData? evaluationTypeData = null;
                WorkflowSelectionData? workflowSelectionData = null;
                Evaluate.ModelConfigurationData? modelConfigurationData = null;
                DatasetConfiguration? datasetConfiguration = null;
                EvaluationMetrics? metricsConfiguration = null;
                
                // Create wizard state object to maintain data across navigation
                var wizardState = new EvaluationWizardState();
                
                // Set up event handlers BEFORE navigation
                dialog.Frame.Navigated += (_, args) =>
                {
                    if (args.Content is SelectEvaluationTypePage evalTypePage)
                    {
                        evalTypePage.ValidationChanged += (isValid) =>
                        {
                            dialog.IsPrimaryButtonEnabled = isValid;
                        };
                        dialog.IsPrimaryButtonEnabled = evalTypePage.IsValid;
                        dialog.IsSecondaryButtonEnabled = false;
                        dialog.PrimaryButtonText = "Next";
                        currentStep = 0;
                        // Update progress following system patterns
                        dialog.UpdateProgress(1, "Choose Evaluation Type", totalSteps);
                    }
                    else if (args.Content is WorkflowSelectionPage workflowPage)
                    {
                        workflowPage.ValidationChanged += (isValid) =>
                        {
                            dialog.IsPrimaryButtonEnabled = isValid;
                        };
                        dialog.IsPrimaryButtonEnabled = workflowPage.IsValid;
                        dialog.IsSecondaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Next";
                        currentStep = 1;
                        // Update progress following system patterns
                        dialog.UpdateProgress(2, "Choose Your Workflow", totalSteps);
                    }
                    else if (args.Content is Evaluate.ModelConfigurationStep modelConfigPage)
                    {
                        modelConfigPage.ValidationChanged += (isValid) =>
                        {
                            dialog.IsPrimaryButtonEnabled = isValid;
                        };
                        dialog.IsPrimaryButtonEnabled = modelConfigPage.IsValid;
                        dialog.IsSecondaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Next";
                        currentStep = 2;
                        // Update progress following system patterns
                        dialog.UpdateProgress(3, "Model Configuration", totalSteps);
                    }
                    else if (args.Content is Evaluate.DatasetUploadPage datasetPage)
                    {
                        datasetPage.ValidationChanged += (isValid) =>
                        {
                            dialog.IsPrimaryButtonEnabled = isValid;
                        };
                        // Pass workflow information to dataset page
                        if (workflowSelectionData != null)
                        {
                            datasetPage.SetWorkflow(workflowSelectionData.Workflow);
                        }
                        dialog.IsPrimaryButtonEnabled = datasetPage.IsValid;
                        dialog.IsSecondaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Next";
                        currentStep = 3;
                        // Update progress - Step 4 for all workflows
                        dialog.UpdateProgress(4, "Upload Dataset", totalSteps);
                    }
                    else if (args.Content is MetricsSelectionPage metricsPage)
                    {
                        metricsPage.ValidationChanged += (isValid) =>
                        {
                            dialog.IsPrimaryButtonEnabled = isValid;
                        };
                        dialog.IsPrimaryButtonEnabled = metricsPage.IsValid;
                        dialog.IsSecondaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Next";
                        currentStep = 4;
                        // Update progress - Step 5 (skip for ImportResults)
                        int stepNumber = workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults ? 4 : 5;
                        dialog.UpdateProgress(stepNumber, "Select Evaluation Methods", totalSteps);
                    }
                    else if (args.Content is ReviewConfigurationPage reviewPage)
                    {
                        // Final step - change button text based on workflow
                        // Don't check IsReadyToExecute here as the page hasn't loaded its data yet
                        dialog.IsPrimaryButtonEnabled = false; // Will be updated by NotifyValidationChanged
                        dialog.IsSecondaryButtonEnabled = true;
                        dialog.PrimaryButtonText = workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults 
                            ? "Log Evaluation" 
                            : "Start Evaluation";
                        // Set current step based on workflow
                        currentStep = workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults ? 4 : 5;
                        // Update progress - Final step
                        dialog.UpdateProgress(totalSteps, "Review Configuration", totalSteps);
                    }
                };
                
                // Handle Next/Create button clicks
                dialog.NextClicked += (_, __) =>
                {
                    if (currentStep == 0)
                    {
                        // Move from Step 1 (Evaluation Type) to Step 2 (Workflow Selection)
                        if (dialog.Frame.Content is SelectEvaluationTypePage currentStep1Page)
                        {
                            evaluationTypeData = currentStep1Page.GetStepData();
                            wizardState.EvaluationType = evaluationTypeData.EvaluationType;
                            wizardState.CurrentStep = 2;
                            dialog.Frame.Navigate(typeof(WorkflowSelectionPage), wizardState);
                        }
                    }
                    else if (currentStep == 1)
                    {
                        // Move from Step 2 (Workflow Selection) to appropriate next step
                        if (dialog.Frame.Content is WorkflowSelectionPage currentStep2Page)
                        {
                            workflowSelectionData = currentStep2Page.GetStepData();
                            wizardState.Workflow = workflowSelectionData.Workflow;
                            
                            // Update total steps based on workflow
                            // TestModel: 6 steps (all)
                            // EvaluateResponses: 5 steps (skip ModelConfiguration)
                            // ImportResults: 4 steps (skip ModelConfiguration and Metrics)
                            totalSteps = workflowSelectionData.Workflow == EvaluationWorkflow.TestModel ? 6 : 
                                        workflowSelectionData.Workflow == EvaluationWorkflow.EvaluateResponses ? 5 : 4;
                            
                            // Only show ModelConfigurationStep for TestModel workflow
                            if (workflowSelectionData.Workflow == EvaluationWorkflow.TestModel)
                            {
                                wizardState.CurrentStep = 3;
                                dialog.Frame.Navigate(typeof(Evaluate.ModelConfigurationStep), wizardState);
                            }
                            else
                            {
                                // Skip to DatasetUploadPage for other workflows (skip ModelConfiguration)
                                wizardState.CurrentStep = 4;
                                dialog.Frame.Navigate(typeof(Evaluate.DatasetUploadPage), wizardState);
                            }
                        }
                    }
                    else if (currentStep == 2)
                    {
                        // Move from Step 3 (Model Configuration) to Step 4 (Dataset Upload)
                        if (dialog.Frame.Content is Evaluate.ModelConfigurationStep currentStep3Page)
                        {
                            modelConfigurationData = currentStep3Page.GetStepData();
                            wizardState.ModelConfig = modelConfigurationData;
                            wizardState.CurrentStep = 4;
                            dialog.Frame.Navigate(typeof(Evaluate.DatasetUploadPage), wizardState);
                        }
                    }
                    else if (currentStep == 3)
                    {
                        // Move from Step 4 (Dataset Upload) to Step 5 (Metrics Selection)
                        if (dialog.Frame.Content is Evaluate.DatasetUploadPage datasetPage)
                        {
                            datasetConfiguration = datasetPage.GetStepData();
                            wizardState.Dataset = datasetConfiguration;
                            
                            System.Diagnostics.Debug.WriteLine($"[EvaluatePage] Dataset from GetStepData: {datasetConfiguration != null}, ValidEntries: {datasetConfiguration?.ValidEntries ?? 0}");
                            System.Diagnostics.Debug.WriteLine($"[EvaluatePage] WizardState.Dataset: {wizardState.Dataset != null}, ValidEntries: {wizardState.Dataset?.ValidEntries ?? 0}");
                            
                            // Skip metrics for ImportResults workflow
                            if (workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults)
                            {
                                // Navigate directly to ReviewConfigurationPage
                                wizardState.CurrentStep = 6;
                                dialog.Frame.Navigate(typeof(ReviewConfigurationPage), wizardState);
                                // The ReviewConfigurationPage will get all data from wizardState in OnNavigatedTo
                            }
                            else
                            {
                                // Navigate to MetricsSelectionPage for other workflows
                                wizardState.CurrentStep = 5;
                                dialog.Frame.Navigate(typeof(MetricsSelectionPage), wizardState);
                            }
                        }
                    }
                    else if (currentStep == 4)
                    {
                        // Move from Step 5 (Metrics Selection) to Step 6 (Review)
                        if (dialog.Frame.Content is MetricsSelectionPage metricsPage)
                        {
                            metricsConfiguration = metricsPage.GetStepData();
                            wizardState.Metrics = metricsConfiguration;
                            wizardState.CurrentStep = 6;
                            
                            // Navigate to ReviewConfigurationPage
                            dialog.Frame.Navigate(typeof(ReviewConfigurationPage), wizardState);
                            // The ReviewConfigurationPage will get all data from wizardState in OnNavigatedTo
                        }
                    }
                    else if (currentStep == 5 || (currentStep == 4 && workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults))
                    {
                        // Execute evaluation from ReviewConfigurationPage
                        if (dialog.Frame.Content is ReviewConfigurationPage reviewPage)
                        {
                            var finalConfig = reviewPage.BuildFinalConfiguration();
                            
                            // Save to AppData
                            _ = Task.Run(async () => await SaveEvaluationConfiguration(finalConfig));
                            
                            // Create UI entry immediately
                            var newEvaluation = new Evaluation
                            {
                                Id = finalConfig.Id,
                                Name = finalConfig.Name,
                                Status = finalConfig.Workflow == EvaluationWorkflow.ImportResults ? "Imported" : "Pending",
                                Progress = finalConfig.Workflow == EvaluationWorkflow.ImportResults ? 100 : 0,
                                Dataset = Path.GetFileName(finalConfig.Dataset?.SourcePath ?? "Unknown"),
                                Rows = finalConfig.Dataset?.ValidEntries ?? 0,
                                Criteria = finalConfig.Metrics?.CustomCriteria?.Count ?? 0
                            };
                            
                            AllEvaluations.Add(newEvaluation);
                            ApplySearchFilter(SearchBox.Text);
                            
                            dialog.Hide();
                        }
                    }
                };
                
                // Handle Back button clicks
                dialog.BackClicked += (_, __) =>
                {
                    if (currentStep == 5 || (currentStep == 4 && workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults))
                    {
                        // Go back from Review Configuration to Metrics Selection
                        // Skip metrics for ImportResults workflow
                        if (workflowSelectionData?.Workflow == EvaluationWorkflow.ImportResults)
                        {
                            wizardState.CurrentStep = 4;
                            dialog.Frame.Navigate(typeof(Evaluate.DatasetUploadPage), wizardState);
                        }
                        else
                        {
                            wizardState.CurrentStep = 5;
                            dialog.Frame.Navigate(typeof(MetricsSelectionPage), wizardState);
                        }
                    }
                    else if (currentStep == 4)
                    {
                        // Go back from Metrics Selection to Dataset Upload
                        wizardState.CurrentStep = 4;
                        dialog.Frame.Navigate(typeof(Evaluate.DatasetUploadPage), wizardState);
                    }
                    else if (currentStep == 3)
                    {
                        // Go back from Dataset Upload
                        if (workflowSelectionData?.Workflow == EvaluationWorkflow.TestModel)
                        {
                            // TestModel: go back to ModelConfiguration
                            wizardState.CurrentStep = 3;
                            dialog.Frame.Navigate(typeof(Evaluate.ModelConfigurationStep), wizardState);
                        }
                        else
                        {
                            // Other workflows: go back to WorkflowSelection
                            wizardState.CurrentStep = 2;
                            dialog.Frame.Navigate(typeof(WorkflowSelectionPage), wizardState);
                        }
                    }
                    else if (currentStep == 2)
                    {
                        // Go back to Step 2 (Workflow Selection)
                        wizardState.CurrentStep = 2;
                        dialog.Frame.Navigate(typeof(WorkflowSelectionPage), wizardState);
                    }
                    else if (currentStep == 1)
                    {
                        // Go back to Step 1 (Evaluation Type)
                        wizardState.CurrentStep = 1;
                        dialog.Frame.Navigate(typeof(SelectEvaluationTypePage), wizardState);
                    }
                    else
                    {
                        // Close dialog from Step 1
                        dialog.Hide();
                    }
                };
                
                // Initialize dialog state
                dialog.IsSecondaryButtonEnabled = false;
                dialog.IsPrimaryButtonEnabled = false;
                
                // Navigate to Step 1 AFTER setting up handlers with initial state
                wizardState.CurrentStep = 1;
                dialog.Frame.Navigate(typeof(SelectEvaluationTypePage), wizardState);
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // Handle any dialog exceptions
                System.Diagnostics.Debug.WriteLine($"Dialog error: {ex.Message}");
            }
            finally
            {
                // Re-enable button
                if (clickedButton != null)
                {
                    clickedButton.IsEnabled = true;
                }
            }
        }
        
        /// <summary>
        /// Saves an evaluation configuration using the new data model
        /// </summary>
        private async Task SaveEvaluationConfiguration(EvaluationConfiguration config)
        {
            try
            {
                var appData = await AppData.GetForApp();
                await appData.AddOrUpdateEvaluationAsync(config);
                
                System.Diagnostics.Debug.WriteLine($"Created evaluation: {config.Name} with type: {config.Type}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving evaluation: {ex.Message}");
            }
        }

        private void CreateNewEvaluation(EvaluationTypeData? typeData, Evaluate.ModelConfigurationData? modelConfigData)
        {
            if (typeData == null || modelConfigData == null) return;
            
            // Create new evaluation with the collected data
            var newEvaluation = new Evaluation
            {
                Id = Guid.NewGuid().ToString(),
                Name = modelConfigData.FinalEvaluationName,
                Status = "In Progress",
                Progress = 0,
                Dataset = "Pending", // Will be configured in future steps
                Rows = 0,
                Criteria = 1
            };
            
            AllEvaluations.Add(newEvaluation);
            ApplySearchFilter(SearchBox.Text);
            
            // Optional: Show success message or navigate to the new evaluation
            System.Diagnostics.Debug.WriteLine($"Created evaluation: {newEvaluation.Name} with type: {typeData.EvaluationType}");
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEvaluations();
        }


        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEvaluation != null)
            {
                try
                {
                    // Delete from database
                    var appData = await AppData.GetForApp();
                    await appData.DeleteEvaluationAsync(SelectedEvaluation.Id);
                    
                    // Remove from UI collections
                    AllEvaluations.Remove(SelectedEvaluation);
                    ApplySearchFilter(SearchBox.Text);
                    SelectedEvaluation = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting evaluation: {ex.Message}");
                }
            }
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement pagination logic
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement pagination logic
        }

        private void ItemsPerPageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Implement items per page logic
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}