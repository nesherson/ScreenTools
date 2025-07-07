using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Core;

namespace ScreenTools.App;

public static class CanvasHelpers
{
    private const double POSITION_OFFSET = 10;
    public static bool IsInArea(ShapeViewModelBase shape, RectangleViewModel eraseArea)
    {
        switch (shape)
        {
            case PolylineViewModel polylineViewModel:
                return CheckOverlap(polylineViewModel, eraseArea);
            // case TextBlockViewModel textBlockViewModel:
            //     return CheckOverlap(textBlockViewModel, eraseArea);
            // case Line line:
            //     return (line.StartPoint.X >= eraseArea.Bounds.TopLeft.X &&
            //             line.StartPoint.X <= eraseArea.Bounds.TopRight.X &&
            //             line.StartPoint.X >= eraseArea.Bounds.BottomLeft.X &&
            //             line.StartPoint.X <= eraseArea.Bounds.BottomRight.X &&
            //             line.StartPoint.Y >= eraseArea.Bounds.TopLeft.Y &&
            //             line.StartPoint.Y <= eraseArea.Bounds.BottomLeft.Y &&
            //             line.StartPoint.Y >= eraseArea.Bounds.TopRight.Y &&
            //             line.StartPoint.Y <= eraseArea.Bounds.BottomRight.Y) ||
            //            (line.EndPoint.X >= eraseArea.Bounds.TopLeft.X &&
            //             line.EndPoint.X <= eraseArea.Bounds.TopRight.X &&
            //             line.EndPoint.X >= eraseArea.Bounds.BottomLeft.X &&
            //             line.EndPoint.X <= eraseArea.Bounds.BottomRight.X &&
            //             line.EndPoint.Y >= eraseArea.Bounds.TopLeft.Y &&
            //             line.EndPoint.Y <= eraseArea.Bounds.BottomLeft.Y &&
            //             line.EndPoint.Y >= eraseArea.Bounds.TopRight.Y &&
            //             line.EndPoint.Y <= eraseArea.Bounds.BottomRight.Y);
            case RectangleViewModel rectangleViewModel:
                return CheckOverlap(rectangleViewModel, eraseArea);
            // case Ellipse ellipse:
            //     return ellipse.Bounds.X >= eraseArea.Bounds.TopLeft.X &&
            //            ellipse.Bounds.X <= eraseArea.Bounds.TopRight.X &&
            //            ellipse.Bounds.X >= eraseArea.Bounds.BottomLeft.X &&
            //            ellipse.Bounds.X <= eraseArea.Bounds.BottomRight.X &&
            //            ellipse.Bounds.Y >= eraseArea.Bounds.TopLeft.Y &&
            //            ellipse.Bounds.Y <= eraseArea.Bounds.BottomLeft.Y &&
            //            ellipse.Bounds.Y >= eraseArea.Bounds.TopRight.Y &&
            //            ellipse.Bounds.Y <= eraseArea.Bounds.BottomRight.Y;
        }

        return false;
    }
    
    private static bool CheckOverlap(RectangleViewModel rectA, RectangleViewModel rectB)
    {
        var rectARight = rectA.X + rectA.Width;
        var rectABottom = rectA.Y + rectA.Height;
        var rectBRight = rectB.X + rectB.Width;
        var rectBBottom = rectB.Y + rectB.Height;
        
        if (rectARight < rectB.X || 
            rectA.X > rectBRight || 
            rectABottom < rectB.Y ||  
            rectA.Y > rectBBottom)   
        {
            return false; 
        }
        
        return true;
    }
    
