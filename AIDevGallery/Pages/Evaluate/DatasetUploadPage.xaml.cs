// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using AIDevGallery.Utils;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Microsoft.UI.Dispatching;

namespace AIDevGallery.Pages.Evaluate
{
    /// <summary>
    /// Step 4: Upload Dataset
    /// Handles dataset upload and validation for all evaluation workflows.
    /// Supports both JSONL files and image folder drops.
    /// </summary>
    public sealed partial class DatasetUploadPage : Page, INotifyPropertyChanged
    {
        public delegate void ValidationChangedEventHandler(bool isValid);
        public event ValidationChangedEventHandler? ValidationChanged;

        // Supported image extensions (case-insensitive)
        private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB
        private const int MaxDatasetSize = 1000; // Maximum number of images

        // Current state
        private DatasetConfiguration? _datasetConfig;
        private EvaluationWorkflow _currentWorkflow;
        private ObservableCollection<DatasetPreviewItem> _previewItems = new();

        public DatasetUploadPage()
        {
            this.InitializeComponent();
            PreviewListView.ItemsSource = _previewItems;
        }

        private void UpdateUIForWorkflow()
        {
            switch (_currentWorkflow)
            {
                case EvaluationWorkflow.TestModel:
                    // For TestModel, we only need images (prompt comes from model config)
                    HeaderDescriptionText.Text = "Provide images for evaluation. The prompt from your model configuration will be applied to each image.";
                    RequirementsText.Inlines.Clear();
                    RequirementsText.Inlines.Add(new Run { Text = "• Format: Image folder or JSONL file\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Required: image paths only\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Image types: .jpg, .jpeg, .png, .gif, .bmp, .webp\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Max dataset size: 1,000 images" });
                    break;
                    
                case EvaluationWorkflow.EvaluateResponses:
                    HeaderDescriptionText.Text = "Provide a JSONL file with images, prompts, and model responses for evaluation.";
                    RequirementsText.Inlines.Clear();
                    RequirementsText.Inlines.Add(new Run { Text = "• Format: JSONL (one JSON object per line)\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Required fields: image_path, prompt, response, model\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Image types: .jpg, .jpeg, .png, .gif, .bmp, .webp\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Max dataset size: 1,000 images" });
                    break;
                    
                case EvaluationWorkflow.ImportResults:
                    HeaderDescriptionText.Text = "Provide a JSONL file with evaluation results to import and analyze.";
                    RequirementsText.Inlines.Clear();
                    RequirementsText.Inlines.Add(new Run { Text = "• Format: JSONL (one JSON object per line)\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Required fields: image_path, prompt, criteria_scores\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Optional fields: response, model\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Max dataset size: 1,000 entries" });
                    break;
            }
        }

        /// <summary>
        /// Sets the current workflow to determine validation rules
        /// </summary>
        internal void SetWorkflow(EvaluationWorkflow workflow)
        {
            _currentWorkflow = workflow;
            UpdateUIForWorkflow();
        }

        /// <summary>
        /// Checks if this step has valid input
        /// </summary>
        public bool IsValid => _datasetConfig?.ValidationResult?.IsValid ?? false;

        /// <summary>
        /// Gets the dataset configuration for this step
        /// </summary>
        internal DatasetConfiguration? GetStepData() => _datasetConfig;

        #region Drag and Drop Handlers

        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Drop to upload dataset";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsGlyphVisible = true;

                // Visual feedback
                VisualStateManager.GoToState(this, "DragOver", true);
                DropZoneBorderBrush.Color = (Color)Application.Current.Resources["SystemAccentColor"];
                DropZoneBorderBrush.Opacity = 1.0;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private async void DropZone_Drop(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            DropZoneBorderBrush.Opacity = 0.5;

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var item = items[0];
                    if (item is StorageFile file)
                    {
                        // Handle JSONL file drop
                        if (Path.GetExtension(file.Path).Equals(".jsonl", StringComparison.OrdinalIgnoreCase))
                        {
                            await ProcessJsonlFile(file.Path);
                        }
                        else
                        {
                            await ShowErrorDialog("Invalid File Type", "Please drop a .jsonl file or an image folder.");
                        }
                    }
                    else if (item is StorageFolder folder)
                    {
                        // Handle folder drop
                        await ProcessImageFolder(folder.Path);
                    }
                }
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            DropZoneBorderBrush.Opacity = 0.5;
        }

