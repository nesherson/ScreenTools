using System;

namespace ScreenTools.App;

public class PageFactory : IPageFactory
{
    private readonly Func<ApplicationPageNames, PageViewModel> _factory;
    
    public PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
    {
        _factory = factory;
    }
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName)
    {
        return _factory(pageName);
    }
}