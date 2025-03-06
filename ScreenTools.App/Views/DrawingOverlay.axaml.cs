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
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Path = System.IO.Path;
using Point = Avalonia.Point;

namespace ScreenTools.App;

public partial class DrawingOverlay : NotifyPropertyChangedWindowBase
{
    private readonly WindowNotificationManager _notificationManager;
    private readonly TextDetectionService _textDetectionService;
    private readonly ScreenCaptureService _screenCaptureService;
    
    private Thickness _windowBorderThickness;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private Polyline? _currentPolyline;
    private Border? _eraseArea;
    private Border? _textDetectionArea;
    private DrawingState _drawingState;
    private Point _startPoint;
    private List<DrawingHistoryItem?> _drawingHistoryItems;
    private Point? _controlToMovePosition;
    private bool _isDragging;
    private bool _isPopupOpen;
    private int _selectedLineStroke;
    private string _selectedLineColor; 
    
    public DrawingOverlay()
    {
        InitializeComponent();
    }

    public DrawingOverlay(TextDetectionService textDetectionService,
        ScreenCaptureService screenCaptureService)
    {
        InitializeComponent();

        DataContext = this;

        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        _textDetectionService = textDetectionService;
        _screenCaptureService = screenCaptureService;

        DrawingState = DrawingState.Draw;
        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        LineStrokes = [5, 10, 15, 20];
        SelectedLineStroke = 5;
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff"];
        SelectedLineColor = "#000000";
        _drawingHistoryItems = [];
    }
    
    public DrawingState DrawingState
    {
        get => _drawingState;
        set => SetField(ref _drawingState, value);
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
    
    protected override async void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1:
                DrawingState = DrawingState.Draw;
                break;
            case Key.D2:
                Undo();
                break;
            case Key.D3:
                DrawingState = DrawingState.Erase;
                break;
            case Key.D4:
                ClearAllCanvasContent();
                break;
            case Key.D5:
                DrawingState = DrawingState.DetectText;
                break;
            case Key.Escape:
                Close();
                break;
            case Key.F5:
                IsPopupOpen = !IsPopupOpen;
                break;
            case Key.S when e.KeyModifiers == KeyModifiers.Control:
                await CaptureWindow();
                break;
            case Key.V when e.KeyModifiers == KeyModifiers.Control:
                var clipboard = GetTopLevel(this)?.Clipboard;

                if (clipboard is null)
                    return;
                
                var text = await clipboard.GetTextAsync();
                
                if (string.IsNullOrEmpty(text))
                    return;
                
                AddTextToCanvas(text);
                
                break;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        WindowLockHook.Hook(this);

        base.OnLoaded(e);
    }
    
