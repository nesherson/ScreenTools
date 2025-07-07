using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Point = Avalonia.Point;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

namespace ScreenTools.App;

public static class CanvasExtensions
{
    public static void AddToPosition(this Canvas canvas, Control control, Point point)
    {
        canvas.Children.Add(control);
        Canvas.SetLeft(control, point.X);
        Canvas.SetTop(control, point.Y);
    }
    
    public static void AddToPosition(this Canvas canvas, Control control, double x, double y)
    {
        AddToPosition(canvas, control, new Point(x, y));
    }

    public static void SetPositionAndSize(this Canvas _, Control control, Point currentPoint, Point startPoint)
    {
        var x = Math.Min(currentPoint.X, startPoint.X);
        var y = Math.Min(currentPoint.Y, startPoint.Y);

        var w = Math.Max(currentPoint.X, startPoint.X) - x;
        var h = Math.Max(currentPoint.Y, startPoint.Y) - y;

        control.Width = w;
        control.Height = h;

        Canvas.SetLeft(control, x);
        Canvas.SetTop(control, y);
    }

    public static void SetPosition(this Canvas _, Control control, Point point)
    {
        Canvas.SetLeft(control, point.X);
        Canvas.SetTop(control, point.Y);
    }
    
    public static void SetPosition(this Canvas canvas, Control control, double x, double y)
    {
        SetPosition(canvas, control, new Point(x, y));
    }

    public static void ClearAll(this Canvas canvas, DrawingHistoryService? drawingHistoryService = null)
    {
        var controlsToSave = canvas.Children
            .Where(x => x is Shape or TextBlock)
            .ToList();

        if (drawingHistoryService != null && controlsToSave.Count != 0)
        {
            drawingHistoryService.Save(controlsToSave, DrawingAction.Clear);
        }

        canvas.Children.Clear();
    }

    public static void AddDebugDot(this Canvas canvas, Point position)
    {
        var debugDot = new Rectangle
        {
            Fill = Brushes.Red,
            Width = 3,
            Height = 3
        };
        
        canvas.SetPosition(debugDot, position);
        canvas.Children.Remove(debugDot);
        canvas.Children.Add(debugDot);
    }
}