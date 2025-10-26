using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ScreenTools.Core;

namespace ScreenTools.App;

public partial class CoordinatePlanePageViewModel : PageViewModel
{
    [ObservableProperty] 
    private ObservableCollection<ShapeViewModelBase> _shapes;
    
    public CoordinatePlanePageViewModel()
    {
        Shapes = [];
    }

    public void DrawGrid(double canvasWidth, double canvasHeight)
    {
        Shapes.Clear();

        const int gridSpacing = 25; 
        
        // Draw vertial numbers
        for (double x = 0; x < canvasWidth; x += gridSpacing)
        {
            var num = new TextBlockViewModel
            {
                Name = "vertical-line",
                X = x,
                Y = canvasWidth / 2,
                Foreground= Colors.Black.ToString(),
                Text = x.ToString(CultureInfo.InvariantCulture),
                FontSize = 12
            };
        
            Shapes.Add(num);
        }
        
        for (double x = 0; x < canvasWidth; x += gridSpacing)
        {
            var line = new LineViewModel
            {
                Name = "vertical-line",
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, canvasHeight),
                Stroke = Colors.LightGray.ToString(),
                StrokeThickness = 0.5
            };
        
            Shapes.Add(line);
        }
        
        // Draw horizontal lines
        for (double y = 0; y < canvasHeight; y += gridSpacing)
        {
            var line = new LineViewModel
            {
                Name = "horizontal-line",
                StartPoint = new Point(0, y),
                EndPoint = new Point(canvasWidth, y),
                Stroke = Colors.LightGray.ToString(),
                StrokeThickness = 0.5
            };
            Shapes.Add(line);
        }
        
        var canvasHalfWidth = canvasWidth / 2;
        var canvasHalfHeight = canvasHeight / 2;
        var centralX = Math.Round(canvasHalfWidth / gridSpacing) * gridSpacing;
        var centralY = Math.Round(canvasHalfHeight / gridSpacing) * gridSpacing;
        
        var xAxis = new LineViewModel
        {
            StartPoint = new Point(0, centralY),
            EndPoint = new Point(canvasWidth, centralY),
            Stroke = "Gray",
            StrokeThickness = 1.5
        };
        
        var yAxis = new LineViewModel
        {
            StartPoint = new Point(centralX, 0),
            EndPoint = new Point(centralX, canvasHeight),
            Stroke = "Gray",
            StrokeThickness = 1.5
        };
        
        Shapes.Add(xAxis);
        Shapes.Add(yAxis);
    }
    
    public void OnPointerPressed(double x, double y)
    {
        const double width = 5;
        const double height = 5;
        
        var newItem = new EllipseViewModel
        {
            X = x - width / 2,
            Y = y- height / 2,
            Width = width,
            Height = height,
            Fill = "#000000"
        };
        
        Shapes.Add(newItem);
    }
}