// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AIDevGallery.Models;
using AIDevGallery.Services.Evaluate;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Microsoft.UI;

namespace AIDevGallery.Pages.Evaluate
{
    public sealed partial class CompareEvaluationsPage : Page
    {
        private readonly IEvaluationResultsStore _evaluationStore;
        private readonly List<EvaluationResult> _evaluations = new();
        private readonly List<string> _commonCriteria = new();
        private readonly Dictionary<string, List<double>> _criteriaScores = new();

        // Chart dimensions
        private const double ChartPadding = 40;
        private const double ChartLeftMargin = 120;
        private const double ChartBottomMargin = 60;
        private const double BarGroupSpacing = 20;
        private const double BarSpacing = 4;

        // Model ranking data
        private class ModelRanking
        {
            public string RankEmoji { get; set; } = "";
            public string ModelName { get; set; } = "";
            public string OverallAverage { get; set; } = "";
            public int CriteriaCount { get; set; }
            public string WinCount { get; set; } = "";
        }

        public CompareEvaluationsPage()
        {
            this.InitializeComponent();
            _evaluationStore = new EvaluationResultsStore();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is List<string> evaluationIds && evaluationIds.Count >= 2)
            {
                await LoadEvaluations(evaluationIds);
            }
            else
            {
                ShowError("Invalid comparison request. Please select 2-5 evaluations to compare.");
            }
        }

        private async Task LoadEvaluations(List<string> evaluationIds)
        {
            try
            {
                LoadingPanel.Visibility = Visibility.Visible;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorPanel.Visibility = Visibility.Collapsed;

                // Load evaluations
                _evaluations.Clear();
                foreach (var id in evaluationIds.Take(5)) // Max 5 evaluations
                {
                    var evaluation = await _evaluationStore.GetEvaluationByIdAsync(id);
                    if (evaluation != null)
                    {
                        _evaluations.Add(evaluation);
                    }
                }

                if (_evaluations.Count < 2)
                {
                    ShowError("Could not load enough evaluations for comparison.");
                    return;
                }

                // Update subtitle
                ComparisonSubtitle.Text = string.Join(" vs ", _evaluations.Select(e => e.ModelName ?? "Unknown"));

                // Find common criteria
                FindCommonCriteria();

                if (_commonCriteria.Count == 0)
                {
                    ShowError("Selected evaluations have no common criteria to compare.");
                    return;
                }

                // Build data structures
                BuildCriteriaScores();

                // Update UI
                UpdateChart();
                UpdateModelRankings();
                UpdateKeyStatistics();
                UpdateDetailedScoresTable();

                LoadingPanel.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading evaluations: {ex.Message}");
            }
        }

        private void FindCommonCriteria()
        {
            if (_evaluations.Count == 0) return;

            // Start with criteria from first evaluation
            var commonCriteria = new HashSet<string>(_evaluations[0].CriteriaScores?.Keys ?? Enumerable.Empty<string>());

            // Intersect with criteria from other evaluations
            foreach (var evaluation in _evaluations.Skip(1))
            {
                if (evaluation.CriteriaScores != null)
                {
                    commonCriteria.IntersectWith(evaluation.CriteriaScores.Keys);
                }
            }

            _commonCriteria = commonCriteria.OrderBy(c => c).ToList();
        }

        private void BuildCriteriaScores()
        {
            _criteriaScores.Clear();

            foreach (var criterion in _commonCriteria)
            {
                var scores = new List<double>();
                foreach (var evaluation in _evaluations)
                {
                    if (evaluation.CriteriaScores != null && evaluation.CriteriaScores.TryGetValue(criterion, out var score))
                    {
                        scores.Add(score);
                    }
                    else
                    {
                        scores.Add(0); // Default if missing
                    }
                }
                _criteriaScores[criterion] = scores;
            }
        }

