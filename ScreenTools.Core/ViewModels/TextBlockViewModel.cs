namespace ScreenTools.Core;

public class TextBlockViewModel : ShapeViewModelBase
{
    private string _text;
    private double _fontSize;
    private string _foreground;
    private string _background;
    private double _x;
    private double _y;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
    
    public double FontSize
    {
        get => _fontSize;
        set => SetProperty(ref _fontSize, value);
    }
    
    public string Foreground
    {
        get => _foreground;
        set => SetProperty(ref _foreground, value);
    }
    
    public string Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }
    
    public double X
    {
        get => _x;
        set => SetProperty(ref _x, value);
    }
    
    public double Y
    {
        get => _y;
        set => SetProperty(ref _y, value);
    }
}