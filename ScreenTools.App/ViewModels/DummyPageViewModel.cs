using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class DummyPageViewModel : PageViewModel
{
    [ObservableProperty]
    private string _welcomeText = "This is dummy text!";

    public DummyPageViewModel()
    {
        PageName = ApplicationPageNames.Unknown;
    }
}