namespace ScreenTools.Core;

public class EllipseViewModel : ShapeViewModelBase
{
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private string _fill;
    
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
    
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }    
    
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }
    
    public string Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }
}