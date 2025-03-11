using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace ScreenTools.App;

public class CanvasHelpers
{
    public static bool IsInEraseArea(Control control, Border eraseArea)
    {
        switch (control)
        {
            case Polyline polyline:
                return polyline.Points
                    .Any(p => p.X >= eraseArea!.Bounds.TopLeft.X &&
                              p.X <= eraseArea!.Bounds.TopRight.X &&
                              p.X >= eraseArea!.Bounds.BottomLeft.X &&
                              p.X <= eraseArea!.Bounds.BottomRight.X &&
                              p.Y >= eraseArea!.Bounds.TopLeft.Y &&
                              p.Y <= eraseArea!.Bounds.BottomLeft.Y &&
                              p.Y >= eraseArea!.Bounds.TopRight.Y &&
                              p.Y <= eraseArea!.Bounds.BottomRight.Y);
            case TextBlock textBlock:
                return textBlock.Bounds.X >= eraseArea!.Bounds.TopLeft.X &&
                       textBlock.Bounds.X <= eraseArea!.Bounds.TopRight.X &&
                       textBlock.Bounds.X >= eraseArea!.Bounds.BottomLeft.X &&
                       textBlock.Bounds.X <= eraseArea!.Bounds.BottomRight.X &&
                       textBlock.Bounds.Y >= eraseArea!.Bounds.TopLeft.Y &&
                       textBlock.Bounds.Y <= eraseArea!.Bounds.BottomLeft.Y &&
                       textBlock.Bounds.Y >= eraseArea!.Bounds.TopRight.Y &&
                       textBlock.Bounds.Y <= eraseArea!.Bounds.BottomRight.Y;
            case Line line:
                return (line.StartPoint.X >= eraseArea!.Bounds.TopLeft.X &&
                        line.StartPoint.X <= eraseArea!.Bounds.TopRight.X &&
                        line.StartPoint.X >= eraseArea!.Bounds.BottomLeft.X &&
                        line.StartPoint.X <= eraseArea!.Bounds.BottomRight.X &&
                        line.StartPoint.Y >= eraseArea!.Bounds.TopLeft.Y &&
                        line.StartPoint.Y <= eraseArea!.Bounds.BottomLeft.Y &&
                        line.StartPoint.Y >= eraseArea!.Bounds.TopRight.Y &&
                        line.StartPoint.Y <= eraseArea!.Bounds.BottomRight.Y) ||
                       (line.EndPoint.X >= eraseArea!.Bounds.TopLeft.X &&
                        line.EndPoint.X <= eraseArea!.Bounds.TopRight.X &&
                        line.EndPoint.X >= eraseArea!.Bounds.BottomLeft.X &&
                        line.EndPoint.X <= eraseArea!.Bounds.BottomRight.X &&
                        line.EndPoint.Y >= eraseArea!.Bounds.TopLeft.Y &&
                        line.EndPoint.Y <= eraseArea!.Bounds.BottomLeft.Y &&
                        line.EndPoint.Y >= eraseArea!.Bounds.TopRight.Y &&
                        line.EndPoint.Y <= eraseArea!.Bounds.BottomRight.Y);
            case Rectangle rectangle:
                return rectangle.Bounds.X >= eraseArea!.Bounds.TopLeft.X &&
                              rectangle.Bounds.X <= eraseArea!.Bounds.TopRight.X &&
                              rectangle.Bounds.X >= eraseArea!.Bounds.BottomLeft.X &&
                              rectangle.Bounds.X <= eraseArea!.Bounds.BottomRight.X &&
                              rectangle.Bounds.Y >= eraseArea!.Bounds.TopLeft.Y &&
                              rectangle.Bounds.Y <= eraseArea!.Bounds.BottomLeft.Y &&
                              rectangle.Bounds.Y >= eraseArea!.Bounds.TopRight.Y &&
                              rectangle.Bounds.Y <= eraseArea!.Bounds.BottomRight.Y;
            case Ellipse ellipse:
                return ellipse.Bounds.X >= eraseArea!.Bounds.TopLeft.X &&
                       ellipse.Bounds.X <= eraseArea!.Bounds.TopRight.X &&
                       ellipse.Bounds.X >= eraseArea!.Bounds.BottomLeft.X &&
                       ellipse.Bounds.X <= eraseArea!.Bounds.BottomRight.X &&
                       ellipse.Bounds.Y >= eraseArea!.Bounds.TopLeft.Y &&
                       ellipse.Bounds.Y <= eraseArea!.Bounds.BottomLeft.Y &&
                       ellipse.Bounds.Y >= eraseArea!.Bounds.TopRight.Y &&
                       ellipse.Bounds.Y <= eraseArea!.Bounds.BottomRight.Y;
        }

        return false;
    }
}