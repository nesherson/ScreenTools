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
using Avalonia.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Infrastructure;
using Path = System.IO.Path;
using Point = Avalonia.Point;
using AvaloniaLine = Avalonia.Controls.Shapes.Line;
using AvaloniaRectangle = Avalonia.Controls.Shapes.Rectangle;
using AvaloniaEllipse = Avalonia.Controls.Shapes.Ellipse;
using Colors = Avalonia.Media.Colors;
using Notification = Avalonia.Controls.Notifications.Notification;
using Shape = Avalonia.Controls.Shapes.Shape;

namespace ScreenTools.App;

public partial class DrawingOverlay : NotifyPropertyChangedWindowBase
{
    private readonly WindowNotificationManager _notificationManager;
    private readonly TextDetectionService _textDetectionService;
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly FilePathRepository _filePathRepository;
    private readonly DrawingHistoryService _drawingHistoryService;
    private readonly ILogger<DrawingOverlay> _logger;
    private readonly IConfiguration _configuration;

    private Thickness _windowBorderThickness;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private Border? _eraseArea;
    private Border? _textDetectionArea;
    private DrawingState _drawingState;
    private Point _startPoint;
    private Point? _dragPosition;
    private bool _isDragging;
    private bool _isPopupOpen;
    private int _selectedLineStroke;
    private string _selectedLineColor;
    private Shape? _selectedShape;
    private ObservableCollection<DrawingToolbarItem> _toolbarItems;
    
    public DrawingOverlay()
    {
        InitializeComponent();
    }

    public DrawingOverlay(TextDetectionService textDetectionService,
        ScreenCaptureService screenCaptureService,
        FilePathRepository filePathRepository,
        DrawingHistoryService drawingHistoryService,
        ILogger<DrawingOverlay> logger,
        IConfiguration configuration)
    {
        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        _textDetectionService = textDetectionService;
        _screenCaptureService = screenCaptureService;
        _filePathRepository = filePathRepository;
        _drawingHistoryService = drawingHistoryService;
        _logger = logger;
        _configuration = configuration;
        
        InitializeComponent();

        DataContext = this;
        
        Closed += OnClosed;
        Deactivated += OnDeactivated;
        Activated += OnActivated;

        DrawingState = DrawingState.Draw;
        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        LineStrokes = [2, 5, 10, 15, 20];
        SelectedLineStroke = LineStrokes.First();
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff", "#47d147"];
        SelectedLineColor = LineColors.First();
        SelectPen();
        SetToolbarItems();
        SetActiveItem(ToolbarItems.First());
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
            AvaloniaEllipse => "Ellipse",
            _ => "Shape not selected"
        };
    
    public ObservableCollection<DrawingToolbarItem> ToolbarItems
    {
        get => _toolbarItems;
        set => SetField(ref _toolbarItems, value);
    }
    
