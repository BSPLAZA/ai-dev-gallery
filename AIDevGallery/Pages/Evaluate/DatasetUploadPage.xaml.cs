// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AIDevGallery.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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

namespace AIDevGallery.Pages.Evaluate
{
    /// <summary>
    /// Step 4: Upload Dataset
    /// Handles dataset upload and validation for all evaluation workflows.
    /// Supports both JSONL files and image folder drops.
    /// </summary>
    public sealed partial class DatasetUploadPage : Page, INotifyPropertyChanged
    {
        private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB
        private const int MaxDatasetSize = 1000; // Maximum number of images

        // Supported image extensions (case-insensitive)
        private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        public delegate void ValidationChangedEventHandler(bool isValid);
        public event ValidationChangedEventHandler? ValidationChanged;

        // Current state
        private DatasetConfiguration? _datasetConfig;
        private EvaluationWorkflow _currentWorkflow;
        private ObservableCollection<DatasetPreviewItem> _previewItems = new();

        private EvaluationWizardState? _wizardState;
        
        // Two-part upload state
        private string? _imagesFolderPath;
        private int _imagesCount;
        private string? _jsonlFilePath;
        private Dictionary<string, string>? _imagePathMapping;  // Maps JSONL paths to actual image paths
        private string? _modelName;  // For workflows 2 & 3

        public DatasetUploadPage()
        {
            this.InitializeComponent();
            PreviewListView.ItemsSource = _previewItems;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check if we have state to restore
            if (e.Parameter is EvaluationWizardState state)
            {
                _wizardState = state;
                RestoreFromState();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SaveToState();
        }

        private void RestoreFromState()
        {
            if (_wizardState?.Dataset == null) return;

            // Restore the dataset configuration
            _datasetConfig = _wizardState.Dataset;
            
            // Update workflow if available
            if (_wizardState.Workflow.HasValue)
            {
                _currentWorkflow = _wizardState.Workflow.Value;
                UpdateUIForWorkflow();
            }
            
            // Show validation results for the restored dataset
            ShowValidationResults();
        }

        private void SaveToState()
        {
            if (_wizardState == null) return;

            // Save current dataset configuration to state
            _wizardState.Dataset = _datasetConfig;
            
            // Save model name if entered
            if (!string.IsNullOrEmpty(ModelNameInput.Text))
            {
                _modelName = ModelNameInput.Text.Trim();
            }
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
                    
                    // Show single upload area for workflow 1
                    SingleUploadArea.Visibility = Visibility.Visible;
                    TwoPartUploadArea.Visibility = Visibility.Collapsed;
                    break;
                    
                case EvaluationWorkflow.EvaluateResponses:
                    HeaderDescriptionText.Text = "Upload your images and evaluation data separately for better organization.";
                    RequirementsText.Inlines.Clear();
                    RequirementsText.Inlines.Add(new Run { Text = "• Step 1: Upload image folder\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Step 2: Upload JSONL with prompts and responses\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Model name will be applied to all entries\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Max dataset size: 1,000 images" });
                    
                    // Show two-part upload for workflow 2
                    SingleUploadArea.Visibility = Visibility.Collapsed;
                    TwoPartUploadArea.Visibility = Visibility.Visible;
                    ModelNamePanel.Visibility = Visibility.Visible;
                    JsonlRequiredFieldsText.Text = "image_path, prompt, response";
                    break;
                    
                case EvaluationWorkflow.ImportResults:
                    HeaderDescriptionText.Text = "Upload your images and evaluation results separately.";
                    RequirementsText.Inlines.Clear();
                    RequirementsText.Inlines.Add(new Run { Text = "• Step 1: Upload image folder\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Step 2: Upload JSONL with results\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Model name will be applied to all entries\n" });
                    RequirementsText.Inlines.Add(new Run { Text = "• Max dataset size: 1,000 entries" });
                    
                    // Show two-part upload for workflow 3
                    SingleUploadArea.Visibility = Visibility.Collapsed;
                    TwoPartUploadArea.Visibility = Visibility.Visible;
                    ModelNamePanel.Visibility = Visibility.Visible;
                    JsonlRequiredFieldsText.Text = "image_path, prompt, response, criteria_scores";
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

        #region Two-Part Upload Handlers
        
        private void ImageDropZone_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Drop image folder";
                e.DragUIOverride.IsCaptionVisible = true;
                ImageDropZoneBorderBrush.Color = (Color)Application.Current.Resources["SystemAccentColor"];
                ImageDropZoneBorderBrush.Opacity = 1.0;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        
        private async void ImageDropZone_Drop(object sender, DragEventArgs e)
        {
            ImageDropZoneBorderBrush.Opacity = 0.5;
            
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0 && items[0] is StorageFolder folder)
                {
                    await ProcessImageFolderForTwoPart(folder.Path);
                }
            }
        }
        