        private void UpdateChart()
        {
            ComparisonChart.Children.Clear();

            if (_commonCriteria.Count == 0 || _evaluations.Count == 0) return;

            // Get chart dimensions
            var actualWidth = ChartContainer.ActualWidth > 0 ? ChartContainer.ActualWidth : 800;
            var actualHeight = ChartContainer.ActualHeight > 0 ? ChartContainer.ActualHeight : 400;

            ComparisonChart.Width = actualWidth;
            ComparisonChart.Height = actualHeight;

            var chartWidth = actualWidth - ChartLeftMargin - ChartPadding;
            var chartHeight = actualHeight - ChartPadding - ChartBottomMargin;

            // Use alphabetical order
            var sortedCriteria = _commonCriteria.OrderBy(c => c).ToList();

            // Calculate bar dimensions
            var barGroupWidth = chartWidth / sortedCriteria.Count - BarGroupSpacing;
            var barWidth = (barGroupWidth - (_evaluations.Count - 1) * BarSpacing) / _evaluations.Count;

            // Colors for models (converted from hex)
            var colors = new Color[] {
                Color.FromArgb(255, 33, 150, 243),  // #2196F3 - Blue
                Color.FromArgb(255, 76, 175, 80),   // #4CAF50 - Green
                Color.FromArgb(255, 255, 152, 0),   // #FF9800 - Orange
                Color.FromArgb(255, 156, 39, 176),  // #9C27B0 - Purple
                Color.FromArgb(255, 244, 67, 54)    // #F44336 - Red
            };

            // Draw axes
            DrawAxes(chartWidth, chartHeight);

            // Draw bars
            for (int i = 0; i < sortedCriteria.Count; i++)
            {
                var criterion = sortedCriteria[i];
                var x = ChartLeftMargin + i * (barGroupWidth + BarGroupSpacing) + BarGroupSpacing / 2;

                // Draw criterion label
                var labelText = new TextBlock
                {
                    Text = criterion,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Width = barGroupWidth,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(labelText, x);
                Canvas.SetTop(labelText, ChartPadding + chartHeight + 5);
                ComparisonChart.Children.Add(labelText);

                // Draw bars for each model
                var scores = _criteriaScores[criterion];
                for (int j = 0; j < _evaluations.Count; j++)
                {
                    var score = scores[j];
                    var barHeight = (score / 5.0) * chartHeight;
                    var barX = x + j * (barWidth + BarSpacing);
                    var barY = ChartPadding + chartHeight - barHeight;

                    var bar = new Rectangle
                    {
                        Width = barWidth,
                        Height = barHeight,
                        Fill = new SolidColorBrush(colors[j % colors.Length])
                    };
                    Canvas.SetLeft(bar, barX);
                    Canvas.SetTop(bar, barY);
                    ComparisonChart.Children.Add(bar);

                    // Add score label on top of bar
                    var scoreLabel = new TextBlock
                    {
                        Text = score.ToString("F1"),
                        FontSize = 11,
                        FontWeight = new Windows.UI.Text.FontWeight(600)
                    };
                    Canvas.SetLeft(scoreLabel, barX + barWidth / 2 - 10);
                    Canvas.SetTop(scoreLabel, barY - 15);
                    ComparisonChart.Children.Add(scoreLabel);
                }
            }

            // Draw legend
            DrawLegend(chartWidth, chartHeight, colors);
        }


        private void DrawAxes(double chartWidth, double chartHeight)
        {
            // Y-axis
            var yAxis = new Line
            {
                X1 = ChartLeftMargin,
                Y1 = ChartPadding,
                X2 = ChartLeftMargin,
                Y2 = ChartPadding + chartHeight,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1
            };
            ComparisonChart.Children.Add(yAxis);

            // X-axis
            var xAxis = new Line
            {
                X1 = ChartLeftMargin,
                Y1 = ChartPadding + chartHeight,
                X2 = ChartLeftMargin + chartWidth,
                Y2 = ChartPadding + chartHeight,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1
            };
            ComparisonChart.Children.Add(xAxis);

            // Y-axis labels and grid lines
            for (int i = 0; i <= 5; i++)
            {
                var y = ChartPadding + chartHeight - (i / 5.0 * chartHeight);

                // Grid line
                var gridLine = new Line
                {
                    X1 = ChartLeftMargin,
                    Y1 = y,
                    X2 = ChartLeftMargin + chartWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Colors.LightGray),
                    StrokeThickness = 0.5,
                    Opacity = 0.5
                };
                ComparisonChart.Children.Add(gridLine);

                // Label
                var label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 12,
                    TextAlignment = TextAlignment.Right
                };
                Canvas.SetLeft(label, ChartLeftMargin - 20);
                Canvas.SetTop(label, y - 8);
                ComparisonChart.Children.Add(label);
            }

