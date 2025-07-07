using System;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace ScreenTools.App;

public partial class DrawingOverlay : Window, IViewFor<DrawingOverlayViewModel>
{
    public static readonly StyledProperty<DrawingOverlayViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<DrawingOverlay, DrawingOverlayViewModel?>(nameof(ViewModel));
    
    public DrawingOverlay(DrawingOverlayViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        
        this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        this.WhenActivated(disposables =>
        {
            this.Events()
                .KeyDown
                .Subscribe(async e => await ViewModel.HandleOnKeyDown(e))
                .DisposeWith(disposables);
        });
        
        WeakReferenceMessenger.Default.Register<HideWindowMessage>(this, HandleHideWindowMessage);
        
        ViewModel.IsPopupOpen = true;
        ViewModel.WindowBorderThickness = new Thickness(2);
        ViewModel.WindowWidth = Width;
        ViewModel.WindowHeight = Height;
        
        Hidden += (_, _) => ViewModel.OnWindowHidden();
        Deactivated += (_, _) => ViewModel.OnWindowDeactivated();
        Activated += (_, _) => ViewModel.OnWindowActivated();
    }
    
    public DrawingOverlayViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }
    
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (DrawingOverlayViewModel?)value;
    }
    
    public event EventHandler? Hidden;
    
    public override void Hide()
    {
        base.Hide();
        
        OnHidden(EventArgs.Empty);
    }
    
    private void OnHidden(EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => Hidden?.Invoke(this, e));
    }

    private void HandleHideWindowMessage(object recipient, HideWindowMessage message)
    {
        if (message.Value)
            Hide();
    }

    // protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    // {
    //     if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) || !e.KeyModifiers.HasFlag(KeyModifiers.Shift)) 
    //         return;
    //
    //     var deltaY = Convert.ToInt32(e.Delta.Y);
    //     var nextColorIndex = LineColors.IndexOf(SelectedLineColor) + deltaY;
    //     
    //     if (nextColorIndex < 0)
    //     {
    //         nextColorIndex = LineColors.Count - 1;
    //     }
    //     
    //     if (nextColorIndex >= LineColors.Count)
    //     {
    //         nextColorIndex = 0;
    //     }
    //     
    //     SelectedLineColor = LineColors[nextColorIndex];
    // }

    // private void OnNotificationClick(string pathToImage)
    // {
    //     ProcessHelpers.ShowFileInFileExplorer(pathToImage);
    // }

    // private void HandleCanvasOnRightMouseButtonPressed(Point position)
    // {
    //     var flyout = new MenuFlyout();
    //     var pasteMenuItem = new MenuItem
    //     {
    //         Header = "Paste",
    //         IsEnabled = _itemsToCopy != null
    //     };
    //
    //     pasteMenuItem.Click += (_, _) =>
    //     {
    //         if (_itemsToCopy?.Count > 0)
    //         {
    //             foreach (var itemToCopy in _itemsToCopy)
    //             {
    //                 CanvasHelpers.CopyControlToPosition(Canvas,
    //                     itemToCopy,
    //                     new Point(position.X, position.Y),
    //                     new Point(_startPoint.X, _startPoint.Y));
    //             }
    //         }
    //     };
    //     
    //     flyout.Items.Add(pasteMenuItem);
    //     
    //     flyout.ShowAt(Canvas, true);
    // }
    //
    
    //
    //
    // private async Task PasteLastItemFromClipboard()
    // {
    //     var clipboard = GetTopLevel(this)?.Clipboard;
    //
    //     if (clipboard is null)
    //         return;
    //             
    //     var text = await clipboard.GetTextAsync();
    //             
    //     if (string.IsNullOrEmpty(text))
    //         return;
    //             
    //     AddTextToCanvas(text);
    // }
    //
    

    // private void TextBlockOnPointerMoved(object? sender, PointerEventArgs e)
    // {
    //     var textBlock = sender as TextBlock;
    //     
    //     if (textBlock is null)
    //         return;
    //
    //     if (_dragPosition is null)
    //         return;
    //     
    //     var point = e.GetCurrentPoint(Canvas);
    //
    //     if (point.Properties.IsLeftButtonPressed)
    //     {
    //         Canvas.SetPosition(textBlock,
    //             point.Position.X - _dragPosition.Value.X,
    //             point.Position.Y - _dragPosition.Value.Y);
    //     }
    // }
    //
    // private void TextBlockOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    // {
    //     _dragPosition = null;
    //     _isDragging = false;
    //     IsPopupOpen = true;
    //     e.Handled = true;
    // }
    //
    // private void TextBlockOnPointerPressed(object? sender, PointerPressedEventArgs e)
    // {
    //     var textBlock = sender as TextBlock;
    //
    //     if (textBlock is null)
    //         return;
    //     
    //     var point = e.GetCurrentPoint(Canvas);
    //
    //     if (point.Properties.IsLeftButtonPressed)
    //     {
    //         IsPopupOpen = false;
    //         _isDragging = true;
    //         _dragPosition = e.GetPosition(textBlock);
    //     }
    //     else if (point.Properties.IsRightButtonPressed)
    //     {
    //         HandleTextBlockRightBtnPressed(textBlock);
    //     }
    // }
    //
    // private void HandleTextBlockRightBtnPressed(TextBlock textBlock)
    // {
    //     var flyout = new Flyout();
    //     var textBox = new TextBox
    //     {
    //         Text = textBlock.Text
    //     };
    //     flyout.Content = textBox;
    //     flyout.Closed += (_, _) =>
    //     {
    //         textBlock.Text = textBox.Text;
    //     };
    //     flyout.ShowAt(textBlock);
    // }
    //
    private void Canvas_OnInitialized(object? sender, EventArgs e)
    {
        if (sender is not Canvas canvas)
            return;

        if (ViewModel is null)
            return;
        
        canvas.PointerPressed += (_, pe) => ViewModel.OnPointerPressed(pe.GetCurrentPoint(Canvas));
        canvas.PointerMoved += (_, pe) => ViewModel.OnPointerMoved(pe.GetCurrentPoint(Canvas));
        canvas.PointerReleased += (_, pe) => ViewModel.OnPointerReleased(pe.GetCurrentPoint(Canvas));
    //     
    //     var canvasFilePath = _configuration["CanvasFilePath"] ?? "";
    //     
    //     CanvasHelpers.LoadCanvasFromFile(canvas, canvasFilePath, _logger);
    }
    
    // private void AssignDraggingToShape(Shape shape)
    // {
    //     shape.PointerPressed += ShapeOnPointerPressed;
    //     shape.PointerReleased += ShapeOnPointerReleased;
    //     shape.PointerMoved += ShapeOnPointerMoved;
    // }
    //
    // private void ShapeOnPointerPressed(object? sender, PointerPressedEventArgs e)
    // {
    //     if (sender is not Shape shape)
    //         return;
    //     
    //     var point = e.GetCurrentPoint(Canvas);
    //
    //     if (point.Properties.IsLeftButtonPressed)
    //     {
    //         IsPopupOpen = false;
    //         _isDragging = true; 
    //         _dragPosition = e.GetPosition(shape);
    //     }
    // }
    //
    // private void ShapeOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    // {
    //     _dragPosition = null;
    //     _isDragging = false;
    //     IsPopupOpen = true;
    //     e.Handled = true;
    // }
    //
    // private void ShapeOnPointerMoved(object? sender, PointerEventArgs e)
    // {
    //     if (sender is not Shape shape)
    //         return;
    //
    //     if (_dragPosition is null)
    //         return;
    //     
    //     var point = e.GetCurrentPoint(Canvas);
    //
    //     if (point.Properties.IsLeftButtonPressed)
    //     {
    //         Canvas.SetPosition(shape,
    //             point.Position.X - _dragPosition.Value.X,
    //             point.Position.Y - _dragPosition.Value.Y);
    //     }
    // }
    //
}
