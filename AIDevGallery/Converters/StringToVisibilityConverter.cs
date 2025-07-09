// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace AIDevGallery.Converters;

/// <summary>
/// Converts a string to Visibility - empty/null strings are collapsed
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a string value to Visibility - empty/null strings become Collapsed, others become Visible
    /// </summary>
    /// <param name="value">The string value to convert</param>
    /// <param name="targetType">The target type (ignored)</param>
    /// <param name="parameter">Optional parameter (ignored)</param>
    /// <param name="language">Language context (ignored)</param>
    /// <returns>Visibility.Visible for non-empty strings, Visibility.Collapsed otherwise</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Converts back from Visibility to string (not implemented)
    /// </summary>
    /// <param name="value">The visibility value</param>
    /// <param name="targetType">The target type</param>
    /// <param name="parameter">Optional parameter</param>
    /// <param name="language">Language context</param>
    /// <returns>Not implemented</returns>
    /// <exception cref="NotImplementedException">This method is not implemented</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}