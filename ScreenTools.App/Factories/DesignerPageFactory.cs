namespace ScreenTools.App;

public class DesignerPageFactory : IPageFactory
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName)
    {
        return new DummyPageViewModel();
    }
}