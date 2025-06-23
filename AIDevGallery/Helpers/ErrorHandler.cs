// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace AIDevGallery.Helpers
{
    /// <summary>
    /// Centralized error handling for the evaluation feature
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// Handles an error by logging it and showing a user-friendly dialog
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <param name="xamlRoot">The XAML root for showing dialogs</param>
        /// <param name="userMessage">Optional custom message for the user</param>
        /// <param name="title">Optional dialog title</param>
        /// <returns>True if user chose to retry, false otherwise</returns>
        public static async Task<bool> HandleErrorAsync(
            Exception ex, 
            XamlRoot xamlRoot, 
            string userMessage = null,
            string title = null)
        {
            // Log the full exception details
            LogError(ex);
            
            // Prepare user-friendly message
            var message = userMessage ?? GetUserFriendlyMessage(ex);
            var dialogTitle = title ?? "Something went wrong";
            
            // Show dialog
            var dialog = new ContentDialog
            {
                Title = dialogTitle,
                Content = message,
                PrimaryButtonText = "Retry",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };
            
            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
        
        /// <summary>
        /// Shows a simple error message without retry option
        /// </summary>
        public static async Task ShowErrorAsync(
            XamlRoot xamlRoot,
            string message,
            string title = "Error")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };
            
            await dialog.ShowAsync();
        }
        
        /// <summary>
        /// Gets a user-friendly error message based on exception type
        /// </summary>
        private static string GetUserFriendlyMessage(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => "You don't have permission to access this file or folder.",
                System.IO.DirectoryNotFoundException => "The specified folder could not be found.",
                System.IO.FileNotFoundException => "The specified file could not be found.",
                System.IO.IOException => "There was a problem accessing the file. It may be in use by another program.",
                System.Text.Json.JsonException => "The file format is invalid. Please check that it's a valid JSONL file.",
                OutOfMemoryException => "The file is too large to process. Try using a smaller dataset.",
                TaskCanceledException => "The operation was cancelled.",
                _ => "An unexpected error occurred. Please try again."
            };
        }
        
        /// <summary>
        /// Logs error details for debugging
        /// </summary>
        private static void LogError(Exception ex)
        {
            // In production, this would send to telemetry
            System.Diagnostics.Debug.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}