        #endregion

        #region File/Folder Selection

        private async void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".jsonl");
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    await ProcessJsonlFile(file.Path);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("File Selection Error", $"Could not open file picker: {ex.Message}");
            }
        }

        private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                var picker = new FolderPicker();
                picker.FileTypeFilter.Add("*");
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    await ProcessImageFolder(folder.Path);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Folder Selection Error", $"Could not open folder picker: {ex.Message}");
            }
        }

        #endregion

        #region Dataset Processing

        private async Task ProcessJsonlFile(string filePath)
        {
            ShowLoadingState("Validating JSONL file...");

            try
            {
                // Check file size
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > MaxFileSizeBytes)
                {
                    await ShowErrorDialog("File Too Large", $"Please select a file smaller than {MaxFileSizeBytes / (1024 * 1024)}MB.");
                    HideLoadingState();
                    return;
                }

                // Parse and validate JSONL
                var config = await Task.Run(() => ValidateJsonlFile(filePath));
                
                if (config != null)
                {
                    _datasetConfig = config;
                    
                    // Check if dataset exceeds limit BEFORE showing results
                    if (_datasetConfig.ExceedsLimit)
                    {
                        HideLoadingState();
                        
                        System.Diagnostics.Debug.WriteLine($"Dataset exceeds limit: {_datasetConfig.TotalEntries} > {MaxDatasetSize}");
                        
                        var shouldContinue = await ShowDatasetLimitWarningAsync();
                        if (!shouldContinue)
                        {
                            // User chose to select a different dataset
                            return;
                        }
                    }
                    
                    ShowValidationResults();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Validation Error", $"Error processing file: {ex.Message}");
            }
            finally
            {
                HideLoadingState();
            }
        }

        private async Task ProcessImageFolder(string folderPath)
        {
            ShowLoadingState("Scanning image folder...");

            try
            {
                var config = await Task.Run(() => CreateDatasetFromFolder(folderPath));
                
                if (config != null)
                {
                    _datasetConfig = config;
                    
                    // Check if dataset exceeds limit BEFORE showing results
                    if (_datasetConfig.ExceedsLimit)
                    {
                        HideLoadingState();
                        
                        System.Diagnostics.Debug.WriteLine($"Folder exceeds limit: {_datasetConfig.TotalEntries} > {MaxDatasetSize}");
                        
                        var shouldContinue = await ShowDatasetLimitWarningAsync();
                        if (!shouldContinue)
                        {
                            // User chose to select a different dataset
                            return;
                        }
                    }
                    
                    ShowValidationResults();
                    
                    // Offer to save generated JSONL
                    var saveButton = new Button { Content = "Save generated JSONL" };
                    saveButton.Click += async (s, e) => await SaveGeneratedJsonl();
                    
                    var infoBar = new InfoBar
                    {
                        Title = "Dataset Generated",
                        Message = "A JSONL file was generated from your image folder. You can save it for future use.",
                        Severity = InfoBarSeverity.Success,
                        IsOpen = true,
                        ActionButton = saveButton
                    };
                    
                    // Add to UI (implementation depends on layout)
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Folder Processing Error", $"Error processing folder: {ex.Message}");
            }
            finally
            {
                HideLoadingState();
            }
        }

        private DatasetConfiguration? ValidateJsonlFile(string filePath)
        {
            var config = new DatasetConfiguration
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                SourcePath = filePath,
                SourceType = DatasetSourceType.JsonlFile,
                BaseDirectory = Path.GetDirectoryName(filePath) ?? "",
                Entries = new List<DatasetEntry>(),
                ValidationResult = new ValidationResult { Issues = new List<ValidationIssue>() }
            };

            var jsonlDirectory = Path.GetDirectoryName(filePath) ?? "";
            var folderCounts = new Dictionary<string, int>();
            var issues = new List<ValidationIssue>();
            var lineNumber = 0;

            using (var reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null && config.Entries.Count < MaxDatasetSize)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var entry = ParseJsonlEntry(line, lineNumber, jsonlDirectory);
                        if (entry != null)
                        {
                            // Only add up to MaxDatasetSize entries
                            if (config.Entries.Count < MaxDatasetSize)
                            {
                                config.Entries.Add(entry);
                                
                                // Track folder structure
                                var folder = Path.GetDirectoryName(entry.OriginalImagePath) ?? "root";
                                folderCounts[folder] = folderCounts.GetValueOrDefault(folder) + 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        issues.Add(new ValidationIssue
                        {
                            Type = IssueType.InvalidJson,
                            Message = $"Line {lineNumber}: {ex.Message}",
                            LineNumber = lineNumber
                        });
                    }
                }

                config.TotalEntries = lineNumber;
            }

            config.ValidEntries = config.Entries.Count;
            config.ExceedsLimit = config.TotalEntries > MaxDatasetSize;
            config.FolderStructure = folderCounts;
            config.ValidationResult.Issues = issues;
            config.ValidationResult.IsValid = config.ValidEntries > 0 && issues.All(i => i.Type != IssueType.Error);
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Dataset validation: TotalEntries={config.TotalEntries}, ValidEntries={config.ValidEntries}, ExceedsLimit={config.ExceedsLimit}");

            if (config.ExceedsLimit)
            {
                config.ValidationResult.Warnings.Add($"Dataset exceeds {MaxDatasetSize} image limit. Only the first {MaxDatasetSize} images will be processed.");
            }

            return config;
        }

        private DatasetEntry? ParseJsonlEntry(string line, int lineNumber, string baseDirectory)
        {
            var json = JsonDocument.Parse(line);
            var root = json.RootElement;

            // Check required fields based on workflow
            if (!root.TryGetProperty("image_path", out var imagePathElement))
            {
                throw new JsonException("Missing required field: image_path");
            }

            var imagePath = imagePathElement.GetString() ?? "";
            string prompt = "";
            
            // For TestModel workflow, prompt is optional (will use model config prompt)
            if (_currentWorkflow == EvaluationWorkflow.TestModel)
            {
                if (root.TryGetProperty("prompt", out var promptElement))
                {
                    prompt = promptElement.GetString() ?? "";
                }
                // If no prompt provided, we'll use the one from model config
            }
            else
            {
                // For other workflows, prompt is required
                if (!root.TryGetProperty("prompt", out var promptElement))
                {
                    throw new JsonException("Missing required field: prompt");
                }
                prompt = promptElement.GetString() ?? "";
            }

            // Resolve image path
            var resolvedPath = ResolveImagePath(imagePath, baseDirectory);
            
            // Validate image exists and has valid extension
            if (!File.Exists(resolvedPath))
            {
                throw new FileNotFoundException($"Image not found: {imagePath}");
            }

            if (!IsValidImageExtension(resolvedPath))
            {
                throw new InvalidOperationException($"Unsupported image format: {Path.GetExtension(resolvedPath)}");
            }

            var entry = new DatasetEntry
            {
                OriginalImagePath = imagePath,
                ResolvedImagePath = resolvedPath,
                Prompt = prompt
            };

            // Additional fields for specific workflows
            if (_currentWorkflow == EvaluationWorkflow.EvaluateResponses || 
                _currentWorkflow == EvaluationWorkflow.ImportResults)
            {
                if (root.TryGetProperty("response", out var responseElement))
                {
                    entry.Response = responseElement.GetString();
                }
                if (root.TryGetProperty("model", out var modelElement))
                {
                    entry.Model = modelElement.GetString();
                }

                if (_currentWorkflow == EvaluationWorkflow.EvaluateResponses && 
                    (string.IsNullOrEmpty(entry.Response) || string.IsNullOrEmpty(entry.Model)))
                {
                    throw new JsonException("EvaluateResponses workflow requires 'response' and 'model' fields");
                }
            }

            if (_currentWorkflow == EvaluationWorkflow.ImportResults)
            {
                if (root.TryGetProperty("criteria_scores", out var scoresElement))
                {
                    entry.Scores = JsonSerializer.Deserialize<Dictionary<string, object>>(scoresElement.GetRawText());
                }
                else
                {
                    throw new JsonException("ImportResults workflow requires 'criteria_scores' field");
                }
            }

            return entry;
        }

        private string ResolveImagePath(string imagePath, string baseDirectory)
        {
            // If absolute path and exists, use it
            if (Path.IsPathRooted(imagePath) && File.Exists(imagePath))
            {
                return imagePath;
            }

            // Try relative to base directory
            var relativePath = Path.Combine(baseDirectory, imagePath);
            if (File.Exists(relativePath))
            {
                return Path.GetFullPath(relativePath);
            }

            // Normalize path separators and try again
            var normalizedPath = imagePath.Replace('/', Path.DirectorySeparatorChar);
            relativePath = Path.Combine(baseDirectory, normalizedPath);
            if (File.Exists(relativePath))
            {
                return Path.GetFullPath(relativePath);
            }

            throw new FileNotFoundException($"Image not found: {imagePath}");
        }

        private bool IsValidImageExtension(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return SupportedImageExtensions.Contains(extension);
        }

        private DatasetConfiguration CreateDatasetFromFolder(string folderPath)
        {
            var config = new DatasetConfiguration
            {
                Name = Path.GetFileName(folderPath),
                SourcePath = folderPath,
                SourceType = DatasetSourceType.ImageFolder,
                BaseDirectory = folderPath,
                Entries = new List<DatasetEntry>(),
                ValidationResult = new ValidationResult { Issues = new List<ValidationIssue>() }
            };

            var folderCounts = new Dictionary<string, int>();
            var searchOption = SearchOption.AllDirectories; // Support subfolders
            var allImageFiles = new List<string>();

            // First, get ALL image files to count total
            foreach (var extension in SupportedImageExtensions)
            {
                var files = Directory.GetFiles(folderPath, $"*{extension}", searchOption)
                    .Union(Directory.GetFiles(folderPath, $"*{extension.ToUpper()}", searchOption));
                allImageFiles.AddRange(files);
            }
            
            // Remove duplicates and get total count
            allImageFiles = allImageFiles.Distinct().ToList();
            config.TotalEntries = allImageFiles.Count;
            
            System.Diagnostics.Debug.WriteLine($"CreateDatasetFromFolder: Found {config.TotalEntries} total images");

            // Now add entries up to the limit
            foreach (var file in allImageFiles.Take(MaxDatasetSize))
            {
                var relativePath = Path.GetRelativePath(folderPath, file);
                var folder = Path.GetDirectoryName(relativePath) ?? "root";
                
                config.Entries.Add(new DatasetEntry
                {
                    OriginalImagePath = relativePath,
                    ResolvedImagePath = file,
                    Prompt = "" // For TestModel, prompt will come from model config
                });

                folderCounts[folder] = folderCounts.GetValueOrDefault(folder) + 1;
            }

            config.ValidEntries = config.Entries.Count;
            config.ExceedsLimit = config.TotalEntries > MaxDatasetSize;
            config.FolderStructure = folderCounts;
            config.ValidationResult.IsValid = config.ValidEntries > 0;

            if (config.ValidEntries == 0)
            {
                config.ValidationResult.Issues.Add(new ValidationIssue
                {
                    Type = IssueType.Error,
                    Message = "No supported image files found in the selected folder."
                });
            }
            
            if (config.ExceedsLimit)
            {
                config.ValidationResult.Warnings.Add($"Folder contains {config.TotalEntries:N0} images. Only the first {MaxDatasetSize:N0} will be processed.");
            }

            return config;
        }

        #endregion

        #region UI Updates

        private void ShowLoadingState(string message)
        {
            UploadPanel.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;
            LoadingText.Text = message;
            ValidationResultsPanel.Visibility = Visibility.Collapsed;
        }

        private void HideLoadingState()
        {
            LoadingPanel.Visibility = Visibility.Collapsed;
            UploadPanel.Visibility = Visibility.Visible;
        }

        private void ShowValidationResults()
        {
            if (_datasetConfig == null) return;

            UploadPanel.Visibility = Visibility.Collapsed;
            ValidationResultsPanel.Visibility = Visibility.Visible;

            // Update dataset path
            DatasetPathText.Text = _datasetConfig.SourcePath;

            // Update validation status
            if (_datasetConfig.ValidationResult.IsValid)
            {
                // Check if dataset exceeds limit
                if (_datasetConfig.ExceedsLimit)
                {
                    ValidationInfoBar.Severity = InfoBarSeverity.Warning;
                    ValidationInfoBar.Title = "Dataset Too Large";
                    ValidationInfoBar.Message = $"Dataset contains {_datasetConfig.TotalEntries:N0} entries but only the first {MaxDatasetSize:N0} will be processed.";
                    
                    // Note: Warning dialog already shown during ProcessJsonlFile/ProcessImageFolder
                }
                else
                {
                    ValidationInfoBar.Severity = InfoBarSeverity.Success;
                    ValidationInfoBar.Title = "Dataset Valid";
                    ValidationInfoBar.Message = $"{_datasetConfig.ValidEntries} entries loaded successfully";
                }

                // Show statistics
                StatisticsPanel.Visibility = Visibility.Visible;
                if (_datasetConfig.ExceedsLimit)
                {
                    EntryCountText.Text = $"📊 {MaxDatasetSize:N0} of {_datasetConfig.TotalEntries:N0} entries will be processed";
                }
                else
                {
                    EntryCountText.Text = $"📊 {_datasetConfig.ValidEntries} valid entries";
                }
                BaseDirectoryText.Text = $"📁 Base: {_datasetConfig.BaseDirectory}";

                // Show folder structure if applicable
                if (_datasetConfig.FolderStructure.Count > 1)
                {
                    FolderStructureExpander.Visibility = Visibility.Visible;
                    BuildFolderTreeView();
                }

                // Enable action buttons
                TestSampleImagesButton.IsEnabled = true;
                GroupByFolderToggle.IsEnabled = _datasetConfig.FolderStructure.Count > 1;

                // Show preview
                ShowPreview();

                // Check for relative paths
                CheckRelativePaths();
            }
            else
            {
                ValidationInfoBar.Severity = InfoBarSeverity.Error;
                ValidationInfoBar.Title = "Validation Failed";
                ValidationInfoBar.Message = GetValidationErrorSummary();
                
                // Show first few errors
                if (_datasetConfig.ValidationResult.Issues.Any())
                {
                    var errorList = string.Join("\n", _datasetConfig.ValidationResult.Issues.Take(5).Select(i => $"• {i.Message}"));
                    ValidationInfoBar.Message += $"\n\nErrors:\n{errorList}";
                    
                    if (_datasetConfig.ValidationResult.Issues.Count > 5)
                    {
                        ValidationInfoBar.Message += $"\n... and {_datasetConfig.ValidationResult.Issues.Count - 5} more";
                    }
                }
            }

            // Notify parent
            ValidationChanged?.Invoke(IsValid);
            UpdateParentDialogState();
        }

        private void BuildFolderTreeView()
        {
            if (_datasetConfig == null) return;

            FolderTreeView.RootNodes.Clear();

            foreach (var folder in _datasetConfig.FolderStructure.OrderBy(f => f.Key))
            {
                var node = new TreeViewNode
                {
                    Content = $"📁 {folder.Key} ({folder.Value} images)",
                    IsExpanded = true
                };
                FolderTreeView.RootNodes.Add(node);
            }
        }

        private void ShowPreview()
        {
            if (_datasetConfig == null || !_datasetConfig.Entries.Any()) return;

            _previewItems.Clear();
            foreach (var entry in _datasetConfig.Entries.Take(5))
            {
                var prompt = entry.Prompt;
                if (_currentWorkflow == EvaluationWorkflow.TestModel && string.IsNullOrEmpty(prompt))
                {
                    prompt = "[Will use prompt from model configuration]";
                }
                
                _previewItems.Add(new DatasetPreviewItem
                {
                    ImagePath = entry.OriginalImagePath,
                    Prompt = TruncateText(prompt, 100)
                });
            }

            PreviewExpander.Visibility = Visibility.Visible;
        }

        private void CheckRelativePaths()
        {
            if (_datasetConfig == null) return;

            var hasRelativePaths = _datasetConfig.Entries.Any(e => !Path.IsPathRooted(e.OriginalImagePath));
            RelativePathWarning.IsOpen = hasRelativePaths;
        }

        private string GetValidationErrorSummary()
        {
            if (_datasetConfig?.ValidationResult == null) return "Unknown error";

            var errorCount = _datasetConfig.ValidationResult.Issues.Count(i => i.Type == IssueType.Error);
            var warningCount = _datasetConfig.ValidationResult.Issues.Count(i => i.Type == IssueType.Warning);

            return $"Found {errorCount} errors and {warningCount} warnings";
        }

        private string TruncateText(string text, int maxLength)
        {
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        private void UpdateParentDialogState()
        {
            // Navigate up the visual tree to find the ContentDialog
            var current = Parent;
            while (current != null)
            {
                if (current is ContentDialog dialog)
                {
                    dialog.IsPrimaryButtonEnabled = IsValid;
                    break;
                }
                current = (current as FrameworkElement)?.Parent;
            }
        }

        #endregion

        #region Button Handlers

        private async void TestSampleImages_Click(object sender, RoutedEventArgs e)
        {
            if (_datasetConfig == null || !_datasetConfig.Entries.Any()) return;

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Sample Images",
                CloseButtonText = "Close"
            };

            var panel = new StackPanel { Spacing = 12 };
            
            foreach (var entry in _datasetConfig.Entries.Take(3))
            {
                try
                {
                    var image = new Image
                    {
                        Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(entry.ResolvedImagePath)),
                        MaxHeight = 150,
                        MaxWidth = 200,
                        Stretch = Stretch.Uniform
                    };
                    panel.Children.Add(image);
                }
                catch
                {
                    panel.Children.Add(new TextBlock { Text = $"Failed to load: {entry.OriginalImagePath}" });
                }
            }

            dialog.Content = panel;
            await dialog.ShowAsync();
        }

        private void GroupByFolder_Checked(object sender, RoutedEventArgs e)
        {
            // TODO: Implement grouped view
        }

        private void GroupByFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO: Implement flat view
        }

        private void ChangeDataset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private async void DownloadExample_Click(object sender, RoutedEventArgs e)
        {
            var example = GetExampleJsonl();
            
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.FileTypeChoices.Add("JSONL", new List<string> { ".jsonl" });
                savePicker.SuggestedFileName = "example_dataset";
                
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, example);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Save Error", $"Could not save file: {ex.Message}");
            }
        }

        private void CopyExample_Click(object sender, RoutedEventArgs e)
        {
            var example = GetExampleJsonl();
            var dataPackage = new DataPackage();
            dataPackage.SetText(example);
            Clipboard.SetContentWithOptions(dataPackage, null);
            
            // Show confirmation
            TeachingTip copyTip = new TeachingTip
            {
                Target = CopyExampleButton,
                Title = "Copied!",
                Subtitle = "Example JSONL copied to clipboard",
                IsOpen = true
            };
            
            // Auto-close after 2 seconds
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) =>
            {
                copyTip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        private string GetExampleJsonl()
        {
            var examples = new StringBuilder();
            
            switch (_currentWorkflow)
            {
                case EvaluationWorkflow.TestModel:
                    examples.AppendLine(@"{""image_path"": ""images/products/shoe1.jpg""}");
                    examples.AppendLine(@"{""image_path"": ""images/products/shoe2.jpg""}");
                    examples.AppendLine(@"{""image_path"": ""C:\\data\\images\\bag1.png""}");
                    examples.AppendLine(@"// Note: For Test Model workflow, the prompt from your model configuration will be used");
                    break;
                    
                case EvaluationWorkflow.EvaluateResponses:
                    examples.AppendLine(@"{""image_path"": ""images/product1.jpg"", ""prompt"": ""Describe this product"", ""response"": ""A red running shoe with white laces and modern design"", ""model"": ""gpt-4o""}");
                    examples.AppendLine(@"{""image_path"": ""images/product2.jpg"", ""prompt"": ""Describe this product"", ""response"": ""A leather handbag with gold hardware"", ""model"": ""gpt-4o""}");
                    break;
                    
                case EvaluationWorkflow.ImportResults:
                    examples.AppendLine(@"{""image_path"": ""images/test1.jpg"", ""prompt"": ""Describe this image"", ""response"": ""A sunny beach scene"", ""model"": ""gpt-4o"", ""criteria_scores"": {""accuracy"": {""score"": 4, ""range"": ""1-5""}, ""completeness"": {""score"": 3, ""range"": ""1-5""}}}");
                    break;
            }
            
            return examples.ToString();
        }

        private async Task SaveGeneratedJsonl()
        {
            if (_datasetConfig == null || _datasetConfig.SourceType != DatasetSourceType.ImageFolder) return;

            var jsonl = new StringBuilder();
            foreach (var entry in _datasetConfig.Entries)
            {
                if (_currentWorkflow == EvaluationWorkflow.TestModel)
                {
                    // For TestModel, only include image_path
                    var obj = new { image_path = entry.OriginalImagePath };
                    jsonl.AppendLine(JsonSerializer.Serialize(obj));
                }
                else
                {
                    // For other workflows, include prompt
                    var obj = new
                    {
                        image_path = entry.OriginalImagePath,
                        prompt = entry.Prompt
                    };
                    jsonl.AppendLine(JsonSerializer.Serialize(obj));
                }
            }

            try
            {
                var savePicker = new FileSavePicker();
                savePicker.FileTypeChoices.Add("JSONL", new List<string> { ".jsonl" });
                savePicker.SuggestedFileName = "dataset";
                
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
                
                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, jsonl.ToString());
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Save Error", $"Could not save file: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        private async Task ShowErrorDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = title,
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }

        public void Reset()
        {
            _datasetConfig = null;
            _previewItems.Clear();
            
            // Reset UI
            UploadPanel.Visibility = Visibility.Visible;
            ValidationResultsPanel.Visibility = Visibility.Collapsed;
            RelativePathWarning.IsOpen = false;
            
            // Clear validation state
            ValidationChanged?.Invoke(false);
            UpdateParentDialogState();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private async Task<bool> ShowDatasetLimitWarningAsync()
        {
            System.Diagnostics.Debug.WriteLine($"ShowDatasetLimitWarningAsync called");
            
            if (_datasetConfig == null) 
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: _datasetConfig is null, returning");
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine($"Creating ContentDialog for dataset warning...");
            
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Large Dataset Warning",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"Your dataset contains {_datasetConfig.TotalEntries:N0} items, but the evaluation wizard has a limit of {MaxDatasetSize:N0} items.",
                            TextWrapping = TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = "Only the first 1,000 items will be processed. This limitation exists to ensure reasonable evaluation times and system performance.",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                        },
                        new TextBlock
                        {
                            Text = "To evaluate more items, consider:",
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 8, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = "• Splitting your dataset into smaller batches\n• Using a representative sample\n• Running multiple evaluations",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(16, 0, 0, 0)
                        }
                    }
                },
                PrimaryButtonText = "Continue with first 1,000",
                SecondaryButtonText = "Choose different dataset",
                DefaultButton = ContentDialogButton.Secondary
            };

            System.Diagnostics.Debug.WriteLine($"Showing dataset warning dialog...");
            
            var result = await dialog.ShowAsync();
            
            System.Diagnostics.Debug.WriteLine($"Dialog result: {result}");
            
            if (result == ContentDialogResult.Secondary)
            {
                System.Diagnostics.Debug.WriteLine($"User chose to select different dataset");
                // Clear current dataset and let user choose again
                Reset();
                return false; // User wants to choose different dataset
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"User chose to continue with first 1000 items");
                return true; // User wants to continue with truncated dataset
            }
        }
    }

    #region Helper Classes

    public class DatasetPreviewItem
    {
        public string ImagePath { get; set; } = "";
        public string Prompt { get; set; } = "";
    }

    #endregion
}