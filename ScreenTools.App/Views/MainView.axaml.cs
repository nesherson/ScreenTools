using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ScreenTools.App;

public partial class MainView : Window
{
    private readonly INotificationManager  _notificationManager;
    public MainView()
    {
        InitializeComponent();
        
        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        
        this.AttachDevTools();
        
        WeakReferenceMessenger.Default
            .Register<ShowWindowNotificationMessage>(this, HandleShowWindowNotificationMessage);
    }
    
    private void HandleShowWindowNotificationMessage(object recipient, ShowWindowNotificationMessage message)
    {
        _notificationManager.Show(message.Notification);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount != 2)
            return;

        if (DataContext is MainViewModel vm)
        {
            vm.ResizeSideMenuCommand.Execute(null);
        }
    }

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
}