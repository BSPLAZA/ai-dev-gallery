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
            
            // Update overall score
            StarRatingText.Text = _viewModel.StarRating;
            ScoreText.Text = _viewModel.AverageScore.ToString("F1") + "/5";
            
            // Update metric cards
            // Use DatasetItemCount for imported results (shows actual count)
            ImagesProcessedText.Text = _viewModel.DatasetItemCount.ToString("N0");
            ProcessingStatusText.Text = _viewModel.Status == EvaluationStatus.Completed ? "100% Complete" : 
                                       _viewModel.Status == EvaluationStatus.Running ? $"{_viewModel.ProgressPercentage ?? 0}% Complete" : 
                                       "";
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
                
                folderItems.Add(new FolderTableItem
                {
                    FolderPath = folder.Key,
                    ItemCount = folder.Value.ItemCount,
                    ItemCountText = $"{folder.Value.ItemCount} items",
                    AverageScore = avgScore,
                    AverageScoreText = avgScore.ToString("F1") + "/5",
                    ScoreBrush = new SolidColorBrush(GetScoreColor(avgScore))
                });
            }
            
            // Sort by average score descending
            FolderStatsRepeater.ItemsSource = folderItems.OrderByDescending(f => f.AverageScore).ToList();
        }
        
        private void UpdateStatisticalSummary()
        {
            // Clear existing content
            StatisticalSummaryCard.Children.Clear();
            
            // Calculate statistics based on criteria scores if individual results not available
            if (!_viewModel.HasDetailedResults || _viewModel.ItemResults == null || _viewModel.ItemResults.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No detailed results available, using criteria averages for statistics");
                
                // Use criteria scores for basic statistics
                var scores = _viewModel.CriteriaScores.Values.ToList();
                if (scores.Count == 0) return;
                
                var mean = scores.Average();
                var min = scores.Min();
                var max = scores.Max();
                var stdDev = CalculateStandardDeviation(scores);
                
                // Add summary header
                var headerText = new TextBlock
                {
                    Text = "Statistical Summary (Criteria Averages)",
                    Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"],
                    Margin = new Thickness(16, 16, 16, 12)
                };
                Grid.SetColumnSpan(headerText, 5);
                StatisticalSummaryCard.Children.Add(headerText);
                
                // Create stats panels
                AddStatPanel(0, "Mean", mean.ToString("F2"), 1);
                AddStatPanel(1, "Std Dev", stdDev.ToString("F2"), 1);
                AddStatPanel(2, "Min", min.ToString("F2"), 1);
                AddStatPanel(3, "Max", max.ToString("F2"), 1);
                AddStatPanel(4, "Criteria", scores.Count.ToString(), 1);
                
                return;
            }
            
            // Use detailed results if available
            System.Diagnostics.Debug.WriteLine($"Using detailed results for statistics ({_viewModel.ItemResults.Count} items)");
            
            var summary = _viewModel.GetStatisticalSummary();
            if (summary == null) return;
            
            // Add summary header
            var detailedHeaderText = new TextBlock
            {
                Text = "Statistical Summary (All Items)",
                Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"],
                Margin = new Thickness(16, 16, 16, 12)
            };
            Grid.SetColumnSpan(detailedHeaderText, 5);
            StatisticalSummaryCard.Children.Add(detailedHeaderText);
            
            // Create stats panels
            AddStatPanel(0, "Mean", summary.Mean.ToString("F2"), 1);
            AddStatPanel(1, "Std Dev", summary.StandardDeviation.ToString("F2"), 1);
            AddStatPanel(2, "Min", summary.Min.ToString("F2"), 1);
            AddStatPanel(3, "Max", summary.Max.ToString("F2"), 1);
            AddStatPanel(4, "Median", summary.Median.ToString("F2"), 1);
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
                var pixels = pixelBuffer.ToArray();
                
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
                            PropertyType.String));
                        
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
        
        private async void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create print document
                var printDoc = new Windows.UI.Xaml.Printing.PrintDocument();
                var printDocSource = printDoc.DocumentSource;
                
                // Create print task
                var printTask = Windows.Graphics.Printing.PrintManager.GetForCurrentView().PrintTaskRequested;
                Windows.Foundation.TypedEventHandler<Windows.Graphics.Printing.PrintManager, Windows.Graphics.Printing.PrintTaskRequestedEventArgs> printTaskHandler = (s, args) =>
                {
                    var task = args.Request.CreatePrintTask($"Evaluation Report - {_viewModel.Name}", sourceRequested =>
                    {
                        sourceRequested.SetSource(printDocSource);
                    });
                    
                    task.Options.DocumentName = $"Evaluation_Report_{_viewModel.Name}_{DateTime.Now:yyyyMMdd}";
                };
                
                Windows.Graphics.Printing.PrintManager.GetForCurrentView().PrintTaskRequested += printTaskHandler;
                
                // Prepare pages when requested
                printDoc.Paginate += (s, args) =>
                {
                    // Create print content
                    var printContent = CreatePrintContent();
                    printDoc.SetPreviewPageCount(1, Windows.UI.Xaml.Printing.PreviewPageCountType.Final);
                };
                
                printDoc.GetPreviewPage += (s, args) =>
                {
                    printDoc.SetPreviewPage(args.PageNumber, CreatePrintContent());
                };
                
                printDoc.AddPages += (s, args) =>
                {
                    printDoc.AddPage(CreatePrintContent());
                    printDoc.AddPagesComplete();
                };
                
                // Show print UI
                await Windows.Graphics.Printing.PrintManager.ShowPrintUIAsync();
                
                // Clean up event handler
                Windows.Graphics.Printing.PrintManager.GetForCurrentView().PrintTaskRequested -= printTaskHandler;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error printing report: {ex.Message}");
            }
        }
        
        private FrameworkElement CreatePrintContent()
        {
            // Create a printable version of the report
            var printGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Padding = new Thickness(40),
                Width = 800
            };
            
            var content = new StackPanel { Spacing = 20 };
            
            // Header
            content.Children.Add(new TextBlock
            {
                Text = "Evaluation Insights Report",
                Style = (Style)Application.Current.Resources["TitleTextBlockStyle"],
                HorizontalAlignment = HorizontalAlignment.Center
            });
            
            // Evaluation details
            var detailsPanel = new StackPanel { Spacing = 8 };
            detailsPanel.Children.Add(new TextBlock
            {
                Text = _viewModel.Name,
                Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"]
            });
            
            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Model: {_viewModel.ModelName} | Dataset: {_viewModel.DatasetName}",
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"]
            });
            
            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Date: {_viewModel.Timestamp:MMMM dd, yyyy h:mm tt}",
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"]
            });
            
            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Overall Score: {_viewModel.AverageScore:F1}/5.0",
                Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
            });
            
            content.Children.Add(detailsPanel);
            
            // Add separator
            content.Children.Add(new Rectangle
            {
                Height = 1,
                Fill = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"],
                Margin = new Thickness(0, 10, 0, 10)
            });
            
            // Criteria scores
            content.Children.Add(new TextBlock
            {
                Text = "Evaluation Criteria Scores",
                Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
            });
            
            var criteriaGrid = new Grid();
            criteriaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            criteriaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            criteriaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            int row = 0;
            foreach (var criterion in _viewModel.CriteriaScores)
            {
                criteriaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                
                var nameText = new TextBlock
                {
                    Text = criterion.Key,
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    Margin = new Thickness(0, 4, 0, 4)
                };
                Grid.SetRow(nameText, row);
                Grid.SetColumn(nameText, 0);
                criteriaGrid.Children.Add(nameText);
                
                var scoreText = new TextBlock
                {
                    Text = criterion.Value.ToString("F1"),
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 4)
                };
                Grid.SetRow(scoreText, row);
                Grid.SetColumn(scoreText, 1);
                criteriaGrid.Children.Add(scoreText);
                
                var ratingText = new TextBlock
                {
                    Text = GetPerformanceText(criterion.Value),
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 4)
                };
                Grid.SetRow(ratingText, row);
                Grid.SetColumn(ratingText, 2);
                criteriaGrid.Children.Add(ratingText);
                
                row++;
            }
            
            content.Children.Add(criteriaGrid);
            
            // Statistical summary if available
            if (StatisticalSummaryCard.Visibility == Visibility.Visible)
            {
                content.Children.Add(new Rectangle
                {
                    Height = 1,
                    Fill = (Brush)Application.Current.Resources["DividerStrokeColorDefaultBrush"],
                    Margin = new Thickness(0, 10, 0, 10)
                });
                
                content.Children.Add(new TextBlock
                {
                    Text = "Statistical Summary",
                    Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
                });
                
                // Add stats text
                var scores = _viewModel.CriteriaScores.Values.ToList();
                content.Children.Add(new TextBlock
                {
                    Text = $"Mean: {scores.Average():F2} | Std Dev: {CalculateStandardDeviation(scores):F2} | Min: {scores.Min():F2} | Max: {scores.Max():F2}",
                    Style = (Style)Application.Current.Resources["BodyTextBlockStyle"]
                });
            }
            
            printGrid.Children.Add(content);
            return printGrid;
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
    public class FolderTableItem
    {
        public string FolderPath { get; set; } = "";
        public int ItemCount { get; set; }
        public string ItemCountText { get; set; } = "";
        public double AverageScore { get; set; }
        public string AverageScoreText { get; set; } = "";
        public Brush ScoreBrush { get; set; } = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
    }
}