using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace ScreenTools.App;

public partial class DialogViewModelBase : ViewModelBase
{
    [ObservableProperty]
    private bool _isDialogOpen;
    [ObservableProperty]
    private double _dialogWidth = double.NaN;

    protected TaskCompletionSource CloseTask = new();
    
    public async Task WaitAsync()
    {
        await CloseTask.Task;
    }

    public void Show()
    {
        if (CloseTask.Task.IsCompleted)
            CloseTask = new TaskCompletionSource();
        
        IsDialogOpen = true;
    }

    public void Close()
    {
        IsDialogOpen = false;
        
        CloseTask.TrySetResult();
    }
    
    public void ShowWindowNotifcation(string title, string message, NotificationType type, Action? onClick = null)
    {
        WeakReferenceMessenger.Default
            .Send(new ShowWindowNotificationMessage(
                new Notification(title, message, type, null, onClick)));
    }
}