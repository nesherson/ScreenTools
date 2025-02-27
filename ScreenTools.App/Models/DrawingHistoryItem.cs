using Avalonia.Controls.Shapes;
using System.Collections.Generic;

namespace ScreenTools.App;

public struct DrawingHistoryItem
{
    public string Name { get; set; }
    public List<Polyline> Lines { get; set; }
    public DrawingAction Action { get; set; }
}
