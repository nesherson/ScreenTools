using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ScreenTools.App;

public class MyCustomShape : Control
{
    // Define a custom property that, when changed, causes the control to re-render
    public static readonly StyledProperty<Color> ShapeColorProperty =
        AvaloniaProperty.Register<MyCustomShape, Color>(nameof(ShapeColor), Colors.Blue);

    public Color ShapeColor
    {
        get => GetValue(ShapeColorProperty);
        set => SetValue(ShapeColorProperty, value);
    }
    
    // Static constructor to register properties that affect rendering¸
    static MyCustomShape()
    {
        AffectsRender<MyCustomShape>(ShapeColorProperty);
        // You can also use AffectsMeasure and AffectsArrange if your drawing affects layout
    }
    
    public override void Render(DrawingContext context)
    {
        // Get the size of the control
        var renderSize = Bounds.Size;

        // Create a brush from the custom ShapeColor property
        var brush = new SolidColorBrush(ShapeColor);
        var pen = new Pen(Brushes.Red, 5);

        // Example: Draw a custom polygon (e.g., a diamond)
        var points = new[]
        {
            new Point(renderSize.Width / 2, 0),             // Top point
            new Point(renderSize.Width, renderSize.Height / 2), // Right point
            new Point(renderSize.Width / 2, renderSize.Height), // Bottom point
            new Point(0, renderSize.Height / 2)             // Left point
        };

        // Create a StreamGeometry to define the path
        var streamGeometry = new StreamGeometry();

        using (var geometryContext = new StreamGeometry().Open())
        {
            // geometryContext.BeginFigure(points[0], true); // Start at the top point, filled
            // geometryContext.LineTo(points[1]);
            // geometryContext.LineTo(points[2]);
            // geometryContext.LineTo(points[3]);
            // geometryContext.EndFigure(true); // Close the figure
            geometryContext.BeginFigure(new Point(30, 30), false); // Start at the top point, filled
            geometryContext.LineTo(new Point(150, 30));
            // geometryContext.LineTo(new Point(60, 60));
            // geometryContext.LineTo(new Point(30, 60));
            geometryContext.EndFigure(false); // Close the figure
        }
        
        // Draw the geometry onto the canvas
        context.DrawGeometry(null, pen, streamGeometry.Clone());

        // You can also draw primitive shapes directly using DrawingContext methods:
        // context.DrawRectangle(Brushes.Red, null, new Rect(10, 10, 50, 50));
        // context.DrawEllipse(Brushes.Green, null, new Rect(70, 10, 50, 50));
        // context.DrawLine(pen, new Point(30, 30), new Point(60, 30));

        base.Render(context); // Call base implementation
    }
}