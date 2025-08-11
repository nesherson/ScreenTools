using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ScreenTools.App;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        
        this.AttachDevTools();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.ClickCount != 2)
            return;

        if (DataContext is MainViewModel vm)
        {
            vm.ResizeSideMenuCommand.Execute(null);
        }
    }

    private void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
}