using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class HomePageViewModel : PageViewModel
{
    [ObservableProperty]
    private string _welcomeText = "Welcome to Home Page!";

    public HomePageViewModel()
    {
        PageName = ApplicationPageNames.Home;
    }
}