        private void ImageDropZone_DragLeave(object sender, DragEventArgs e)
        {
            ImageDropZoneBorderBrush.Opacity = 0.5;
        }
        
        private async void BrowseImageFolder_Click(object sender, RoutedEventArgs e)
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
                    await ProcessImageFolderForTwoPart(folder.Path);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Folder Selection Error", $"Could not open folder picker: {ex.Message}");
            }
        }
        
        private void JsonlDropZone_DragOver(object sender, DragEventArgs e)
        {
            if (_imagesFolderPath == null) return; // Must have images first
            
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Drop JSONL file";
                e.DragUIOverride.IsCaptionVisible = true;
                JsonlDropZoneBorderBrush.Color = (Color)Application.Current.Resources["SystemAccentColor"];
                JsonlDropZoneBorderBrush.Opacity = 1.0;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        
        private async void JsonlDropZone_Drop(object sender, DragEventArgs e)
        {
            JsonlDropZoneBorderBrush.Opacity = 0.5;
            
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0 && items[0] is StorageFile file)
                {
                    if (Path.GetExtension(file.Path).Equals(".jsonl", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessJsonlForTwoPart(file.Path);
                    }
                }
            }
        }
        
        private void JsonlDropZone_DragLeave(object sender, DragEventArgs e)
        {
            JsonlDropZoneBorderBrush.Opacity = 0.5;
        }
        
        private async void BrowseJsonl_Click(object sender, RoutedEventArgs e)
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
                    await ProcessJsonlForTwoPart(file.Path);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("File Selection Error", $"Could not open file picker: {ex.Message}");
            }
        }
        
        private void ChangeImageFolder_Click(object sender, RoutedEventArgs e)
        {
            ResetImageUpload();
        }
        
        private void ChangeJsonlFile_Click(object sender, RoutedEventArgs e)
        {
            ResetJsonlUpload();
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
                        
                        
                        // Show inline error instead of dialog
                        ShowDatasetSizeError();
                        return; // Don't show validation results - dataset is rejected
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
                        
                        
                        // Show inline error instead of dialog
                        ShowDatasetSizeError();
                        return; // Don't show validation results - dataset is rejected
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
                BaseDirectory = Path.GetDirectoryName(filePath) ?? string.Empty,
                Entries = new List<DatasetEntry>(),
                ValidationResult = new ValidationResult { Issues = new List<ValidationIssue>() }
            };

            var jsonlDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
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

            var imagePath = imagePathElement.GetString() ?? string.Empty;
            string prompt = string.Empty;
            
            // For TestModel workflow, prompt is optional (will use model config prompt)
            if (_currentWorkflow == EvaluationWorkflow.TestModel)
            {
                if (root.TryGetProperty("prompt", out var promptElement))
                {
                    prompt = promptElement.GetString() ?? string.Empty;
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
                prompt = promptElement.GetString() ?? string.Empty;
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
                    .Union(Directory.GetFiles(folderPath, $"*{extension.ToUpperInvariant()}", searchOption));
                allImageFiles.AddRange(files);
            }
            
            // Remove duplicates and get total count
            allImageFiles = allImageFiles.Distinct().ToList();
            config.TotalEntries = allImageFiles.Count;
            

            // Now add entries up to the limit
            foreach (var file in allImageFiles.Take(MaxDatasetSize))
            {
                var relativePath = Path.GetRelativePath(folderPath, file);
                var folder = Path.GetDirectoryName(relativePath) ?? "root";
                
                config.Entries.Add(new DatasetEntry
                {
                    OriginalImagePath = relativePath,
                    ResolvedImagePath = file,
                    Prompt = string.Empty // For TestModel, prompt will come from model config
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

        #region Two-Part Upload Processing
        
        private async Task ProcessImageFolderForTwoPart(string folderPath)
        {
            try
            {
                // Count images in folder
                var imageFiles = new List<string>();
                foreach (var extension in SupportedImageExtensions)
                {
                    var files = Directory.GetFiles(folderPath, $"*{extension}", SearchOption.AllDirectories)
                        .Union(Directory.GetFiles(folderPath, $"*{extension.ToUpperInvariant()}", SearchOption.AllDirectories));
                    imageFiles.AddRange(files);
                }
                
                imageFiles = imageFiles.Distinct().ToList();
                
                if (imageFiles.Count == 0)
                {
                    await ShowErrorDialog("No Images Found", "The selected folder does not contain any supported image files.");
                    return;
                }
                
                if (imageFiles.Count > MaxDatasetSize)
                {
                    await ShowErrorDialog("Too Many Images", $"The folder contains {imageFiles.Count:N0} images. Please select a folder with no more than {MaxDatasetSize:N0} images.");
                    return;
                }
                
                // Store image folder info
                _imagesFolderPath = folderPath;
                _imagesCount = imageFiles.Count;
                
                // Update UI
                ImageUploadPanel.Visibility = Visibility.Collapsed;
                ImageUploadSuccess.Visibility = Visibility.Visible;
                ImageFolderPathText.Text = Path.GetFileName(folderPath);
                ImageCountText.Text = $"{_imagesCount:N0} images found";
                
                // Enable JSONL upload
                JsonlDropZone.IsHitTestVisible = true;
                JsonlDropZone.Opacity = 1.0;
                BrowseJsonlButton.IsEnabled = true;
                JsonlUploadText.Text = "Drop JSONL file here or click to browse";
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error Processing Folder", ex.Message);
            }
        }
        
        private async Task ProcessJsonlForTwoPart(string jsonlPath)
        {
            try
            {
                // Validate JSONL and match with images
                var validationResult = await ValidateTwoPartUpload(jsonlPath);
                
                if (validationResult.IsValid)
                {
                    _jsonlFilePath = jsonlPath;
                    
                    // Update UI to show success
                    JsonlUploadPanel.Visibility = Visibility.Collapsed;
                    JsonlUploadSuccess.Visibility = Visibility.Visible;
                    JsonlFilePathText.Text = Path.GetFileName(jsonlPath);
                    JsonlEntryCountText.Text = $"{validationResult.MatchedCount:N0} entries matched";
                    
                    // Show validation report
                    ShowTwoPartValidationReport(validationResult);
                    
                    // Create final dataset configuration if valid
                    if (validationResult.IsValid)
                    {
                        await CreateFinalDatasetConfiguration(validationResult);
                    }
                }
                else
                {
                    // Show validation errors
                    ShowTwoPartValidationReport(validationResult);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error Processing JSONL", ex.Message);
            }
        }
        
        private async Task<TwoPartValidationResult> ValidateTwoPartUpload(string jsonlPath)
        {
            return await Task.Run(() =>
            {
                var result = new TwoPartValidationResult();
                var imageFiles = GetAllImageFiles(_imagesFolderPath!);
                var imageLookup = CreateImageLookup(imageFiles);
                
                // Parse JSONL and match with images
                var entries = new List<DatasetEntry>();
                var unmatchedPaths = new List<string>();
                
                using (var reader = new StreamReader(jsonlPath))
                {
                    string? line;
                    int lineNumber = 0;
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        try
                        {
                            var json = JsonDocument.Parse(line);
                            var root = json.RootElement;
                            
                            // Get image path from JSONL
                            if (!root.TryGetProperty("image_path", out var imagePathElement))
                            {
                                result.Errors.Add($"Line {lineNumber}: Missing image_path field");
                                continue;
                            }
                            
                            var imagePath = imagePathElement.GetString() ?? string.Empty;
                            
                            // Try to match with actual images
                            var matchedImagePath = MatchImagePath(imagePath, imageLookup);
                            if (matchedImagePath == null)
                            {
                                unmatchedPaths.Add(imagePath);
                                continue;
                            }
                            
                            // Validate other required fields based on workflow
                            if (!ValidateJsonlFields(root, _currentWorkflow, lineNumber, result))
                            {
                                continue;
                            }
                            
                            // Create entry
                            var entry = CreateDatasetEntryFromJson(root, matchedImagePath, imagePath);
                            if (entry != null)
                            {
                                entries.Add(entry);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Line {lineNumber}: {ex.Message}");
                        }
                    }
                }
                
                result.MatchedCount = entries.Count;
                result.UnmatchedCount = unmatchedPaths.Count;
                result.Entries = entries;
                result.UnmatchedPaths = unmatchedPaths;
                
                // Check if we have enough matches
                if (result.MatchedCount == 0)
                {
                    result.Errors.Add("No images could be matched between the folder and JSONL file.");
                    result.IsValid = false;
                }
                else if (result.UnmatchedCount > 0)
                {
                    result.Warnings.Add($"{result.UnmatchedCount} image paths in JSONL could not be matched to images in the folder.");
                    result.IsValid = result.Errors.Count == 0; // Valid if no errors, just warnings
                }
                else
                {
                    result.IsValid = result.Errors.Count == 0;
                }
                
                return result;
            });
        }
        
        private Dictionary<string, string> CreateImageLookup(List<string> imagePaths)
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var path in imagePaths)
            {
                // Add various lookup keys for flexible matching
                var fileName = Path.GetFileName(path);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                var relativePath = Path.GetRelativePath(_imagesFolderPath!, path);
                
                // Exact filename
                lookup[fileName] = path;
                
                // Filename without extension
                if (!lookup.ContainsKey(fileNameWithoutExt))
                    lookup[fileNameWithoutExt] = path;
                
                // Relative path
                lookup[relativePath] = path;
                
                // Relative path with forward slashes
                var relativePathForward = relativePath.Replace('\\', '/');
                lookup[relativePathForward] = path;
            }
            
            return lookup;
        }
        
        private string? MatchImagePath(string jsonlPath, Dictionary<string, string> imageLookup)
        {
            // Direct match
            if (imageLookup.TryGetValue(jsonlPath, out var matched))
                return matched;
            
            // Try just the filename
            var fileName = Path.GetFileName(jsonlPath);
            if (imageLookup.TryGetValue(fileName, out matched))
                return matched;
            
            // Try without extension
            var withoutExt = Path.GetFileNameWithoutExtension(jsonlPath);
            if (imageLookup.TryGetValue(withoutExt, out matched))
                return matched;
            
            // Try normalizing slashes
            var normalized = jsonlPath.Replace('/', Path.DirectorySeparatorChar);
            if (imageLookup.TryGetValue(normalized, out matched))
                return matched;
            
            return null;
        }
        
        private bool ValidateJsonlFields(JsonElement root, EvaluationWorkflow workflow, int lineNumber, TwoPartValidationResult result)
        {
            // Check prompt field
            if (!root.TryGetProperty("prompt", out _))
            {
                result.Errors.Add($"Line {lineNumber}: Missing required field 'prompt'");
                return false;
            }
            
            // Check workflow-specific fields
            if (workflow == EvaluationWorkflow.EvaluateResponses)
            {
                if (!root.TryGetProperty("response", out _))
                {
                    result.Errors.Add($"Line {lineNumber}: Missing required field 'response'");
                    return false;
                }
                // Model field is now optional - will use UI input
            }
            else if (workflow == EvaluationWorkflow.ImportResults)
            {
                if (!root.TryGetProperty("response", out _))
                {
                    result.Errors.Add($"Line {lineNumber}: Missing required field 'response'");
                    return false;
                }
                if (!root.TryGetProperty("criteria_scores", out _))
                {
                    result.Errors.Add($"Line {lineNumber}: Missing required field 'criteria_scores'");
                    return false;
                }
            }
            
            return true;
        }
        
        private DatasetEntry? CreateDatasetEntryFromJson(JsonElement root, string matchedImagePath, string originalImagePath)
        {
            var entry = new DatasetEntry
            {
                OriginalImagePath = originalImagePath,
                ResolvedImagePath = matchedImagePath,
                Prompt = root.GetProperty("prompt").GetString() ?? string.Empty
            };
            
            if (root.TryGetProperty("response", out var responseElement))
            {
                entry.Response = responseElement.GetString();
            }
            
            // Use model from UI input if not in JSONL
            if (root.TryGetProperty("model", out var modelElement))
            {
                entry.Model = modelElement.GetString();
            }
            else if (!string.IsNullOrEmpty(_modelName))
            {
                entry.Model = _modelName;
            }
            
            if (_currentWorkflow == EvaluationWorkflow.ImportResults && root.TryGetProperty("criteria_scores", out var scoresElement))
            {
                entry.Scores = JsonSerializer.Deserialize<Dictionary<string, object>>(scoresElement.GetRawText());
            }
            
            return entry;
        }
        
        private List<string> GetAllImageFiles(string folderPath)
        {
            var imageFiles = new List<string>();
            foreach (var extension in SupportedImageExtensions)
            {
                var files = Directory.GetFiles(folderPath, $"*{extension}", SearchOption.AllDirectories)
                    .Union(Directory.GetFiles(folderPath, $"*{extension.ToUpperInvariant()}", SearchOption.AllDirectories));
                imageFiles.AddRange(files);
            }
            return imageFiles.Distinct().ToList();
        }
        
        private void ShowTwoPartValidationReport(TwoPartValidationResult result)
        {
            TwoPartValidationPanel.Visibility = Visibility.Visible;
            
            if (result.IsValid)
            {
                TwoPartValidationInfoBar.Severity = InfoBarSeverity.Success;
                TwoPartValidationInfoBar.Title = "Validation Successful";
                TwoPartValidationInfoBar.Message = $"Successfully matched {result.MatchedCount} entries";
                
                if (result.Warnings.Any())
                {
                    TwoPartValidationInfoBar.Severity = InfoBarSeverity.Warning;
                    TwoPartValidationInfoBar.Message += $"\n{string.Join("\n", result.Warnings)}";
                }
            }
            else
            {
                TwoPartValidationInfoBar.Severity = InfoBarSeverity.Error;
                TwoPartValidationInfoBar.Title = "Validation Failed";
                TwoPartValidationInfoBar.Message = string.Join("\n", result.Errors.Take(3));
                
                if (result.Errors.Count > 3)
                {
                    TwoPartValidationInfoBar.Message += $"\n... and {result.Errors.Count - 3} more errors";
                }
            }
            
            // Show match summary if there are unmatched items
            if (result.UnmatchedCount > 0)
            {
                MatchSummaryPanel.Visibility = Visibility.Visible;
                MatchSummaryText.Text = $"Matched: {result.MatchedCount} | Unmatched: {result.UnmatchedCount} | Total images in folder: {_imagesCount}";
            }
            
            // Update button visibility
            FixIssuesButton.Visibility = result.UnmatchedCount > 0 ? Visibility.Visible : Visibility.Collapsed;
            ProceedAnywayButton.Visibility = (result.Warnings.Any() && result.Errors.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private async Task CreateFinalDatasetConfiguration(TwoPartValidationResult validationResult)
        {
            // Get model name from UI
            _modelName = ModelNameInput.Text.Trim();
            if (string.IsNullOrEmpty(_modelName))
            {
                _modelName = "Unknown Model";
            }
            
            // Create dataset configuration
            _datasetConfig = new DatasetConfiguration
            {
                Name = Path.GetFileNameWithoutExtension(_jsonlFilePath!),
                SourcePath = _jsonlFilePath!,
                SourceType = DatasetSourceType.JsonlFile,
                BaseDirectory = _imagesFolderPath!,
                Entries = validationResult.Entries,
                ValidEntries = validationResult.MatchedCount,
                TotalEntries = validationResult.MatchedCount,
                ValidationResult = new ValidationResult
                {
                    IsValid = true,
                    Issues = new List<ValidationIssue>(),
                    Warnings = validationResult.Warnings
                }
            };
            
            // Apply model name to all entries that don't have one
            foreach (var entry in _datasetConfig.Entries)
            {
                if (string.IsNullOrEmpty(entry.Model))
                {
                    entry.Model = _modelName;
                }
            }
            
            // Notify parent
            ValidationChanged?.Invoke(IsValid);
            UpdateParentDialogState();
        }
        
        private void ResetImageUpload()
        {
            _imagesFolderPath = null;
            _imagesCount = 0;
            ImageUploadPanel.Visibility = Visibility.Visible;
            ImageUploadSuccess.Visibility = Visibility.Collapsed;
            
            // Also reset JSONL upload
            ResetJsonlUpload();
            
            // Disable JSONL upload
            JsonlDropZone.IsHitTestVisible = false;
            JsonlDropZone.Opacity = 0.5;
            BrowseJsonlButton.IsEnabled = false;
            JsonlUploadText.Text = "Upload images first";
        }
        
        private void ResetJsonlUpload()
        {
            _jsonlFilePath = null;
            JsonlUploadPanel.Visibility = Visibility.Visible;
            JsonlUploadSuccess.Visibility = Visibility.Collapsed;
            TwoPartValidationPanel.Visibility = Visibility.Collapsed;
            
            _datasetConfig = null;
            ValidationChanged?.Invoke(false);
            UpdateParentDialogState();
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
                // Dataset is valid and within size limits
                ValidationInfoBar.Severity = InfoBarSeverity.Success;
                ValidationInfoBar.Title = "Dataset Valid";
                ValidationInfoBar.Message = $"{_datasetConfig.ValidEntries} entries loaded successfully";

                // Show statistics
                StatisticsPanel.Visibility = Visibility.Visible;
                EntryCountText.Text = $"📊 {_datasetConfig.ValidEntries} valid entries";
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
            
            // Reset single upload UI
            UploadPanel.Visibility = Visibility.Visible;
            ValidationResultsPanel.Visibility = Visibility.Collapsed;
            DatasetSizeErrorPanel.Visibility = Visibility.Collapsed;
            RelativePathWarning.IsOpen = false;
            
            // Reset two-part upload state
            ResetImageUpload();
            ResetJsonlUpload();
            ModelNameInput.Text = string.Empty;
            _modelName = null;
            
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

        private void ShowDatasetSizeError()
        {
            // Hide other panels
            UploadPanel.Visibility = Visibility.Collapsed;
            ValidationResultsPanel.Visibility = Visibility.Collapsed;
            
            // Show error panel
            DatasetSizeErrorPanel.Visibility = Visibility.Visible;
            
            // Update the total entries count
            if (_datasetConfig != null)
            {
                TotalEntriesRun.Text = _datasetConfig.TotalEntries.ToString("N0", System.Globalization.CultureInfo.CurrentCulture);
            }
            
            // Ensure dialog buttons are disabled since dataset is invalid
            UpdateParentDialogState();
        }
        
        private void SelectDifferentDataset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
        
        private async void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            // Show detailed validation report
            // TODO: Implement detailed view dialog
        }
        
        private async void FixIssues_Click(object sender, RoutedEventArgs e)
        {
            // Launch quick fix dialog for unmatched images
            // TODO: Implement quick fix functionality
        }
        
        private void ProceedAnyway_Click(object sender, RoutedEventArgs e)
        {
            // User wants to proceed despite warnings
            ValidationChanged?.Invoke(IsValid);
            UpdateParentDialogState();
        }
        
        private void ChangeImages_Click(object sender, RoutedEventArgs e)
        {
            ResetImageUpload();
        }
        
        private void ChangeJsonl_Click(object sender, RoutedEventArgs e)
        {
            ResetJsonlUpload();
        }
        
        private void ModelNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            _modelName = ModelNameInput.Text.Trim();
            
            // If we already have a valid dataset, update the model name in all entries
            if (_datasetConfig != null && !string.IsNullOrEmpty(_modelName))
            {
                foreach (var entry in _datasetConfig.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Model) || entry.Model == "Unknown Model")
                    {
                        entry.Model = _modelName;
                    }
                }
            }
        }
        
    }

    #region Helper Classes

    public class DatasetPreviewItem
    {
        public string ImagePath { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
    }
    
    internal class TwoPartValidationResult
    {
        public bool IsValid { get; set; }
        public int MatchedCount { get; set; }
        public int UnmatchedCount { get; set; }
        public List<DatasetEntry> Entries { get; set; } = new();
        public List<string> UnmatchedPaths { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    #endregion
}