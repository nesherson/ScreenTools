using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;

namespace ScreenTools.Core;

public class PolylineViewModel : ShapeViewModelBase
{
    private string _stroke;
    private double _strokeThickness;
    private PenLineJoin _strokeJoin;
    private PenLineCap _strokeLineCap;
    private ObservableCollection<Point> _points;

    public PolylineViewModel()
    {
        Points = [];
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
    
    public PenLineJoin StrokeJoin
    {
        get => _strokeJoin;
        set => SetProperty(ref _strokeJoin, value);
    }
    
    public PenLineCap StrokeLineCap
    {
        get => _strokeLineCap;
        set => SetProperty(ref _strokeLineCap, value);
    }
    
    public ObservableCollection<Point> Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }
}