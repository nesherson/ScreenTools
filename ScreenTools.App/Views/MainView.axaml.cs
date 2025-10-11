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
}