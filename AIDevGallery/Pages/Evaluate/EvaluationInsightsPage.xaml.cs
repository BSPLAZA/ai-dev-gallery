// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIDevGallery.Models;
using AIDevGallery.Services.Evaluate;
using AIDevGallery.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;

namespace AIDevGallery.Pages.Evaluate
{
    public sealed partial class EvaluationInsightsPage : Page
    {
        private IEvaluationResultsStore _evaluationStore = default!;
        private EvaluationInsightsViewModel _viewModel = default!;
        
        public EvaluationInsightsPage()
        {
            this.InitializeComponent();
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Create the evaluation store service (same as EvaluatePage does)
            _evaluationStore = new EvaluationResultsStore();
            
            if (e.Parameter is string evaluationId)
            {
                await LoadEvaluationAsync(evaluationId);
            }
            else
            {
                ShowEmptyState();
            }
        }
        
        private async Task LoadEvaluationAsync(string evaluationId)
        {
            try
            {
                var evaluation = await _evaluationStore.GetEvaluationByIdAsync(evaluationId);
                
                if (evaluation == null)
                {
                    ShowEmptyState();
                    return;
                }
                
                _viewModel = new EvaluationInsightsViewModel(evaluation);
                UpdateUI();
                
                // Create chart visualization
                CreateChartVisualization();
                
                // Show content
                LoadingPanel.Visibility = Visibility.Collapsed;
                ContentScrollViewer.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
            catch
            {
                ShowEmptyState();
            }
        }
        
        private void UpdateUI()
        {
            // Update breadcrumb
            EvaluationNameText.Text = _viewModel.Name;
            
            // Update title and metadata
            EvaluationTitle.Text = _viewModel.Name;
            ModelNameRun.Text = _viewModel.ModelName;
            DatasetNameRun.Text = _viewModel.DatasetName;
            TimestampText.Text = _viewModel.Timestamp.ToString("MMM dd, yyyy h:mm tt");
            
            // Update status
            StatusText.Text = _viewModel.Status.ToString();
            UpdateStatusIcon();
            
            // Update overall score
            StarRatingText.Text = _viewModel.StarRating;
            ScoreText.Text = _viewModel.AverageScore.ToString("F1") + "/5";
            
            // Update metric cards
            ImagesProcessedText.Text = _viewModel.ItemCount.ToString("N0");
            ProcessingStatusText.Text = _viewModel.Status == EvaluationStatus.Completed ? "100% Complete" : 
                                       _viewModel.Status == EvaluationStatus.Running ? $"{_viewModel.ProgressPercentage ?? 0}% Complete" : 
                                       "";
            AverageScoreText.Text = _viewModel.AverageScore.ToString("F1") + "/5";
            CriteriaCountText.Text = _viewModel.CriteriaScores.Count.ToString();
            DurationText.Text = _viewModel.Duration?.ToString(@"h\h\ m\m") ?? "N/A";
            
            // Update table view
            UpdateTableView();
            
            // Show statistical summary if detailed results available
            if (_viewModel.HasDetailedResults)
            {
                UpdateStatisticalSummary();
                StatisticalSummaryCard.Visibility = Visibility.Visible;
            }
        }
        
        private void UpdateStatusIcon()
        {
            switch (_viewModel.Status)
            {
                case EvaluationStatus.Completed:
                    StatusIcon.Glyph = "\uE73E"; // Checkmark
                    StatusIcon.Foreground = (Brush)Application.Current.Resources["SystemFillColorSuccessBrush"];
                    break;
                case EvaluationStatus.Running:
                    StatusIcon.Glyph = "\uE768"; // Clock
                    StatusIcon.Foreground = (Brush)Application.Current.Resources["SystemFillColorCautionBrush"];
                    break;
                case EvaluationStatus.Failed:
                    StatusIcon.Glyph = "\uE783"; // Error
                    StatusIcon.Foreground = (Brush)Application.Current.Resources["SystemFillColorCriticalBrush"];
                    break;
                default:
                    StatusIcon.Glyph = "\uE946"; // Info
                    StatusIcon.Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"];
                    break;
            }
        }
        
        private void CreateChartVisualization()
        {
            ChartContentGrid.Children.Clear();
            
            // Create a simple bar chart using XAML shapes
            var chartGrid = new Grid();
            chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            chartGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Chart area
            var chartCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinHeight = 300
            };
            
            // Add to grid
            Grid.SetRow(chartCanvas, 0);
            chartGrid.Children.Add(chartCanvas);
            
            // Add axis labels
            var axisGrid = new Grid
            {
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(axisGrid, 1);
            
            // X-axis labels (1-5 scale)
            var xLabels = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Spacing = 50
            };
            
            for (int i = 1; i <= 5; i++)
            {
                xLabels.Children.Add(new TextBlock
                {
                    Text = i.ToString(),
                    Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                });
            }
            
            axisGrid.Children.Add(xLabels);
            chartGrid.Children.Add(axisGrid);
            
            // Draw bars after layout is updated
            chartCanvas.Loaded += (s, e) => DrawBars(chartCanvas);
            
            ChartContentGrid.Children.Add(chartGrid);
        }
        
