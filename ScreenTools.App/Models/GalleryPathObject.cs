using ReactiveUI;

namespace ScreenTools.App;

public class GalleryPathObject : ReactiveObject
{
    private string _path;

    public GalleryPathObject(int id, string path)
    {
        Id = id;
        Path = path;
    }
    public string Path
    {
        get => _path;
        set => this.RaiseAndSetIfChanged(ref _path, value);
    }
    
    public int Id { get; set; }
}