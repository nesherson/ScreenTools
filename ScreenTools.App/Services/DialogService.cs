using System.Threading.Tasks;

namespace ScreenTools.App;

public class DialogService
{
    public async Task ShowDialog<THost, TDialogViewModel>(THost host, TDialogViewModel dialogViewModel) 
        where THost : IDialogProvider
        where TDialogViewModel : DialogViewModelBase
    {
        host.Dialog = dialogViewModel;
        
        host.Dialog.Show();
        await host.Dialog.WaitAsync();
    }
}