using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace ScreenTools.App.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<string> IconTextProperty = AvaloniaProperty.Register<IconButton, string>(
        nameof(IconText));
    
    public static readonly StyledProperty<IBrush> IconColorProperty = AvaloniaProperty.Register<IconButton, IBrush>(
        nameof(IconColor), SolidColorBrush.Parse("#4f5d64"));

    public static readonly StyledProperty<bool> IsTextVisibleProperty = AvaloniaProperty.Register<IconButton, bool>(
        nameof(IsTextVisible), true);

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
    
    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);

    }
}