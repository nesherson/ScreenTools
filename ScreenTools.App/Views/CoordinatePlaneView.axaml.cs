using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ScreenTools.App;

public partial class CoordinatePlaneView : Window
{
    private Point? _firstLinePoint;
    
    public CoordinatePlaneView()
    {
        InitializeComponent();
        
        Loaded += (_, _) => DrawGrid(); 
    }
    
    private void DrawGrid()
    {
        // Clear any previous drawings
        CoordinateCanvas.Children.Clear();
        
        var width = CoordinateCanvas.Bounds.Width;
        var height = CoordinateCanvas.Bounds.Height;
        var gridSpacing = 25; // Draw a line every 25 pixels

        // Draw vertical lines
        for (double x = 0; x < width; x += gridSpacing)
        {
            var line = new Line
            {
                Name = "vertical-line",
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, height),
                Stroke = Brushes.LightGray,
                StrokeThickness = 0.5
            };
            CoordinateCanvas.Children.Add(line);
        }

        // Draw horizontal lines
        for (double y = 0; y < height; y += gridSpacing)
        {
            var line = new Line
            {
                Name = "horizontal-line",
                StartPoint = new Point(0, y),
                EndPoint = new Point(width, y),
                Stroke = Brushes.LightGray,
                StrokeThickness = 0.5
            };
            CoordinateCanvas.Children.Add(line);
        }
        
        var canvasHalfWidth = width / 2;
        var canvasHalfHeight = height / 2;
        var centralXPoint = CoordinateCanvas.Children
            .Where(x => x.Name == "vertical-line")
            .Where(x =>
            {
                if (x is not Line line)
                    return false;
                
                if (line.StartPoint.X >= canvasHalfWidth - gridSpacing && line.StartPoint.X <= canvasHalfWidth)
                {
                    return true;
                }

                return line.StartPoint.X >= canvasHalfWidth && line.StartPoint.X <= canvasHalfWidth + gridSpacing;
            })
            .MaxBy(x => Math.Abs(canvasHalfWidth - (x as Line).StartPoint.X)) as Line;
        var centralYPoint = CoordinateCanvas.Children
            .Where(x => x.Name == "horizontal-line")
            .Where(x =>
            {
                if (x is not Line line)
                    return false;
                
                if (line.StartPoint.Y >= canvasHalfHeight - gridSpacing && line.StartPoint.Y <= canvasHalfHeight)
                {
                    return true;
                }

                return line.StartPoint.Y >= canvasHalfHeight && line.StartPoint.Y <= canvasHalfHeight + gridSpacing;
            })
            .MaxBy(x => Math.Abs(canvasHalfHeight - (x as Line).StartPoint.Y)) as Line;
        
        var xAxis = new Line
        {
            StartPoint = new Point(0, centralYPoint.StartPoint.Y),
            EndPoint = new Point(width, centralYPoint.StartPoint.Y),
            Stroke = Brushes.Gray,
            StrokeThickness = 1.5
        };
        
        var yAxis = new Line
        {
            StartPoint = new Point(centralXPoint.StartPoint.X, 0),
            EndPoint = new Point(centralXPoint.StartPoint.X, height),
            Stroke = Brushes.Gray,
            StrokeThickness = 1.5
        };
        
        CoordinateCanvas.Children.Add(xAxis);
        CoordinateCanvas.Children.Add(yAxis);
    }
    
    private void CoordinateCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Get the click position relative to the canvas
        var currentPoint = e.GetPosition(CoordinateCanvas);

        // Add a dot at the click position
        AddDot(currentPoint);

        // Logic for drawing a line between two points
        if (_firstLinePoint == null)
        {
            // This is the first click, so store this point
            _firstLinePoint = currentPoint;
        }
        else
        {
            // This is the second click, draw a line and reset
            AddLine(_firstLinePoint.Value, currentPoint);
            _firstLinePoint = null; // Reset for the next line
        }
    }
    
    private void AddDot(Point position)
    {
        var dot = new Ellipse
        {
            Width = 8,
            Height = 8,
            Fill = Brushes.DodgerBlue
        };
        
        // Center the dot on the cursor's position
        Canvas.SetLeft(dot, position.X - dot.Width / 2);
        Canvas.SetTop(dot, position.Y - dot.Height / 2);
        
        CoordinateCanvas.Children.Add(dot);
    }

    private void AddLine(Point start, Point end)
    {
        var line = new Line
        {
            StartPoint = start,
            EndPoint = end,
            Stroke = Brushes.Crimson,
            StrokeThickness = 2
        };
        
        CoordinateCanvas.Children.Add(line);
    }
}