namespace ScreenTools.Core;

public class RectangleViewModel : ShapeViewModelBase
{
    private double _width;
    private double _height;
    private string _fill;
    private string _stroke;
    private double _strokeThickness;
    
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
    
    public string Stroke
    {
        get => _stroke;
        set => SetProperty(ref _stroke, value);
    }
    
    public double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, value);
    }
}