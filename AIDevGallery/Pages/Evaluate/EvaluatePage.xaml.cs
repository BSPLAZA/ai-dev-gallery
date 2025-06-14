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


namespace AIDevGallery.Pages
{
    public class Evaluation
    {
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
                    CopyButton.IsEnabled = selectedEvaluation != null;
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
            CopyButton.Click += CopyButton_Click;
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
            // Example data
            AllEvaluations.Add(new Evaluation
            {
                Name = "gpt4o eval",
                Status = "Succeeded",
                Progress = 65,
                Dataset = "gpt4o eval",
                Rows = 1001,
                Criteria = 1
            });
            // Add more evaluations as needed

            ApplySearchFilter(SearchBox.Text);
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
                EvaluationTypeData? evaluationTypeData = null;
                Evaluate.EvaluationDetailsData? evaluationDetailsData = null;
                
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
                        dialog.UpdateProgress(1, "Choose Evaluation Type");
                    }
                    else if (args.Content is Evaluate.EvaluationDetailsStep detailsPage)
                    {
                        detailsPage.ValidationChanged += (isValid) =>
                        {
                            dialog.IsPrimaryButtonEnabled = isValid;
                        };
                        dialog.IsPrimaryButtonEnabled = detailsPage.IsValid;
                        dialog.IsSecondaryButtonEnabled = true;
                        dialog.PrimaryButtonText = "Next";
                        currentStep = 1;
                        // Update progress following system patterns
                        dialog.UpdateProgress(2, "Evaluation Setup");
                    }
                };
                
                // Handle Next/Create button clicks
                dialog.NextClicked += (_, __) =>
                {
                    if (currentStep == 0)
                    {
                        // Move from Step 1 to Step 2
                        if (dialog.Frame.Content is SelectEvaluationTypePage currentStep1Page)
                        {
                            evaluationTypeData = currentStep1Page.GetStepData();
                            dialog.Frame.Navigate(typeof(Evaluate.EvaluationDetailsStep));
                        }
                    }
                    else if (currentStep == 1)
                    {
                        // Move from Step 2 to Step 3 (future: criteria selection)
                        if (dialog.Frame.Content is Evaluate.EvaluationDetailsStep currentStep2Page)
                        {
                            evaluationDetailsData = currentStep2Page.GetStepData();
                            
                            // TODO: Navigate to Step 3 (Criteria Selection) when implemented
                            // For now, close dialog and show message about next steps
                            dialog.Hide();
                            
                            // Temporary: Show debug message about what would happen next
                            System.Diagnostics.Debug.WriteLine($"Next: Navigate to Step 3 (Criteria Selection) for evaluation: {evaluationDetailsData.EvaluationName}");
                        }
                    }
                };
                
                // Handle Back button clicks
                dialog.BackClicked += (_, __) =>
                {
                    if (currentStep == 1)
                    {
                        // Go back to Step 1
                        dialog.Frame.Navigate(typeof(SelectEvaluationTypePage));
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
                
                // Navigate to Step 1 AFTER setting up handlers
                dialog.Frame.Navigate(typeof(SelectEvaluationTypePage));
                
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
        
        private void CreateNewEvaluation(EvaluationTypeData? typeData, Evaluate.EvaluationDetailsData? detailsData)
        {
            if (typeData == null || detailsData == null) return;
            
            // Create new evaluation with the collected data
            var newEvaluation = new Evaluation
            {
                Name = detailsData.FinalEvaluationName,
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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEvaluation != null)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText($"Evaluation: {SelectedEvaluation.Name}");
                Clipboard.SetContentWithOptions(dataPackage, null);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEvaluation != null)
            {
                AllEvaluations.Remove(SelectedEvaluation);
                ApplySearchFilter(SearchBox.Text);
                SelectedEvaluation = null;
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

    public enum EvaluationType
    {
        ImageDescription,
        TextSummarization,
        Translation,
        QuestionAnswering,
        CustomTask
    }
}