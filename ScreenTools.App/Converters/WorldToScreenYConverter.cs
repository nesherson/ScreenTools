using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ScreenTools.App;

public class WorldToScreenYConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 3 &&
            values[0] is double worldY && 
            values[1] is double scale && 
            values[2] is Point offset)
        {
            return worldY * scale + offset.Y;
        }

        return 0.0;
    }
}