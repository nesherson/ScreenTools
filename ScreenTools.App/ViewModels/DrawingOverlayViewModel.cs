using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Core;
using ScreenTools.Infrastructure;
using Point = Avalonia.Point;
using SystemIOPath = System.IO.Path;

namespace ScreenTools.App;

public class DrawingOverlayViewModel : ViewModelBase
{
    private readonly TextDetectionService _textDetectionService;
    private readonly ScreenCaptureService _screenCaptureService;
    private readonly FilePathRepository _filePathRepository;
    private readonly DrawingHistoryService _drawingHistoryService;
    private readonly ILogger<DrawingOverlay> _logger;
    private readonly IConfiguration _configuration;

    private Thickness _windowBorderThickness;
    private bool _isPopupOpen;
    private Point _startPoint;
    private DrawingState _drawingState;
    private ShapeType? _drawingShape;
    private ShapeViewModelBase? _currentShape;
    private ObservableCollection<int> _lineStrokes;
    private ObservableCollection<string> _lineColors;
    private int _selectedLineStroke;
    private string _selectedLineColor;
    private ObservableCollection<DrawingToolbarItemViewModel> _toolbarItems;
    private RectangleViewModel? _eraseArea;
    private RectangleViewModel? _textDetectionArea;
    private RectangleViewModel? _copyShapesArea;
    private Point? _dragPosition;
    private bool _isDragging;
    private List<ShapeViewModelBase>? _itemsToCopy;


