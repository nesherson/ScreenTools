using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScreenTools.App;

public partial class ConfirmDialogViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private string _title = "Confirm";
    [ObservableProperty]
    private string _message = "Are you sure you want to continue?";
    [ObservableProperty]
    private string _confirmText = "Yes";
    [ObservableProperty]
    private string _cancelText = "No";
    [ObservableProperty]
    private string _iconText = "\xe4e0";
    [ObservableProperty]
    private bool _confirmed;
    
    [RelayCommand]
    private void Confirm()
    {
        Confirmed = true;
        
        Close();
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Confirmed = false;
        
        Close();
    }
}