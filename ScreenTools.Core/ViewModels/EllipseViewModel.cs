namespace ScreenTools.Core;

public class EllipseViewModel : ShapeViewModelBase
{
    private double _worldWidth;
    private double _worldHeight;
    private string _fill;
  
    public double ScreenWidth => 5; 
    public double ScreenHeight => 5;
    
    public double WorldWidth
    {
        get => _worldWidth;
        set => SetProperty(ref _worldWidth, value);
    }
    
    public double WorldHeight
    {
        get => _worldHeight;
        set => SetProperty(ref _worldHeight, value);
    }    
    
   
    
    public string? Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }

    
 
}