using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class SettingsPageViewModel : PageViewModel
{
    [ObservableProperty]
    private string _welcomeText = "Welcome to Settings Page!";

    public SettingsPageViewModel()
    {
        PageName = ApplicationPageNames.Settings;
    }
}