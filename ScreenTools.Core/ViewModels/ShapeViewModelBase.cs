using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.Core;

public class ShapeViewModelBase : ObservableObject
{
    private double _x;
    private double _y;
    
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
}