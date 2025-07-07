using Avalonia;

namespace ScreenTools.Core;

public class LineViewModel : ShapeViewModelBase
{
    private Point _startPoint;
    private Point _endPoint;
    private string _stroke;
    private double _strokeThickness;
    
    public Point StartPoint
    {
        get => _startPoint;
        set => SetProperty(ref _startPoint, value);
    }
    
    public double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, value);
    }
    
    public Point EndPoint
    {
        get => _endPoint;
        set => SetProperty(ref _endPoint, value);
    }   
    
    public string Stroke
    {
        get => _stroke;
        set => SetProperty(ref _stroke, value);
    }
}