using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class PathsPageViewModel : PageViewModel
{
    [ObservableProperty]
    private string _welcomeText = "Welcome to Paths Page!";

    public PathsPageViewModel()
    {
        PageName = ApplicationPageNames.Paths;
    }
}