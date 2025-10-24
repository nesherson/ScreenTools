using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScreenTools.App;

public partial class GalleryImageViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _path;
    
    [ObservableProperty]
    private Bitmap _bitmap;

    public GalleryImageViewModel()
    {
    }
}