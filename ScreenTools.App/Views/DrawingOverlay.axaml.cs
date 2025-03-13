using System;
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
using ScreenTools.Infrastructure;
using Path = System.IO.Path;
using Point = Avalonia.Point;
using AvaloniaLine = Avalonia.Controls.Shapes.Line;
using AvaloniaRectangle = Avalonia.Controls.Shapes.Rectangle;
using AvaloniaEllipse = Avalonia.Controls.Shapes.Ellipse;

namespace ScreenTools.App;

public partial class DrawingOverlay : NotifyPropertyChangedWindowBase
{
    private readonly WindowNotificationManager _notificationManager;
    private readonly TextDetectionService _textDetectionService;
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly FilePathRepository _filePathRepository;
    private readonly DrawingHistoryService _drawingHistoryService;

    private Thickness _windowBorderThickness;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private Border? _eraseArea;
    private Border? _textDetectionArea;
    private DrawingState _drawingState;
    private Point _startPoint;
    private Point? _controlToMovePosition;
    private bool _isDragging;
    private bool _isPopupOpen;
    private int _selectedLineStroke;
    private string _selectedLineColor;
    private Shape? _selectedShape;
    
    public DrawingOverlay()
    {
        InitializeComponent();
    }

    public DrawingOverlay(TextDetectionService textDetectionService,
        ScreenCaptureService screenCaptureService,
        FilePathRepository filePathRepository,
        DrawingHistoryService drawingHistoryService)
    {
        InitializeComponent();

        DataContext = this;

        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        _textDetectionService = textDetectionService;
        _screenCaptureService = screenCaptureService;
        _filePathRepository = filePathRepository;
        _drawingHistoryService = drawingHistoryService;

        DrawingState = DrawingState.Draw;
        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        LineStrokes = [5, 10, 15, 20];
        SelectedLineStroke = 5;
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff"];
        SelectedLineColor = "#000000";
        _selectedShape = CreatePolyline();
    }
    
