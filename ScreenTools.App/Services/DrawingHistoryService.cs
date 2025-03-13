using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace ScreenTools.App;

public class DrawingHistoryService
{
    private readonly List<DrawingHistoryItem?> _drawingHistoryItems;

    public DrawingHistoryService()
    {
        _drawingHistoryItems = [];
    }
    
    public void Save(Control canvasControl, DrawingAction drawingAction)
    {
        Save([canvasControl], drawingAction);
    }
    
    public void Save(List<Control> canvasControls, DrawingAction drawingAction)
    {
        _drawingHistoryItems.Add(new DrawingHistoryItem(canvasControls, drawingAction));
    }
    
    public void Undo(Canvas canvas)
    {
        var itemToUndo = _drawingHistoryItems.LastOrDefault();

        if (itemToUndo is null)
            return;

        switch (itemToUndo.Action)
        {
            case DrawingAction.Draw:
                foreach (var canvasControl in itemToUndo.CanvasControls)
                {
                    canvas.Children.Remove(canvasControl);
                }
                _drawingHistoryItems.Remove(itemToUndo);
                break;
            case DrawingAction.Delete:
            case DrawingAction.Clear:
                canvas.Children.AddRange(itemToUndo.CanvasControls);
                _drawingHistoryItems.Remove(itemToUndo);
                break;
        }
    }

}