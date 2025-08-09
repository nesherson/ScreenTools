using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ScreenTools.Core;

namespace ScreenTools.App;

public class DrawingHistoryService
{
    private readonly List<DrawingHistoryItem?> _drawingHistoryItems;

    public DrawingHistoryService()
    {
        _drawingHistoryItems = [];
    }
    
    public void Save(ShapeViewModelBase shape, DrawingAction drawingAction)
    {
        Save([shape], drawingAction);
    }
    
    public void Save(List<ShapeViewModelBase> shapes, DrawingAction drawingAction)
    {
        if (shapes.Count == 0)
            return;
        
        _drawingHistoryItems.Add(new DrawingHistoryItem(shapes, drawingAction));
    }
    
    public void Undo(ObservableCollection<ShapeViewModelBase> shapes)
    {
        var itemToUndo = _drawingHistoryItems.LastOrDefault();

        if (itemToUndo is null)
            return;

        switch (itemToUndo.Action)
        {
            case DrawingAction.Draw:
                shapes.RemoveMany(itemToUndo.Shapes);
                _drawingHistoryItems.Remove(itemToUndo);
                
                break;
            case DrawingAction.Delete:
            case DrawingAction.Clear:
                shapes.AddRange(itemToUndo.Shapes);
                _drawingHistoryItems.Remove(itemToUndo);
                
                break;
        }
    }
}