using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ScreenTools.App;

public class WorldToScreenXConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 3 &&
                              values[0] is double worldX && 
                              values[1] is double scale && 
                              values[2] is Point offset)
        {
            return worldX * scale + offset.X;
        }







        return 0.0;
    }
}