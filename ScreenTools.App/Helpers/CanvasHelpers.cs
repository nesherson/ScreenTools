using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Microsoft.Extensions.Logging;

namespace ScreenTools.App;

public static class CanvasHelpers
{
    public static bool IsInArea(Control control, Rectangle eraseArea)
    {
        switch (control)
        {
            case Polyline polyline:
                return polyline.Points
                    .Any(p => p.X >= eraseArea.Bounds.TopLeft.X &&
                              p.X <= eraseArea.Bounds.TopRight.X &&
                              p.X >= eraseArea.Bounds.BottomLeft.X &&
                              p.X <= eraseArea.Bounds.BottomRight.X &&
                              p.Y >= eraseArea.Bounds.TopLeft.Y &&
                              p.Y <= eraseArea.Bounds.BottomLeft.Y &&
                              p.Y >= eraseArea.Bounds.TopRight.Y &&
                              p.Y <= eraseArea.Bounds.BottomRight.Y);
            case TextBlock textBlock:
                return textBlock.Bounds.X >= eraseArea.Bounds.TopLeft.X &&
                       textBlock.Bounds.X <= eraseArea.Bounds.TopRight.X &&
                       textBlock.Bounds.X >= eraseArea.Bounds.BottomLeft.X &&
                       textBlock.Bounds.X <= eraseArea.Bounds.BottomRight.X &&
                       textBlock.Bounds.Y >= eraseArea.Bounds.TopLeft.Y &&
                       textBlock.Bounds.Y <= eraseArea.Bounds.BottomLeft.Y &&
                       textBlock.Bounds.Y >= eraseArea.Bounds.TopRight.Y &&
                       textBlock.Bounds.Y <= eraseArea.Bounds.BottomRight.Y;
            case Line line:
                return (line.StartPoint.X >= eraseArea.Bounds.TopLeft.X &&
                        line.StartPoint.X <= eraseArea.Bounds.TopRight.X &&
                        line.StartPoint.X >= eraseArea.Bounds.BottomLeft.X &&
                        line.StartPoint.X <= eraseArea.Bounds.BottomRight.X &&
                        line.StartPoint.Y >= eraseArea.Bounds.TopLeft.Y &&
                        line.StartPoint.Y <= eraseArea.Bounds.BottomLeft.Y &&
                        line.StartPoint.Y >= eraseArea.Bounds.TopRight.Y &&
                        line.StartPoint.Y <= eraseArea.Bounds.BottomRight.Y) ||
                       (line.EndPoint.X >= eraseArea.Bounds.TopLeft.X &&
                        line.EndPoint.X <= eraseArea.Bounds.TopRight.X &&
                        line.EndPoint.X >= eraseArea.Bounds.BottomLeft.X &&
                        line.EndPoint.X <= eraseArea.Bounds.BottomRight.X &&
                        line.EndPoint.Y >= eraseArea.Bounds.TopLeft.Y &&
                        line.EndPoint.Y <= eraseArea.Bounds.BottomLeft.Y &&
                        line.EndPoint.Y >= eraseArea.Bounds.TopRight.Y &&
                        line.EndPoint.Y <= eraseArea.Bounds.BottomRight.Y);
            case Rectangle rectangle:
                return rectangle.Bounds.X >= eraseArea.Bounds.TopLeft.X &&
                              rectangle.Bounds.X <= eraseArea.Bounds.TopRight.X &&
                              rectangle.Bounds.X >= eraseArea.Bounds.BottomLeft.X &&
                              rectangle.Bounds.X <= eraseArea.Bounds.BottomRight.X &&
                              rectangle.Bounds.Y >= eraseArea.Bounds.TopLeft.Y &&
                              rectangle.Bounds.Y <= eraseArea.Bounds.BottomLeft.Y &&
                              rectangle.Bounds.Y >= eraseArea.Bounds.TopRight.Y &&
                              rectangle.Bounds.Y <= eraseArea.Bounds.BottomRight.Y;
            case Ellipse ellipse:
                return ellipse.Bounds.X >= eraseArea.Bounds.TopLeft.X &&
                       ellipse.Bounds.X <= eraseArea.Bounds.TopRight.X &&
                       ellipse.Bounds.X >= eraseArea.Bounds.BottomLeft.X &&
                       ellipse.Bounds.X <= eraseArea.Bounds.BottomRight.X &&
                       ellipse.Bounds.Y >= eraseArea.Bounds.TopLeft.Y &&
                       ellipse.Bounds.Y <= eraseArea.Bounds.BottomLeft.Y &&
                       ellipse.Bounds.Y >= eraseArea.Bounds.TopRight.Y &&
                       ellipse.Bounds.Y <= eraseArea.Bounds.BottomRight.Y;
        }

        return false;
    }
    
