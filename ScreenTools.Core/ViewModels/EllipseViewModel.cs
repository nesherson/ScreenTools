using Avalonia;

namespace ScreenTools.Core;

public class EllipseViewModel : ShapeViewModelBase
{
    private double _x;
    private double _y;
    private double _radiusX;
    private double _radiusY;
    private string _fill;
    private Point _center;
    
    public Point Center
    {
        get => _center;
        set => SetProperty(ref _center, value);
    }
    
    public double RadiusX
    {
        get => _radiusX;
        set => SetProperty(ref _radiusX, value);
    }
    
    public double RadiusY
    {
        get => _radiusY;
        set => SetProperty(ref _radiusY, value);
    }
    
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }
    
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }    
    
    public string Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }
}