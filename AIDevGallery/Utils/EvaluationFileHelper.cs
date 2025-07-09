// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace AIDevGallery.Utils;

/// <summary>
/// Helper class for evaluation file operations following established security patterns
/// </summary>
internal static class EvaluationFileHelper
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB limit

    /// <summary>
    /// Opens a file picker for dataset selection with proper validation
    /// Following UserAddedModelUtil.cs security patterns
    /// </summary>
    public static async Task<string?> PickDatasetFileAsync(XamlRoot xamlRoot)
    {
        try
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            var picker = new FileOpenPicker();
            
            // Common dataset formats
            picker.FileTypeFilter.Add(".csv");
            picker.FileTypeFilter.Add(".json");
            picker.FileTypeFilter.Add(".jsonl");
            picker.FileTypeFilter.Add(".txt");
            
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                return await ValidateAndGetFilePath(file.Path, xamlRoot);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog(xamlRoot, "File Selection Error", 
                $"Could not open file picker: {ex.Message}");
        }
        
        return null;
    }

    /// <summary>
    /// Validates a file path and shows appropriate error dialogs
    /// </summary>
    private static async Task<string?> ValidateAndGetFilePath(string filePath, XamlRoot xamlRoot)
    {
        try
        {
            // Check if file exists and is accessible
            if (!File.Exists(filePath))
            {
                await ShowErrorDialog(xamlRoot, "File Not Found", 
                    "The selected file could not be found or accessed.");
                return null;
            }
            
            // Check file size to prevent memory issues
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                await ShowErrorDialog(xamlRoot, "File Too Large", 
                    $"Please select a file smaller than {MaxFileSizeBytes / (1024 * 1024)}MB.");
                return null;
            }
            
            // Basic format validation
            if (!IsValidDatasetFormat(filePath))
            {
                await ShowErrorDialog(xamlRoot, "Unsupported Format", 
                    "Please select a CSV, JSON, JSONL, or TXT file.");
                return null;
            }
            
            return filePath;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog(xamlRoot, "Validation Error", 
                $"Could not validate file: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validates dataset file format based on extension
    /// </summary>
    private static bool IsValidDatasetFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension is ".csv" or ".json" or ".jsonl" or ".txt";
    }

    /// <summary>
    /// Shows error dialog following existing app patterns
    /// </summary>
    private static async Task ShowErrorDialog(XamlRoot xamlRoot, string title, string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "OK"
        };
        
        await dialog.ShowAsync();
    }

    /// <summary>
    /// Gets a relative path for storage if possible, absolute path as fallback
    /// This helps with portability while maintaining functionality
    /// </summary>
    public static string GetStoragePath(string originalPath)
    {
        try
        {
            var appDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            if (originalPath.StartsWith(appDataFolder, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetRelativePath(appDataFolder, originalPath);
            }
        }
        catch
        {
            // Fall back to absolute path if relative path calculation fails
        }
        
        return originalPath;
    }

    /// <summary>
    /// Resolves a stored path back to a full path
    /// </summary>
    public static string ResolvePath(string storedPath)
    {
        if (Path.IsPathRooted(storedPath))
        {
            return storedPath;
        }
        
        var appDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        return Path.Combine(appDataFolder, storedPath);
    }

    /// <summary>
    /// Validates that a stored file path still exists and is accessible
    /// </summary>
    public static bool ValidateStoredPath(string storedPath)
    {
        try
        {
            var resolvedPath = ResolvePath(storedPath);
            return File.Exists(resolvedPath);
        }
        catch
        {
            return false;
        }
    }
}