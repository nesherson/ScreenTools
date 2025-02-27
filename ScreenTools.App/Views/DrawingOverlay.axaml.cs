using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Path = System.IO.Path;
using Point = Avalonia.Point;

namespace ScreenTools.App;

public partial class DrawingOverlay : NotifyPropertyChangedWindowBase
{
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly WindowsToastService _windowsToastService;
    
    private Polyline? _currentPolyline;
    private Border? _eraseArea;
    private bool _isPopupOpen;
    private Thickness _windowBorderThickness;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private int _selectedLineStroke;
    private string _selectedLineColor;
    private Point _startPoint;
    private DrawingState _drawingState;
    private List<DrawingHistoryItem?> _drawingHistoryItems;

    public DrawingOverlay()
    {
        InitializeComponent();
    }

    public DrawingOverlay(ScreenCaptureService screenCaptureService,
        WindowsToastService windowsToastService)
    {
        InitializeComponent();

        DataContext = this;

        _screenCaptureService = screenCaptureService;
        _windowsToastService = windowsToastService;
        DrawingState = DrawingState.Drawing;

        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        LineStrokes = [5, 10, 15, 20];
        SelectedLineStroke = 5;
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff"];
        SelectedLineColor = "#000000";
        _drawingHistoryItems = new();
    }

    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set => SetField(ref _isPopupOpen, value);
    }
    
    public Thickness WindowBorderThickness
    {
        get => _windowBorderThickness;
        set => SetField(ref _windowBorderThickness, value);
    }
    
    public ObservableCollection<int> LineStrokes
    {
        get => _lineStrokes;
        set => SetField(ref _lineStrokes, value);
    }
    
    public int SelectedLineStroke
    {
        get => _selectedLineStroke;
        set => SetField(ref _selectedLineStroke, value);
    }
    
    public ObservableCollection<string> LineColors
    {
        get => _lineColors;
        set => SetField(ref _lineColors, value);
    }
    
    public string SelectedLineColor
    {
        get => _selectedLineColor;
        set => SetField(ref _selectedLineColor, value);
    }

    public DrawingState DrawingState
    {
        get => _drawingState;
        set => SetField(ref _drawingState, value);
    }
    
    private async Task CaptureWindow()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "ScreenTools",
            "Captures");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        IsPopupOpen = false;
        WindowBorderThickness = new Thickness(0);
        
        await Task.Delay(100);
        var bmp = new Bitmap(Convert.ToInt32(Width), Convert.ToInt32(Height), PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
            g.CopyFromScreen(Position.X, Position.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
        
        bmp.Save(Path.Combine(path, $"Capture-{DateTime.Now:dd-MM-yyyy-hhmmss}.jpg"));
        
        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                break;
            case Key.F5:
                IsPopupOpen = !IsPopupOpen;
                break;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        WindowLockHook.Hook(this);
        
        base.OnLoaded(e); 
    } 
    
    private void Canvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var canvas = sender as Canvas;
        switch (DrawingState)
        {
            case DrawingState.Drawing:
                AddHistoryItem(_currentPolyline, DrawingAction.Draw);
                _currentPolyline = null;

                break;
            case DrawingState.Erasing:
                if (canvas is null)
                {
                    return;
                }

                var polylinesToRemove = canvas.Children
                    .Where(x => x is Polyline)
                    .Cast<Polyline>()
                    .Where(pl => IsInBorderArea(_eraseArea, pl))
                    .ToList();

                if (polylinesToRemove.Any())
                {
                    AddHistoryItem(polylinesToRemove, DrawingAction.Delete);
                }

                foreach (var polylineToRemove in polylinesToRemove)
                {
                    canvas.Children.Remove(polylineToRemove);
                }

                canvas.Children.Remove(_eraseArea);

                _eraseArea = null;
                break;

        }

        IsPopupOpen = true;
    }

    private void AddHistoryItem(Polyline polyline, DrawingAction drawingAction)
    {
        _drawingHistoryItems.Add(new DrawingHistoryItem
        {
            Lines = new List<Polyline> { polyline },
            Action = drawingAction
        });
    }

    private void AddHistoryItem(List<Polyline> polylines, DrawingAction drawingAction)
    {
        _drawingHistoryItems.Add(new DrawingHistoryItem
        {
            Lines = polylines,
            Action = drawingAction
        });
    }

    private bool IsInBorderArea(Border border, Polyline polyline)
    {
        return polyline.Points
            .Any(p => p.X >= _eraseArea.Bounds.TopLeft.X &&
                 p.X <= _eraseArea.Bounds.TopRight.X &&
                 p.X >= _eraseArea.Bounds.BottomLeft.X &&
                 p.X <= _eraseArea.Bounds.BottomRight.X &&
                 p.Y >= _eraseArea.Bounds.TopLeft.Y &&
                 p.Y <= _eraseArea.Bounds.BottomLeft.Y &&
                 p.Y >= _eraseArea.Bounds.TopRight.Y &&
                 p.Y <= _eraseArea.Bounds.BottomRight.Y);
    }

    private void Canvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var canvas = sender as Canvas;

        if (canvas is null)
            return;

        var point = e.GetCurrentPoint(canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
      
            switch (DrawingState)
            {
                case DrawingState.Drawing:
                    if (_currentPolyline is null)
                        return;

                    _currentPolyline.Points.Add(point.Position);
                    break;
                case DrawingState.Erasing:
                    var x = Math.Min(point.Position.X, _startPoint.X);
                    var y = Math.Min(point.Position.Y, _startPoint.Y);

                    var w = Math.Max(point.Position.X, _startPoint.X) - x;
                    var h = Math.Max(point.Position.Y, _startPoint.Y) - y;

                    _eraseArea.Width = w;
                    _eraseArea.Height = h;

                    Canvas.SetLeft(_eraseArea, x);
                    Canvas.SetTop(_eraseArea, y);
                    break;

            }
        }
    }

    private void Canvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsPopupOpen = false;

        var canvas = sender as Canvas;

        if (canvas is null)
            return;

        _startPoint = e.GetPosition(canvas);

        switch (DrawingState)
        {
            case DrawingState.Drawing:
                if (_currentPolyline == null)
                {
                    _currentPolyline = new Polyline
                    {
                        Stroke = SolidColorBrush.Parse(SelectedLineColor),
                        StrokeThickness = SelectedLineStroke
                    };
                    canvas.Children.Add(_currentPolyline);
                }
                break;
            case DrawingState.Erasing:
                if (_eraseArea == null)
                {
                    _eraseArea = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.Red)
                    };

                    Canvas.SetLeft(_eraseArea, _startPoint.X);
                    Canvas.SetLeft(_eraseArea, _startPoint.Y);
                    canvas.Children.Add(_eraseArea);
                }
                break;
        }   
    }

    private void ButtonEraser_OnClick(object? sender, RoutedEventArgs e)
    {
        DrawingState = DrawingState.Erasing;
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private async void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await CaptureWindow();
            _windowsToastService.ShowMessage("Screenshot captured!");
        }
        catch (Exception)
        {
            _windowsToastService.ShowMessage("An error occured!");
        }
    }
    
    private void ButtonClear_OnClick(object? sender, RoutedEventArgs e)
    {
        var polylinesToSave = Canvas.Children
                .Where(x => x is Polyline)
                .Cast<Polyline>()
                .ToList();
      
        if (polylinesToSave.Any())
        {
            AddHistoryItem(polylinesToSave, DrawingAction.Clear);
        }

        Canvas.Children.Clear();
    }

    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e)
    {
        DrawingHistoryItem? itemToUndo = _drawingHistoryItems.LastOrDefault();

        if (itemToUndo == null)
            return;

        switch (itemToUndo.Value.Action)
        {
            case DrawingAction.Draw:
                Canvas.Children.Remove(itemToUndo.Value.Lines.First());
                _drawingHistoryItems.Remove(itemToUndo.Value);
                break;
            case DrawingAction.Delete:
                Canvas.Children.AddRange(itemToUndo.Value.Lines);
                _drawingHistoryItems.Remove(itemToUndo.Value);
                break;
            case DrawingAction.Clear:
                Canvas.Children.AddRange(itemToUndo.Value.Lines);
                _drawingHistoryItems.Remove(itemToUndo.Value);
                break;
        }

    }

    private void ColorComboBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        DrawingState = DrawingState.Drawing;
    }

    private void LineComboBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        DrawingState = DrawingState.Drawing;
    }
}
