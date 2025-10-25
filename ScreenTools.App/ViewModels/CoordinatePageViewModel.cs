using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        
        // Draw vertical lines
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
        var centralXPoint = Shapes
            .Where(x => x.Name == "vertical-line")
            .Where(x =>
            {
                if (x is not LineViewModel line)
                    return false;
                
                if (line.StartPoint.X >= canvasHalfWidth - gridSpacing && line.StartPoint.X <= canvasHalfWidth)
                {
                    return true;
                }
        
                return line.StartPoint.X >= canvasHalfWidth && line.StartPoint.X <= canvasHalfWidth + gridSpacing;
            })
            .MaxBy(x => Math.Abs(canvasHalfWidth - (x as LineViewModel).StartPoint.X)) as LineViewModel;
        var centralYPoint = Shapes
            .Where(x => x.Name == "horizontal-line")
            .Where(x =>
            {
                if (x is not LineViewModel line)
                    return false;
                
                if (line.StartPoint.Y >= canvasHalfHeight - gridSpacing && line.StartPoint.Y <= canvasHalfHeight)
                {
                    return true;
                }
        
                return line.StartPoint.Y >= canvasHalfHeight && line.StartPoint.Y <= canvasHalfHeight + gridSpacing;
            })
            .MaxBy(x => Math.Abs(canvasHalfHeight - (x as LineViewModel).StartPoint.Y)) as LineViewModel;
        
        var xAxis = new LineViewModel
        {
            StartPoint = new Point(0, centralYPoint.StartPoint.Y),
            EndPoint = new Point(canvasWidth, centralYPoint.StartPoint.Y),
            Stroke = "Gray",
            StrokeThickness = 1.5
        };
        
        var yAxis = new LineViewModel
        {
            StartPoint = new Point(centralXPoint.StartPoint.X, 0),
            EndPoint = new Point(centralXPoint.StartPoint.X, canvasHeight),
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