    private static bool CheckOverlap(PolylineViewModel polyline, RectangleViewModel rectangle)
    {
            return polyline.Points
                .Any(p => p.X >= rectangle.X && p.X <= rectangle.X + rectangle.Width &&
                          p.Y >= rectangle.Y && p.Y <= rectangle.Y + rectangle.Height);
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
                
                var newPolylinePoints = CalculateNewPolylinePoints(polyline, selectedAreaPos, newPos);
                var outOfCanvasWidthPoints = newPolylinePoints
                    .Where(p => p.X > canvas.Bounds.Width)
                    .ToList();

                if (outOfCanvasWidthPoints.Count > 0)
                {
                    var furthestPoint = outOfCanvasWidthPoints.MaxBy(p => p.X);
                    var outOfCanvasDistance = new Point(furthestPoint.X - canvas.Bounds.Width, furthestPoint.Y);
                    
                    newPos = new Point(newPos.X - outOfCanvasDistance.X - POSITION_OFFSET, newPos.Y);
                    newPolylinePoints = CalculateNewPolylinePoints(polyline, selectedAreaPos, newPos);
                }
                
                var outOfCanvasHeightPoints = newPolylinePoints
                    .Where(p => p.Y > canvas.Bounds.Height)
                    .ToList();

                if (outOfCanvasHeightPoints.Count > 0)
                {
                    var furthestPoint = outOfCanvasHeightPoints.MaxBy(p => p.Y);
                    var outOfCanvasDistance = new Point(furthestPoint.X, furthestPoint.Y - canvas.Bounds.Height);
                    
                    newPos = new Point(newPos.X, newPos.Y - outOfCanvasDistance.Y - POSITION_OFFSET);
                    newPolylinePoints = CalculateNewPolylinePoints(polyline, selectedAreaPos, newPos);
                }
                
                newPolyline.Points = newPolylinePoints;
                
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

                if (pos.X + newRectangle.Width > canvas.Bounds.Width)
                {
                    pos = new Point(pos.X - (pos.X + newRectangle.Width - canvas.Bounds.Width) - POSITION_OFFSET, pos.Y);
                }
                
                if (pos.Y + newRectangle.Height > canvas.Bounds.Height)
                {
                    pos = new Point(pos.X, pos.Y - (pos.Y + newRectangle.Height - canvas.Bounds.Height) - POSITION_OFFSET);
                }
                
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

                if (pos.X + newEllipse.Width > canvas.Bounds.Width)
                {
                    pos = new Point(pos.X - (pos.X + newEllipse.Width - canvas.Bounds.Width) - POSITION_OFFSET, pos.Y);
                }
                
                if (pos.Y + newEllipse.Height > canvas.Bounds.Height)
                {
                    pos = new Point(pos.X, pos.Y - (pos.Y + newEllipse.Height - canvas.Bounds.Height) - POSITION_OFFSET);
                }
                
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

                if (newLine.EndPoint.X > canvas.Bounds.Width)
                {
                    newLine.StartPoint = new Point(newLine.StartPoint.X - (newLine.EndPoint.X - canvas.Bounds.Width) - POSITION_OFFSET, newLine.StartPoint.Y);
                    newLine.EndPoint = new Point(newLine.EndPoint.X - (newLine.EndPoint.X - canvas.Bounds.Width) - POSITION_OFFSET, newLine.EndPoint.Y);
                }
                
                if (newLine.EndPoint.Y > canvas.Bounds.Height)
                {
                    newLine.StartPoint = new Point(newLine.StartPoint.X, newLine.StartPoint.Y - (newLine.EndPoint.Y - canvas.Bounds.Height) - POSITION_OFFSET);
                    newLine.EndPoint = new Point(newLine.EndPoint.X, newLine.EndPoint.Y - (newLine.EndPoint.Y - canvas.Bounds.Height) - POSITION_OFFSET);
                }

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
                    Background = textBlock.Background,
                };
                
                var pos = new Point(newPos.X + textBlock.Bounds.X - selectedAreaPos.X,
                    newPos.Y + textBlock.Bounds.Y - selectedAreaPos.Y);
                
                if (pos.X + textBlock.Bounds.Width > canvas.Bounds.Width)
                {
                    pos = new Point(pos.X - (pos.X + textBlock.Bounds.Width - canvas.Bounds.Width) - POSITION_OFFSET, pos.Y);
                }
                
                if (pos.Y + textBlock.Bounds.Height > canvas.Bounds.Height)
                {
                    pos = new Point(pos.X, pos.Y - (pos.Y + textBlock.Bounds.Height - canvas.Bounds.Height) - POSITION_OFFSET);
                }
                
                canvas.SetPosition(newTextBlock, pos);
                canvas.Children.Add(newTextBlock);
                
                break;
            }
        }
    }
    
    private static List<Point> CalculateNewPolylinePoints(Polyline oldPolyline, Point selectedAreaPos, Point newPos)
    {
        var points = new List<Point>();
        var currentPoint = new Point(
            newPos.X + oldPolyline.Points.First().X - selectedAreaPos.X,
            newPos.Y + oldPolyline.Points.First().Y - selectedAreaPos.Y);
                
        points.Add(currentPoint);
                
        for (var i = 0; i < oldPolyline.Points.Count - 1; i++)
        {
            var diffX = oldPolyline.Points[i + 1].X - oldPolyline.Points[i].X;
            var diffY = oldPolyline.Points[i + 1].Y - oldPolyline.Points[i].Y;
            var nextPoint = new Point(currentPoint.X + diffX, currentPoint.Y + diffY);
                    
            currentPoint = new Point(nextPoint.X, nextPoint.Y);
                    
            points.Add(nextPoint);
        }

        return points;
    }

    public static void SetRectanglePosAndSize(RectangleViewModel rectangleViewModel, Point currentPos, Point startPos)
    {
        rectangleViewModel.X = Math.Min(currentPos.X, startPos.X);
        rectangleViewModel.Y = Math.Min(currentPos.Y, startPos.Y);

        rectangleViewModel.Width = Math.Max(currentPos.X, startPos.X) - rectangleViewModel.X;
        rectangleViewModel.Height = Math.Max(currentPos.Y, startPos.Y) - rectangleViewModel.Y;
    }
    
    public static void SetRectanglePosAndSize(EllipseViewModel ellipseViewModel, Point currentPos, Point startPos)
    {
        ellipseViewModel.X = Math.Min(currentPos.X, startPos.X);
        ellipseViewModel.Y = Math.Min(currentPos.Y, startPos.Y);

        ellipseViewModel.Width = Math.Max(currentPos.X, startPos.X) - ellipseViewModel.X;
        ellipseViewModel.Height = Math.Max(currentPos.Y, startPos.Y) - ellipseViewModel.Y;
    }
    
    public static void RemoveByArea(ObservableCollection<ShapeViewModelBase> shapes, 
        RectangleViewModel area, 
        DrawingHistoryService? drawingHistoryService = null)
    {
        var shapesToRemove = shapes
            .Where(x => IsInArea(x, area) && x != area)
            .ToList();

        // if (drawingHistoryService != null && controlsToRemove.Any())
        // {
        //     drawingHistoryService.Save(controlsToRemove, DrawingAction.Delete);
        // }
        
        shapes.RemoveMany(shapesToRemove);
    }
}