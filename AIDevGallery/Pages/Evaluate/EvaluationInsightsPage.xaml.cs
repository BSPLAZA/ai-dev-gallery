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
                    System.Diagnostics.Debug.WriteLine($"Evaluation not found for ID: {evaluationId}");
                    ShowEmptyState();
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"Loaded evaluation: {evaluation.Name}");
                System.Diagnostics.Debug.WriteLine($"  Criteria Scores: {evaluation.CriteriaScores.Count}");
                System.Diagnostics.Debug.WriteLine($"  Item Results: {evaluation.ItemResults?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  Folder Statistics: {evaluation.FolderStatistics?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  Has Detailed Results: {evaluation.HasDetailedResults}");
                
                // Check if we have at least criteria scores to display
                if (evaluation.CriteriaScores.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No criteria scores available");
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading evaluation: {ex.Message}");
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
            
            // Show/hide status info badge with appropriate tooltip
            if (_viewModel.Status == EvaluationStatus.Imported)
            {
                StatusInfoBadge.Visibility = Visibility.Visible;
                // Set tooltip on the parent Grid container
                if (StatusInfoBadge.Parent is Grid parentGrid)
                {
                    ToolTipService.SetToolTip(parentGrid, 
                        "This evaluation was imported from a JSONL file. Individual scores are aggregated from the import data.");
                }
            }
            else if (_viewModel.Status == EvaluationStatus.Completed)
            {
                StatusInfoBadge.Visibility = Visibility.Visible;
                // Set tooltip on the parent Grid container
                if (StatusInfoBadge.Parent is Grid parentGrid)
                {
                    ToolTipService.SetToolTip(parentGrid, 
                        $"Evaluation completed successfully on {_viewModel.Timestamp:MMM dd, yyyy}. All criteria have been evaluated.");
                }
            }
            else if (_viewModel.Status == EvaluationStatus.Running)
            {
                StatusInfoBadge.Visibility = Visibility.Visible;
                // Set tooltip on the parent Grid container
                if (StatusInfoBadge.Parent is Grid parentGrid)
                {
                    ToolTipService.SetToolTip(parentGrid, 
                        "Evaluation is currently in progress. Results will update as items are processed.");
                }
            }
            else
            {
                StatusInfoBadge.Visibility = Visibility.Collapsed;
            }
            
            // Update overall score
            StarRatingText.Text = _viewModel.StarRating;
            ScoreText.Text = _viewModel.AverageScore.ToString("F1") + "/5";
            
            // Update metric cards
            // Images processed logic based on workflow type and status
            int processedCount = 0;
            string processingStatus = "";
            
            if (_viewModel.WorkflowType == EvaluationWorkflow.ImportResults)
            {
                // For imports, show the count of items that have results
                processedCount = _viewModel.ItemResults?.Count ?? _viewModel.DatasetItemCount;
                processingStatus = "Imported";
            }
            else if (_viewModel.Status == EvaluationStatus.Completed)
            {
                // For completed evaluations, show items with criteria scores
                processedCount = _viewModel.ItemResults?.Count(r => r.CriteriaScores.Any()) ?? _viewModel.DatasetItemCount;
                processingStatus = "100% Complete";
            }
            else if (_viewModel.Status == EvaluationStatus.Running)
            {
                // For running evaluations, calculate based on progress
                var progress = _viewModel.ProgressPercentage ?? 0;
                processedCount = (int)(_viewModel.DatasetItemCount * progress / 100.0);
                processingStatus = $"{progress}% Complete";
            }
            else
            {
                processedCount = 0;
                processingStatus = "Not Started";
            }
            
            ImagesProcessedText.Text = processedCount.ToString("N0");
            ProcessingStatusText.Text = processingStatus;
            AverageScoreText.Text = _viewModel.AverageScore.ToString("F1") + "/5";
            CriteriaCountText.Text = _viewModel.CriteriaScores.Count.ToString();
            DurationText.Text = _viewModel.Duration?.ToString(@"h\h\ m\m") ?? "N/A";
            
            // Update table view
            UpdateTableView();
            
            // Update folder view if available
            if (_viewModel.HasFolderStatistics)
            {
                UpdateFolderView();
                FolderViewToggle.Visibility = Visibility.Visible;
            }
            else
            {
                FolderViewToggle.Visibility = Visibility.Collapsed;
            }
            
            // Show image preview area if we have individual results
            if (_viewModel.ItemResults != null && _viewModel.ItemResults.Count > 0)
            {
                UpdateImagePreviewArea();
                ImagePreviewArea.Visibility = Visibility.Visible;
            }
            else
            {
                ImagePreviewArea.Visibility = Visibility.Collapsed;
            }
            
            // Show statistical summary - always show if we have criteria scores
            if (_viewModel.CriteriaScores != null && _viewModel.CriteriaScores.Count > 0)
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
            var barHeight = Math.Min(40, (canvasHeight - 60) / _viewModel.CriteriaScores.Count); // More space for axis
            var maxScore = 5.0;
            var leftMargin = 130; // Space for criterion labels
            var rightMargin = 150; // Space for badges
            var chartWidth = canvasWidth - leftMargin - rightMargin;
            var chartStartY = 20;
            var chartHeight = canvasHeight - 60; // Leave space for axis labels
            
            // Add grid lines and labels for X-axis (1-5 scale)
            for (int i = 1; i <= 5; i++)
            {
                var x = leftMargin + (i - 1) * (chartWidth / 4);
                
                // Vertical grid line
                var gridLine = new Line
                {
                    X1 = x,
                    Y1 = chartStartY,
                    X2 = x,
                    Y2 = chartStartY + chartHeight,
                    Stroke = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"],
                    StrokeThickness = 1,
                    Opacity = 0.2
                };
                
                // Make the max score line more prominent
                if (i == 5)
                {
                    gridLine.Stroke = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128));
                    gridLine.StrokeDashArray = new DoubleCollection { 2, 2 };
                    gridLine.Opacity = 1;
                }
                
                canvas.Children.Add(gridLine);
                
                // X-axis label
                var label = new TextBlock
                {
                    Text = i.ToString(),
                    Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(label, x - 5);
                Canvas.SetTop(label, chartStartY + chartHeight + 5);
                canvas.Children.Add(label);
            }
            
            // Draw criteria bars
            int index = 0;
            foreach (var criterion in _viewModel.CriteriaScores)
            {
                var y = chartStartY + index * (barHeight + 10);
                
                // Criterion label
                var label = new TextBlock
                {
                    Text = criterion.Key,
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    Width = leftMargin - 20,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    TextAlignment = TextAlignment.Right
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
                Canvas.SetLeft(bar, leftMargin);
                Canvas.SetTop(bar, y);
                canvas.Children.Add(bar);
                
                // Score label
                var scoreLabel = new TextBlock
                {
                    Text = criterion.Value.ToString("F1"),
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                };
                Canvas.SetLeft(scoreLabel, leftMargin + barWidth + 8);
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
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                };
                
                badge.Child = badgeText;
                Canvas.SetLeft(badge, leftMargin + chartWidth + 20);
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
        
        private void UpdateFolderView()
        {
            if (_viewModel.FolderStatistics == null) return;
            
            var folderItems = new List<FolderTableItem>();
            
            foreach (var folder in _viewModel.FolderStatistics)
            {
                var avgScore = folder.Value.AverageScores.Values.Any() 
                    ? folder.Value.AverageScores.Values.Average() 
                    : 0.0;
                
                var folderItem = new FolderTableItem
                {
                    FolderPath = folder.Key,
                    ItemCount = folder.Value.ItemCount,
                    ItemCountText = $"{folder.Value.ItemCount} items",
                    AverageScore = avgScore,
                    AverageScoreText = avgScore.ToString("F1") + "/5",
                    ScoreBrush = new SolidColorBrush(GetScoreColor(avgScore))
                };
                
                // Add criteria scores for this folder
                foreach (var criteriaScore in folder.Value.AverageScores)
                {
                    folderItem.CriteriaScores.Add(new FolderCriteriaScore
                    {
                        Name = criteriaScore.Key,
                        Score = criteriaScore.Value,
                        ScoreText = criteriaScore.Value.ToString("F1"),
                        StatusBrush = new SolidColorBrush(GetScoreColor(criteriaScore.Value))
                    });
                }
                
                // Sort criteria scores by score descending
                folderItem.CriteriaScores = folderItem.CriteriaScores.OrderByDescending(c => c.Score).ToList();
                
                folderItems.Add(folderItem);
            }
            
            // Sort by average score descending
            FolderStatsRepeater.ItemsSource = folderItems.OrderByDescending(f => f.AverageScore).ToList();
        }
        
        private void UpdateStatisticalSummary()
        {
            // Clear existing content
            StatisticalSummaryCard.Children.Clear();
            
            // Add summary header
            var headerText = new TextBlock
            {
                Text = "Statistical Summary by Criterion",
                Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"],
                Margin = new Thickness(16, 16, 16, 12)
            };
            Grid.SetColumnSpan(headerText, 5);
            StatisticalSummaryCard.Children.Add(headerText);
            
            // If we have detailed results, show per-criterion statistics
            if (_viewModel.HasDetailedResults && _viewModel.ItemResults != null && _viewModel.ItemResults.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Using detailed results for per-criterion statistics ({_viewModel.ItemResults.Count} items)");
                
                // Create a scrollable panel for all criteria
                var scrollViewer = new ScrollViewer
                {
                    MaxHeight = 300,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Margin = new Thickness(0, 0, 0, 16)
                };
                Grid.SetRow(scrollViewer, 1);
                Grid.SetColumnSpan(scrollViewer, 5);
                
                var criteriaPanel = new StackPanel { Spacing = 12 };
                
                foreach (var criterion in _viewModel.CriteriaScores.Keys)
                {
                    var stats = _viewModel.GetCriterionStatistics(criterion);
                    if (stats != null)
                    {
                        // Create a card for this criterion
                        var criterionCard = new Grid
                        {
                            Style = (Style)Application.Current.Resources["CardGridStyle"],
                            Padding = new Thickness(12),
                            Margin = new Thickness(16, 0, 16, 0)
                        };
                        
                        criterionCard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        criterionCard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        criterionCard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        criterionCard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        criterionCard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        criterionCard.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        
                        // Criterion name
                        var nameText = new TextBlock
                        {
                            Text = criterion,
                            Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"],
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 16, 0)
                        };
                        Grid.SetColumn(nameText, 0);
                        criterionCard.Children.Add(nameText);
                        
                        // Stats
                        AddCriterionStat(criterionCard, 1, "Mean", stats.Mean.ToString("F2"));
                        AddCriterionStat(criterionCard, 2, "Std Dev", stats.StandardDeviation.ToString("F2"));
                        AddCriterionStat(criterionCard, 3, "Min", stats.Min.ToString("F2"));
                        AddCriterionStat(criterionCard, 4, "Max", stats.Max.ToString("F2"));
                        AddCriterionStat(criterionCard, 5, "Count", stats.Count.ToString());
                        
                        criteriaPanel.Children.Add(criterionCard);
                    }
                }
                
                scrollViewer.Content = criteriaPanel;
                StatisticalSummaryCard.Children.Add(scrollViewer);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No detailed results available, showing overall statistics");
                
                // Show overall statistics only
                var scores = _viewModel.CriteriaScores.Values.ToList();
                if (scores.Count == 0) return;
                
                var mean = scores.Average();
                var min = scores.Min();
                var max = scores.Max();
                var stdDev = CalculateStandardDeviation(scores);
                
                // Create overall stats row
                AddStatPanel(0, "Overall Mean", mean.ToString("F2"), 1);
                AddStatPanel(1, "Overall Std Dev", stdDev.ToString("F2"), 1);
                AddStatPanel(2, "Overall Min", min.ToString("F2"), 1);
                AddStatPanel(3, "Overall Max", max.ToString("F2"), 1);
                AddStatPanel(4, "Criteria Count", scores.Count.ToString(), 1);
            }
        }
        
        private void AddCriterionStat(Grid container, int column, string label, string value)
        {
            var panel = new StackPanel 
            { 
                Spacing = 2,
                Margin = new Thickness(8, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            var labelText = new TextBlock
            {
                Text = label,
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            var valueText = new TextBlock
            {
                Text = value,
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            panel.Children.Add(labelText);
            panel.Children.Add(valueText);
            
            Grid.SetColumn(panel, column);
            container.Children.Add(panel);
        }
        
        private void AddStatPanel(int column, string label, string value, int row)
        {
            var panel = new StackPanel 
            { 
                Spacing = 4,
                Margin = new Thickness(16, 8, 16, 16)
            };
            
            var labelText = new TextBlock
            {
                Text = label,
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };
            
            var valueText = new TextBlock
            {
                Text = value,
                Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
            };
            
            panel.Children.Add(labelText);
            panel.Children.Add(valueText);
            
            Grid.SetColumn(panel, column);
            Grid.SetRow(panel, row);
            StatisticalSummaryCard.Children.Add(panel);
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
                FolderViewToggle.IsChecked = false;
                ChartView.Visibility = Visibility.Visible;
                TableView.Visibility = Visibility.Collapsed;
                FolderView.Visibility = Visibility.Collapsed;
            }
            else if (sender == TableViewToggle)
            {
                TableViewToggle.IsChecked = true;
                ChartViewToggle.IsChecked = false;
                FolderViewToggle.IsChecked = false;
                TableView.Visibility = Visibility.Visible;
                ChartView.Visibility = Visibility.Collapsed;
                FolderView.Visibility = Visibility.Collapsed;
            }
            else if (sender == FolderViewToggle)
            {
                FolderViewToggle.IsChecked = true;
                ChartViewToggle.IsChecked = false;
                TableViewToggle.IsChecked = false;
                FolderView.Visibility = Visibility.Visible;
                ChartView.Visibility = Visibility.Collapsed;
                TableView.Visibility = Visibility.Collapsed;
            }
        }
        
        private async void CopyChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Find the chart canvas
                var chartCanvas = FindChartCanvas();
                if (chartCanvas == null)
                {
                    System.Diagnostics.Debug.WriteLine("Chart canvas not found");
                    return;
                }
                
                // Create a render target bitmap
                var renderTargetBitmap = new Microsoft.UI.Xaml.Media.Imaging.RenderTargetBitmap();
                await renderTargetBitmap.RenderAsync(ChartView);
                
                // Get pixels from the rendered bitmap
                var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                // IBuffer doesn't have ToArray, just use the buffer directly
                
                // Create a software bitmap
                var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                    pixelBuffer,
                    BitmapPixelFormat.Bgra8,
                    renderTargetBitmap.PixelWidth,
                    renderTargetBitmap.PixelHeight,
                    BitmapAlphaMode.Premultiplied);
                
                // Create a bitmap encoder
                var stream = new InMemoryRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
                
                // Create data package
                var dataPackage = new DataPackage();
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
                dataPackage.Properties.Title = $"{_viewModel.Name} - Evaluation Chart";
                
                // Copy to clipboard
                Clipboard.SetContent(dataPackage);
                
                // Show a subtle confirmation (could add a TeachingTip here)
                System.Diagnostics.Debug.WriteLine("Chart copied to clipboard");
                
                // Optional: Show confirmation in UI
                CopyChartButton.Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4,
                    Children =
                    {
                        new FontIcon { Glyph = "\uE73E", FontSize = 14 }, // Checkmark
                        new TextBlock { Text = "Copied!" }
                    }
                };
                
                // Reset button after 2 seconds
                await Task.Delay(2000);
                CopyChartButton.Content = new FontIcon { Glyph = "\uE8C8", FontSize = 14 };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying chart: {ex.Message}");
            }
        }
        
        private Canvas? FindChartCanvas()
        {
            // Helper method to find the chart canvas in the visual tree
            return ChartContentGrid.Children.OfType<Grid>().FirstOrDefault()?.Children.OfType<Canvas>().FirstOrDefault();
        }
        
        private async void SaveChartAsImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
                savePicker.FileTypeChoices.Add("JPEG Image", new List<string>() { ".jpg", ".jpeg" });
                savePicker.SuggestedFileName = $"{_viewModel.Name}_evaluation_chart_{DateTime.Now:yyyyMMdd}";
                
                // Get the window handle
                var window = App.MainWindow;
                if (window == null) return;
                
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Create a render target bitmap
                    var renderTargetBitmap = new Microsoft.UI.Xaml.Media.Imaging.RenderTargetBitmap();
                    await renderTargetBitmap.RenderAsync(ChartView);
                    
                    // Get pixels from the rendered bitmap
                    var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                    
                    // Create a software bitmap
                    var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                        pixelBuffer,
                        BitmapPixelFormat.Bgra8,
                        renderTargetBitmap.PixelWidth,
                        renderTargetBitmap.PixelHeight,
                        BitmapAlphaMode.Premultiplied);
                    
                    // Save to file
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoderId = file.FileType.ToLower() == ".png" 
                            ? BitmapEncoder.PngEncoderId 
                            : BitmapEncoder.JpegEncoderId;
                            
                        var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);
                        encoder.SetSoftwareBitmap(softwareBitmap);
                        
                        // Set image metadata
                        var properties = new BitmapPropertySet();
                        properties.Add("System.Comment", new BitmapTypedValue(
                            $"Evaluation Chart for {_viewModel.Name} - Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}", 
                            Windows.Foundation.PropertyType.String));
                        
                        await encoder.BitmapProperties.SetPropertiesAsync(properties);
                        await encoder.FlushAsync();
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Chart saved to: {file.Path}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving chart: {ex.Message}");
            }
        }
        
        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            // Flyout will handle the click
        }
        
        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
                savePicker.SuggestedFileName = $"{_viewModel.Name}_evaluation_results_{DateTime.Now:yyyyMMdd}";
                
                // Get the window handle
                var window = App.MainWindow;
                if (window == null) return;
                
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var csvContent = GenerateCsvContent();
                    await Windows.Storage.FileIO.WriteTextAsync(file, csvContent);
                    
                    // Show success notification (optional)
                    System.Diagnostics.Debug.WriteLine($"CSV exported to: {file.Path}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting CSV: {ex.Message}");
            }
        }
        
        private async void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
                savePicker.SuggestedFileName = $"{_viewModel.Name}_evaluation_results_{DateTime.Now:yyyyMMdd}";
                
                // Get the window handle
                var window = App.MainWindow;
                if (window == null) return;
                
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var jsonContent = GenerateJsonContent();
                    await Windows.Storage.FileIO.WriteTextAsync(file, jsonContent);
                    
                    // Show success notification (optional)
                    System.Diagnostics.Debug.WriteLine($"JSON exported to: {file.Path}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting JSON: {ex.Message}");
            }
        }
        
        private void Share_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement share functionality (future)
        }
        
        private void FolderItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is FolderTableItem folderItem)
            {
                // Toggle expansion state
                folderItem.IsExpanded = !folderItem.IsExpanded;
            }
        }
        
        private void UpdateImagePreviewArea()
        {
            // This method prepares the image preview area when individual results are available
            // Currently, individual results are not persisted, but the UI is ready for when they are
            if (_viewModel.ItemResults != null && _viewModel.ItemResults.Count > 0)
            {
                var resultCount = _viewModel.ItemResults.Count;
                var avgScore = _viewModel.ItemResults.Average(r => r.AverageScore);
                ImageResultsSummary.Text = $"{resultCount} images evaluated • Average score: {avgScore:F1}/5";
                
                System.Diagnostics.Debug.WriteLine($"Individual results available: {resultCount} items");
            }
            else
            {
                ImageResultsSummary.Text = "No individual results available";
            }
        }
        
        private async void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // For now, export to HTML and let the user print from browser
                // WinUI 3 printing is complex and requires significant setup
                
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("HTML Report", new List<string>() { ".html" });
                savePicker.SuggestedFileName = $"{_viewModel.Name}_evaluation_report_{DateTime.Now:yyyyMMdd}";
                
                // Get the window handle
                var window = App.MainWindow;
                if (window == null) return;
                
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var htmlContent = GenerateHtmlReport();
                    await Windows.Storage.FileIO.WriteTextAsync(file, htmlContent);
                    
                    // Optionally open the file
                    await Windows.System.Launcher.LaunchFileAsync(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating print report: {ex.Message}");
            }
        }
        
        private string GenerateHtmlReport()
        {
            var html = new System.Text.StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine($"<title>Evaluation Report - {_viewModel.Name}</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; color: #333; }");
            html.AppendLine("h1 { color: #0078D4; margin-bottom: 0.5em; }");
            html.AppendLine("h2 { color: #005A9E; margin-bottom: 0.5em; }");
            html.AppendLine("h3 { color: #333; margin-top: 1.5em; margin-bottom: 0.5em; }");
            html.AppendLine(".metadata { background-color: #F5F5F5; padding: 20px; border-radius: 8px; margin: 20px 0; }");
            html.AppendLine(".metadata p { margin: 5px 0; }");
            html.AppendLine(".metrics-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin: 20px 0; }");
            html.AppendLine(".metric-card { background-color: #FFFFFF; border: 1px solid #E0E0E0; padding: 20px; border-radius: 8px; text-align: center; }");
            html.AppendLine(".metric-card h4 { margin: 0; color: #666; font-size: 0.9em; font-weight: normal; }");
            html.AppendLine(".metric-card .value { font-size: 1.8em; font-weight: bold; color: #0078D4; margin: 5px 0; }");
            html.AppendLine(".metric-card .status { font-size: 0.85em; color: #4CAF50; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            html.AppendLine("th { background-color: #0078D4; color: white; padding: 12px; text-align: left; font-weight: 600; }");
            html.AppendLine("td { border-bottom: 1px solid #E0E0E0; padding: 12px; }");
            html.AppendLine("tr:hover { background-color: #F5F5F5; }");
            html.AppendLine(".score { font-weight: bold; }");
            html.AppendLine(".badge { display: inline-block; padding: 4px 12px; border-radius: 4px; font-weight: 600; font-size: 0.85em; }");
            html.AppendLine(".excellent { background-color: #4CAF50; color: white; }");
            html.AppendLine(".good { background-color: #2196F3; color: white; }");
            html.AppendLine(".fair { background-color: #FFC107; color: #333; }");
            html.AppendLine(".needs-improvement { background-color: #FF5722; color: white; }");
            html.AppendLine(".chart-container { margin: 20px 0; padding: 20px; background-color: #FFF; border: 1px solid #E0E0E0; border-radius: 8px; }");
            html.AppendLine(".bar-chart { margin: 20px 0; }");
            html.AppendLine(".bar-row { display: flex; align-items: center; margin: 8px 0; }");
            html.AppendLine(".bar-label { width: 150px; text-align: right; padding-right: 15px; font-size: 0.95em; }");
            html.AppendLine(".bar-container { flex: 1; background-color: #F0F0F0; height: 28px; position: relative; border-radius: 4px; overflow: hidden; }");
            html.AppendLine(".bar-fill { height: 100%; transition: width 0.3s; }");
            html.AppendLine(".bar-value { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); font-weight: bold; font-size: 0.9em; }");
            html.AppendLine(".rating-stars { color: #FFB400; font-size: 1.2em; }");
            html.AppendLine(".print-info { margin-top: 40px; padding-top: 20px; border-top: 2px solid #E0E0E0; color: #666; font-size: 0.9em; }");
            html.AppendLine("@media print {");
            html.AppendLine("  body { margin: 20px; }");
            html.AppendLine("  .no-print { display: none; }");
            html.AppendLine("  .page-break { page-break-before: always; }");
            html.AppendLine("  h1, h2, h3 { page-break-after: avoid; }");
            html.AppendLine("  table { page-break-inside: avoid; }");
            html.AppendLine("}");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine("<h1>Evaluation Insights Report</h1>");
            html.AppendLine($"<h2>{_viewModel.Name}</h2>");
            
            // Metadata section with improved styling
            html.AppendLine("<div class='metadata'>");
            html.AppendLine($"<p><strong>Model:</strong> {_viewModel.ModelName}</p>");
            html.AppendLine($"<p><strong>Dataset:</strong> {_viewModel.DatasetName}</p>");
            html.AppendLine($"<p><strong>Date:</strong> {_viewModel.Timestamp:MMMM dd, yyyy h:mm tt}</p>");
            html.AppendLine($"<p><strong>Status:</strong> {_viewModel.Status}");
            
            // Add status description based on type
            if (_viewModel.Status == EvaluationStatus.Imported)
            {
                html.AppendLine(" <em>(Imported from JSONL file)</em>");
            }
            else if (_viewModel.Status == EvaluationStatus.Completed)
            {
                html.AppendLine(" <em>(All criteria evaluated successfully)</em>");
            }
            
            html.AppendLine("</p>");
            html.AppendLine($"<p><strong>Overall Score:</strong> <span class='rating-stars'>{_viewModel.StarRating}</span> <span class='score'>{_viewModel.AverageScore:F1}/5.0</span></p>");
            html.AppendLine("</div>");
            
            // Key Metrics Grid
            html.AppendLine("<h3>Key Metrics</h3>");
            html.AppendLine("<div class='metrics-grid'>");
            
            // Images Processed
            html.AppendLine("<div class='metric-card'>");
            html.AppendLine("<h4>Images Processed</h4>");
            html.AppendLine($"<div class='value'>{ImagesProcessedText.Text}</div>");
            html.AppendLine($"<div class='status'>{ProcessingStatusText.Text}</div>");
            html.AppendLine("</div>");
            
            // Average Score
            html.AppendLine("<div class='metric-card'>");
            html.AppendLine("<h4>Average Score</h4>");
            html.AppendLine($"<div class='value'>{_viewModel.AverageScore:F1}/5</div>");
            html.AppendLine("</div>");
            
            // Criteria Evaluated
            html.AppendLine("<div class='metric-card'>");
            html.AppendLine("<h4>Criteria Evaluated</h4>");
            html.AppendLine($"<div class='value'>{_viewModel.CriteriaScores.Count}</div>");
            html.AppendLine("</div>");
            
            // Duration
            html.AppendLine("<div class='metric-card'>");
            html.AppendLine("<h4>Duration</h4>");
            html.AppendLine($"<div class='value'>{_viewModel.Duration?.ToString(@"h\h\ m\m") ?? "N/A"}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            
            // Criteria Scores with Visual Chart
            html.AppendLine("<h3>Evaluation Criteria Scores</h3>");
            
            // Visual Bar Chart
            html.AppendLine("<div class='chart-container'>");
            html.AppendLine("<h4 style='margin-top: 0;'>Average Scores by Evaluation Criteria</h4>");
            html.AppendLine("<div class='bar-chart'>");
            
            foreach (var criterion in _viewModel.CriteriaScores.OrderByDescending(c => c.Value))
            {
                var percentage = (criterion.Value / 5.0) * 100;
                var color = GetScoreColor(criterion.Value);
                var colorHex = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                
                html.AppendLine("<div class='bar-row'>");
                html.AppendLine($"<div class='bar-label'>{criterion.Key}</div>");
                html.AppendLine("<div class='bar-container'>");
                html.AppendLine($"<div class='bar-fill' style='width: {percentage:F0}%; background-color: #{colorHex};'></div>");
                html.AppendLine($"<div class='bar-value'>{criterion.Value:F1}</div>");
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }
            
            html.AppendLine("</div>");
            html.AppendLine("<p style='margin-top: 15px; font-size: 0.85em; color: #666;'>");
            html.AppendLine("<strong>Scale:</strong> ");
            html.AppendLine("<span class='badge needs-improvement'>1-2.4 Needs Improvement</span> ");
            html.AppendLine("<span class='badge fair'>2.5-3.4 Fair</span> ");
            html.AppendLine("<span class='badge good'>3.5-4.4 Good</span> ");
            html.AppendLine("<span class='badge excellent'>4.5-5.0 Excellent</span>");
            html.AppendLine("</p>");
            html.AppendLine("</div>");
            
            // Detailed Table
            html.AppendLine("<h4>Criteria Performance Details</h4>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Criterion</th><th>Score</th><th>Rating</th></tr>");
            
            foreach (var criterion in _viewModel.CriteriaScores.OrderByDescending(c => c.Value))
            {
                var rating = GetPerformanceText(criterion.Value);
                var cssClass = rating.ToLower().Replace(" ", "-");
                html.AppendLine($"<tr>");
                html.AppendLine($"<td>{criterion.Key}</td>");
                html.AppendLine($"<td class='score'>{criterion.Value:F1}/5.0</td>");
                html.AppendLine($"<td><span class='badge {cssClass}'>{rating}</span></td>");
                html.AppendLine($"</tr>");
            }
            
            html.AppendLine("</table>");
            
            // Statistical Summary
            if (StatisticalSummaryCard.Visibility == Visibility.Visible)
            {
                html.AppendLine("<h3>Statistical Summary</h3>");
                
                if (_viewModel.HasDetailedResults && _viewModel.ItemResults != null)
                {
                    // Per-criterion statistics
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>Criterion</th><th>Mean</th><th>Std Dev</th><th>Min</th><th>Max</th><th>Count</th></tr>");
                    
                    foreach (var criterion in _viewModel.CriteriaScores.Keys)
                    {
                        var stats = _viewModel.GetCriterionStatistics(criterion);
                        if (stats != null)
                        {
                            html.AppendLine("<tr>");
                            html.AppendLine($"<td>{criterion}</td>");
                            html.AppendLine($"<td>{stats.Mean:F2}</td>");
                            html.AppendLine($"<td>{stats.StandardDeviation:F2}</td>");
                            html.AppendLine($"<td>{stats.Min:F2}</td>");
                            html.AppendLine($"<td>{stats.Max:F2}</td>");
                            html.AppendLine($"<td>{stats.Count}</td>");
                            html.AppendLine("</tr>");
                        }
                    }
                    html.AppendLine("</table>");
                }
                else
                {
                    // Overall statistics only
                    var scores = _viewModel.CriteriaScores.Values.ToList();
                    html.AppendLine("<p>");
                    html.AppendLine($"<strong>Overall Mean:</strong> {scores.Average():F2}<br>");
                    html.AppendLine($"<strong>Overall Standard Deviation:</strong> {CalculateStandardDeviation(scores):F2}<br>");
                    html.AppendLine($"<strong>Overall Min:</strong> {scores.Min():F2}<br>");
                    html.AppendLine($"<strong>Overall Max:</strong> {scores.Max():F2}<br>");
                    html.AppendLine($"<strong>Criteria Count:</strong> {scores.Count}<br>");
                    html.AppendLine("</p>");
                }
            }
            
            // Folder Statistics
            if (_viewModel.HasFolderStatistics && _viewModel.FolderStatistics != null)
            {
                html.AppendLine("<h3>Performance by Folder</h3>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Folder Path</th><th>Item Count</th><th>Average Score</th></tr>");
                
                var sortedFolders = _viewModel.FolderStatistics
                    .OrderByDescending(f => f.Value.AverageScores.Values.Any() ? f.Value.AverageScores.Values.Average() : 0);
                
                foreach (var folder in sortedFolders)
                {
                    var avgScore = folder.Value.AverageScores.Values.Any() 
                        ? folder.Value.AverageScores.Values.Average() 
                        : 0.0;
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{folder.Key}</td>");
                    html.AppendLine($"<td>{folder.Value.ItemCount}</td>");
                    html.AppendLine($"<td class='score'>{avgScore:F1}/5.0</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }
            
            // Summary metrics
            html.AppendLine("<div class='page-break'></div>");
            html.AppendLine("<h3>Additional Information</h3>");
            
            html.AppendLine("<div class='metadata'>");
            html.AppendLine("<h4>Processing Details</h4>");
            html.AppendLine($"<p><strong>Total Images Processed:</strong> {ImagesProcessedText.Text}</p>");
            html.AppendLine($"<p><strong>Processing Status:</strong> {ProcessingStatusText.Text}</p>");
            html.AppendLine($"<p><strong>Duration:</strong> {_viewModel.Duration?.ToString(@"h\h\ m\m") ?? "N/A"}</p>");
            html.AppendLine($"<p><strong>Workflow Type:</strong> {_viewModel.WorkflowType}</p>");
            
            if (_viewModel.Status == EvaluationStatus.Imported)
            {
                html.AppendLine("<p><em>Note: This evaluation was imported from a JSONL file. Individual scores are aggregated from the import data.</em></p>");
            }
            html.AppendLine("</div>");
            
            // Footer with print instructions
            html.AppendLine("<div class='print-info'>");
            html.AppendLine($"<p><strong>Report generated on:</strong> {DateTime.Now:MMMM dd, yyyy h:mm tt}</p>");
            html.AppendLine("<p class='no-print'>To print this report, use your browser's print function (Ctrl+P or Cmd+P).</p>");
            html.AppendLine("<p><small>AI Dev Gallery Evaluation Report - Confidential</small></p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        
        private string GenerateCsvContent()
        {
            var csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine($"Evaluation Results: {_viewModel.Name}");
            csv.AppendLine($"Model: {_viewModel.ModelName}");
            csv.AppendLine($"Dataset: {_viewModel.DatasetName}");
            csv.AppendLine($"Date: {_viewModel.Timestamp:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Status: {_viewModel.Status}");
            csv.AppendLine($"Overall Average Score: {_viewModel.AverageScore:F2}/5.0");
            csv.AppendLine();
            
            // Criteria Scores
            csv.AppendLine("Criteria Scores");
            csv.AppendLine("Criterion,Score,Rating");
            
            foreach (var criterion in _viewModel.CriteriaScores)
            {
                var rating = GetPerformanceText(criterion.Value);
                csv.AppendLine($"\"{criterion.Key}\",{criterion.Value:F2},\"{rating}\"");
            }
            
            csv.AppendLine();
            
            // Statistical Summary (if available)
            if (_viewModel.HasDetailedResults && _viewModel.ItemResults != null)
            {
                csv.AppendLine("Statistical Summary");
                csv.AppendLine($"Total Items,{_viewModel.ItemResults.Count}");
                
                // Calculate statistics per criterion
                foreach (var criterionName in _viewModel.CriteriaScores.Keys)
                {
                    var scores = _viewModel.ItemResults
                        .Where(item => item.CriteriaScores.ContainsKey(criterionName))
                        .Select(item => item.CriteriaScores[criterionName])
                        .ToList();
                    
                    if (scores.Any())
                    {
                        var mean = scores.Average();
                        var min = scores.Min();
                        var max = scores.Max();
                        var stdDev = CalculateStandardDeviation(scores);
                        
                        csv.AppendLine();
                        csv.AppendLine($"Statistics for {criterionName}");
                        csv.AppendLine($"Mean,{mean:F2}");
                        csv.AppendLine($"Min,{min:F2}");
                        csv.AppendLine($"Max,{max:F2}");
                        csv.AppendLine($"Std Dev,{stdDev:F2}");
                    }
                }
                
                // Folder Statistics (if available)
                if (_viewModel.FolderStatistics != null && _viewModel.FolderStatistics.Any())
                {
                    csv.AppendLine();
                    csv.AppendLine("Folder Performance");
                    csv.AppendLine("Folder Path,Item Count,Average Score");
                    
                    foreach (var folder in _viewModel.FolderStatistics)
                    {
                        var avgScore = folder.Value.AverageScores.Values.Any() 
                            ? folder.Value.AverageScores.Values.Average() 
                            : 0.0;
                        csv.AppendLine($"\"{folder.Key}\",{folder.Value.ItemCount},{avgScore:F2}");
                    }
                }
            }
            
            return csv.ToString();
        }
        
        private double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count <= 1) return 0.0;
            
            var mean = values.Average();
            var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }
        
        private string GenerateJsonContent()
        {
            var exportData = new
            {
                evaluationName = _viewModel.Name,
                modelName = _viewModel.ModelName,
                datasetName = _viewModel.DatasetName,
                timestamp = _viewModel.Timestamp,
                status = _viewModel.Status.ToString(),
                overallScore = _viewModel.AverageScore,
                itemCount = _viewModel.DatasetItemCount,
                criteriaScores = _viewModel.CriteriaScores.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        score = kvp.Value,
                        rating = GetPerformanceText(kvp.Value)
                    }
                ),
                statistics = _viewModel.HasDetailedResults && _viewModel.ItemResults != null
                    ? _viewModel.CriteriaScores.Keys.ToDictionary(
                        criterionName => criterionName,
                        criterionName =>
                        {
                            var scores = _viewModel.ItemResults
                                .Where(item => item.CriteriaScores.ContainsKey(criterionName))
                                .Select(item => item.CriteriaScores[criterionName])
                                .ToList();
                            
                            return scores.Any() ? new
                            {
                                mean = scores.Average(),
                                min = scores.Min(),
                                max = scores.Max(),
                                stdDev = CalculateStandardDeviation(scores),
                                count = scores.Count
                            } : null;
                        })
                    : null,
                folderStatistics = _viewModel.FolderStatistics?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        itemCount = kvp.Value.ItemCount,
                        averageScores = kvp.Value.AverageScores,
                        overallAverage = kvp.Value.AverageScores.Values.Any() 
                            ? kvp.Value.AverageScores.Values.Average() 
                            : 0.0
                    }
                )
            };
            
            return System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
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
        public Brush StatusBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
    }
    
    // Helper class for folder view
    public class FolderTableItem : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isExpanded;
        private Visibility _expandedVisibility = Visibility.Collapsed;
        private string _chevronGlyph = "\uE70D"; // Right chevron

        public string FolderPath { get; set; } = "";
        public int ItemCount { get; set; }
        public string ItemCountText { get; set; } = "";
        public double AverageScore { get; set; }
        public string AverageScoreText { get; set; } = "";
        public Brush ScoreBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
        public List<FolderCriteriaScore> CriteriaScores { get; set; } = new List<FolderCriteriaScore>();

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                ExpandedVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                ChevronGlyph = value ? "\uE70E" : "\uE70D"; // Down/Right chevron
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public Visibility ExpandedVisibility
        {
            get => _expandedVisibility;
            set
            {
                _expandedVisibility = value;
                OnPropertyChanged(nameof(ExpandedVisibility));
            }
        }

        public string ChevronGlyph
        {
            get => _chevronGlyph;
            set
            {
                _chevronGlyph = value;
                OnPropertyChanged(nameof(ChevronGlyph));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    // Helper class for folder criteria scores
    public class FolderCriteriaScore
    {
        public string Name { get; set; } = "";
        public double Score { get; set; }
        public string ScoreText { get; set; } = "";
        public Brush StatusBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
    }
}