    private async Task CaptureWindow()
    {
        try
        {
            IsPopupOpen = false;
            WindowBorderThickness = new Thickness(0);

            await Task.Delay(100);
            _screenCaptureService.CaptureVisibleWindow(Width, Height, Position.X, Position.Y);
            _notificationManager.Show(new Notification("Success", "Screenshot captured!", NotificationType.Success));
        }
        catch (Exception ex)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(ex);
        }
        finally
        {
            IsPopupOpen = true;
            WindowBorderThickness = new Thickness(2); 
        }
    }
    
    private void Canvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_isDragging)
            return;

        var point = e.GetCurrentPoint(Canvas);

        if (!point.Properties.IsLeftButtonPressed)
            return;
        
        IsPopupOpen = false;
        _startPoint = e.GetPosition(Canvas);

        switch (DrawingState)
        {
            case DrawingState.Draw:
                if (_currentPolyline is null)
                {
                    _currentPolyline = new Polyline
                    {
                        Stroke = SolidColorBrush.Parse(SelectedLineColor),
                        StrokeThickness = SelectedLineStroke
                    };
                    Canvas.Children.Add(_currentPolyline);
                }

                break;
            case DrawingState.Erase:
                if (_eraseArea is null)
                {
                    _eraseArea = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.Red)
                    };

                    Canvas.SetLeft(_eraseArea, _startPoint.X);
                    Canvas.SetTop(_eraseArea, _startPoint.Y);
                    Canvas.Children.Add(_eraseArea);
                }

                break;
            case DrawingState.DetectText:
                if (_textDetectionArea is null)
                {
                    _textDetectionArea = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.Red)
                    };

                    Canvas.SetLeft(_textDetectionArea, _startPoint.X);
                    Canvas.SetTop(_textDetectionArea, _startPoint.Y);
                    Canvas.Children.Add(_textDetectionArea);
                }
                break;
        }
    }
    
    private void Canvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
            return;

        try
        {
            switch (DrawingState)
            {
                case DrawingState.Draw:
                    if (_currentPolyline is null)
                        return;
                    
                    AddHistoryItem(_currentPolyline, DrawingAction.Draw);
                    _currentPolyline = null;

                    break;
                case DrawingState.Erase:
                    if (_eraseArea is null)
                        return;

                    var controlsToRemove = Canvas.Children
                        .Where(x => x is Polyline or TextBlock)
                        .Where(IsInEraseArea)
                        .ToList();

                    if (controlsToRemove.Any())
                    {
                        AddHistoryItem(controlsToRemove, DrawingAction.Delete);
                    }

                    foreach (var controlToRemove in controlsToRemove)
                    {
                        Canvas.Children.Remove(controlToRemove);
                    }

                    Canvas.Children.Remove(_eraseArea);

                    _eraseArea = null;
                    break;
                case DrawingState.DetectText:
                    if (_textDetectionArea is null)
                        return;

                    var startX = _textDetectionArea.Bounds.X;
                    var startY = _textDetectionArea.Bounds.Y;
                    var width = _textDetectionArea.Width;
                    var height = _textDetectionArea.Height;

                    Canvas.Children.Remove(_textDetectionArea);
                    _textDetectionArea = null;

                    if (width == 0 || height == 0)
                        throw new ArgumentException("TextDetectError: Width and Height cannot be 0");

                    var bmp = new Bitmap(Convert.ToInt32(width),
                        Convert.ToInt32(height),
                        PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(bmp))
                        g.CopyFromScreen(Convert.ToInt32(startX),
                            Convert.ToInt32(startY),
                            0,
                            0,
                            bmp.Size,
                            CopyPixelOperation.SourceCopy);

                    var ms = new MemoryStream();
                    bmp.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        "ScreenTools",
                        "Captures",
                        "test-123.png"), ImageFormat.Png);

                    bmp.Save(ms, ImageFormat.Png);

                    var text = _textDetectionService
                        .ProcessImage(ms.ToArray())
                        .Trim();

                    if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                        return;

                    AddTextToCanvas(text);

                    break;
            }
        }
        catch (ArgumentException argEx) when (argEx.Message.Contains("TextDetectError"))
        {
            _notificationManager.Show(new Notification(
                "Error",
                argEx.Message[argEx.Message.IndexOf(':').. + 2],
                NotificationType.Error));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            IsPopupOpen = true;
        }
    }
    
    private void Canvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
            return;
        
        var point = e.GetCurrentPoint(Canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            switch (DrawingState)
            {
                case DrawingState.Draw:
                    if (_currentPolyline is null)
                        return;

                    _currentPolyline.Points.Add(point.Position);
                    break;
                case DrawingState.Erase:
                {
                    if (_eraseArea is null)
                        return;
                    
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
                case DrawingState.DetectText:
                {
                    if (_textDetectionArea is null)
                        return;

                    var x = Math.Min(point.Position.X, _startPoint.X);
                    var y = Math.Min(point.Position.Y, _startPoint.Y);

                    var w = Math.Max(point.Position.X, _startPoint.X) - x;
                    var h = Math.Max(point.Position.Y, _startPoint.Y) - y;

                    _textDetectionArea.Width = w;
                    _textDetectionArea.Height = h;

                    Canvas.SetLeft(_textDetectionArea, x);
                    Canvas.SetTop(_textDetectionArea, y);
                    break;
                }
            }
        }
    }

    private void AddTextToCanvas(string text)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 20,
            Foreground = new SolidColorBrush(Colors.Black),
            Background = new SolidColorBrush(Colors.Transparent)
        };
        textBlock.PointerPressed += TextBlockOnPointerPressed;
        textBlock.PointerReleased += TextBlockOnPointerReleased;
        textBlock.PointerMoved += TextBlockOnPointerMoved;
                    
        Canvas.SetLeft(textBlock, Width * 0.85);
        Canvas.SetTop(textBlock, Height * 0.15);

        Canvas.Children.Add(textBlock);
    }
    
    private void AddHistoryItem(Control control, DrawingAction drawingAction)
    {
        switch (control)
        {
            case Polyline polyline:
                _drawingHistoryItems.Add(new DrawingHistoryItem
                {
                    Lines = [polyline],
                    Action = drawingAction
                });
                break;
            case TextBlock textBlock:
                _drawingHistoryItems.Add(new DrawingHistoryItem
                {
                    TextBlocks = [textBlock],
                    Action = drawingAction
                });
                break;
        }
    }

    private void AddHistoryItem(List<Control> controls, DrawingAction drawingAction)
    {
        _drawingHistoryItems.Add(new DrawingHistoryItem
        {
            Lines = controls.Where(x => x is Polyline).Cast<Polyline>().ToList(),
            TextBlocks = controls.Where(x => x is TextBlock).Cast<TextBlock>().ToList(),
            Action = drawingAction
        });
    }
    
    private void TextBlockOnPointerMoved(object? sender, PointerEventArgs e)
    {
        var textBlock = sender as TextBlock;
        
        if (textBlock is null)
            return;

        if (_controlToMovePosition is null)
            return;
            
        
        var point = e.GetCurrentPoint(Canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            var posX = point.Position.X - _controlToMovePosition.Value.X;
            var posY = point.Position.Y - _controlToMovePosition.Value.Y;
            
            Canvas.SetLeft(textBlock, posX);
            Canvas.SetTop(textBlock, posY);
        }
    }

    private void TextBlockOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _controlToMovePosition = null;
        _isDragging = false;
        IsPopupOpen = true;
        e.Handled = true;
    }

    private void TextBlockOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var textBlock = sender as TextBlock;

        if (textBlock is null)
            return;
        
        var point = e.GetCurrentPoint(Canvas);
    
        if (point.Properties.IsLeftButtonPressed)
        {
            IsPopupOpen = false;
            _isDragging = true;
            _controlToMovePosition = e.GetPosition(textBlock);
        }
        else if (point.Properties.IsRightButtonPressed)
        {
            HandleTextBlockRightBtnPressed(textBlock);
        }
    }

    private void HandleTextBlockRightBtnPressed(TextBlock textBlock)
    {
        var flyout = new Flyout();
        var textBox = new TextBox
        {
            Text = textBlock.Text
        };
        flyout.Content = textBox;
        flyout.Closed += (_, _) =>
        {
            textBlock.Text = textBox.Text;
        };
        flyout.ShowAt(textBlock);
    }
    
    private bool IsInEraseArea(Control control)
    {
        switch (control)
        {
            case Polyline polyline:
                return polyline.Points
                    .Any(p => p.X >= _eraseArea!.Bounds.TopLeft.X &&
                              p.X <= _eraseArea!.Bounds.TopRight.X &&
                              p.X >= _eraseArea!.Bounds.BottomLeft.X &&
                              p.X <= _eraseArea!.Bounds.BottomRight.X &&
                              p.Y >= _eraseArea!.Bounds.TopLeft.Y &&
                              p.Y <= _eraseArea!.Bounds.BottomLeft.Y &&
                              p.Y >= _eraseArea!.Bounds.TopRight.Y &&
                              p.Y <= _eraseArea!.Bounds.BottomRight.Y);
            case TextBlock textBlock:
                return textBlock.Bounds.X >= _eraseArea!.Bounds.TopLeft.X &&
                       textBlock.Bounds.X <= _eraseArea!.Bounds.TopRight.X &&
                       textBlock.Bounds.X >= _eraseArea!.Bounds.BottomLeft.X &&
                       textBlock.Bounds.X <= _eraseArea!.Bounds.BottomRight.X &&
                       textBlock.Bounds.Y >= _eraseArea!.Bounds.TopLeft.Y &&
                       textBlock.Bounds.Y <= _eraseArea!.Bounds.BottomLeft.Y &&
                       textBlock.Bounds.Y >= _eraseArea!.Bounds.TopRight.Y &&
                       textBlock.Bounds.Y <= _eraseArea!.Bounds.BottomRight.Y;
        }

        return false;
    }

    private void Undo()
    {
        var itemToUndo = _drawingHistoryItems.LastOrDefault();

        if (itemToUndo is null)
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

    private void ClearAllCanvasContent()
    {
        var controlsToSave = Canvas.Children
            .Where(x => x is Polyline or TextBlock)
            .ToList();

        if (controlsToSave.Count != 0)
        {
            AddHistoryItem(controlsToSave, DrawingAction.Clear);
        }

        Canvas.Children.Clear();
    }
    
    private void ButtonEraser_OnClick(object? sender, RoutedEventArgs e)
    {
        DrawingState = DrawingState.Erase;
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        await CaptureWindow();
    }

    private void ButtonClear_OnClick(object? sender, RoutedEventArgs e)
    {
        ClearAllCanvasContent();
    }

    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e)
    {
        Undo();
    }
    
    private void ButtonPen_OnClick(object? sender, RoutedEventArgs e)
    {
        DrawingState = DrawingState.Draw;
    }

    private void ButtonDetectText_OnClick(object? sender, RoutedEventArgs e)
    {
        DrawingState = DrawingState.DetectText;
    }

    private void ColorComboBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        DrawingState = DrawingState.Draw;
    }

    private void LineComboBox_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        DrawingState = DrawingState.Draw;
    }
}
