// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AIDevGallery.Interfaces
{
    /// <summary>
    /// Interface for wizard pages that support validation
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Gets whether the current state is valid
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// Gets the validation error message if not valid
        /// </summary>
        string ValidationError { get; }
        
        /// <summary>
        /// Event raised when validation state changes
        /// </summary>
        event EventHandler ValidationChanged;
        
        /// <summary>
        /// Forces validation to run
        /// </summary>
        bool Validate();
    }
    
    /// <summary>
    /// Interface for pages that can provide progress updates
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Reports progress with a message
        /// </summary>
        void ReportProgress(int percentage, string message);
        
        /// <summary>
        /// Reports indeterminate progress
        /// </summary>
        void ReportIndeterminateProgress(string message);
        
        /// <summary>
        /// Clears progress indication
        /// </summary>
        void ClearProgress();
    }
}