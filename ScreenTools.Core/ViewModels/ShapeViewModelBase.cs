using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.Core;

public class ShapeViewModelBase : ObservableObject
{
    private double _x;
    private double _y;
    private double _worldPositionX;
    private double _worldPositionY;
    
    public Point WorldStartPoint { get; set; }
    public Point WorldEndPoint { get; set; }

    
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
    
    public double WorldPositionX
    {
        get => _worldPositionX;
        set => SetProperty(ref _worldPositionX, value);
    }   
    
    public double WorldPositionY
    {
        get => _worldPositionY;
        set => SetProperty(ref _worldPositionY, value);
    }   
    
   
}