        private void DrawBars(Canvas canvas)
        {
            canvas.Children.Clear();
            
            if (_viewModel.CriteriaScores.Count == 0)
                return;
            
            var canvasWidth = canvas.ActualWidth;
            var canvasHeight = canvas.ActualHeight;
            var barHeight = Math.Min(40, (canvasHeight - 40) / _viewModel.CriteriaScores.Count);
            var maxScore = 5.0;
            var chartWidth = canvasWidth - 150; // Leave space for labels
            
            int index = 0;
            foreach (var criterion in _viewModel.CriteriaScores)
            {
                var y = index * (barHeight + 10) + 20;
                
                // Criterion label
                var label = new TextBlock
                {
                    Text = criterion.Key,
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    Width = 120,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Canvas.SetLeft(label, 10);
                Canvas.SetTop(label, y + (barHeight - 20) / 2);
                canvas.Children.Add(label);
                
                // Bar
                var barWidth = (criterion.Value / maxScore) * chartWidth;
                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(GetScoreColor(criterion.Value)),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(bar, 130);
                Canvas.SetTop(bar, y);
                canvas.Children.Add(bar);
                
                // Score label
                var scoreLabel = new TextBlock
                {
                    Text = criterion.Value.ToString("F1"),
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };
                Canvas.SetLeft(scoreLabel, 130 + barWidth + 8);
                Canvas.SetTop(scoreLabel, y + (barHeight - 20) / 2);
                canvas.Children.Add(scoreLabel);
                
                // Performance badge
                var badge = new Border
                {
                    Background = new SolidColorBrush(GetScoreColor(criterion.Value)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8, 4, 8, 4)
                };
                
                var badgeText = new TextBlock
                {
                    Text = GetPerformanceText(criterion.Value),
                    FontSize = 12,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Colors.White)
                };
                
                badge.Child = badgeText;
                Canvas.SetRight(badge, 10);
                Canvas.SetTop(badge, y + (barHeight - 24) / 2);
                canvas.Children.Add(badge);
                
                index++;
            }
        }
        
        private Color GetScoreColor(double score)
        {
            return score switch
            {
                >= 4.5 => Color.FromArgb(255, 76, 175, 80),    // Green
                >= 3.5 => Color.FromArgb(255, 33, 150, 243),   // Blue
                >= 2.5 => Color.FromArgb(255, 255, 193, 7),    // Yellow
                _ => Color.FromArgb(255, 255, 87, 34)          // Orange
            };
        }
        
        private string GetPerformanceText(double score)
        {
            return score switch
            {
                >= 4.5 => "Excellent",
                >= 3.5 => "Good",
                >= 2.5 => "Fair",
                _ => "Needs Improvement"
            };
        }
        
        private void UpdateTableView()
        {
            var tableItems = new List<CriteriaTableItem>();
            
            foreach (var criterion in _viewModel.CriteriaScores)
            {
                tableItems.Add(new CriteriaTableItem
                {
                    Name = criterion.Key,
                    Score = criterion.Value,
                    ScoreText = criterion.Value.ToString("F1"),
                    StatusText = GetPerformanceText(criterion.Value),
                    StatusGlyph = "\uE73E", // Checkmark
                    StatusBrush = new SolidColorBrush(GetScoreColor(criterion.Value))
                });
            }
            
            CriteriaTableRepeater.ItemsSource = tableItems;
        }
        
        private void UpdateStatisticalSummary()
        {
            // This will be implemented when individual results are available
            // For now, just show aggregate stats
        }
        
        private void ShowEmptyState()
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            ContentScrollViewer.Visibility = Visibility.Collapsed;
            EmptyStatePanel.Visibility = Visibility.Visible;
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        
        private void ViewToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ChartViewToggle)
            {
                ChartViewToggle.IsChecked = true;
                TableViewToggle.IsChecked = false;
                ChartView.Visibility = Visibility.Visible;
                TableView.Visibility = Visibility.Collapsed;
            }
            else if (sender == TableViewToggle)
            {
                TableViewToggle.IsChecked = true;
                ChartViewToggle.IsChecked = false;
                TableView.Visibility = Visibility.Visible;
                ChartView.Visibility = Visibility.Collapsed;
            }
        }
        
        private async void CopyChart_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement chart copy to clipboard
            await Task.CompletedTask;
        }
        
        private async void SaveChartAsImage_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement save chart as image
            await Task.CompletedTask;
        }
        
        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            // Flyout will handle the click
        }
        
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement CSV export
            await Task.CompletedTask;
        }
        
        private async void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement JSON export
            await Task.CompletedTask;
        }
        
        private void Share_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement share functionality (future)
        }
        
        private async void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement print report
            await Task.CompletedTask;
        }
    }
    
    // Helper class for table view
    public class CriteriaTableItem
    {
        public string Name { get; set; } = "";
        public double Score { get; set; }
        public string ScoreText { get; set; } = "";
        public string StatusText { get; set; } = "";
        public string StatusGlyph { get; set; } = "";
        public Brush StatusBrush { get; set; } = new SolidColorBrush(Colors.Gray);
    }
}