using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Core;
using ScreenTools.Infrastructure;
using Point = Avalonia.Point;

namespace ScreenTools.App;

public class DrawingOverlayViewModel : ObservableObject
{
    // private readonly WindowNotificationManager _notificationManager;
    private readonly TextDetectionService _textDetectionService;
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly FilePathRepository _filePathRepository;
    private readonly DrawingHistoryService _drawingHistoryService;
    private readonly ILogger<DrawingOverlay> _logger;
    private readonly IConfiguration _configuration;

    private Thickness _windowBorderThickness;
    private double _windowWidth;
    private double _windowHeight;
    private bool _isPopupOpen;
    private Point _startPoint;
    private DrawingState _drawingState;
    private DrawingShape? _drawingShape;
    private ShapeViewModelBase? _currentShape;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private int _selectedLineStroke;
    private string _selectedLineColor;
    private ObservableCollection<DrawingToolbarItem> _toolbarItems;
    private RectangleViewModel? _eraseArea;
    private RectangleViewModel? _textDetectionArea;
    // private Rectangle? _copyShapesArea;
    // private Point? _dragPosition;
    // private bool _isDragging;
    // private List<Control>? _itemsToCopy;

    
    public DrawingOverlayViewModel(TextDetectionService textDetectionService,
        ScreenCaptureService screenCaptureService,
        FilePathRepository filePathRepository,
        DrawingHistoryService drawingHistoryService,
        ILogger<DrawingOverlay> logger,
        IConfiguration configuration)
    {
        // _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        _textDetectionService = textDetectionService;
        _screenCaptureService = screenCaptureService;
        _filePathRepository = filePathRepository;
        _drawingHistoryService = drawingHistoryService;
        _logger = logger;
        _configuration = configuration;

        SetToolbarItems();
        _drawingShape = DrawingShape.Rectangle;
        LineStrokes = [2, 5, 10, 15, 20];
        SelectedLineStroke = LineStrokes.First();
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff", "#47d147"];
        SelectedLineColor = LineColors.First();
        DrawingState = DrawingState.Draw;
        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        SelectPen();
        
        Shapes.Add(new RectangleViewModel { X = 50, Y = 50, Width = 50, Height = 50, Fill = "Red" });
        Shapes.Add(new RectangleViewModel { X = 100, Y = 100, Width = 50, Height = 50, Fill = "Blue" });
        Shapes.Add(new RectangleViewModel { X = 150, Y = 150, Width = 50, Height = 50, Fill = "Green" });
    }
    
    public double WindowWidth
    {
        get => _windowWidth;
        set => SetProperty(ref _windowWidth, value);
    }
    
    public double WindowHeight
    {
        get => _windowHeight;
        set => SetProperty(ref _windowHeight, value);
    }
    
    public bool IsPopupOpen
    {
        get => _isPopupOpen;
        set => SetProperty(ref _isPopupOpen, value);
    }
    
    public Thickness WindowBorderThickness
    {
        get => _windowBorderThickness;
        set => SetProperty(ref _windowBorderThickness, value);
    }
    
    public DrawingState DrawingState
    {
        get => _drawingState;
        set
        {
            // OnPropertyChanged(nameof(SelectedShapeToolTip));
            SetProperty(ref _drawingState, value);
        }
    }
    
    public ObservableCollection<int> LineStrokes
    {
        get => _lineStrokes;
        set => SetProperty(ref _lineStrokes, value);
    }
    
    public int SelectedLineStroke
    {
        get => _selectedLineStroke;
        set => SetProperty(ref _selectedLineStroke, value);
    }
    
    
    public ObservableCollection<string> LineColors
    {
        get => _lineColors;
        set => SetProperty(ref _lineColors, value);
    }
    
    public string SelectedLineColor
    {
        get => _selectedLineColor;
        set => SetProperty(ref _selectedLineColor, value);
    }
    
    public ObservableCollection<DrawingToolbarItem> ToolbarItems
    {
        get => _toolbarItems;
        set => SetProperty(ref _toolbarItems, value);
    }

    public string SelectedShapeToolTip => "Test";
        // _selectedShape switch
        // {
        //     Line => "Line",
        //     Rectangle => "Rectangle",
        //     Ellipse => "Ellipse",
        //     _ => "Shape not selected"
        // };
    
