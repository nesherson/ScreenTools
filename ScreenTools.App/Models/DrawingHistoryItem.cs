using System.Collections.Generic;
using Avalonia.Controls;

namespace ScreenTools.App;

public class DrawingHistoryItem
{
    public DrawingHistoryItem(List<Control> canvasControls, DrawingAction drawingAction)
    {
        CanvasControls = canvasControls;
        Action = drawingAction;
    }
    public List<Control> CanvasControls { get; set; }
    public DrawingAction Action { get; set; }
}
