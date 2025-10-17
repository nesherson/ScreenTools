using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ScreenTools.App;

public partial class MainViewModel : ViewModelBase
{
    private readonly IPageFactory _pageFactory;
    [ObservableProperty]
    private bool _isSideMenuExpanded;
    [ObservableProperty]
    private bool _isPaneOpen = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(PathsPageIsActive))]
    [NotifyPropertyChangedFor(nameof(GalleryPageIsActive))]
    [NotifyPropertyChangedFor(nameof(SettingsPageIsActive))]
    private PageViewModel _currentPage;

    public MainViewModel()
    {
        if (!Design.IsDesignMode) 
            return;
        
        _pageFactory = new DesignerPageFactory();
            
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Dummy);
    }
    
    public MainViewModel(IPageFactory pageFactory)
    {
        _pageFactory = pageFactory;
        
        CurrentPage = pageFactory.GetPageViewModel(ApplicationPageNames.Home);

        Title = "ScreenTools";
        IsSideMenuExpanded = true;
    }

    public string Title { get; set; }
    public bool HomePageIsActive => CurrentPage.PageName == ApplicationPageNames.Home;
    public bool GalleryPageIsActive => CurrentPage.PageName == ApplicationPageNames.Gallery;
    public bool PathsPageIsActive => CurrentPage.PageName == ApplicationPageNames.Paths;
    public bool SettingsPageIsActive => CurrentPage.PageName == ApplicationPageNames.Settings;
    
    
    [RelayCommand]
    private void ResizeSideMenu()
    {
        IsSideMenuExpanded = !IsSideMenuExpanded;
    }

    [RelayCommand]
    private void GoToHomePage()
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Home);
    }
    
    [RelayCommand]
    private void GoToPathsPage()
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Paths);
    }
    
    [RelayCommand]
    private void GoToGalleryPage()
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Gallery);
    }
    
    [RelayCommand]
    private void GoToSettingsPage()
    {
        CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Settings);
    }
    
    [RelayCommand]
    private void ToggleSideMenu()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}