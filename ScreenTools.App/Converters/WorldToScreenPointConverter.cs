using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ScreenTools.App;

public class WorldToScreenPointConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Count == 3 && 
            values[0] is Point worldPoint && 
            values[1] is double scale && 
            values[2] is Point offset)
        {
            var screenX = (worldPoint.X * scale) + offset.X;
            var screenY = (worldPoint.Y * scale) + offset.Y;
            
            return new Point(screenX, screenY);
        }
        return new Point(0, 0);
    }
}