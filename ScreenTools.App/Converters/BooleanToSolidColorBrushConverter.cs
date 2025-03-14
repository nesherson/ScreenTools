﻿using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ScreenTools.App;

public class BooleanToSolidColorBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return new SolidColorBrush(Color.Parse("#DEDEDE"));
        }
        
        return (bool)value ?
            new SolidColorBrush(Color.Parse("#b3d9ff")) :
            new SolidColorBrush(Color.Parse("#DEDEDE"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}