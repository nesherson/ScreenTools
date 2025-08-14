using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace ScreenTools.App;

public partial class PageViewModel : ViewModelBase
{
    public void ShowWindowNotifcation(string title, string message, NotificationType type, Action? onClick = null)
    {
        WeakReferenceMessenger.Default
            .Send(new ShowWindowNotificationMessage(
                new Notification(title, message, type, null, onClick)));
    }
    
    [ObservableProperty]
    public ApplicationPageNames _pageName;
}