using System.Collections.Generic;
using Avalonia.Controls;
using ScreenTools.Core;

namespace ScreenTools.App;

public sealed class DrawingHistoryItem
{
    public DrawingHistoryItem(List<ShapeViewModelBase> canvasControls, DrawingAction drawingAction)
    {
        Shapes = canvasControls;
        Action = drawingAction;
    }
    public List<ShapeViewModelBase> Shapes { get; }
    public DrawingAction Action { get; }
}
