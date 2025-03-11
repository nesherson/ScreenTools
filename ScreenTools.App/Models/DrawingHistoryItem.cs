using Avalonia.Controls.Shapes;
using System.Collections.Generic;
using Avalonia.Controls;

namespace ScreenTools.App;

public struct DrawingHistoryItem
{
    public List<Control>? CanvasControls { get; set; }
    public DrawingAction Action { get; set; }
}
