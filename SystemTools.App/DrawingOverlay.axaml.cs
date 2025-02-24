using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace SystemTools.App;

public partial class DrawingOverlay : Window, INotifyPropertyChanged
{
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly WindowsToastService _windowsToastService;
    
    private Polyline? _currentPolyline;
    private Avalonia.Controls.Shapes.Rectangle? _currentRectangle;
    private bool _isPopupOpen;
    private Thickness _windowBorderThickness;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private int _selectedLineStroke;
    private string _selectedLineColor;
    private Point _startPoint;
    
    public DrawingOverlay(ScreenCaptureService screenCaptureService,
        WindowsToastService windowsToastService)
    {
        InitializeComponent();
        this.AttachDevTools();

        DataContext = this;
        
        _screenCaptureService = screenCaptureService;
        _windowsToastService = windowsToastService;

        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        LineStrokes = [5, 10, 15, 20];
        SelectedLineStroke = 5;
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff"];
        SelectedLineColor = "#000000";
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
  
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
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
        var bmp = new System.Drawing.Bitmap(Convert.ToInt32(Width), Convert.ToInt32(Height), PixelFormat.Format32bppArgb);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
            g.CopyFromScreen(Position.X, Position.Y, 0, 0, bmp.Size, System.Drawing.CopyPixelOperation.SourceCopy);
        
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
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void Canvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _currentPolyline = null;
        
        IsPopupOpen = true;
    }

    private void Canvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var canvas = sender as Canvas;

        if (canvas is null)
            return;

        //var point = e.GetCurrentPoint(canvas);
        var point = e.GetCurrentPoint(canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            //var pos = e.GetPosition(canvas);

            var x = Math.Min(point.Position.X, _startPoint.X);
            var y = Math.Min(point.Position.Y, _startPoint.Y);

            var w = Math.Max(point.Position.X, _startPoint.X) - x;
            var h = Math.Max(point.Position.Y, _startPoint.Y) - y;

            _currentRectangle.Width = w;
            _currentRectangle.Height = h;

            Canvas.SetLeft(_currentRectangle, x);
            Canvas.SetTop(_currentRectangle, y);
        }

        //if (point.Properties.IsLeftButtonPressed)
        //{
        //    if (_currentPolyline == null)
        //    {
        //        _currentPolyline = new Polyline
        //        {
        //            Stroke = SolidColorBrush.Parse(SelectedLineColor),
        //            StrokeThickness = SelectedLineStroke
        //        };
        //        canvas.Children.Add(_currentPolyline);
        //    }
        //    else
        //    {
        //        _currentPolyline.Points.Add(point.Position);
        //    }
        //}
    }

    private void Canvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsPopupOpen = false;

        var canvas = sender as Canvas;

        //var point = e.GetCurrentPoint(canvas);
        _startPoint = e.GetPosition(canvas);

        if (_currentRectangle == null)
        {
            _currentRectangle = new Avalonia.Controls.Shapes.Rectangle
            {
                Fill = SolidColorBrush.Parse(SelectedLineColor),
                StrokeThickness = 2,
            };
            Canvas.SetLeft(_currentRectangle, _startPoint.X);
            Canvas.SetLeft(_currentRectangle, _startPoint.Y);
            canvas.Children.Add(_currentRectangle);
        }
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
        Canvas.Children.Clear();
    }
}
