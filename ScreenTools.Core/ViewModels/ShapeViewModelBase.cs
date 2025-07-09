using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.Core;

public class ShapeViewModelBase : ObservableObject
{
    public ICommand? PointerPressedCommand { get; set; } 
}