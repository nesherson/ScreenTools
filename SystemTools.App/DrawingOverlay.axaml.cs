using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace SystemTools.App;

public partial class DrawingOverlay : Window
{
    Polyline? _currentPolyline;
    public DrawingOverlay()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        WindowLockHook.Hook(this);
        
        base.OnLoaded(e);
    }

    private void Canvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // var point = e.GetCurrentPoint(sender as Control);
        
        // if (point.Properties.IsLeftButtonPressed)
        //     _currentPoint = point.Position;
    }
    
    private void Canvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _currentPolyline = null;
    }

    private void Canvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var canvas = sender as Canvas;

        if (canvas is null)
            return;
        
        var point = e.GetCurrentPoint(canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            if (_currentPolyline == null)
            {
                _currentPolyline = new Polyline
                {
                    Stroke = new SolidColorBrush(Colors.AliceBlue),
                    StrokeThickness = 10
                };
                canvas.Children.Add(_currentPolyline);
            }
            else
            {
                _currentPolyline.Points.Add(point.Position);
            }
        }
    }
}