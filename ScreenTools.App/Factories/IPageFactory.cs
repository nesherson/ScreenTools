namespace ScreenTools.App;

public interface IPageFactory
{
    PageViewModel GetPageViewModel(ApplicationPageNames pageName);
}