            // Y-axis title
            var yAxisTitle = new TextBlock
            {
                Text = "Score (0-5)",
                FontSize = 14,
                FontWeight = new Windows.UI.Text.FontWeight(600),
                RenderTransform = new RotateTransform { Angle = -90 }
            };
            Canvas.SetLeft(yAxisTitle, 20);
            Canvas.SetTop(yAxisTitle, ChartPadding + chartHeight / 2);
            ComparisonChart.Children.Add(yAxisTitle);
            
            // Chart title
            var chartTitle = new TextBlock
            {
                Text = "Average Scores by Evaluation Criteria",
                FontSize = 16,
                FontWeight = new Windows.UI.Text.FontWeight(600),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Canvas.SetLeft(chartTitle, ChartLeftMargin + (chartWidth / 2) - 120);
            Canvas.SetTop(chartTitle, 10);
            ComparisonChart.Children.Add(chartTitle);
        }

        private void DrawLegend(double chartWidth, double chartHeight, Color[] colors)
        {
            var legendPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 20
            };
            Canvas.SetLeft(legendPanel, ChartLeftMargin);
            Canvas.SetTop(legendPanel, ChartPadding + chartHeight + 40);

            for (int i = 0; i < _evaluations.Count; i++)
            {
                var legendItem = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 4
                };

                var colorBox = new Rectangle
                {
                    Width = 12,
                    Height = 12,
                    Fill = new SolidColorBrush(colors[i % colors.Length])
                };
                legendItem.Children.Add(colorBox);

                var label = new TextBlock
                {
                    Text = _evaluations[i].ModelName ?? $"Model {i + 1}",
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };
                legendItem.Children.Add(label);

