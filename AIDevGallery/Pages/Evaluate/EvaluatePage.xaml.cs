using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;

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

        private string? selectedEvaluationType;
        public string? SelectedEvaluationType
        {
            get => selectedEvaluationType;
            set
            {
                if (selectedEvaluationType != value)
                {
                    selectedEvaluationType = value;
                    OnPropertyChanged(nameof(SelectedEvaluationType));
                    OnPropertyChanged(nameof(IsEvaluationTypeSelected));
                }
            }
        }

        public bool IsEvaluationTypeSelected => !string.IsNullOrEmpty(SelectedEvaluationType);

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

        private void NewEvaluationButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedEvaluationType = null;
            EvaluationWizardDialog.XamlRoot = this.Content.XamlRoot;
            _ = EvaluationWizardDialog.ShowAsync();
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

        private void EvaluationType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                SelectedEvaluationType = tag;
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
}