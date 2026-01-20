using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ScreenTools.App;

public partial class CoordinatePlanePageView : UserControl
{
    private CoordinatePlanePageViewModel _vm;
 
    // State for panning
    private bool _isPanning;
    private Point _panStart;
    
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
        
        _vm = vm;
        
        _vm.Offset = new Point(canvas.Bounds.Width / 2, canvas.Bounds.Height / 2);
        
        vm.DrawGrid();
        
        canvas.PointerPressed += (_, pe) =>
        {
            var pointerPoint = pe.GetCurrentPoint(canvas);

            if (pointerPoint.Properties.IsRightButtonPressed)
            {
                // Start Panning
                _isPanning = true;
                _panStart = pe.GetPosition(this); // Use 'this' (UserControl) for pan reference
                pe.Handled = true;
            }
            else
            {
                var screenPos = pointerPoint.Position;
            
                // Convert Screen Coords to World Coords
                var worldX = (screenPos.X - _vm.Offset.X) / _vm.Scale;
                var worldY = (screenPos.Y - _vm.Offset.Y) / _vm.Scale;
                vm.OnPointerPressed(worldX, worldY);
            }
        };
        // 3. Pointer Moved for Panning
        canvas.PointerMoved += (s, pe) =>
        {
            if (_isPanning)
            {
                var currentPos = pe.GetPosition(this);
                var delta = currentPos - _panStart;
                _panStart = currentPos;
                
                _vm.Offset += delta; // Update the offset
                Console.WriteLine("Offset: " + _vm.Offset);
            }
        };
        
        // 4. Pointer Released to Stop Panning
        canvas.PointerReleased += (s, pe) =>
        {
            if (_isPanning)
            {
                _isPanning = false;
                _vm.RedrawGrid();
            }
        };
        canvas.PointerWheelChanged += (_, pe) =>
        {
            var delta = pe.Delta.Y;

            if (delta == 0)
                return;
            
            var mousePos = pe.GetPosition(canvas);
            
            var oldScale = _vm.Scale;
            var newScale = _vm.Scale * (1.0 + delta * 0.1); // Adjust 0.1 to change zoom speed
            _vm.Scale = newScale;
            
            // This formula calculates the new offset to make it zoom-in/out
            // on the cursor's position.
            _vm.Offset = mousePos - (mousePos - _vm.Offset) * (newScale / oldScale);
            
            pe.Handled = true;
        };
    }
}