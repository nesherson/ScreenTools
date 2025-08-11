using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty]
    public ApplicationPageNames _pageName;
}