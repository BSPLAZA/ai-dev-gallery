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
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

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
            
            // Show/hide status info badge with appropriate tooltip
            if (_viewModel.Status == EvaluationStatus.Imported)
            {
                StatusInfoBadge.Visibility = Visibility.Visible;
                ToolTipService.SetToolTip(StatusInfoBadge, 
                    "This evaluation was imported from a JSONL file. Individual scores are aggregated from the import data.");
            }
            else if (_viewModel.Status == EvaluationStatus.Completed)
            {
                StatusInfoBadge.Visibility = Visibility.Visible;
                ToolTipService.SetToolTip(StatusInfoBadge, 
                    $"Evaluation completed successfully on {_viewModel.Timestamp:MMM dd, yyyy}. All criteria have been evaluated.");
            }
            else if (_viewModel.Status == EvaluationStatus.Running)
            {
                StatusInfoBadge.Visibility = Visibility.Visible;
                ToolTipService.SetToolTip(StatusInfoBadge, 
                    "Evaluation is currently in progress. Results will update as items are processed.");
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
                ImagePreviewArea.Visibility = Visibility.Visible;
                // Load image preview asynchronously to improve page load performance
                _ = UpdateImagePreviewAreaAsync();
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
            
            // Add grid lines and labels for X-axis (0-5 scale)
            for (int i = 0; i <= 5; i++)
            {
                var x = leftMargin + i * (chartWidth / 5);
                
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
            
            // Show box plot legend if we have detailed results
            if (_viewModel.HasDetailedResults && _viewModel.ItemResults != null && _viewModel.ItemResults.Count > 0)
            {
                BoxPlotLegend.Visibility = Visibility.Visible;
            }
            else
            {
                BoxPlotLegend.Visibility = Visibility.Collapsed;
            }
            
            // Draw criteria bars/box plots
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
                
                // Check if we have detailed results for box plot
                if (_viewModel.HasDetailedResults && _viewModel.ItemResults != null && _viewModel.ItemResults.Count > 0)
                {
                    // Get individual scores for this criterion
                    var scores = _viewModel.ItemResults
                        .Where(item => item.CriteriaScores.ContainsKey(criterion.Key))
                        .Select(item => item.CriteriaScores[criterion.Key])
                        .OrderBy(s => s)
                        .ToList();
                    
                    if (scores.Count > 0)
                    {
                        // Note: Scores are already converted from 0-1 to 0-5 scale during import
                        // This conversion happens in EvaluationResultsStore.ImportFromJsonlAsync
                        // for demo purposes. In production, consider standardizing score ranges.
                        
                        // Calculate box plot statistics
                        var min = scores.Min();
                        var max = scores.Max();
                        var median = GetMedian(scores);
                        var q1 = GetQuartile(scores, 0.25);
                        var q3 = GetQuartile(scores, 0.75);
                        var mean = scores.Average();
                        
                        // Draw box and whisker plot
                        DrawBoxPlot(canvas, leftMargin, y, chartWidth, barHeight, min, q1, median, q3, max, mean, maxScore);
                        
                        // Remove mean label to declutter the chart display
                    }
                    else
                    {
                        // Fallback to regular bar
                        DrawRegularBar(canvas, leftMargin, y, chartWidth, barHeight, criterion.Value, maxScore);
                    }
                }
                else
                {
                    // Draw regular bar when no detailed results
                    DrawRegularBar(canvas, leftMargin, y, chartWidth, barHeight, criterion.Value, maxScore);
                }
                
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
        
        private void DrawBoxPlot(Canvas canvas, double x, double y, double width, double height, 
                                  double min, double q1, double median, double q3, double max, double mean, double maxScore)
        {
            var scale = width / maxScore;
            var boxY = y + height * 0.25;
            var boxHeight = height * 0.5;
            
            // Whiskers (min to q1, q3 to max)
            var minX = x + min * scale;
            var q1X = x + q1 * scale;
            var medianX = x + median * scale;
            var q3X = x + q3 * scale;
            var maxX = x + max * scale;
            var meanX = x + mean * scale;
            
            // Left whisker
            var leftWhisker = new Line
            {
                X1 = minX, Y1 = y + height / 2,
                X2 = q1X, Y2 = y + height / 2,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
                StrokeThickness = 2
            };
            canvas.Children.Add(leftWhisker);
            
            // Right whisker
            var rightWhisker = new Line
            {
                X1 = q3X, Y1 = y + height / 2,
                X2 = maxX, Y2 = y + height / 2,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
                StrokeThickness = 2
            };
            canvas.Children.Add(rightWhisker);
            
            // Min/Max caps
            DrawWhiskerCap(canvas, minX, y + height * 0.35, height * 0.3);
            DrawWhiskerCap(canvas, maxX, y + height * 0.35, height * 0.3);
            
            // Box (Q1 to Q3)
            var box = new Rectangle
            {
                Width = q3X - q1X,
                Height = boxHeight,
                Fill = new SolidColorBrush(Color.FromArgb(100, 33, 150, 243)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 33, 150, 243)),
                StrokeThickness = 2
            };
            Canvas.SetLeft(box, q1X);
            Canvas.SetTop(box, boxY);
            canvas.Children.Add(box);
            
            // Median line
            var medianLine = new Line
            {
                X1 = medianX, Y1 = boxY,
                X2 = medianX, Y2 = boxY + boxHeight,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50)),
                StrokeThickness = 3
            };
            canvas.Children.Add(medianLine);
            
            // Mean marker (diamond)
            var meanMarker = new Polygon
            {
                Points = new Microsoft.UI.Xaml.Media.PointCollection
                {
                    new Windows.Foundation.Point(meanX - 4, y + height / 2),
                    new Windows.Foundation.Point(meanX, y + height / 2 - 4),
                    new Windows.Foundation.Point(meanX + 4, y + height / 2),
                    new Windows.Foundation.Point(meanX, y + height / 2 + 4)
                },
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 152, 0)),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 111, 0)),
                StrokeThickness = 1
            };
            canvas.Children.Add(meanMarker);
        }
        
        private void DrawWhiskerCap(Canvas canvas, double x, double y, double height)
        {
            var cap = new Line
            {
                X1 = x, Y1 = y,
                X2 = x, Y2 = y + height,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100)),
                StrokeThickness = 2
            };
            canvas.Children.Add(cap);
        }
        
        private void DrawRegularBar(Canvas canvas, double x, double y, double width, double height, double value, double maxScore)
        {
            // Regular bar
            var barWidth = (value / maxScore) * width;
            var bar = new Rectangle
            {
                Width = barWidth,
                Height = height,
                Fill = new SolidColorBrush(GetScoreColor(value)),
                RadiusX = 4,
                RadiusY = 4
            };
            Canvas.SetLeft(bar, x);
            Canvas.SetTop(bar, y);
            canvas.Children.Add(bar);
            
            // Score label
            var scoreLabel = new TextBlock
            {
                Text = value.ToString("F1"),
                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };
            Canvas.SetLeft(scoreLabel, x + barWidth + 8);
            Canvas.SetTop(scoreLabel, y + (height - 20) / 2);
            canvas.Children.Add(scoreLabel);
        }
        
        private double GetMedian(List<double> sortedValues)
        {
            int count = sortedValues.Count;
            if (count == 0) return 0;
            
            if (count % 2 == 0)
            {
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
            }
            else
            {
                return sortedValues[count / 2];
            }
        }
        
        private double GetQuartile(List<double> sortedValues, double quartile)
        {
            if (sortedValues.Count == 0) return 0;
            
            double position = (sortedValues.Count - 1) * quartile;
            int lower = (int)Math.Floor(position);
            int upper = (int)Math.Ceiling(position);
            
            if (lower == upper)
            {
                return sortedValues[lower];
            }
            
            double weight = position - lower;
            return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
        }
        
        private Color GetScoreColor(double score)
        {
            return score switch
            {
                >= 4.0 => Color.FromArgb(255, 76, 175, 80),    // Green
                >= 3.0 => Color.FromArgb(255, 33, 150, 243),   // Blue
                >= 2.0 => Color.FromArgb(255, 255, 193, 7),    // Yellow
                _ => Color.FromArgb(255, 255, 87, 34)          // Orange
            };
        }
        
        private string GetPerformanceText(double score)
        {
            return score switch
            {
                >= 4.0 => "Excellent",
                >= 3.0 => "Good",
                >= 2.0 => "Fair",
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
                
                // Calculate statistics
                if (folderItem.CriteriaScores.Any())
                {
                    var scores = folderItem.CriteriaScores.Select(c => c.Score).ToList();
                    var mean = scores.Average();
                    var min = scores.Min();
                    var max = scores.Max();
                    var stdDev = CalculateStandardDeviation(scores);
                    
                    folderItem.MeanScoreText = mean.ToString("F2");
                    folderItem.StdDevText = stdDev.ToString("F2");
                    folderItem.RangeText = $"{min:F1}-{max:F1}";
                }
                
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
        
        private async void ImageFileTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            if (args.Node.Content is FileTreeItem folderItem && folderItem.IsFolder)
            {
                // Check if we need to load children
                if (folderItem.Children.Count == 0 && folderItem.Tag is List<EvaluationItemResult> folderResults)
                {
                    folderItem.IsLoading = true;
                    
                    try
                    {
                        // Load children on background thread
                        await Task.Run(() =>
                        {
                            foreach (var item in folderResults.OrderBy(i => i.FileName))
                            {
                                var fileItem = new FileTreeItem
                                {
                                    Name = item.FileName,
                                    FullPath = item.ImagePath,
                                    IsFolder = false,
                                    IconGlyph = "\uEB9F", // Image icon
                                    ScoreText = $"{item.AverageScore:F1}/5",
                                    Children = new List<FileTreeItem>(),
                                    Tag = item
                                };
                                folderItem.Children.Add(fileItem);
                            }
                        });
                        
                        // Small delay to show loading animation
                        await Task.Delay(100);
                    }
                    finally
                    {
                        folderItem.IsLoading = false;
                    }
                }
            }
        }
        
        private void FolderItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is FolderTableItem folderItem)
            {
                // Toggle expansion state
                folderItem.IsExpanded = !folderItem.IsExpanded;
            }
        }
        
        private async Task UpdateImagePreviewAreaAsync()
        {
            try
            {
                // Show loading indicator
                ImageFileTreeView.Visibility = Visibility.Collapsed;
                ImageFileEmptyState.Visibility = Visibility.Collapsed;
                
                // Show a loading message
                ImageResultsSummary.Text = "Loading individual results...";
                
                // This method prepares the image preview area when individual results are available
                if (_viewModel.ItemResults != null && _viewModel.ItemResults.Count > 0)
                {
                    var resultCount = _viewModel.ItemResults.Count;
                    var avgScore = _viewModel.ItemResults.Average(r => r.AverageScore);
                    
                    // Build tree structure on background thread
                    var rootItems = await Task.Run(() =>
                    {
                        var items = new List<FileTreeItem>();
                        
                        // Group by folder path
                        var groupedByFolder = _viewModel.ItemResults
                            .Where(r => !string.IsNullOrEmpty(r.ImagePath))
                            .GroupBy(r => {
                                var dir = System.IO.Path.GetDirectoryName(r.ImagePath) ?? "";
                                return string.IsNullOrEmpty(dir) ? "Images" : dir;
                            })
                            .OrderBy(g => g.Key);
                        
                        System.Diagnostics.Debug.WriteLine($"Grouped into {groupedByFolder.Count()} folders");
                        
                        foreach (var folderGroup in groupedByFolder)
                        {
                            var folderItem = new FileTreeItem
                            {
                                Name = System.IO.Path.GetFileName(folderGroup.Key) ?? folderGroup.Key,
                                FullPath = folderGroup.Key,
                                IsFolder = true,
                                IconGlyph = "\uE8B7", // Folder icon
                                Children = new List<FileTreeItem>(),
                                IsExpanded = false, // Start collapsed for better performance
                                // Store folder group for lazy loading
                                Tag = folderGroup.ToList()
                            };
                            
                            // Calculate folder average score
                            var folderAvgScore = folderGroup.Average(i => i.AverageScore);
                            folderItem.ScoreText = $"{folderAvgScore:F1}/5 ({folderGroup.Count()} items)";
                            
                            // Don't add children yet - we'll load them on demand
                            items.Add(folderItem);
                        }
                        
                        return items;
                    });
                    
                    // Update UI on main thread
                    ImageResultsSummary.Text = $"{resultCount} images evaluated • Average score: {avgScore:F1}/5";
                    
                    // If we have items, show the tree view
                    if (rootItems.Count > 0)
                    {
                        ImageFileTreeView.ItemsSource = rootItems;
                        ImageFileTreeView.Visibility = Visibility.Visible;
                        ImageFileEmptyState.Visibility = Visibility.Collapsed;
                        System.Diagnostics.Debug.WriteLine($"Showing tree view with {rootItems.Count} root items");
                    }
                    else
                    {
                        // No valid paths found
                        ImageFileTreeView.Visibility = Visibility.Collapsed;
                        ImageFileEmptyState.Visibility = Visibility.Visible;
                        System.Diagnostics.Debug.WriteLine("No valid paths found in item results");
                    }
                }
                else
                {
                    ImageResultsSummary.Text = "No individual results available";
                    ImageFileTreeView.Visibility = Visibility.Collapsed;
                    ImageFileEmptyState.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image preview: {ex.Message}");
                ImageResultsSummary.Text = "Error loading individual results";
                ImageFileTreeView.Visibility = Visibility.Collapsed;
                ImageFileEmptyState.Visibility = Visibility.Visible;
            }
        }
        
        private async void LoadDatasetFolderStructure(string datasetPath)
        {
            try
            {
                if (!System.IO.Directory.Exists(datasetPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Dataset folder not found: {datasetPath}");
                    ImageFileTreeView.Visibility = Visibility.Collapsed;
                    ImageFileEmptyState.Visibility = Visibility.Visible;
                    return;
                }
                
                // Build tree structure
                var rootItems = new List<FileTreeItem>();
                await Task.Run(() => BuildFolderTree(datasetPath, rootItems));
                
                // Update UI
                ImageFileTreeView.ItemsSource = rootItems;
                ImageFileTreeView.Visibility = Visibility.Visible;
                ImageFileEmptyState.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dataset folder: {ex.Message}");
                ImageFileTreeView.Visibility = Visibility.Collapsed;
                ImageFileEmptyState.Visibility = Visibility.Visible;
            }
        }
        
        private void BuildFolderTree(string path, List<FileTreeItem> items)
        {
            try
            {
                // Add folders first
                var folders = System.IO.Directory.GetDirectories(path);
                foreach (var folder in folders.OrderBy(f => f))
                {
                    var folderItem = new FileTreeItem
                    {
                        Name = System.IO.Path.GetFileName(folder),
                        FullPath = folder,
                        IsFolder = true,
                        IconGlyph = "\uE8B7", // Folder icon
                        Children = new List<FileTreeItem>()
                    };
                    
                    // Recursively build children
                    BuildFolderTree(folder, folderItem.Children);
                    
                    // Add score if available from folder statistics
                    if (_viewModel.FolderStatistics?.ContainsKey(folder) == true)
                    {
                        var stats = _viewModel.FolderStatistics[folder];
                        var avgScore = stats.AverageScores.Values.Any() ? stats.AverageScores.Values.Average() : 0;
                        folderItem.ScoreText = $"{avgScore:F1}/5";
                    }
                    
                    items.Add(folderItem);
                }
                
                // Add image files
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
                var files = System.IO.Directory.GetFiles(path)
                    .Where(f => imageExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()))
                    .OrderBy(f => f);
                
                foreach (var file in files)
                {
                    var fileItem = new FileTreeItem
                    {
                        Name = System.IO.Path.GetFileName(file),
                        FullPath = file,
                        IsFolder = false,
                        IconGlyph = "\uEB9F", // Image icon
                        Children = new List<FileTreeItem>()
                    };
                    
                    // Add score if available from item results
                    if (_viewModel.ItemResults != null)
                    {
                        // Try different matching approaches
                        var fileName = System.IO.Path.GetFileName(file);
                        var itemResult = _viewModel.ItemResults.FirstOrDefault(r => 
                            r.ImagePath == file || // Exact match
                            r.ImagePath == fileName || // Just filename
                            r.FileName == fileName || // Match by filename property
                            (!string.IsNullOrEmpty(r.ImagePath) && file.EndsWith(r.ImagePath)) // File ends with stored path
                        );
                        
                        if (itemResult != null)
                        {
                            fileItem.ScoreText = $"{itemResult.AverageScore:F1}/5";
                            System.Diagnostics.Debug.WriteLine($"Matched image: {fileName} with score {itemResult.AverageScore:F1}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No match found for image: {fileName}");
                            System.Diagnostics.Debug.WriteLine($"  Full path: {file}");
                            // Log first few item results for debugging
                            var sampleResults = _viewModel.ItemResults.Take(3);
                            foreach (var sample in sampleResults)
                            {
                                System.Diagnostics.Debug.WriteLine($"  Sample ImagePath: {sample.ImagePath}");
                            }
                        }
                    }
                    
                    items.Add(fileItem);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error building folder tree: {ex.Message}");
            }
        }
        
        private void ImageSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implement search filtering
            var searchText = ImageSearchBox.Text?.Trim() ?? "";
            System.Diagnostics.Debug.WriteLine($"Search text: {searchText}");
        }
        
        private async void ImageFileTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is FileTreeItem fileItem && !fileItem.IsFolder)
            {
                System.Diagnostics.Debug.WriteLine($"Selected image: {fileItem.FullPath}");
                
                // Get the associated EvaluationItemResult
                if (fileItem.Tag is EvaluationItemResult itemResult)
                {
                    await ShowImagePreview(fileItem, itemResult);
                }
                else
                {
                    // If no item result is stored, try to find it
                    var result = _viewModel.ItemResults?.FirstOrDefault(r => 
                        r.ImagePath == fileItem.FullPath || 
                        r.FileName == fileItem.Name);
                    
                    if (result != null)
                    {
                        await ShowImagePreview(fileItem, result);
                    }
                    else
                    {
                        // Show empty state
                        ImagePreviewEmptyState.Visibility = Visibility.Visible;
                        ImagePreviewContainer.Visibility = Visibility.Collapsed;
                        ImageMetadataPanel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        
        private void ShowImageError(string message)
        {
            PreviewImage.Visibility = Visibility.Collapsed;
            
            var previewGrid = PreviewImage.Parent as Grid;
            if (previewGrid != null)
            {
                // Remove any existing error messages
                for (int i = previewGrid.Children.Count - 1; i >= 0; i--)
                {
                    if (previewGrid.Children[i] is StackPanel)
                    {
                        previewGrid.Children.RemoveAt(i);
                    }
                }
                
                // Add error message
                var errorPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 8
                };
                
                var errorIcon = new FontIcon
                {
                    Glyph = "\uE783", // Error icon
                    FontSize = 48,
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                };
                errorPanel.Children.Add(errorIcon);
                
                var errorText = new TextBlock
                {
                    Text = message,
                    Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    TextAlignment = TextAlignment.Center
                };
                errorPanel.Children.Add(errorText);
                
                previewGrid.Children.Add(errorPanel);
            }
        }
        
        private async Task ShowImagePreview(FileTreeItem fileItem, EvaluationItemResult itemResult)
        {
            try
            {
                // Show loading state
                ImagePreviewEmptyState.Visibility = Visibility.Collapsed;
                ImagePreviewContainer.Visibility = Visibility.Visible;
                ImageLoadingRing.IsActive = true;
                PreviewImage.Source = null;
                
                // Update file info
                ImageFileNameText.Text = fileItem.Name;
                ImagePathText.Text = fileItem.FullPath;
                
                // Update scores
                ImageScoresPanel.Children.Clear();
                foreach (var score in itemResult.CriteriaScores.OrderByDescending(s => s.Value))
                {
                    var scorePanel = new Grid();
                    scorePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    scorePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    
                    var criterionText = new TextBlock
                    {
                        Text = score.Key,
                        Style = (Style)Application.Current.Resources["BodyTextBlockStyle"]
                    };
                    Grid.SetColumn(criterionText, 0);
                    scorePanel.Children.Add(criterionText);
                    
                    var scoreText = new TextBlock
                    {
                        Text = $"{score.Value:F1}/5",
                        Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"],
                        Foreground = new SolidColorBrush(GetScoreColor(score.Value))
                    };
                    Grid.SetColumn(scoreText, 1);
                    scorePanel.Children.Add(scoreText);
                    
                    ImageScoresPanel.Children.Add(scorePanel);
                }
                
                // Update prompt/response if available
                if (!string.IsNullOrEmpty(itemResult.Prompt) || !string.IsNullOrEmpty(itemResult.ModelResponse))
                {
                    ImagePromptText.Text = itemResult.Prompt ?? "No prompt available";
                    ImageResponseText.Text = itemResult.ModelResponse ?? "No response available";
                    PromptResponseContainer.Visibility = Visibility.Visible;
                }
                else
                {
                    PromptResponseContainer.Visibility = Visibility.Collapsed;
                }
                
                // Update processing info and metadata
                ProcessingTimeText.Visibility = Visibility.Collapsed; // Hide processing time
                AverageScoreText2.Text = $"Average score: {itemResult.AverageScore:F1}/5";
                
                // Display custom metadata if available
                if (itemResult.HasCustomMetadata && itemResult.CustomMetadata != null)
                {
                    var metadataSection = ProcessingTimeText.Parent as StackPanel;
                    if (metadataSection != null)
                    {
                        // Clear existing metadata entries (keep the header and average score)
                        while (metadataSection.Children.Count > 2)
                        {
                            metadataSection.Children.RemoveAt(2);
                        }
                        
                        // Add each metadata entry
                        foreach (var metadata in itemResult.CustomMetadata.OrderBy(m => m.Key))
                        {
                            var metadataPanel = new Grid { Margin = new Thickness(0, 4, 0, 0) };
                            metadataPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            metadataPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            
                            var keyText = new TextBlock
                            {
                                Text = $"{metadata.Key}:",
                                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                                Margin = new Thickness(0, 0, 8, 0)
                            };
                            Grid.SetColumn(keyText, 0);
                            metadataPanel.Children.Add(keyText);
                            
                            var valueText = new TextBlock
                            {
                                Text = metadata.Value?.ToString() ?? "null",
                                Style = (Style)Application.Current.Resources["BodyTextBlockStyle"],
                                TextWrapping = TextWrapping.Wrap
                            };
                            Grid.SetColumn(valueText, 1);
                            metadataPanel.Children.Add(valueText);
                            
                            metadataSection.Children.Add(metadataPanel);
                        }
                    }
                }
                
                // Show metadata panel
                ImageMetadataPanel.Visibility = Visibility.Visible;
                
                // Try to load the actual image
                try
                {
                    string imagePath = fileItem.FullPath;
                    
                    // If we have a dataset base path and the image path is relative, combine them
                    if (!string.IsNullOrEmpty(_viewModel.DatasetBasePath) && !Path.IsPathRooted(imagePath))
                    {
                        imagePath = Path.Combine(_viewModel.DatasetBasePath, imagePath);
                        System.Diagnostics.Debug.WriteLine($"Resolved image path: {imagePath}");
                    }
                    
                    // Check if file exists first
                    if (!File.Exists(imagePath))
                    {
                        // Try with just the filename in the dataset base path
                        if (!string.IsNullOrEmpty(_viewModel.DatasetBasePath))
                        {
                            var fileName = Path.GetFileName(fileItem.FullPath);
                            var altPath = Path.Combine(_viewModel.DatasetBasePath, fileName);
                            if (File.Exists(altPath))
                            {
                                imagePath = altPath;
                                System.Diagnostics.Debug.WriteLine($"Found image at alternate path: {imagePath}");
                            }
                        }
                    }
                    
                    // Attempt to load the image file
                    var file = await StorageFile.GetFileFromPathAsync(imagePath);
                    var stream = await file.OpenAsync(FileAccessMode.Read);
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    PreviewImage.Source = bitmapImage;
                    PreviewImage.Visibility = Visibility.Visible;
                }
                catch (UnauthorizedAccessException)
                {
                    // Show error message for access denied
                    ShowImageError("Access denied to image file");
                }
                catch (FileNotFoundException)
                {
                    // Show error message for file not found
                    ShowImageError($"Image file not found: {fileItem.Name}");
                }
                catch (Exception ex)
                {
                    // Show generic error message
                    System.Diagnostics.Debug.WriteLine($"Error loading image: {ex}");
                    ShowImageError($"Cannot load image: {ex.Message}");
                }
                finally
                {
                    ImageLoadingRing.IsActive = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing image preview: {ex.Message}");
                ImageLoadingRing.IsActive = false;
                
                // Show error state
                ImagePreviewEmptyState.Visibility = Visibility.Visible;
                ImagePreviewContainer.Visibility = Visibility.Collapsed;
                ImageMetadataPanel.Visibility = Visibility.Collapsed;
            }
        }
        
        private async void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show a dialog explaining the print process
                var dialog = new ContentDialog
                {
                    Title = "Print Evaluation Report",
                    Content = new StackPanel
                    {
                        Spacing = 12,
                        Children =
                        {
                            new TextBlock 
                            { 
                                Text = "To print your evaluation report:",
                                Style = (Style)Application.Current.Resources["BodyStrongTextBlockStyle"]
                            },
                            new TextBlock 
                            { 
                                Text = "1. Save the report as an HTML file\n2. The file will open in your browser\n3. Use your browser's print function (Ctrl+P or Cmd+P)",
                                TextWrapping = TextWrapping.Wrap
                            },
                            new TextBlock 
                            { 
                                Text = "The report includes all evaluation data, charts, and statistics in a print-friendly format.",
                                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
                                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    },
                    PrimaryButtonText = "Continue",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot,
                    DefaultButton = ContentDialogButton.Primary
                };
                
                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
                
                // Proceed with saving HTML file
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
                    
                    // Open the file in the default browser
                    await Windows.System.Launcher.LaunchFileAsync(file);
                    
                    // Show success message
                    var successDialog = new ContentDialog
                    {
                        Title = "Report Saved Successfully",
                        Content = "Your report has been saved and opened in your browser. Use your browser's print function (Ctrl+P or Cmd+P) to print the report.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating print report: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while generating the report. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
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
            html.AppendLine("<span class='badge needs-improvement'>0-1.9 Needs Improvement</span> ");
            html.AppendLine("<span class='badge fair'>2.0-2.9 Fair</span> ");
            html.AppendLine("<span class='badge good'>3.0-3.9 Good</span> ");
            html.AppendLine("<span class='badge excellent'>4.0-5.0 Excellent</span>");
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
                    // Overall summary first
                    var summary = _viewModel.GetStatisticalSummary();
                    if (summary != null)
                    {
                        html.AppendLine("<div class='metadata'>");
                        html.AppendLine("<h4>Overall Performance Statistics</h4>");
                        html.AppendLine($"<p><strong>Mean Score:</strong> {summary.Mean:F2}/5.0</p>");
                        html.AppendLine($"<p><strong>Median Score:</strong> {summary.Median:F2}/5.0</p>");
                        html.AppendLine($"<p><strong>Standard Deviation:</strong> {summary.StandardDeviation:F2}</p>");
                        html.AppendLine($"<p><strong>Score Range:</strong> {summary.Min:F2} - {summary.Max:F2}</p>");
                        html.AppendLine($"<p><strong>Total Items Evaluated:</strong> {summary.Count}</p>");
                        html.AppendLine("</div>");
                    }
                    
                    // Per-criterion statistics with box plot data
                    html.AppendLine("<h4>Detailed Statistics by Criterion</h4>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>Criterion</th><th>Mean</th><th>Median</th><th>Std Dev</th><th>Min</th><th>Q1</th><th>Q3</th><th>Max</th><th>Count</th></tr>");
                    
                    foreach (var criterion in _viewModel.CriteriaScores.Keys)
                    {
                        var stats = _viewModel.GetCriterionStatistics(criterion);
                        if (stats != null)
                        {
                            // Calculate quartiles for box plot data
                            var scores = _viewModel.ItemResults
                                .Where(r => r.IsSuccess && r.CriteriaScores.ContainsKey(criterion))
                                .Select(r => r.CriteriaScores[criterion])
                                .OrderBy(s => s)
                                .ToList();
                            
                            var q1 = GetPercentile(scores, 25);
                            var q3 = GetPercentile(scores, 75);
                            
                            html.AppendLine("<tr>");
                            html.AppendLine($"<td>{criterion}</td>");
                            html.AppendLine($"<td class='score'>{stats.Mean:F2}</td>");
                            html.AppendLine($"<td class='score'>{stats.Median:F2}</td>");
                            html.AppendLine($"<td>{stats.StandardDeviation:F2}</td>");
                            html.AppendLine($"<td>{stats.Min:F2}</td>");
                            html.AppendLine($"<td>{q1:F2}</td>");
                            html.AppendLine($"<td>{q3:F2}</td>");
                            html.AppendLine($"<td>{stats.Max:F2}</td>");
                            html.AppendLine($"<td>{stats.Count}</td>");
                            html.AppendLine("</tr>");
                        }
                    }
                    html.AppendLine("</table>");
                    html.AppendLine("<p style='margin-top: 10px; font-size: 0.85em; color: #666;'>");
                    html.AppendLine("<strong>Box Plot Legend:</strong> Min/Max | Q1-Q3 (interquartile range) | Median | Mean");
                    html.AppendLine("</p>");
                }
                else
                {
                    // Overall statistics only
                    var scores = _viewModel.CriteriaScores.Values.ToList();
                    html.AppendLine("<div class='metadata'>");
                    html.AppendLine("<p>");
                    html.AppendLine($"<strong>Overall Mean:</strong> {scores.Average():F2}<br>");
                    html.AppendLine($"<strong>Overall Standard Deviation:</strong> {CalculateStandardDeviation(scores):F2}<br>");
                    html.AppendLine($"<strong>Overall Min:</strong> {scores.Min():F2}<br>");
                    html.AppendLine($"<strong>Overall Max:</strong> {scores.Max():F2}<br>");
                    html.AppendLine($"<strong>Criteria Count:</strong> {scores.Count}<br>");
                    html.AppendLine("</p>");
                    html.AppendLine("</div>");
                }
            }
            
            // Folder Statistics
            if (_viewModel.HasFolderStatistics && _viewModel.FolderStatistics != null)
            {
                html.AppendLine("<h3>Performance by Folder</h3>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Folder Path</th><th>Item Count</th><th>Success Rate</th><th>Average Score</th><th>Score Range</th></tr>");
                
                var sortedFolders = _viewModel.FolderStatistics
                    .OrderByDescending(f => f.Value.AverageScores.Values.Any() ? f.Value.AverageScores.Values.Average() : 0);
                
                foreach (var folder in sortedFolders)
                {
                    var avgScore = folder.Value.AverageScores.Values.Any() 
                        ? folder.Value.AverageScores.Values.Average() 
                        : 0.0;
                    
                    // Get score range for this folder
                    var folderScores = folder.Value.AverageScores.Values.ToList();
                    var minScore = folderScores.Any() ? folderScores.Min() : 0;
                    var maxScore = folderScores.Any() ? folderScores.Max() : 0;
                    
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{folder.Key}</td>");
                    html.AppendLine($"<td>{folder.Value.ItemCount}</td>");
                    html.AppendLine($"<td>{folder.Value.SuccessRate:F1}%</td>");
                    html.AppendLine($"<td class='score'>{avgScore:F1}/5.0</td>");
                    html.AppendLine($"<td>{minScore:F1} - {maxScore:F1}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }
            
            // Individual Results Summary
            if (_viewModel.HasDetailedResults && _viewModel.ItemResults != null)
            {
                html.AppendLine("<div class='page-break'></div>");
                html.AppendLine("<h3>Individual Results Summary</h3>");
                
                // Top performing items
                var topItems = _viewModel.ItemResults
                    .Where(r => r.IsSuccess)
                    .OrderByDescending(r => r.AverageScore)
                    .Take(10);
                
                html.AppendLine("<h4>Top 10 Performing Items</h4>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Image</th><th>Average Score</th><th>Processing Time</th></tr>");
                
                foreach (var item in topItems)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{item.FileName}</td>");
                    html.AppendLine($"<td class='score'>{item.AverageScore:F1}/5.0</td>");
                    html.AppendLine($"<td>{item.ProcessingTime.TotalSeconds:F1}s</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
                
                // Items needing improvement
                var bottomItems = _viewModel.ItemResults
                    .Where(r => r.IsSuccess)
                    .OrderBy(r => r.AverageScore)
                    .Take(10);
                
                html.AppendLine("<h4>Items Needing Improvement</h4>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Image</th><th>Average Score</th><th>Lowest Criterion</th></tr>");
                
                foreach (var item in bottomItems)
                {
                    var lowestCriterion = item.CriteriaScores.Any() 
                        ? item.CriteriaScores.OrderBy(c => c.Value).First()
                        : new KeyValuePair<string, double>("N/A", 0);
                    
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{item.FileName}</td>");
                    html.AppendLine($"<td class='score'>{item.AverageScore:F1}/5.0</td>");
                    html.AppendLine($"<td>{lowestCriterion.Key}: {lowestCriterion.Value:F1}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
                
                // Perfect score items (5.0)
                var perfectScoreItems = _viewModel.ItemResults
                    .Where(r => r.IsSuccess && Math.Abs(r.AverageScore - 5.0) < 0.01)
                    .ToList();
                
                if (perfectScoreItems.Any())
                {
                    html.AppendLine("<h4>Perfect Score Items (5.0/5.0)</h4>");
                    html.AppendLine($"<p><strong>Total:</strong> {perfectScoreItems.Count} items achieved perfect scores</p>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>Image</th><th>All Criteria</th></tr>");
                    
                    foreach (var item in perfectScoreItems.Take(20))
                    {
                        var criteriaList = string.Join(", ", item.CriteriaScores.Keys);
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td>{item.FileName}</td>");
                        html.AppendLine($"<td>{criteriaList}</td>");
                        html.AppendLine("</tr>");
                    }
                    
                    if (perfectScoreItems.Count > 20)
                    {
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td colspan='2'><em>... and {perfectScoreItems.Count - 20} more items with perfect scores</em></td>");
                        html.AppendLine("</tr>");
                    }
                    html.AppendLine("</table>");
                }
                
                // Zero score items (0.0)
                var zeroScoreItems = _viewModel.ItemResults
                    .Where(r => r.IsSuccess && r.AverageScore < 0.01)
                    .ToList();
                
                if (zeroScoreItems.Any())
                {
                    html.AppendLine("<h4>Zero Score Items (0.0/5.0)</h4>");
                    html.AppendLine($"<p><strong>Total:</strong> {zeroScoreItems.Count} items scored 0.0</p>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>Image</th><th>Failed Criteria</th></tr>");
                    
                    foreach (var item in zeroScoreItems.Take(20))
                    {
                        var criteriaList = string.Join(", ", item.CriteriaScores.Where(c => c.Value < 0.01).Select(c => c.Key));
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td>{item.FileName}</td>");
                        html.AppendLine($"<td>{criteriaList}</td>");
                        html.AppendLine("</tr>");
                    }
                    
                    if (zeroScoreItems.Count > 20)
                    {
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td colspan='2'><em>... and {zeroScoreItems.Count - 20} more items with zero scores</em></td>");
                        html.AppendLine("</tr>");
                    }
                    html.AppendLine("</table>");
                }
                
                // Error summary if any
                var errorItems = _viewModel.ItemResults.Where(r => !r.IsSuccess).ToList();
                if (errorItems.Any())
                {
                    html.AppendLine("<h4>Processing Errors</h4>");
                    html.AppendLine($"<p><strong>Total Errors:</strong> {errorItems.Count}</p>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>Image</th><th>Error Message</th></tr>");
                    
                    foreach (var item in errorItems.Take(5))
                    {
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td>{item.FileName}</td>");
                        html.AppendLine($"<td>{item.Error ?? "Unknown error"}</td>");
                        html.AppendLine("</tr>");
                    }
                    
                    if (errorItems.Count > 5)
                    {
                        html.AppendLine("<tr>");
                        html.AppendLine($"<td colspan='2'><em>... and {errorItems.Count - 5} more errors</em></td>");
                        html.AppendLine("</tr>");
                    }
                    html.AppendLine("</table>");
                }
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
        
        private double GetPercentile(List<double> sortedValues, double percentile)
        {
            if (sortedValues.Count == 0) return 0;
            if (sortedValues.Count == 1) return sortedValues[0];
            
            double index = (percentile / 100.0) * (sortedValues.Count - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);
            
            if (lower == upper)
            {
                return sortedValues[lower];
            }
            
            double weight = index - lower;
            return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
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
        
        // Statistical properties
        public Visibility HasStatistics => ItemCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        public string MeanScoreText { get; set; } = "0.0";
        public string StdDevText { get; set; } = "0.0";
        public string RangeText { get; set; } = "0.0-0.0";

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
    
    // Helper class for file tree items
    public class FileTreeItem : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public bool IsFolder { get; set; }
        public string IconGlyph { get; set; } = "";
        public string ScoreText { get; set; } = "";
        public List<FileTreeItem> Children { get; set; } = new List<FileTreeItem>();
        
        private bool _isExpanded;
        public bool IsExpanded 
        { 
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
        
        private bool _isLoading;
        public bool IsLoading 
        { 
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        public object? Tag { get; set; } // For storing associated data like EvaluationItemResult
        
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}