using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SystemTools.App
{
    public class DrawingStateToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || parameter is null)
            {
                return new SolidColorBrush(Color.Parse("#DEDEDE"));
            }

            var drawingState = (DrawingState)value;
            var drawingStateParameter = (DrawingState)parameter;

            return drawingState == drawingStateParameter ?
                new SolidColorBrush(Color.Parse("#b3d9ff")) :
                new SolidColorBrush(Color.Parse("#DEDEDE"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
