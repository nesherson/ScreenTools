using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class ShowWindowNotificationMessage(Notification notification) : RequestMessage<Notification>
{
    public Notification Notification { get; } = notification;
}