    public DrawingState DrawingState
    {
        get => _drawingState;
        set
        {
            OnPropertyChanged(nameof(SelectedShapeToolTip));
            SetField(ref _drawingState, value);
        }
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
    
    public string SelectedShapeToolTip =>
        _selectedShape switch

        {
            AvaloniaLine => "Line",
            AvaloniaRectangle => "Rectangle",
            _ => "Shape not selected"
        };
    
    protected override async void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1:
                DrawingState = DrawingState.Draw;
                break;
            case Key.D2:
                // select currently selected shape
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
                await PasteLastItemFromClipboard();
                break;
            case Key.Z when e.KeyModifiers == KeyModifiers.Control:
                Undo();
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
            
            var filePath = await _filePathRepository.GetByFilePathTypeAbrvAsync("draw-scr");

            if (!Directory.Exists(filePath.Path))
            {
                Directory.CreateDirectory(filePath.Path);
            }
            
            var imageSavePath = Path.Combine(filePath.Path, $"Screenshot-{DateTime.Now:dd-MM-yyyy-hhmmss}.png");
            var bmp = _screenCaptureService.CaptureVisibleWindow(Width, Height, Position.X, Position.Y);
            
            bmp.Save(imageSavePath, ImageFormat.Png);
            _notificationManager.Show(new Notification(
                "Screenshot captured!",
                "Click to show image in explorer.",
                NotificationType.Success,
                null,
                () => OnNotificationClick(imageSavePath)));
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

    private void OnNotificationClick(string pathToImage)
    {
        ProcessHelpers.ShowFileInFileExplorer(pathToImage);
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
                if (_selectedShape is Polyline polyline)
                {
                    polyline.Points.Add(_startPoint);
                    Canvas.Children.Add(_selectedShape);
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
            case DrawingState.DrawShape:
            {
                switch (_selectedShape)
                {
                    case AvaloniaLine line:
                        line.StartPoint = _startPoint;
                        line.EndPoint = _startPoint;
                        Canvas.Children.Add(line);
                        break;
                    case AvaloniaRectangle rectangle:
                        Canvas.SetLeft(rectangle, _startPoint.X);
                        Canvas.SetTop(rectangle, _startPoint.Y);
                        Canvas.Children.Add(rectangle);
                        break;
                    case AvaloniaEllipse ellipse:
                        Canvas.SetLeft(ellipse, _startPoint.X);
                        Canvas.SetTop(ellipse, _startPoint.Y);
                        Canvas.Children.Add(ellipse);
                        break;
                }

                break;
            }
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
                    if (_selectedShape is null)
                        return;
                    
                    _drawingHistoryService.Save(_selectedShape, DrawingAction.Draw);
                    _selectedShape = CreatePolyline();
                    
                    break;
                case DrawingState.Erase:
                    if (_eraseArea is null)
                        return;

                    var controlsToRemove = Canvas.Children
                        .Where(x => CanvasHelpers.IsInEraseArea(x, _eraseArea))
                        .ToList();

                    if (controlsToRemove.Any())
                    {
                        _drawingHistoryService.Save(controlsToRemove, DrawingAction.Delete);
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
                    
                    bmp.Save(ms, ImageFormat.Png);
                    
                    var text = _textDetectionService
                        .ProcessImage(ms.ToArray())
                        .Trim();

                    if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                    {
                        _notificationManager.Show(new Notification(
                            "Information",
                            "Text could not be detected."));
                        
                        return;
                    }

                    AddTextToCanvas(text);

                    break;
                case DrawingState.DrawShape:

                    if (_selectedShape is null)
                    {
                        return;
                    }
                    
                    _drawingHistoryService.Save([_selectedShape], DrawingAction.Draw);
                    
                    switch (_selectedShape)
                    {
                        case AvaloniaLine:
                            _selectedShape = CreateLine();
                            break;
                        case AvaloniaRectangle:
                            _selectedShape = CreateRectangle();
                            break;
                        case AvaloniaEllipse:
                            _selectedShape = CreateEllipse();
                            break;
                    }
                    
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
                    if (_selectedShape is Polyline polyline)
                    {
                        polyline.Points.Add(point.Position);
                    }
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
                case DrawingState.DrawShape:
                {
                    switch (_selectedShape)
                    {
                        case AvaloniaLine line:
                            line.EndPoint = e.GetPosition(Canvas);
                            break;
                        case AvaloniaRectangle rectangle:
                            SetControlPosAndSize(_startPoint, point.Position, rectangle);
                            break;
                        case AvaloniaEllipse ellipse:
                            SetControlPosAndSize(_startPoint, point.Position, ellipse);
                            break;
                    }

                    break;
                }
            }
        }
    }

    private void SetControlPosAndSize(Point startPoint, Point endPoint, Control control)
    {
        var x = Math.Min(endPoint.X, startPoint.X);
        var y = Math.Min(endPoint.Y, startPoint.Y);

        var w = Math.Max(endPoint.X, startPoint.X) - x;
        var h = Math.Max(endPoint.Y, startPoint.Y) - y;
        
        control.Width = w;
        control.Height = h;
        
        Canvas.SetLeft(control, x);
        Canvas.SetTop(control, y);
    }
    
    private async Task PasteLastItemFromClipboard()
    {
        var clipboard = GetTopLevel(this)?.Clipboard;

        if (clipboard is null)
            return;
                
        var text = await clipboard.GetTextAsync();
                
        if (string.IsNullOrEmpty(text))
            return;
                
        AddTextToCanvas(text);
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
    
    private Polyline CreatePolyline()
    {
        return new Polyline
        {
            Stroke = SolidColorBrush.Parse(SelectedLineColor),
            StrokeThickness = SelectedLineStroke,
            StrokeJoin = PenLineJoin.Miter,
            StrokeLineCap = PenLineCap.Round
        };
    }
    
    private Line CreateLine()
    {
        return new AvaloniaLine
        {
            Stroke = SolidColorBrush.Parse(SelectedLineColor),
            StrokeThickness = SelectedLineStroke
        };
    }
    
    private AvaloniaRectangle CreateRectangle()
    {
        return new AvaloniaRectangle
        {
            Fill = SolidColorBrush.Parse(SelectedLineColor)
        };
    }

    private Ellipse CreateEllipse()
    {
        return new Ellipse
        {
            Fill = SolidColorBrush.Parse(SelectedLineColor)
        };
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
    
    private void Undo()
    {
        _drawingHistoryService.Undo(Canvas);
    }

    private void UpdateSelectedShape()
    {
        switch (_selectedShape)
        {
            case AvaloniaLine line:
                line.Stroke = SolidColorBrush.Parse(SelectedLineColor);
                line.StrokeThickness = SelectedLineStroke;
                break;
            case AvaloniaRectangle rectangle:
                rectangle.Fill = SolidColorBrush.Parse(SelectedLineColor);
                break;
            case Polyline polyline:
                polyline.Stroke = SolidColorBrush.Parse(SelectedLineColor);
                polyline.StrokeThickness = SelectedLineStroke;
                break;
        }
    }

    private void ClearAllCanvasContent()
    {
        var controlsToSave = Canvas.Children
            .Where(x => x is Shape or TextBlock)
            .ToList();

        if (controlsToSave.Count != 0)
        {
            _drawingHistoryService.Save(controlsToSave, DrawingAction.Clear);
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
        _selectedShape = CreatePolyline();
        DrawingState = DrawingState.Draw;
    }
    
    private void ContextMenuItemShapes_OnClick(object? sender, RoutedEventArgs e)
    {
        var menuItem = sender as MenuItem;

        if (menuItem == null)
            return;

        switch (menuItem.Name)
        {
            case "Line":
                if (_selectedShape is not AvaloniaLine)
                {
                    _selectedShape = CreateLine();
                }
                
                break;
            case "Rectangle":
                if (_selectedShape is not AvaloniaRectangle)
                {
                    _selectedShape = CreateRectangle();
                }
                break;
            case "Ellipse":
                if (_selectedShape is not AvaloniaEllipse)
                {
                    _selectedShape = CreateEllipse();
                }
                break;
        }
        
        DrawingState = DrawingState.DrawShape;
    }

    private void ButtonDetectText_OnClick(object? sender, RoutedEventArgs e)
    {
        DrawingState = DrawingState.DetectText;
    }

    private void StrokeWidthComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_selectedShape != null)
            UpdateSelectedShape();
    }
    
    private void StrokeColorComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_selectedShape != null)
            UpdateSelectedShape();
    }
}
