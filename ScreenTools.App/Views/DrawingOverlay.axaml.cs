using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ScreenTools.Core;
using WeakReferenceMessenger = CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger;

namespace ScreenTools.App;

public partial class DrawingOverlay : ReactiveWindow<DrawingOverlayViewModel>
{
    private readonly WindowNotificationManager _notificationManager;

    public DrawingOverlay(DrawingOverlayViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        _notificationManager = new WindowNotificationManager(GetTopLevel(this));

        this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        this.WhenActivated(disposables =>
        {
            this.Events()
                .KeyDown
                .Subscribe(async e => await ViewModel.HandleOnKeyDown(e))
                .DisposeWith(disposables);
            this.Events()
                .PointerWheelChanged
                .Subscribe(e => ViewModel.HandleOnPointerWheelChanged(e))
                .DisposeWith(disposables);
        });

        WeakReferenceMessenger.Default
            .Register<DrawingOverlayMessage>(this, HandleDrawingOverlayMessage);
        WeakReferenceMessenger.Default
            .Register<PasteLastItemFromClipboardMessage>(this, HandlePasteLastItemFromClipboardMessage);
        WeakReferenceMessenger.Default
            .Register<GetWindowSizeMessage>(this, HandleGetWindowSizeMessage);
        WeakReferenceMessenger.Default
            .Register<ShowWindowNotificationMessage>(this, HandleShowWindowNotificationMessage);
        WeakReferenceMessenger.Default
            .Register<ShowContextMenuMessage>(this, HandleShowContextMenuMessage);
        WeakReferenceMessenger.Default
            .Register<ShowTextBoxMessage>(this, HandleShowTextBoxMessage);

        ViewModel.IsPopupOpen = true;
        ViewModel.WindowBorderThickness = new Thickness(2);

        Deactivated += (_, _) => ViewModel.OnWindowDeactivated();
        Activated += (_, _) => ViewModel.OnWindowActivated();
    }

    public event EventHandler? Hidden;

    public override void Hide()
    {
        base.Hide();

        OnHidden(EventArgs.Empty);
    }

    private void Canvas_OnInitialized(object? sender, EventArgs e)
    {
        if (sender is not Canvas canvas)
            return;

        if (ViewModel is null)
            return;

        canvas.PointerPressed += (_, pe) => ViewModel.OnPointerPressed(pe.GetCurrentPoint(canvas));
        canvas.PointerMoved += (_, pe) => ViewModel.OnPointerMoved(pe.GetCurrentPoint(canvas));
        canvas.PointerReleased += (_, _) => ViewModel.OnPointerReleased();
    }

    private void OnHidden(EventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => Hidden?.Invoke(this, e));
    }

    private void HandleDrawingOverlayMessage(object recipient, DrawingOverlayMessage message)
    {
        switch (message.Value)
        {
            case DrawingOverlayMessageType.HideWindow:
                Hide();
                break;

            case DrawingOverlayMessageType.ChangeMonitor:
                ChangeMonitor();
                break;
        }
    }

    private void HandlePasteLastItemFromClipboardMessage(object recipient, PasteLastItemFromClipboardMessage message)
    {
        var clipboard = GetTopLevel(this)?.Clipboard;

        if (clipboard is null)
        {
            message.Reply(string.Empty);

            return;
        }

        message.Reply(clipboard.GetTextAsync());
    }

    private void HandleGetWindowSizeMessage(object recipient, GetWindowSizeMessage message)
    {
        message.Reply(new WindowSize
        {
            Width = Width,
            Height = Height
        });
    }
    
    private void HandleShowWindowNotificationMessage(object recipient, ShowWindowNotificationMessage message)
    {
        _notificationManager.Show(message.Notification);
    }

    private void ChangeMonitor()
    {
        var currentScreen = Screens.ScreenFromWindow(this);
        var targetScreen = Screens.All.FirstOrDefault(x => x != currentScreen);

        if (targetScreen is null)
        {
            return;
        }

        var rect = targetScreen.WorkingArea;

        WindowState = WindowState.Normal;
        Position = new PixelPoint(rect.X, rect.Y);
        Width = rect.Width;
        Height = rect.Height;
        WindowState = WindowState.Maximized;
    }

    private void HandleShowContextMenuMessage(object recipient, ShowContextMenuMessage message)
    {
        var flyout = new MenuFlyout();
        var pasteMenuItem = new MenuItem
        {
            Header = "Paste",
            IsEnabled = message.IsPasteEnabled
        };

        pasteMenuItem.Click += (_, _) => { message.OnPaste?.Invoke(); };

        flyout.Items.Add(pasteMenuItem);
        flyout.ShowAt(this, true);
    }

    private void HandleShowTextBoxMessage(object recipient, ShowTextBoxMessage message)
    {
        var flyout = new Flyout();
        flyout.FlyoutPresenterClasses.Add("drawingOverlayTextBoxFlyout");
        
        var textBox = new TextBox
        {
            Width = 320,
            Height = 200,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true
        };
        
        textBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                textBox.Text = null;
                flyout.Hide();
            }
        };

        flyout.Content = textBox;
        flyout.Closed += (_, _) =>
        {
            message.Content.OnClosed?.Invoke(textBox.Text);
        };
        flyout.ShowAt(this, true);
    }
}