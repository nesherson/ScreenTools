using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using ScreenTools.Core;

namespace ScreenTools.App;

public class ShapeTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new();

    public Control Build(object? param)
    {
        if (param is null)
        {
            return new TextBlock { Text = "param is null" };
        }

        var type = param.GetType().Name;

        if (Templates.TryGetValue(type, out var template))
        {
            var test = template.Build(param);
            return test;
        }

        return new TextBlock { Text = $"No template found for {type}" };
    }

    public bool Match(object? data)
    {
        return data is ShapeViewModelBase;
    }
}