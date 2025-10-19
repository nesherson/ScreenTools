using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class DialogViewModelBase : ViewModelBase
{
    [ObservableProperty]
    private bool _isDialogOpen;

    protected TaskCompletionSource CloseTask = new();
    
    
    public async Task WaitAsync()
    {
        await CloseTask.Task;
    }

    public void Show()
    {
        if  (CloseTask.Task.IsCompleted)
            CloseTask = new TaskCompletionSource();
        
        IsDialogOpen = true;
    }

    public void Close()
    {
        IsDialogOpen = false;
        
        CloseTask.TrySetResult();
    }
}