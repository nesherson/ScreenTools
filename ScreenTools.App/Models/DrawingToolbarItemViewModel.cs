using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ScreenTools.Core;

namespace ScreenTools.App;

public partial class DrawingToolbarItemViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isActive;
    [ObservableProperty]
    private string _iconPath;
    
    public ToolbarItemType Type { get; set; }
    public string Name { get; set; }
    public string ToolTip { get; set; }
    public string ShortcutText { get; set; }
    public Key ShortcutKey { get; set; }
    public string Text { get; set; }
    public ICommand OnClickCommand { get; set; }
    public List<DrawingToolbarItemViewModel>? SubItems { get; set; }
    public bool IsContextMenuVisible => SubItems?.Count > 0;
    public DrawingToolbarItemViewModel? Parent { get; set; }
    public bool CanBeActive { get; set; }
}