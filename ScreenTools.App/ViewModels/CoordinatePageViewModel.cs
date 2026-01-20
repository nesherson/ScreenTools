using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using ScreenTools.Core;

namespace ScreenTools.App;

public partial class CoordinatePlanePageViewModel : PageViewModel
{
    [ObservableProperty] 
    private ObservableCollection<ShapeViewModelBase> _shapes;
    [ObservableProperty] 
    private double _scale = 1.0;
    [ObservableProperty] 
    private Point _offset;
    
    public CoordinatePlanePageViewModel()
    {
        Shapes = [];
    }

    public void RedrawGrid()
    {
        var tempShapes = Shapes
            .Where(x => x is EllipseViewModel or TextBlockViewModel)
            .ToList();

        DrawGrid();
        
        Shapes.AddRange(tempShapes);
    }

    public void DrawGrid()
    {
        Shapes.Clear();

        const int gridSpacing = 25;
        const int worldSize = 500;
        
        // Draw vertical lines
        for (double x = -worldSize; x <= worldSize; x += gridSpacing)
        {
            var line = new LineViewModel
            {
                WorldStartPoint = new Point(x, -worldSize),
                WorldEndPoint = new Point(x, worldSize),
                Stroke = Colors.LightGray.ToString(),
                StrokeThickness = 0.5
            };
            Shapes.Add(line);
        }
        
        var xAxis = new LineViewModel
        {
            WorldStartPoint = new Point(-worldSize, 0),
            WorldEndPoint = new Point(worldSize, 0),
            Stroke = "Gray",
            StrokeThickness = 1.5
        };
        var yAxis = new LineViewModel
        {
            WorldStartPoint = new Point(0, -worldSize),
            WorldEndPoint = new Point(0, worldSize),
            Stroke = "Gray",
            StrokeThickness = 1.5
        };
        
        Shapes.Add(xAxis);
        Shapes.Add(yAxis);
    }
    
    public void OnPointerPressed(double worldX, double worldY)
    {
        var newPoint = new EllipseViewModel
        {
            WorldPositionX = worldX,
            WorldPositionY = worldY,
            WorldWidth = 1,
            WorldHeight = 1,
            Fill = "#000000"
        };
        var pointText = new TextBlockViewModel
        {
            WorldPositionX = worldX + 10,
            WorldPositionY = worldY + 10,
            Text = $"({worldX},{worldY})"
        };
        
        Shapes.Add(newPoint);
        Shapes.Add(pointText);
    }

    [RelayCommand]
    public void ClearEllipses()
    {
        Shapes.RemoveMany(Shapes.Where(x => x is EllipseViewModel));
    }
}