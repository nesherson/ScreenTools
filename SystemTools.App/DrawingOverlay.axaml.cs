using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SystemTools.App;

public partial class DrawingOverlay : Window
{
    public DrawingOverlay()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        WindowLockHook.Hook(this);
        
        base.OnLoaded(e);
    }
}