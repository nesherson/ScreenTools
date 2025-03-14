using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Input;
using ReactiveUI;

namespace ScreenTools.App;

public class DrawingToolbarItem : ReactiveObject
{
    private bool _isActive;
    private string _iconPath;
    
    public string Id { get; set; }
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string ShortcutText { get; set; }
    public Key ShortcutKey { get; set; }
    public string Text { get; set; }
    public ICommand OnClickCommand { get; set; }
    public List<DrawingToolbarItem>? SubItems { get; set; }
    public bool IsContextMenuVisible => SubItems?.Count > 0;
    public DrawingToolbarItem? Parent { get; set; }
    public bool CanBeActive { get; set; }
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }
    public string IconPath
    {
        get => _iconPath;
        set => this.RaiseAndSetIfChanged(ref _iconPath, value);
    }
}