    public static void SaveCanvasToFile(Canvas canvas, string fileName)
    {
        var canvasItems = canvas.Children.ToList();
        var itemsToSave = new List<SavedShape>();

        foreach (var item in canvasItems)
        {
            var shapeToSave = new SavedShape();
            
            switch (item)
            {
                case Polyline polyline:
                {
                    shapeToSave.ShapeName = "polyline";
                    shapeToSave.StrokeColor = polyline.Stroke?.ToString();
                    shapeToSave.StrokeWidth = polyline.StrokeThickness;
                    shapeToSave.Points = polyline.Points
                        .Select(p => new SavedPoint(p.X, p.Y))
                        .ToArray();
                    break;
                }
                case Rectangle rectangle:
                    shapeToSave.ShapeName = "rectangle";
                    shapeToSave.FillColor = rectangle.Fill?.ToString();
                    shapeToSave.StartPoint = new SavedPoint(rectangle.Bounds.X, rectangle.Bounds.Y);
                    shapeToSave.Width = rectangle.Width;
                    shapeToSave.Height = rectangle.Height;
                    break;
                case Line line:
                    shapeToSave.ShapeName = "line";
                    shapeToSave.StrokeColor = line.Stroke?.ToString();
                    shapeToSave.StrokeWidth = line.StrokeThickness;
                    shapeToSave.StartPoint = new SavedPoint(line.StartPoint.X, line.StartPoint.Y);
                    shapeToSave.EndPoint = new SavedPoint(line.EndPoint.X, line.EndPoint.Y);
                    break;
                case Ellipse ellipse:
                    shapeToSave.ShapeName = "ellipse";
                    shapeToSave.FillColor = ellipse.Fill?.ToString();
                    shapeToSave.StartPoint = new SavedPoint(ellipse.Bounds.X, ellipse.Bounds.Y);
                    shapeToSave.Width = ellipse.Bounds.Width;
                    shapeToSave.Height = ellipse.Bounds.Height;
                    break;
            }
            
            itemsToSave.Add(shapeToSave);
        }
        
        var serializedShape = JsonSerializer.Serialize(itemsToSave);
        using var streamWriter = new StreamWriter(fileName);
            
        streamWriter.WriteLine(serializedShape);
    }

    public static void LoadCanvasFromFile(Canvas canvas, string fileName, ILogger logger)
    {
        if (!File.Exists(fileName))
        {
            File.Create(fileName);
        }

        var readLine = "";

        try
        {
            using var sr = new StreamReader(fileName);
            readLine = sr.ReadLine();
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to read line from file '{fileName}'. Exception: {ex}");
        }

        if (string.IsNullOrEmpty(readLine))
            return;
                    
        var savedItems = JsonSerializer.Deserialize<SavedShape[]>(readLine);
        
        if (savedItems == null)
            return;
        
        foreach (var savedItem in savedItems)
        {
            switch (savedItem.ShapeName)
            {
                case "polyline":
                {
                    var polyline = new Polyline
                    {
                        StrokeThickness = savedItem.StrokeWidth.GetValueOrDefault(5),
                        Stroke = savedItem.StrokeColor != null ? SolidColorBrush.Parse(savedItem.StrokeColor) : null,
                        Points = savedItem.Points.Select(p => new Point(p.X, p.Y)).ToArray()
                    };
                    
                    canvas.Children.Add(polyline);
                    break;
                }
                case "rectangle":
                    var rectangle = new Rectangle
                    {
                        Fill = savedItem.FillColor != null ? SolidColorBrush.Parse(savedItem.FillColor) : null,
                        Width = savedItem.Width.GetValueOrDefault(100),
                        Height = savedItem.Height.GetValueOrDefault(100),
                    };
                    
                    canvas.AddToPosition(rectangle, savedItem.StartPoint.X, savedItem.StartPoint.Y);
                    
                    break;
                case "line":
                    var line = new Line
                    {
                        StrokeThickness = savedItem.StrokeWidth.GetValueOrDefault(5),
                        Stroke = savedItem.StrokeColor != null ? SolidColorBrush.Parse(savedItem.StrokeColor) : null,
                        StartPoint = new Point(savedItem.StartPoint.X, savedItem.StartPoint.Y),
                        EndPoint = new Point(savedItem.EndPoint.X, savedItem.EndPoint.Y)
                    };
                    
                    canvas.Children.Add(line);

                    break;
                case "ellipse":
                    var ellipse = new Ellipse
                    {
                        Fill = savedItem.FillColor != null ? SolidColorBrush.Parse(savedItem.FillColor) : null,
                        Width = savedItem.Width.GetValueOrDefault(100),
                        Height = savedItem.Height.GetValueOrDefault(100)
                    };
                    
                    canvas.AddToPosition(ellipse, savedItem.StartPoint.X, savedItem.StartPoint.Y);
                    
                    break;
            }
        }
    }

