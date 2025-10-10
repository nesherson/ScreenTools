using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ScreenTools.App.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<string> IconTextProperty = AvaloniaProperty.Register<IconButton, string>(
        nameof(IconText));
    
    public static readonly StyledProperty<IBrush> IconColorProperty = AvaloniaProperty.Register<IconButton, IBrush>(
        nameof(IconColor), SolidColorBrush.Parse("#4f5d64"));

    public string IconText
    {
        get => GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }
    
    public IBrush IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }
}