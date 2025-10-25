using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ScreenTools.App;

public partial class CoordinatePlanePageView : UserControl
{
    public CoordinatePlanePageView()
    {
        InitializeComponent();
        
        DataContext = new CoordinatePlanePageViewModel();
    }
    private void Canvas_OnInitialized(object sender, RoutedEventArgs e)
    {
        if (sender is not Canvas canvas) 
            return;

        if (DataContext is not CoordinatePlanePageViewModel vm)
            return;
        
        vm.DrawGrid(canvas.Bounds.Width, canvas.Bounds.Height); 
        
        canvas.PointerPressed += (_, pe) =>
        {
            var (x, y) = pe.GetCurrentPoint(canvas).Position; 
            vm.OnPointerPressed(x, y);
        };
    }
}