    protected override async void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1:
            case Key.D2:
            case Key.D3:
            case Key.D4:
            case Key.D5:
            case Key.D6:
            case Key.Escape:
            case Key.F11:
            case Key.S when e.KeyModifiers == KeyModifiers.Control:
            case Key.Z when e.KeyModifiers == KeyModifiers.Control:
                TriggerToolbarItemOnClick(e.Key);
                break;
            case Key.F5:
                IsPopupOpen = !IsPopupOpen;
                break;
            case Key.V when e.KeyModifiers == KeyModifiers.Control:
                await PasteLastItemFromClipboard();
                break;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) || !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) 
            return;

        var deltaY = Convert.ToInt32(e.Delta.Y);
        var nextColorIndex = LineColors.IndexOf(SelectedLineColor) + deltaY;
        
        if (nextColorIndex < 0)
        {
            nextColorIndex = LineColors.Count - 1;
        }
        
        if (nextColorIndex >= LineColors.Count)
        {
            nextColorIndex = 0;
        }
        
        SelectedLineColor = LineColors[nextColorIndex];
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
            _logger.LogError($"Failed to capture window. Exception: {ex}");
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
            case DrawingState.Erase:
                if (_eraseArea is null)
                {
                    _eraseArea = new Border
                    {
                        BorderThickness = new Thickness(1),
                        BorderBrush = new SolidColorBrush(Colors.Red)
                    };
                    
                    Canvas.AddToPosition(_eraseArea, _startPoint);
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
                    
                    Canvas.AddToPosition(_textDetectionArea, _startPoint);
                }
                break;
            case DrawingState.Draw:
            {
                switch (_selectedShape)
                {
                    case Polyline polyline:
                        polyline.Points.Add(_startPoint);
                        Canvas.Children.Add(polyline);
                        break;
                    case AvaloniaLine line:
                        line.StartPoint = _startPoint;
                        line.EndPoint = _startPoint;
                        Canvas.Children.Add(line);
                        break;
                    case AvaloniaRectangle rectangle:
                        Canvas.AddToPosition(rectangle, _startPoint);
                        break;
                    case AvaloniaEllipse ellipse:
                        Canvas.AddToPosition(ellipse, _startPoint);
                        break;
                }

                break;
            }
            case DrawingState.AddText:
                var flyout = new Flyout();
                var textBox = new TextBox
                {
                    Width = 220,
                    Height = 30
                };
                flyout.Content = textBox;
                flyout.Closed += (_, _) =>
                {
                    var text = textBox.Text;

                    if (string.IsNullOrEmpty(text))
                        return;

                    var textBlock = new TextBlock
                    {
                        Text = text,
                        FontSize = 20,
                        Foreground = new SolidColorBrush(Colors.Black),
                        Background = new SolidColorBrush(Colors.Transparent)
                    };

                    Canvas.AddToPosition(textBlock, _startPoint.X, _startPoint.Y);
                };
                flyout.ShowAt(Canvas, true);
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
                case DrawingState.Erase:
                    if (_eraseArea is null)
                        return;
                    
                    Canvas.RemoveByArea(_eraseArea,_drawingHistoryService);

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
                case DrawingState.Draw:
                    if (_selectedShape is null)
                    {
                        return;
                    }
                    
                    _drawingHistoryService.Save([_selectedShape], DrawingAction.Draw);
                    
                    switch (_selectedShape)
                    {
                        case Polyline:
                            _selectedShape = CreatePolyline();
                            break;
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
            _logger.LogError($"Failed to detect text. Exception: {argEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to draw a shape: Exception: {ex.Message}");
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
                case DrawingState.Erase:
                {
                    if (_eraseArea is null)
                        return;
                    
                    Canvas.SetPositionAndSize(_eraseArea, point.Position, _startPoint);
                    
                    break;
                }
                case DrawingState.DetectText:
                {
                    if (_textDetectionArea is null)
                        return;
                    
                    Canvas.SetPositionAndSize(_textDetectionArea, point.Position, _startPoint);
                    
                    break;
                }
                case DrawingState.Draw:
                {
                    switch (_selectedShape)
                    {
                        case Polyline polyline:
                            polyline.Points.Add(point.Position);

                            break;
                        case AvaloniaLine line:
                            line.EndPoint = e.GetPosition(Canvas);
                            
                            break;
                        case AvaloniaRectangle rectangle:
                            Canvas.SetPositionAndSize(rectangle, point.Position, _startPoint);
                            
                            break;
                        case AvaloniaEllipse ellipse:
                            Canvas.SetPositionAndSize(ellipse, point.Position, _startPoint);

                            break;
                    }

                    break;
                }
            }
        }
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
        
        Canvas.AddToPosition(textBlock, Width * 0.85, Height * 0.15);
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
        var rectangle = new AvaloniaRectangle
        {
            Fill = SolidColorBrush.Parse(SelectedLineColor)
        };

        AssignDraggingToShape(rectangle);
        
        return rectangle;
    }
    
    private AvaloniaEllipse CreateEllipse()
    {
        var ellipse = new AvaloniaEllipse
        {
            Fill = SolidColorBrush.Parse(SelectedLineColor)
        };
        
        AssignDraggingToShape(ellipse);
        
        return ellipse;
    }
    
    private void TextBlockOnPointerMoved(object? sender, PointerEventArgs e)
    {
        var textBlock = sender as TextBlock;
        
        if (textBlock is null)
            return;

        if (_dragPosition is null)
            return;
        
        var point = e.GetCurrentPoint(Canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            Canvas.SetPosition(textBlock,
                point.Position.X - _dragPosition.Value.X,
                point.Position.Y - _dragPosition.Value.Y);
        }
    }

    private void TextBlockOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragPosition = null;
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
            _dragPosition = e.GetPosition(textBlock);
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
    
    private void SetToolbarItems()
    {
        ToolbarItems =
        [
            new DrawingToolbarItem
            {
                Id = "item-pen",
                ShortcutText = "1",
                IconPath = "/Assets/pen.svg",
                ToolTip = "Pen",
                ShortcutKey = Key.D1,
                CanBeActive = true
            },
            new DrawingToolbarItem
            {
                Id = "item-shape",
                ShortcutText = "2",
                IconPath = "/Assets/square.svg",
                Name = "Rectangle",
                ToolTip = SelectedShapeToolTip,
                ShortcutKey = Key.D2,
                CanBeActive = true,
                SubItems = 
                    [
                        new DrawingToolbarItem
                        {
                            Id = "sub-item-line",
                            Name = "Line",
                            Text = "Line",
                            IconPath = "/Assets/line.svg",
                            CanBeActive = true
                        },
                        new DrawingToolbarItem
                        {
                            Id = "sub-item-rectangle",
                            Name = "Rectangle",
                            Text = "Rectangle",
                            IconPath = "/Assets/square.svg",
                            CanBeActive = true
                        },
                        new DrawingToolbarItem
                        {
                            Id = "sub-item-ellipse",
                            Name = "Ellipse",
                            Text = "Ellipse",
                            IconPath = "/Assets/circle.svg",
                            CanBeActive = true
                        },
                    ]
            },
            new DrawingToolbarItem
            {
                Id = "item-eraser",
                ShortcutText = "3",
                IconPath = "/Assets/eraser.svg",
                ToolTip = "Erase content using area selector tool",
                ShortcutKey = Key.D3,
                CanBeActive = true,
            },
            new DrawingToolbarItem
            {
                Id = "item-clear",
                ShortcutText = "4",
                IconPath = "/Assets/trash.svg",
                ToolTip = "Clear all content",
                ShortcutKey = Key.D4,
                CanBeActive = false,
            },
            new DrawingToolbarItem
            {
                Id = "item-detect-text",
                ShortcutText = "5",
                IconPath = "/Assets/detect.svg",
                ToolTip = "Detect text using area selector tool",
                ShortcutKey = Key.D5,
                CanBeActive = true,
            },
            new DrawingToolbarItem
            {
                Id = "item-add-text",
                ShortcutText = "6",
                IconPath = "/Assets/type.svg",
                ToolTip = "Add text",
                ShortcutKey = Key.D6,
                CanBeActive = true,
            },
            new DrawingToolbarItem
            {
                Id = "item-save",
                ShortcutText = "C+S",
                IconPath = "/Assets/save.svg",
                ToolTip = "Save",
                ShortcutKey = Key.S,
                CanBeActive = false
            },
            new DrawingToolbarItem
            {
                Id = "item-undo",
                ShortcutText = "C+Z",
                IconPath = "/Assets/undo.svg",
                ToolTip = "Undo",
                ShortcutKey = Key.Z,
                CanBeActive = false
            },
            new DrawingToolbarItem
            {
                Id = "item-change-monitor",
                ShortcutText = "F11",
                IconPath = "/Assets/monitor.svg",
                ToolTip = "Change monitor",
                ShortcutKey = Key.F11,
                CanBeActive = false
            },
            new DrawingToolbarItem
            {
                Id = "item-close",
                ShortcutText = "ESC",
                IconPath = "/Assets/x.svg",
                ToolTip = "Close window",
                ShortcutKey = Key.Escape,
                CanBeActive = false
            }
        ];

        foreach (var item in ToolbarItems)
        {
            item.OnClickCommand = ReactiveCommand
                .CreateFromTask(async () => await ToolbarBtnOnClick(item));

            if (item.SubItems?.Count > 0)
            {
                foreach (var subItem in item.SubItems)
                {
                    subItem.Parent = item;
                    subItem.OnClickCommand = ReactiveCommand
                        .CreateFromTask(async () => await ToolbarBtnOnClick(subItem));
                }      
            }
                
        }
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
            case AvaloniaEllipse ellipse:
                ellipse.Fill = SolidColorBrush.Parse(SelectedLineColor);
                break;
        }
    }
    
    private void SelectEraser()
    {
        DrawingState = DrawingState.Erase;
    }
    
    private void SelectPen()
    {
        _selectedShape = CreatePolyline();
        DrawingState = DrawingState.Draw;
    }
    
    private void SelectShape(string shapeName)
    {
        switch (shapeName)
        {
            case "Line":
                if (_selectedShape is not AvaloniaLine)
                    _selectedShape = CreateLine();
                
                break;
            case "Rectangle":
                if (_selectedShape is not AvaloniaRectangle)
                    _selectedShape = CreateRectangle();
                
                break;
            case "Ellipse":
                if (_selectedShape is not AvaloniaEllipse)
                    _selectedShape = CreateEllipse();
                
                break;
        }
        
        DrawingState = DrawingState.Draw;
    }

    private void SelectDetectText()
    {
        DrawingState = DrawingState.DetectText;
    }

    private void SelectAddText()
    {
        DrawingState = DrawingState.AddText;
    }
    
    private void OnActivated(object? sender, EventArgs e)
    {
        IsPopupOpen = true;
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        IsPopupOpen = false;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        CanvasHelpers.SaveCanvasToFile(Canvas, _configuration["CanvasFilePath"]);
    }

    private void Canvas_OnInitialized(object? sender, EventArgs e)
    {
        if (sender is not Canvas canvas)
            return;

        var canvasFilePath = _configuration["CanvasFilePath"] ?? "";
        
        CanvasHelpers.LoadCanvasFromFile(canvas, canvasFilePath, _logger);
    }

    private async Task ToolbarBtnOnClick(DrawingToolbarItem toolbarItem)
    {
        SetActiveItem(toolbarItem);
        
        switch (toolbarItem.Id)
        {
            case "item-pen":
                SelectPen();
                break;
            case "item-shape":
                SelectShape(toolbarItem.Name);
                break;
            case "item-eraser":
                SelectEraser();
                break;
            case "item-clear":
                Canvas.ClearAll();
                break;
            case "item-detect-text":
                SelectDetectText();
                break;
            case "item-add-text":
                SelectAddText();
                break;
            case "item-save":
                await CaptureWindow();
                break;
            case "item-undo":
                Undo();
                break;
            case "item-close":
                Close();
                break;
            case "sub-item-line":
            case "sub-item-rectangle":
            case "sub-item-ellipse":
                SelectShape(toolbarItem.Name);
                break;
            case "item-change-monitor":
                ChangeMonitor();
                break;
        }
    }

    private void ChangeMonitor()
    {
        var currentScreen = Screens.ScreenFromWindow(this);
        var targetScreen = Screens.All.FirstOrDefault(x => x != currentScreen);

        if (targetScreen is null)
            return;
        
        Canvas.ClearAll();
        
        var rect = targetScreen.WorkingArea;
        
        WindowState = WindowState.Normal;
        CanResize = true;
        Position = new PixelPoint(rect.X, rect.Y);
        Width = rect.Width;
        Height = rect.Height;

        WindowState = WindowState.Maximized;
        CanResize = false;
    }

    private void SetActiveItem(DrawingToolbarItem toolbarItem)
    {
        if (!toolbarItem.CanBeActive)
            return;
        
        foreach (var item in ToolbarItems)
        {
            if (item.IsActive)
                item.IsActive = false;
        }

        if (toolbarItem.Parent != null)
        {
            toolbarItem.Parent.IsActive = true;
            toolbarItem.Parent.IconPath = toolbarItem.IconPath;
            toolbarItem.Parent.Name = toolbarItem.Name;
        }
        else
        {
            toolbarItem.IsActive = toolbarItem.CanBeActive;
        }
        
    }
    
    private void TriggerToolbarItemOnClick(Key key)
    {
        ToolbarItems.FirstOrDefault(x => x.ShortcutKey == key)
            ?.OnClickCommand
            .Execute(null);
    }
    
    private void AssignDraggingToShape(Shape shape)
    {
        shape.PointerPressed += ShapeOnPointerPressed;
        shape.PointerReleased += ShapeOnPointerReleased;
        shape.PointerMoved += ShapeOnPointerMoved;
    }
    
    private void ShapeOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Shape shape)
            return;
        
        var point = e.GetCurrentPoint(Canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            IsPopupOpen = false;
            _isDragging = true; 
            _dragPosition = e.GetPosition(shape);
        }
    }
    
    private void ShapeOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _dragPosition = null;
        _isDragging = false;
        IsPopupOpen = true;
        e.Handled = true;
    }
    
    private void ShapeOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (sender is not Shape shape)
            return;

        if (_dragPosition is null)
            return;
        
        var point = e.GetCurrentPoint(Canvas);

        if (point.Properties.IsLeftButtonPressed)
        {
            Canvas.SetPosition(shape,
                point.Position.X - _dragPosition.Value.X,
                point.Position.Y - _dragPosition.Value.Y);
        }
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
