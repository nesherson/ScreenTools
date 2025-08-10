using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using DynamicData;
using Microsoft.Extensions.Logging;
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
            case LineViewModel lineViewModel:
                return CheckOverlap(lineViewModel, eraseArea);
            case RectangleViewModel rectangleViewModel:
                return CheckOverlap(rectangleViewModel, eraseArea);
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
    
    private static bool CheckOverlap(LineViewModel line, RectangleViewModel rectangle)
    {
        var rect = new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        
        if (rect.Contains(line.StartPoint) || rect.Contains(line.EndPoint))
        {
            return true;
        }

        return LineSegmentIntersects(line.StartPoint, line.EndPoint, rect.TopLeft, rect.TopRight) ||
               LineSegmentIntersects(line.StartPoint, line.EndPoint, rect.TopRight, rect.BottomRight) ||
               LineSegmentIntersects(line.StartPoint, line.EndPoint, rect.BottomRight, rect.BottomLeft) ||
               LineSegmentIntersects(line.StartPoint, line.EndPoint, rect.BottomLeft, rect.TopLeft);
    }
    
    private static bool LineSegmentIntersects(Point lineOneStartPoint, Point lineOneEndPoint, Point lineTwoStartPoint, Point lineTwoEndPoint)
    {
        var denominator = (lineTwoEndPoint.Y - lineTwoStartPoint.Y) *
            (lineOneEndPoint.X - lineOneStartPoint.X) - (lineTwoEndPoint.X - lineTwoStartPoint.X) 
            * (lineOneEndPoint.Y - lineOneStartPoint.Y);
        
        if (Math.Abs(denominator) < 1e-9)
        {
            return false;
        }

        var t = ((lineTwoEndPoint.X - lineTwoStartPoint.X)
            * (lineOneStartPoint.Y - lineTwoStartPoint.Y) - (lineTwoEndPoint.Y - lineTwoStartPoint.Y)
            * (lineOneStartPoint.X - lineTwoStartPoint.X)) / denominator;
        var u = -((lineOneEndPoint.X - lineOneStartPoint.X) * 
            (lineOneStartPoint.Y - lineTwoStartPoint.Y) - (lineOneEndPoint.Y - lineOneStartPoint.Y) 
            * (lineOneStartPoint.X - lineTwoStartPoint.X)) / denominator;
        
        return t is >= 0 and <= 1 && u is >= 0 and <= 1;
    }
    
    public static void DeleteSavedCanvas(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    public static void CopyShapeToPosition(ObservableCollection<ShapeViewModelBase> shapes,
        ShapeViewModelBase shapeToCopy, 
        Point newPos, 
        Point selectedAreaPos,
        double windowWidth,
        double windowHeight)
    {
        switch (shapeToCopy)
        {
            case PolylineViewModel polylineViewModel:
            {
                var newPolylineViewModel = new PolylineViewModel
                {
                    Stroke = polylineViewModel.Stroke,
                    StrokeThickness = polylineViewModel.StrokeThickness,
                    StrokeJoin = polylineViewModel.StrokeJoin,
                    StrokeLineCap = polylineViewModel.StrokeLineCap
                };
                
                var newPolylinePoints = CalculateNewPolylinePoints(polylineViewModel, selectedAreaPos, newPos);
                var outOfCanvasWidthPoints = newPolylinePoints
                    .Where(p => p.X > windowWidth)
                    .ToList();

                if (outOfCanvasWidthPoints.Count > 0)
                {
                    var furthestPoint = outOfCanvasWidthPoints.MaxBy(p => p.X);
                    var outOfCanvasDistance = new Point(furthestPoint.X - windowWidth, furthestPoint.Y);
                    
                    newPos = new Point(newPos.X - outOfCanvasDistance.X - POSITION_OFFSET, newPos.Y);
                    newPolylinePoints = CalculateNewPolylinePoints(polylineViewModel, selectedAreaPos, newPos);
                }
                
                var outOfCanvasHeightPoints = newPolylinePoints
                    .Where(p => p.Y > windowHeight)
                    .ToList();

                if (outOfCanvasHeightPoints.Count > 0)
                {
                    var furthestPoint = outOfCanvasHeightPoints.MaxBy(p => p.Y);
                    var outOfCanvasDistance = new Point(furthestPoint.X, furthestPoint.Y - windowHeight);
                    
                    newPos = new Point(newPos.X, newPos.Y - outOfCanvasDistance.Y - POSITION_OFFSET);
                    newPolylinePoints = CalculateNewPolylinePoints(polylineViewModel, selectedAreaPos, newPos);
                }
                
                newPolylineViewModel.Points = newPolylinePoints.ToObservable();
                
                shapes.Add(newPolylineViewModel);
                
                break;
            }
            case RectangleViewModel rectangleViewModel:
            {
                var newRectangleViewModel = new RectangleViewModel
                {
                    Fill = rectangleViewModel.Fill,
                    Width = rectangleViewModel.Width,
                    Height = rectangleViewModel.Height
                };
                
                var pos = new Point(newPos.X + rectangleViewModel.X - selectedAreaPos.X,
                    newPos.Y + rectangleViewModel.Y - selectedAreaPos.Y);

                if (pos.X + newRectangleViewModel.Width > windowWidth)
                {
                    pos = new Point(pos.X - (pos.X + newRectangleViewModel.Width - windowWidth) - POSITION_OFFSET, pos.Y);
                }
                
                if (pos.Y + newRectangleViewModel.Height > windowHeight)
                {
                    pos = new Point(pos.X, pos.Y - (pos.Y + newRectangleViewModel.Height - windowHeight) - POSITION_OFFSET);
                }
                
                newRectangleViewModel.X = pos.X;
                newRectangleViewModel.Y = pos.Y;
                
                shapes.Add(newRectangleViewModel);
                
                break;
            }
            case LineViewModel lineViewModel:
            {
                var newLineViewModel = new LineViewModel
                {
                    Stroke = lineViewModel.Stroke,
                    StrokeThickness = lineViewModel.StrokeThickness,
                    StartPoint = new Point(newPos.X + lineViewModel.StartPoint.X - selectedAreaPos.X,
                        newPos.Y + lineViewModel.StartPoint.Y - selectedAreaPos.Y),
                    EndPoint = new Point(newPos.X + lineViewModel.EndPoint.X - selectedAreaPos.X,
                        newPos.Y + lineViewModel.EndPoint.Y - selectedAreaPos.Y)
                };

                if (newLineViewModel.EndPoint.X > windowWidth)
                {
                    newLineViewModel.StartPoint = new Point(newLineViewModel.StartPoint.X - (newLineViewModel.EndPoint.X - windowWidth) - POSITION_OFFSET, newLineViewModel.StartPoint.Y);
                    newLineViewModel.EndPoint = new Point(newLineViewModel.EndPoint.X - (newLineViewModel.EndPoint.X - windowWidth) - POSITION_OFFSET, newLineViewModel.EndPoint.Y);
                }
                
                if (newLineViewModel.EndPoint.Y > windowHeight)
                {
                    newLineViewModel.StartPoint = new Point(newLineViewModel.StartPoint.X, newLineViewModel.StartPoint.Y - (newLineViewModel.EndPoint.Y - windowHeight) - POSITION_OFFSET);
                    newLineViewModel.EndPoint = new Point(newLineViewModel.EndPoint.X, newLineViewModel.EndPoint.Y - (newLineViewModel.EndPoint.Y - windowHeight) - POSITION_OFFSET);
                }

                shapes.Add(newLineViewModel);
                
                break;
            }
            case TextBlockViewModel textBlockViewModel:
            {
                var newTextBlockViewModel = new TextBlockViewModel
                {
                    Text = textBlockViewModel.Text,
                    FontSize = textBlockViewModel.FontSize,
                    Foreground = textBlockViewModel.Foreground,
                    Background = textBlockViewModel.Background,
                };
                
                var pos = new Point(newPos.X + textBlockViewModel.X - selectedAreaPos.X,
                    newPos.Y + textBlockViewModel.Y - selectedAreaPos.Y);
                
                // if (pos.X + textBlockViewModel.Width > windowWidth)
                // {
                //     pos = new Point(pos.X - (pos.X + textBlockViewModel.Width - windowWidth) - POSITION_OFFSET, pos.Y);
                // }
                //
                // if (pos.Y + textBlockViewModel.Height > windowHeight)
                // {
                //     pos = new Point(pos.X, pos.Y - (pos.Y + textBlockViewModel.Height - windowHeight) - POSITION_OFFSET);
                // }
                
                shapes.Add(newTextBlockViewModel);
                
                break;
            }
        }
    }
    
    private static List<Point> CalculateNewPolylinePoints(PolylineViewModel oldPolylineViewModel, Point selectedAreaPos, Point newPos)
    {
        var points = new List<Point>();
        var currentPoint = new Point(
            newPos.X + oldPolylineViewModel.Points.First().X - selectedAreaPos.X,
            newPos.Y + oldPolylineViewModel.Points.First().Y - selectedAreaPos.Y);
                
        points.Add(currentPoint);
                
        for (var i = 0; i < oldPolylineViewModel.Points.Count - 1; i++)
        {
            var diffX = oldPolylineViewModel.Points[i + 1].X - oldPolylineViewModel.Points[i].X;
            var diffY = oldPolylineViewModel.Points[i + 1].Y - oldPolylineViewModel.Points[i].Y;
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
    
    public static void RemoveByArea(ObservableCollection<ShapeViewModelBase> shapes, 
        RectangleViewModel area, 
        DrawingHistoryService? drawingHistoryService = null)
    {
        var shapesToRemove = shapes
            .Where(x => IsInArea(x, area) && x != area)
            .ToList();

        drawingHistoryService?.Save(shapesToRemove, DrawingAction.Delete);
        
        shapes.RemoveMany(shapesToRemove);
    }
}