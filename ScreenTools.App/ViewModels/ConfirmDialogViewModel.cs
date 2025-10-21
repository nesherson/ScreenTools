using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScreenTools.App;

public partial class ConfirmDialogViewModel : DialogViewModelBase
{
    [ObservableProperty]
    public string _title = "Confirm";
    [ObservableProperty]
    public string _message = "Are you sure you want to continue?";
    [ObservableProperty]
    public string _confirmText = "Yes";
    [ObservableProperty]
    public string _cancelText = "No";
    [ObservableProperty]
    public string _iconText = "\xe4e0";
    [ObservableProperty]
    public bool _confirmed;
    
    [RelayCommand]
    public void Confirm()
    {
        Confirmed = true;
        
        Close();
    }
    
    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
        
        Close();
    }
}