    public DrawingOverlayViewModel(TextDetectionService textDetectionService,
        ScreenCaptureService screenCaptureService,
        FilePathRepository filePathRepository,
        DrawingHistoryService drawingHistoryService,
        ILogger<DrawingOverlay> logger,
        IConfiguration configuration)
    {
        _textDetectionService = textDetectionService;
        _screenCaptureService = screenCaptureService;
        _filePathRepository = filePathRepository;
        _drawingHistoryService = drawingHistoryService;
        _logger = logger;
        _configuration = configuration;

        _drawingShape = ShapeType.Rectangle;
        LineStrokes = [2, 5, 10, 15, 20];
        SelectedLineStroke = LineStrokes.First();
        LineColors = ["#000000", "#ff0000", "#ffffff", "#3399ff", "#47d147"];
        SelectedLineColor = LineColors.First();
        DrawingState = DrawingState.Draw;
        IsPopupOpen = true;
        WindowBorderThickness = new Thickness(2);
        SetToolbarItems();
        SelectPen();
        
        Shapes = CanvasHelpers.LoadShapesFromFile(
            _configuration["CanvasFilePath"] ?? "",
            _logger)
            .ToObservable();
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
            OnPropertyChanged(nameof(SelectedShapeToolTip));
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

    public ObservableCollection<DrawingToolbarItemViewModel> ToolbarItems
    {
        get => _toolbarItems;
        set => SetProperty(ref _toolbarItems, value);
    }

    public string SelectedShapeToolTip =>
        _drawingShape switch
        {
            ShapeType.Line => "Line",
            ShapeType.Rectangle => "Rectangle",
            _ => "Shape not selected"
        };

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
                await PasteLastItemFromClipboard();
                break;
        }
    }

    public void HandleOnPointerWheelChanged(PointerWheelEventArgs e)
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

    public void HandleOnRightMouseButtonPressed(PointerPoint pointerPoint)
    {
        WeakReferenceMessenger.Default
            .Send(new ShowContextMenuMessage(
                new ShowContextMenuMessageContent
                {
                    IsPasteEnabled = _itemsToCopy?.Count > 0,
                    OnPaste = async () => await OnPaste(pointerPoint)
                }));
    }

    private async Task OnPaste(PointerPoint pointerPoint)
    {
        if (_itemsToCopy != null && _itemsToCopy.Count == 0)
            return;

        foreach (var itemToCopy in _itemsToCopy)
        {
            var windowSize = await WeakReferenceMessenger.Default
                .Send(new GetWindowSizeMessage());

            CanvasHelpers.CopyShapeToPosition(Shapes,
                itemToCopy,
                new Point(pointerPoint.Position.X, pointerPoint.Position.Y),
                new Point(_startPoint.X, _startPoint.Y),
                windowSize.Width,
                windowSize.Height);
        }
    }

    public void OnPointerPressed(PointerPoint pointerPoint)
    {
        if (_isDragging)
            return;

        if (pointerPoint.Properties.IsRightButtonPressed)
        {
            HandleOnRightMouseButtonPressed(pointerPoint);
        }

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
                    case ShapeType.Polyline:
                        _currentShape = new PolylineViewModel
                        {
                            Stroke = SelectedLineColor,
                            StrokeThickness = SelectedLineStroke,
                            StrokeJoin = PenLineJoin.Miter,
                            StrokeLineCap = PenLineCap.Round
                        };

                        Shapes.Add(_currentShape);

                        break;
                    case ShapeType.Line:
                        _currentShape = new LineViewModel
                        {
                            Stroke = SelectedLineColor,
                            StrokeThickness = SelectedLineStroke,
                            StartPoint = _startPoint,
                            EndPoint = _startPoint
                        };

                        Shapes.Add(_currentShape);
                        break;
                    case ShapeType.Rectangle:
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
                }

                break;
            }
            case DrawingState.AddText:
                var content = new ShowTextBoxMessageContent
                {
                    OnClosed = text =>
                    {
                        if (string.IsNullOrEmpty(text))
                            return;

                        var textBlockViewModel = new TextBlockViewModel
                        {
                            Text = text,
                            FontSize = 20,
                            Foreground = "Black",
                            Background = "Transparent",
                            X = _startPoint.X,
                            Y = _startPoint.Y
                        };

                        Shapes.Add(textBlockViewModel);
                    }
                };


                WeakReferenceMessenger.Default
                    .Send(new ShowTextBoxMessage(content));
                break;
            case DrawingState.CopyShapes:
                if (_copyShapesArea is null)
                {
                    _copyShapesArea = new RectangleViewModel
                    {
                        X = _startPoint.X,
                        Y = _startPoint.Y,
                        Height = 1,
                        Width = 1,
                        Stroke = "Purple",
                        StrokeThickness = 1
                    };

                    Shapes.Add(_copyShapesArea);
                }

                break;
        }
    }

    public void OnPointerMoved(PointerPoint pointerPoint)
    {
        if (_isDragging)
            return;

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
                        case ShapeType.Polyline:
                            if (_currentShape is not PolylineViewModel polylineViewModel)
                                return;

                            polylineViewModel.Points.Add(pointerPoint.Position);
                            break;
                        case ShapeType.Line:
                            if (_currentShape is not LineViewModel lineViewModel)
                                return;

                            lineViewModel.EndPoint = pointerPoint.Position;

                            break;
                        case ShapeType.Rectangle:
                            if (_currentShape is not RectangleViewModel rectangleViewModel)
                                return;

                            CanvasHelpers.SetRectanglePosAndSize(rectangleViewModel, pointerPoint.Position,
                                _startPoint);
                            break;
                    }

                    break;
                }
                case DrawingState.CopyShapes:
                {
                    if (_copyShapesArea is null)
                        return;

                    CanvasHelpers.SetRectanglePosAndSize(_copyShapesArea, pointerPoint.Position, _startPoint);

                    break;
                }
            }
        }
    }

    public void OnPointerReleased()
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

                    CanvasHelpers.RemoveByArea(Shapes, _eraseArea, _drawingHistoryService);
                    Shapes.Remove(_eraseArea);
                    _eraseArea = null;

                    break;
                case DrawingState.DetectText:
                    if (_textDetectionArea is null)
                        return;
                    
                    Shapes.Remove(_textDetectionArea);
                    
                    var text = _textDetectionService
                        .DetectText(_textDetectionArea.X, 
                            _textDetectionArea.Y, 
                            _textDetectionArea.Width, 
                            _textDetectionArea.Height);

                    if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text))
                    {
                        ShowWindowNotifcation("Information",
                            "Text could not be detected",
                            NotificationType.Information);

                        return;
                    }

                    AddTextToCanvas(text);
                    
                    _textDetectionArea = null;

                    break;
                case DrawingState.Draw:
                    if (_currentShape is not null)
                    {
                        _drawingHistoryService.Save([_currentShape], DrawingAction.Draw);

                        _currentShape = null;
                    }
                    
                    break;
                case DrawingState.CopyShapes:
                    if (_copyShapesArea is null)
                        return;

                    var itemsToCopy = Shapes
                        .Where(x => CanvasHelpers.IsInArea(x, _copyShapesArea) && x != _copyShapesArea)
                        .ToList();

                    if (itemsToCopy.Count == 0)
                    {
                        Shapes.Remove(_copyShapesArea);
                        _copyShapesArea = null;

                        return;
                    }

                    _itemsToCopy = itemsToCopy;
                    _startPoint = new Point(_copyShapesArea.X, _copyShapesArea.Y);

                    Shapes.Remove(_copyShapesArea);
                    _copyShapesArea = null;

                    break;
            }
        }
        catch (ArgumentException argEx) when (argEx.Message.Contains("TextDetectError"))
        {
            ShowWindowNotifcation("Error",
                argEx.Message[argEx.Message.IndexOf(':').. +2],
                NotificationType.Error);
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
        CanvasHelpers.SaveCanvasToFile(Shapes, _configuration["CanvasFilePath"]);
    }

    private void HideWindow()
    {
        WeakReferenceMessenger.Default
            .Send(new DrawingOverlayMessage(DrawingOverlayMessageType.HideWindow));
    }

    private async Task PasteLastItemFromClipboard()
    {
        var text = await WeakReferenceMessenger.Default
            .Send(new PasteLastItemFromClipboardMessage());

        if (string.IsNullOrEmpty(text))
            return;

        AddTextToCanvas(text);
    }

    private void TriggerToolbarItemOnClick(Key key)
    {
        ToolbarItems.FirstOrDefault(x => x.ShortcutKey == key)
            ?.OnClickCommand
            .Execute(null);
    }

    private void SetToolbarItems()
    {
        var toolbarItems = new List<DrawingToolbarItemViewModel>()
        {
            new()
            {
                Type = ToolbarItemType.PenDrawing,
                ShortcutText = "1",
                IconPath = "/Assets/pen.svg",
                ToolTip = "Pen",
                ShortcutKey = Key.D1,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(SelectPen),
                Order = 1
            },
            new()
            {
                Type = ToolbarItemType.ShapeDrawing,
                ShortcutText = "2",
                IconPath = "/Assets/square.svg",
                Name = "Rectangle",
                ToolTip = SelectedShapeToolTip,
                ShortcutKey = Key.D2,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(() => SelectShape(ShapeType.Rectangle)),
                Order = 2,
                SubItems =
                [
                    new DrawingToolbarItemViewModel
                    {
                        Type = ToolbarItemType.ShapeDrawing,
                        Name = "Line",
                        Text = "Line",
                        IconPath = "/Assets/line.svg",
                        CanBeActive = true,
                        OnClickCommand = ReactiveCommand.Create(() => SelectShape(ShapeType.Line))
                    },
                    new DrawingToolbarItemViewModel
                    {
                        Type = ToolbarItemType.ShapeDrawing,
                        Name = "Rectangle",
                        Text = "Rectangle",
                        IconPath = "/Assets/square.svg",
                        CanBeActive = true,
                        OnClickCommand = ReactiveCommand.Create(() => SelectShape(ShapeType.Rectangle))
                    }
                ]
            },
            new()
            {
                Type = ToolbarItemType.Eraser,
                ShortcutText = "3",
                IconPath = "/Assets/eraser.svg",
                ToolTip = "Erase content using area selector tool",
                ShortcutKey = Key.D3,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(SelectEraser),
                Order = 3
            },
            new()
            {
                Type = ToolbarItemType.ClearAll,
                ShortcutText = "4",
                IconPath = "/Assets/trash.svg",
                ToolTip = "Clear all content",
                ShortcutKey = Key.D4,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(ClearCanvas),
                Order = 4
            },
            new()
            {
                Type = ToolbarItemType.DetectText,
                ShortcutText = "5",
                IconPath = "/Assets/detect.svg",
                ToolTip = "Detect text using area selector tool",
                ShortcutKey = Key.D5,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(SelectDetectText),
                Order = 5
            },
            new()
            {
                Type = ToolbarItemType.AddText,
                ShortcutText = "6",
                IconPath = "/Assets/type.svg",
                ToolTip = "Add text",
                ShortcutKey = Key.D6,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(SelectAddText),
                Order = 6
            },
            new()
            {
                Type = ToolbarItemType.CopyShapes,
                ShortcutText = "7",
                IconPath = "/Assets/copy.svg",
                ToolTip = "Copy selected shapes",
                ShortcutKey = Key.D7,
                CanBeActive = true,
                OnClickCommand = ReactiveCommand.Create(SelectCopyShapes),
                Order = 7
            },
            new()
            {
                Type = ToolbarItemType.Save,
                ShortcutText = "C+S",
                IconPath = "/Assets/save.svg",
                ToolTip = "Save",
                ShortcutKey = Key.S,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.CreateFromTask(CaptureWindow),
                Order = 8
            },
            new()
            {
                Type = ToolbarItemType.Undo,
                ShortcutText = "C+Z",
                IconPath = "/Assets/undo.svg",
                ToolTip = "Undo",
                ShortcutKey = Key.Z,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(Undo),
                Order = 9
            },
            new()
            {
                Type = ToolbarItemType.Exit,
                ShortcutText = "ESC",
                IconPath = "/Assets/x.svg",
                ToolTip = "Close window",
                ShortcutKey = Key.Escape,
                CanBeActive = false,
                OnClickCommand = ReactiveCommand.Create(HideWindow),
                Order = 11
            }
        };

        var result = WeakReferenceMessenger.Default
            .Send<IsUsingMultipleMonitorsMessage>();

        // if (isUsingMultipleMonitors.Response)
        // {
        //     toolbarItems.Add(
        //         new DrawingToolbarItemViewModel
        //         {
        //             Type = ToolbarItemType.ChangeMonitor,
        //             ShortcutText = "F11",
        //             IconPath = "/Assets/monitor.svg",
        //             ToolTip = "Change monitor",
        //             ShortcutKey = Key.F11,
        //             CanBeActive = false,
        //             OnClickCommand = ReactiveCommand.Create(ChangeMonitor),
        //             Order = 10
        //         });
        // }

        ToolbarItems = toolbarItems
            .OrderBy(x => x.Order)
            .ToObservable();
    }

    private void SelectPen()
    {
        _drawingShape = ShapeType.Polyline;
        DrawingState = DrawingState.Draw;

        SetActiveItem(ToolbarItems.First(x => x.Type == ToolbarItemType.PenDrawing));
    }

    private void SelectShape(ShapeType shapeType)
    {
        switch (shapeType)
        {
            case ShapeType.Line:
                _drawingShape = ShapeType.Line;

                break;
            case ShapeType.Rectangle:
                _drawingShape = ShapeType.Rectangle;

                break;
        }

        DrawingState = DrawingState.Draw;

        SetActiveItem(ToolbarItems.First(x => x.Type == ToolbarItemType.ShapeDrawing));
    }

    private void SelectEraser()
    {
        DrawingState = DrawingState.Erase;

        SetActiveItem(ToolbarItems.First(x => x.Type == ToolbarItemType.Eraser));
    }

    private void ClearCanvas()
    {
        _drawingHistoryService?.Save(Shapes.ToList(),
            DrawingAction.Clear);
        
        Shapes.Clear();
    }

    private void SelectDetectText()
    {
        DrawingState = DrawingState.DetectText;

        SetActiveItem(ToolbarItems.First(x => x.Type == ToolbarItemType.DetectText));
    }

    private void SelectAddText()
    {
        DrawingState = DrawingState.AddText;

        SetActiveItem(ToolbarItems.First(x => x.Type == ToolbarItemType.AddText));
    }

    private void SelectCopyShapes()
    {
        DrawingState = DrawingState.CopyShapes;

        SetActiveItem(ToolbarItems.First(x => x.Type == ToolbarItemType.CopyShapes));
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

            var windowSize = await WeakReferenceMessenger.Default
                .Send(new GetWindowSizeMessage());
            var imageSavePath = SystemIOPath.Combine(filePath.Path, $"Screenshot-{DateTime.Now:dd-MM-yyyy-hhmmss}.png");
            var bmp = _screenCaptureService.CaptureVisibleWindow(windowSize.Width, windowSize.Height, 0, 0);

            bmp.Save(imageSavePath, ImageFormat.Png);
            ShowWindowNotifcation(
                "Screenshot captured!",
                "Click to show image in explorer.",
                NotificationType.Success,
                () => OnNotificationClick(imageSavePath));
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
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
        _drawingHistoryService.Undo(Shapes);
    }

    private void ChangeMonitor()
    {
        WeakReferenceMessenger.Default
            .Send(new DrawingOverlayMessage(DrawingOverlayMessageType.ChangeMonitor));
        Shapes.Clear();
    }

    private void SetActiveItem(DrawingToolbarItemViewModel toolbarItemViewModel)
    {
        if (!toolbarItemViewModel.CanBeActive)
            return;

        foreach (var item in ToolbarItems)
        {
            if (item.IsActive)
                item.IsActive = false;
        }

        if (toolbarItemViewModel.Parent != null)
        {
            toolbarItemViewModel.Parent.IsActive = true;
            toolbarItemViewModel.Parent.IconPath = toolbarItemViewModel.IconPath;
            toolbarItemViewModel.Parent.Name = toolbarItemViewModel.Name;
        }
        else
        {
            toolbarItemViewModel.IsActive = toolbarItemViewModel.CanBeActive;
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
            X = _startPoint.X,
            Y = _startPoint.Y
        };

        Shapes.Add(textBlock);
    }

    private void OnNotificationClick(string pathToImage)
    {
        ProcessHelpers.ShowFileInFileExplorer(pathToImage);
    }
}