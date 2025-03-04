using Avalonia.Controls.Shapes;
using System.Collections.Generic;
using Avalonia.Controls;

namespace ScreenTools.App;

public struct DrawingHistoryItem
{
    public string Name { get; set; }
    public List<Polyline> Lines { get; set; }
    public List<TextBlock> TextBlocks { get; set; }
    public DrawingAction Action { get; set; }
}
