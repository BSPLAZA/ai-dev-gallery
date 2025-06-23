// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace AIDevGallery.Converters;

/// <summary>
/// Converts boolean values to Visibility
/// </summary>
internal class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            // If parameter is "Inverse", invert the logic
            var inverse = parameter as string == "Inverse";
            return (boolValue ^ inverse) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            var inverse = parameter as string == "Inverse";
            return (visibility == Visibility.Visible) ^ inverse;
        }
        
        return false;
    }
}