                legendPanel.Children.Add(legendItem);
            }

            ComparisonChart.Children.Add(legendPanel);
        }


        private void UpdateModelRankings()
        {
            var rankings = new List<ModelRanking>();
            var modelStats = new List<(string ModelName, double Average, int Wins)>();

            // Calculate statistics for each model
            for (int i = 0; i < _evaluations.Count; i++)
            {
                var evaluation = _evaluations[i];
                var wins = 0;
                var totalScore = 0.0;
                var count = 0;

                foreach (var criterion in _commonCriteria)
                {
                    var scores = _criteriaScores[criterion];
                    totalScore += scores[i];
                    count++;

                    // Check if this model has the highest score for this criterion
                    if (scores[i] == scores.Max())
                    {
                        wins++;
                    }
                }

                var average = count > 0 ? totalScore / count : 0;
                modelStats.Add((evaluation.ModelName ?? $"Model {i + 1}", average, wins));
            }

            // Sort by average score (descending)
            modelStats = modelStats.OrderByDescending(m => m.Average).ToList();

            // Create ranking objects
            for (int i = 0; i < modelStats.Count; i++)
            {
                var (modelName, average, wins) = modelStats[i];
                var winRate = _commonCriteria.Count > 0 ? (wins * 100.0 / _commonCriteria.Count) : 0;

                rankings.Add(new ModelRanking
                {
                    RankEmoji = i switch
                    {
                        0 => "🥇",
                        1 => "🥈",
                        2 => "🥉",
                        _ => "🏅"
                    },
                    ModelName = modelName,
                    OverallAverage = average.ToString("F1"),
                    CriteriaCount = _commonCriteria.Count,
                    WinCount = wins.ToString()
                });
            }

            ModelRankingsRepeater.ItemsSource = rankings;
        }

        private void UpdateKeyStatistics()
        {
            // Consistency (model with lowest standard deviation)
            var modelStdDevs = new List<(string ModelName, double StdDev)>();
            for (int i = 0; i < _evaluations.Count; i++)
            {
                var modelScores = new List<double>();
                foreach (var criterion in _commonCriteria)
                {
                    modelScores.Add(_criteriaScores[criterion][i]);
                }
                var stdDev = CalculateStandardDeviation(modelScores);
                modelStdDevs.Add((_evaluations[i].ModelName ?? $"Model {i + 1}", stdDev));
            }
            var mostConsistent = modelStdDevs.OrderBy(m => m.StdDev).First();
            ConsistencyText.Text = $"{mostConsistent.ModelName} (σ={mostConsistent.StdDev:F2})";

            // Best Performer (criterion with largest gap)
            var maxGap = 0.0;
            var bestPerformerCriterion = "";
            var bestPerformerModel = "";
            foreach (var criterion in _commonCriteria)
            {
                var scores = _criteriaScores[criterion];
                var max = scores.Max();
                var min = scores.Min();
                var gap = max - min;
                if (gap > maxGap)
                {
                    maxGap = gap;
                    bestPerformerCriterion = criterion;
                    var maxIndex = scores.IndexOf(max);
                    bestPerformerModel = _evaluations[maxIndex].ModelName ?? $"Model {maxIndex + 1}";
                }
            }
            BestPerformerText.Text = $"{bestPerformerCriterion}: {bestPerformerModel} (+{maxGap:F1})";

            // Most Agreement (criterion with smallest variance)
            var minVariance = double.MaxValue;
            var mostAgreementCriterion = "";
            foreach (var criterion in _commonCriteria)
            {
                var scores = _criteriaScores[criterion];
                var variance = CalculateVariance(scores);
                if (variance < minVariance)
                {
                    minVariance = variance;
                    mostAgreementCriterion = criterion;
                }
            }
            MostAgreementText.Text = $"{mostAgreementCriterion} (σ={Math.Sqrt(minVariance):F2})";

            // Evaluation Coverage
            EvaluationCoverageText.Text = $"100% ({_commonCriteria.Count}/{_commonCriteria.Count} criteria)";
        }

        private void UpdateDetailedScoresTable()
        {
            DetailedScoresGrid.Children.Clear();
            DetailedScoresGrid.RowDefinitions.Clear();
            DetailedScoresGrid.ColumnDefinitions.Clear();

            // Add columns
            DetailedScoresGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Criterion
            foreach (var eval in _evaluations)
            {
                DetailedScoresGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            }
            DetailedScoresGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Best
            DetailedScoresGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // Delta

            // Add header row
            DetailedScoresGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Header cells
            AddTableCell("Criterion", 0, 0, true);
            for (int i = 0; i < _evaluations.Count; i++)
            {
                AddTableCell(_evaluations[i].ModelName ?? $"Model {i + 1}", 0, i + 1, true);
            }
            AddTableCell("Best", 0, _evaluations.Count + 1, true);
            AddTableCell("Δ", 0, _evaluations.Count + 2, true);

            // Add data rows
            int row = 1;
            foreach (var criterion in _commonCriteria)
            {
                DetailedScoresGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                
                AddTableCell(criterion, row, 0);
                
                var scores = _criteriaScores[criterion];
                var maxScore = scores.Max();
                var minScore = scores.Min();
                var maxIndex = scores.IndexOf(maxScore);
                
                // Score cells
                for (int i = 0; i < scores.Count; i++)
                {
                    var cell = AddTableCell(scores[i].ToString("F1"), row, i + 1);
                    if (scores[i] == maxScore)
                    {
                        cell.FontWeight = new Windows.UI.Text.FontWeight(600);
                        cell.Foreground = new SolidColorBrush(Color.FromArgb(255, 76, 175, 80)); // Green
                    }
                }
                
                // Best model
                AddTableCell(_evaluations[maxIndex].ModelName ?? $"Model {maxIndex + 1}", row, _evaluations.Count + 1);
                
                // Delta
                AddTableCell((maxScore - minScore).ToString("F1"), row, _evaluations.Count + 2);
                
                row++;
            }
        }

        private TextBlock AddTableCell(string text, int row, int column, bool isHeader = false)
        {
            var cell = new TextBlock
            {
                Text = text,
                Margin = new Thickness(8),
                FontSize = isHeader ? 13 : 12,
                FontWeight = isHeader ? new Windows.UI.Text.FontWeight(600) : new Windows.UI.Text.FontWeight(400)
            };
            
            if (isHeader)
            {
                var border = new Border
                {
                    Child = cell,
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    BorderThickness = new Thickness(0, 0, 0, 1)
                };
                Grid.SetRow(border, row);
                Grid.SetColumn(border, column);
                DetailedScoresGrid.Children.Add(border);
            }
            else
            {
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);
                DetailedScoresGrid.Children.Add(cell);
            }
            
            return cell;
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count == 0) return 0;
            var mean = values.Average();
            var variance = values.Average(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(variance);
        }

        private double CalculateVariance(List<double> values)
        {
            if (values.Count == 0) return 0;
            var mean = values.Average();
            return values.Average(v => Math.Pow(v - mean, 2));
        }

        private void ShowError(string message)
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            ContentPanel.Visibility = Visibility.Collapsed;
            ErrorPanel.Visibility = Visibility.Visible;
            ErrorText.Text = message;
        }

        // Event handlers
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }


        private async void CopyChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var renderBitmap = new RenderTargetBitmap();
                await renderBitmap.RenderAsync(ComparisonChart);

                var pixels = await renderBitmap.GetPixelsAsync();
                var stream = new InMemoryRandomAccessStream();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)renderBitmap.PixelWidth,
                    (uint)renderBitmap.PixelHeight,
                    96.0, 96.0,
                    pixels.ToArray());

                await encoder.FlushAsync();

                var dataPackage = new DataPackage();
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));
                dataPackage.Properties.Title = "Evaluation Comparison Chart";
                Clipboard.SetContent(dataPackage);

                // Show confirmation
                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Chart copied to clipboard!",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to copy chart: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void SaveChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                savePicker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
                savePicker.FileTypeChoices.Add("JPEG Image", new List<string>() { ".jpg", ".jpeg" });
                savePicker.SuggestedFileName = $"EvaluationComparison_{DateTime.Now:yyyyMMdd_HHmmss}";

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var renderBitmap = new RenderTargetBitmap();
                    await renderBitmap.RenderAsync(ComparisonChart);

                    var pixels = await renderBitmap.GetPixelsAsync();
                    using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoderId = file.FileType.ToLower() == ".png" ? BitmapEncoder.PngEncoderId : BitmapEncoder.JpegEncoderId;
                        var encoder = await BitmapEncoder.CreateAsync(encoderId, stream);

                        encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Premultiplied,
                            (uint)renderBitmap.PixelWidth,
                            (uint)renderBitmap.PixelHeight,
                            96.0, 96.0,
                            pixels.ToArray());

                        await encoder.FlushAsync();
                    }

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Chart saved to {file.Path}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to save chart: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void PrintInsights_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show file save picker
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("HTML File", new List<string>() { ".html" });
                savePicker.SuggestedFileName = $"ComparisonReport_{DateTime.Now:yyyyMMdd_HHmmss}";

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var htmlContent = GeneratePrintHtml();
                    await FileIO.WriteTextAsync(file, htmlContent);
                    
                    // Open the file in the default browser
                    await Launcher.LaunchFileAsync(file);
                    
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
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to generate report: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private string GeneratePrintHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>Evaluation Comparison Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("h1, h2 { color: #333; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            sb.AppendLine(".ranking { margin: 10px 0; padding: 10px; border: 1px solid #ddd; }");
            sb.AppendLine(".stat-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }");
            sb.AppendLine(".stat-box { border: 1px solid #ddd; padding: 10px; }");
            sb.AppendLine("@media print { body { margin: 0; } }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine($"<h1>Evaluation Comparison Report</h1>");
            sb.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            sb.AppendLine($"<p>Comparing: {string.Join(" vs ", _evaluations.Select(e => e.ModelName ?? "Unknown"))}</p>");

            // Model Rankings
            sb.AppendLine("<h2>Model Rankings</h2>");
            var rankings = (ModelRankingsRepeater.ItemsSource as List<ModelRanking>) ?? new List<ModelRanking>();
            foreach (var ranking in rankings)
            {
                sb.AppendLine($"<div class='ranking'>");
                sb.AppendLine($"<h3>{ranking.RankEmoji} {ranking.ModelName}</h3>");
                sb.AppendLine($"<p>Overall Average: {ranking.OverallAverage}/5.0</p>");
                sb.AppendLine($"<p>Criteria Evaluated: {ranking.CriteriaCount}</p>");
                sb.AppendLine($"<p>Best in: {ranking.WinCount} criteria</p>");
                sb.AppendLine("</div>");
            }

            // Key Statistics
            sb.AppendLine("<h2>Key Statistics</h2>");
            sb.AppendLine("<div class='stat-grid'>");
            sb.AppendLine($"<div class='stat-box'><strong>Consistency:</strong><br>{ConsistencyText.Text}</div>");
            sb.AppendLine($"<div class='stat-box'><strong>Best Performer:</strong><br>{BestPerformerText.Text}</div>");
            sb.AppendLine($"<div class='stat-box'><strong>Most Agreement:</strong><br>{MostAgreementText.Text}</div>");
            sb.AppendLine($"<div class='stat-box'><strong>Evaluation Coverage:</strong><br>{EvaluationCoverageText.Text}</div>");
            sb.AppendLine("</div>");

            // Detailed Scores
            sb.AppendLine("<h2>Detailed Scores</h2>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Criterion</th>");
            foreach (var eval in _evaluations)
            {
                sb.AppendLine($"<th>{eval.ModelName ?? "Unknown"}</th>");
            }
            sb.AppendLine("<th>Best</th>");
            sb.AppendLine("<th>Δ</th>");
            sb.AppendLine("</tr>");

            foreach (var criterion in _commonCriteria)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{criterion}</td>");
                
                var scores = _criteriaScores[criterion];
                var maxScore = scores.Max();
                var minScore = scores.Min();
                var maxIndex = scores.IndexOf(maxScore);
                
                foreach (var score in scores)
                {
                    var style = score == maxScore ? " style='font-weight:bold;color:#4CAF50;'" : "";
                    sb.AppendLine($"<td{style}>{score:F1}</td>");
                }
                
                sb.AppendLine($"<td>{_evaluations[maxIndex].ModelName ?? $"Model {maxIndex + 1}"}</td>");
                sb.AppendLine($"<td>{maxScore - minScore:F1}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private async void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
                savePicker.SuggestedFileName = $"EvaluationComparison_{DateTime.Now:yyyyMMdd_HHmmss}";

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var csv = GenerateCsvContent();
                    await FileIO.WriteTextAsync(file, csv);

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Data exported to {file.Path}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to export data: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private string GenerateCsvContent()
        {
            var sb = new StringBuilder();
            
            // Header
            sb.Append("Criterion");
            foreach (var eval in _evaluations)
            {
                sb.Append($",{eval.ModelName ?? "Unknown"}");
            }
            sb.AppendLine(",Best,Delta");

            // Data rows
            foreach (var criterion in _commonCriteria)
            {
                sb.Append(criterion);
                
                var scores = _criteriaScores[criterion];
                var maxScore = scores.Max();
                var minScore = scores.Min();
                var maxIndex = scores.IndexOf(maxScore);
                
                foreach (var score in scores)
                {
                    sb.Append($",{score:F1}");
                }
                
                sb.Append($",{_evaluations[maxIndex].ModelName ?? $"Model {maxIndex + 1}"}");
                sb.AppendLine($",{maxScore - minScore:F1}");
            }

            return sb.ToString();
        }

        private async void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
                savePicker.SuggestedFileName = $"EvaluationComparison_{DateTime.Now:yyyyMMdd_HHmmss}";

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    var data = new
                    {
                        timestamp = DateTime.Now,
                        evaluations = _evaluations.Select(e => new
                        {
                            id = e.Id,
                            name = e.Name,
                            model = e.ModelName,
                            dataset = e.DatasetName
                        }),
                        commonCriteria = _commonCriteria,
                        scores = _criteriaScores,
                        rankings = ModelRankingsRepeater.ItemsSource,
                        statistics = new
                        {
                            consistency = ConsistencyText.Text,
                            bestPerformer = BestPerformerText.Text,
                            mostAgreement = MostAgreementText.Text,
                            coverage = EvaluationCoverageText.Text
                        }
                    };

                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    await FileIO.WriteTextAsync(file, json);

                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Data exported to {file.Path}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to export data: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}