    public static void DeleteSavedCanvas(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    public static void CopyControlToPosition(Canvas canvas, 
        Control controlToCopy, 
        Point newPos, 
        Point selectedAreaPos)
    {
       canvas.AddDebugDot(newPos);
        
        switch (controlToCopy)
        {
            case Polyline polyline:
            {
                var newPolyline = new Polyline
                {
                    Stroke = polyline.Stroke,
                    StrokeThickness = polyline.StrokeThickness,
                    StrokeJoin = polyline.StrokeJoin,
                    StrokeLineCap = polyline.StrokeLineCap
                };
                
                var firstPoint = new Point(
                    newPos.X + polyline.Points.First().X - selectedAreaPos.X,
                    newPos.Y + polyline.Points.First().Y - selectedAreaPos.Y);
                var lastPosition = new Point(firstPoint.X, firstPoint.Y);
                
                newPolyline.Points.Add(firstPoint);
                
                for (var i = 0; i < polyline.Points.Count - 1; i++)
                {
                    var diffX = polyline.Points[i + 1].X - polyline.Points[i].X;
                    var diffY = polyline.Points[i + 1].Y - polyline.Points[i].Y;
                    var tempPos = new Point(lastPosition.X + diffX, lastPosition.Y + diffY);
                    
                    newPolyline.Points.Add(tempPos);
                    lastPosition = new Point(tempPos.X, tempPos.Y);
                }
                
                canvas.Children.Add(newPolyline);
                
                break;
            }
            case Rectangle rectangle:
            {
                var newRectangle = new Rectangle
                {
                    Fill = rectangle.Fill,
                    Width = rectangle.Width,
                    Height = rectangle.Height
                };
                
                var pos = new Point(newPos.X + rectangle.Bounds.X - selectedAreaPos.X,
                    newPos.Y + rectangle.Bounds.Y - selectedAreaPos.Y);
                
                canvas.SetPosition(newRectangle, pos);
                canvas.Children.Add(newRectangle);
                
                break;
            }
            case Ellipse ellipse:
            {
                var newEllipse = new Ellipse
                {
                    Fill = ellipse.Fill,
                    Width = ellipse.Width,
                    Height = ellipse.Height
                };
                
                var pos = new Point(newPos.X + ellipse.Bounds.X - selectedAreaPos.X,
                    newPos.Y + ellipse.Bounds.Y - selectedAreaPos.Y);
                
                canvas.SetPosition(newEllipse, pos);
                canvas.Children.Add(newEllipse);
                
                break;
            }
            case Line line:
            {
                var newLine = new Line
                {
                    Stroke = line.Stroke,
                    StrokeThickness = line.StrokeThickness,
                    StartPoint = new Point(newPos.X + line.StartPoint.X - selectedAreaPos.X,
                        newPos.Y + line.StartPoint.Y - selectedAreaPos.Y),
                    EndPoint = new Point(newPos.X + line.EndPoint.X - selectedAreaPos.X,
                        newPos.Y + line.EndPoint.Y - selectedAreaPos.Y)
                };

                canvas.Children.Add(newLine);
                break;
            }
            case TextBlock textBlock:
            {
                var newTextBlock = new TextBlock
                {
                    Text = textBlock.Text,
                    FontSize = textBlock.FontSize,
                    Foreground = textBlock.Foreground,
                    Background = textBlock.Background
                };
                
                var pos = new Point(newPos.X + textBlock.Bounds.X - selectedAreaPos.X,
                    newPos.Y + textBlock.Bounds.Y - selectedAreaPos.Y);
                
                canvas.SetPosition(newTextBlock, pos);
                canvas.Children.Add(newTextBlock);
                
                break;
            }
        }
    }
}