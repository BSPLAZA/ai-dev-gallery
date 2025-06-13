using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace AIDevGallery.Pages.Evaluate.Dialogs
{
    public sealed partial class SelectEvaluationTypeDialog : ContentDialog, INotifyPropertyChanged
    {
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

        public SelectEvaluationTypeDialog()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void EvaluationType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                SelectedEvaluationType = tag;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