    public ObservableCollection<ShapeViewModelBase> Shapes { get; } = new();
    
    public async Task HandleOnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1:
            case Key.D2:
            case Key.D3:
            case Key.D4:
            case Key.D5:
            case Key.D6:
            case Key.D7:
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
                // await PasteLastItemFromClipboard();
                break;
        }
    }
    
    public void OnPointerPressed(PointerPoint pointerPoint)
    {
        // if (_isDragging)
        //     return;
    
        // if (pointerPoint.Properties.IsRightButtonPressed)
        // {
        //     HandleCanvasOnRightMouseButtonPressed(e.GetPosition(Canvas));
        // }
        
        if (!pointerPoint.Properties.IsLeftButtonPressed)
            return;
        
        IsPopupOpen = false;
        _startPoint = pointerPoint.Position;
        
        switch (DrawingState)
        {
            case DrawingState.Erase:
                _eraseArea = new RectangleViewModel
                {
                    X = _startPoint.X,
                    Y = _startPoint.Y,
                    Height = 1,
                    Width = 1,
                    Stroke = "Red",
                    StrokeThickness = 1
                };
                
                Shapes.Add(_eraseArea);
                break;
            case DrawingState.DetectText:
                _textDetectionArea = new RectangleViewModel
                {
                    X = _startPoint.X,
                    Y = _startPoint.Y,
                    Height = 1,
                    Width = 1,
                    Stroke = "LightBlue",
                    StrokeThickness = 1
                };
                
                Shapes.Add(_textDetectionArea);
                break;
            case DrawingState.Draw:
            {
                switch (_drawingShape)
                {
                        case DrawingShape.Polyline:
                            _currentShape = new PolylineViewModel
                            {
                                Stroke = SelectedLineColor,
                                StrokeThickness = SelectedLineStroke,
                                StrokeJoin = PenLineJoin.Miter,
                                StrokeLineCap = PenLineCap.Round
                            };
                            
                            Shapes.Add(_currentShape);
                            
                            break;
                    case DrawingShape.Line:
                        _currentShape = new LineViewModel
                        {
                            Stroke = SelectedLineColor,
                            StrokeThickness = SelectedLineStroke,
                            StartPoint = _startPoint,
                            EndPoint = _startPoint
                        };
                       
                        Shapes.Add(_currentShape);
                        break;
                    case DrawingShape.Rectangle:
                        _currentShape = new RectangleViewModel
                        {
                            X = _startPoint.X,
                            Y = _startPoint.Y,
                            Height = 1,
                            Width = 1,
                            Fill = SelectedLineColor,
                        };
                        
                        Shapes.Add(_currentShape);
                        break;
                    case DrawingShape.Ellipse:
                        _currentShape = new EllipseViewModel
                        {
                            X = _startPoint.X,
                            Y = _startPoint.Y,
                            Height = 1,
                            Width = 1,
                            Fill = SelectedLineColor,
                        };
                        
                        Shapes.Add(_currentShape);
                        break;
                }
        
                break;
            }
            case DrawingState.AddText:
                // var flyout = new Flyout();
                // var textBox = new TextBox
                // {
                //     Width = 220,
                //     Height = 30
                // };
                // flyout.Content = textBox;
                // flyout.Closed += (_, _) =>
                // {
                //     var text = textBox.Text;
                //
                //     if (string.IsNullOrEmpty(text))
                //         return;
                //
                //     var textBlock = new TextBlock
                //     {
                //         Text = text,
                //         FontSize = 20,
                //         Foreground = new SolidColorBrush(Colors.Black),
                //         Background = new SolidColorBrush(Colors.Transparent)
                //     };
                //
                //     Canvas.AddToPosition(textBlock, _startPoint.X, _startPoint.Y);
                // };
                // flyout.ShowAt(Canvas, true);
                break;
            case DrawingState.CopyShapes:
                // if (_copyShapesArea is null)
                // {
                //     _copyShapesArea = new Rectangle()
                //     {
                //         Stroke = new SolidColorBrush(Colors.Blue),
                //         StrokeThickness = 1,
                //         Name = "CopyShapes"
                //     };
                //     
                //     Canvas.AddToPosition(_copyShapesArea, _startPoint);
                // }
                break;
        }
    }
    
    public void OnPointerMoved(PointerPoint pointerPoint)
    {
        // if (_isDragging)
        //     return;
        
        if (pointerPoint.Properties.IsLeftButtonPressed)
        {
            switch (DrawingState)
            {
                case DrawingState.Erase:
                {
                    if (_eraseArea is null)
                        return;

                    CanvasHelpers.SetRectanglePosAndSize(_eraseArea, pointerPoint.Position, _startPoint);
                    
                    break;
                }
                case DrawingState.DetectText:
                {
                    if (_textDetectionArea is null)
                        return;

                    CanvasHelpers.SetRectanglePosAndSize(_textDetectionArea, pointerPoint.Position, _startPoint);
                    break;
                }
                case DrawingState.Draw:
                {
                    switch (_drawingShape)
                    {
                        case DrawingShape.Polyline:
                            if (_currentShape is not PolylineViewModel polylineViewModel)
                                return;
                            
                            polylineViewModel.Points.Add(pointerPoint.Position);
                            break;
                        case DrawingShape.Line:
                            if (_currentShape is not LineViewModel lineViewModel)
                                return;

                            lineViewModel.EndPoint = pointerPoint.Position;
                            
                            break;
                        case DrawingShape.Rectangle:
                            if (_currentShape is not RectangleViewModel rectangleViewModel)
                                return;

                            CanvasHelpers.SetRectanglePosAndSize(rectangleViewModel, pointerPoint.Position, _startPoint);
                            break;
                        case DrawingShape.Ellipse:
                            if (_currentShape is not EllipseViewModel ellipseViewModel)
                                return;
                            
                            CanvasHelpers.SetRectanglePosAndSize(ellipseViewModel, pointerPoint.Position, _startPoint);
                            break;
                    }
                    break;
                }
                // case DrawingState.CopyShapes:
                // {
                //     if (_copyShapesArea is null)
                //         return;
                //     
                //     Canvas.SetPositionAndSize(_copyShapesArea, point.Position, _startPoint);
                //     
                //     break;
                // }
            }
        }
    }
     
    public void OnPointerReleased(PointerPoint pointerPoint)
    {
        // if (_isDragging)
        //     return;
        
        try
        {
            switch (DrawingState)
            {
                case DrawingState.Erase:
                    if (_eraseArea is null)
                        return;
                    
                    CanvasHelpers.RemoveByArea(Shapes, _eraseArea,_drawingHistoryService);
                    Shapes.Remove(_eraseArea);
                    _eraseArea = null;
                    
                    break;
                case DrawingState.DetectText:
                    if (_textDetectionArea is null)
                        return;
                
                    var startX = _textDetectionArea.X;
                    var startY = _textDetectionArea.Y;
                    var width = _textDetectionArea.Width;
                    var height = _textDetectionArea.Height;
                
                    Shapes.Remove(_textDetectionArea);
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
                        // _notificationManager.Show(new Notification(
                        //     "Information",
                        //     "Text could not be detected."));
                        
                        return;
                    }
                
                    AddTextToCanvas(text);
                
                    break;
                case DrawingState.Draw:
                    // _drawingHistoryService.Save([_selectedShape], DrawingAction.Draw);
                    
                    _currentShape = null;
                    
                    break;
                // case DrawingState.CopyShapes:
                //     if (_copyShapesArea is null)
                //         return;
                //     
                //     var items = Canvas.GetItemsByArea(_copyShapesArea);
                //
                //     if (items.Count == 0)
                //     {
                //         Canvas.Children.Remove(_copyShapesArea);
                //         _copyShapesArea = null;
                //
                //         return;
                //     }
                //     
                //     _itemsToCopy = items;
                //     _startPoint = new Point(_copyShapesArea.Bounds.X, _copyShapesArea.Bounds.Y);
                //
                //     Canvas.Children.Remove(_copyShapesArea);
                //     _copyShapesArea = null;
                //     break;
            }
        }
        catch (ArgumentException argEx) when (argEx.Message.Contains("TextDetectError"))
        {
            // _notificationManager.Show(new Notification(
            //     "Error",
            //     argEx.Message[argEx.Message.IndexOf(':').. + 2],
            //     NotificationType.Error));
            // _logger.LogError($"Failed to detect text. Exception: {argEx.Message}");
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
    
    public void OnWindowActivated()
    {
        IsPopupOpen = true;
    }
    
    public void OnWindowDeactivated()
    {
        IsPopupOpen = false;
    }
    
    public void OnWindowHidden()
    {
        // CanvasHelpers.SaveCanvasToFile(Canvas, _configuration["CanvasFilePath"]);
    }

    private void HideWindow()
    {
        WeakReferenceMessenger.Default.Send(new HideWindowMessage(true));
    }
    
    private void TriggerToolbarItemOnClick(Key key)
    {
        ToolbarItems.FirstOrDefault(x => x.ShortcutKey == key)
            ?.OnClickCommand
            .Execute(null);
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
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-pen", SelectPen))
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
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-shape", () => SelectShape("Rectangle"))),
                SubItems = 
                    [
                        new DrawingToolbarItem
                        {
                            Id = "sub-item-line",
                            Name = "Line",
                            Text = "Line",
                            IconPath = "/Assets/line.svg",
                            CanBeActive = true,
                            OnClickCommand = ReactiveCommand.Create(() => SelectItem("sub-item-line", () => SelectShape("Line")))
                        },
                        new DrawingToolbarItem
                        {
                            Id = "sub-item-rectangle",
                            Name = "Rectangle",
                            Text = "Rectangle",
                            IconPath = "/Assets/square.svg",
                            CanBeActive = true,
                            OnClickCommand = ReactiveCommand.Create(() => SelectItem("sub-item-rectangle", () => SelectShape("Rectangle")))
                        },
                        new DrawingToolbarItem
                        {
                            Id = "sub-item-ellipse",
                            Name = "Ellipse",
                            Text = "Ellipse",
                            IconPath = "/Assets/circle.svg",
                            CanBeActive = true,
                            OnClickCommand = ReactiveCommand.Create(() => SelectItem("sub-item-ellipse", () => SelectShape("Ellipse")))
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
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-eraser", SelectEraser))
            },
            new DrawingToolbarItem
            {
                Id = "item-clear",
                ShortcutText = "4",
                IconPath = "/Assets/trash.svg",
                ToolTip = "Clear all content",
                ShortcutKey = Key.D4,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-clear", ClearCanvas))
            },
            new DrawingToolbarItem
            {
                Id = "item-detect-text",
                ShortcutText = "5",
                IconPath = "/Assets/detect.svg",
                ToolTip = "Detect text using area selector tool",
                ShortcutKey = Key.D5,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-detect-text", SelectDetectText))
            },
            new DrawingToolbarItem
            {
                Id = "item-add-text",
                ShortcutText = "6",
                IconPath = "/Assets/type.svg",
                ToolTip = "Add text",
                ShortcutKey = Key.D6,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-add-text", SelectAddText))
            },
            new DrawingToolbarItem
            {
                Id = "item-copy-shapes",
                ShortcutText = "7",
                IconPath = "/Assets/copy.svg",
                ToolTip = "Copy selected shapes",
                ShortcutKey = Key.D7,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(() => SelectItem("item-copy-shapes", SelectCopyShapes))
            },
            new DrawingToolbarItem
            {
                Id = "item-save",
                ShortcutText = "C+S",
                IconPath = "/Assets/save.svg",
                ToolTip = "Save",
                ShortcutKey = Key.S,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.CreateFromTask(CaptureWindow)
            },
            new DrawingToolbarItem
            {
                Id = "item-undo",
                ShortcutText = "C+Z",
                IconPath = "/Assets/undo.svg",
                ToolTip = "Undo",
                ShortcutKey = Key.Z,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(Undo)
            },
            new DrawingToolbarItem
            {
                Id = "item-change-monitor",
                ShortcutText = "F11",
                IconPath = "/Assets/monitor.svg",
                ToolTip = "Change monitor",
                ShortcutKey = Key.F11,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(ChangeMonitor)
            },
            new DrawingToolbarItem
            {
                Id = "item-close",
                ShortcutText = "ESC",
                IconPath = "/Assets/x.svg",
                ToolTip = "Close window",
                ShortcutKey = Key.Escape,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(HideWindow)
            }
        ];
    }

    private void SelectItem(string toolbarItemId, Action action)
    {
        DrawingToolbarItem? toolbarItem;

        if (toolbarItemId.Contains("sub-item"))
        {
            toolbarItem = ToolbarItems
                .Where(x => x.SubItems != null)
                .SelectMany(x => x.SubItems)
                .FirstOrDefault(x => x.Id == toolbarItemId);
        }
        else
        {
            toolbarItem = ToolbarItems.FirstOrDefault(x => x.Id == toolbarItemId);
        }

        if (toolbarItem is null)
            return;
        
        SetActiveItem(toolbarItem);
        action.Invoke();
    }
    
    private void SelectPen()
    {
        _drawingShape = DrawingShape.Polyline;
        DrawingState = DrawingState.Draw;
    }
    
    private void SelectShape(string shapeName)
    {
        switch (shapeName)
        {
            case "Line":
                _drawingShape = DrawingShape.Line;
                
                break;
            case "Rectangle":
                _drawingShape = DrawingShape.Rectangle;
                
                break;
            case "Ellipse":
                _drawingShape = DrawingShape.Ellipse;
                
                break;
        }
        
        DrawingState = DrawingState.Draw;
    }
    
    private void SelectEraser()
    {
        DrawingState = DrawingState.Erase;
    }

    private void ClearCanvas()
    {
        Shapes.Clear();
    }
    
    private void SelectDetectText()
    {
        DrawingState = DrawingState.DetectText;
    }
    
    private void SelectAddText()
    {
        DrawingState = DrawingState.AddText;
    }
    
    private void SelectCopyShapes()
    {
        DrawingState = DrawingState.CopyShapes;
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
            
            // var imageSavePath = SystemIOPath.Combine(filePath.Path, $"Screenshot-{DateTime.Now:dd-MM-yyyy-hhmmss}.png");
            // var bmp = _screenCaptureService.CaptureVisibleWindow(Width, Height, Position.X, Position.Y);
            //
            // bmp.Save(imageSavePath, ImageFormat.Png);
            // _notificationManager.Show(new Notification(
            //     "Screenshot captured!",
            //     "Click to show image in explorer.",
            //     NotificationType.Success,
            //     null,
            //     () => OnNotificationClick(imageSavePath)));
        }
        catch (Exception ex)
        {
            // _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            _logger.LogError($"Failed to capture window. Exception: {ex}");
        }
        finally
        {
            IsPopupOpen = true;
            WindowBorderThickness = new Thickness(2); 
        }
    }
    
    private void Undo()
    {
        // _drawingHistoryService.Undo(Canvas);
    }
    
    private void ChangeMonitor()
    {
        // var currentScreen = Screens.ScreenFromWindow(this);
        // var targetScreen = Screens.All.FirstOrDefault(x => x != currentScreen);
        //
        // if (targetScreen is null)
        //     return;
        //
        // Canvas.ClearAll();
        //
        // var rect = targetScreen.WorkingArea;
        //
        // WindowState = WindowState.Normal;
        // CanResize = true;
        // Position = new PixelPoint(rect.X, rect.Y);
        // Width = rect.Width;
        // Height = rect.Height;
        //
        // WindowState = WindowState.Maximized;
        // CanResize = false;
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
    
    private void AddTextToCanvas(string text)
    {
        var textBlock = new TextBlockViewModel
        {
            Text = text,
            FontSize = 20,
            Foreground = "Black",
            Background = "Transparent",
            X = WindowWidth * 0.85,
            Y = WindowHeight * 0.15,
        };
        
        Shapes.Add(textBlock);
        
        // textBlock.PointerPressed += TextBlockOnPointerPressed;
        // textBlock.PointerReleased += TextBlockOnPointerReleased;
        // textBlock.PointerMoved += TextBlockOnPointerMoved;
        
        // Canvas.AddToPosition(textBlock, Width * 0.85, Height * 0.15);
    }
}