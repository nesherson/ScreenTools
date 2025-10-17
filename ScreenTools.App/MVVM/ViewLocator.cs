using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace ScreenTools.App;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        
        var viewName = data.GetType().FullName
            !.Replace("ViewModel", "View", StringComparison.InvariantCulture);
        var type = Type.GetType(viewName);

        if (type is null)
            return null;
        
        var view = (Control)Activator.CreateInstance(type)!;
        
        view.DataContext = data;

        return view;
    }

    public bool Match(object? data) => data is ViewModelBase;
}