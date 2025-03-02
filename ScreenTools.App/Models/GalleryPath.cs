using ReactiveUI;

namespace ScreenTools.App;

public class GalleryPath : ReactiveObject
{
    private string _path;

    public GalleryPath(string path)
    {
        Path = path;
    }
    public string Path
    {
        get => _path;
        set => this.RaiseAndSetIfChanged(ref _path, value);
    }
}