using System.Drawing;
using ReactiveUI;

namespace ScreenTools.App;

public class FilePathModel : ReactiveObject
{
    private string _path;

    public FilePathModel(int id, string path = "", string filePathTypeName = "")
    {
        Id = id;
        Path = path;
        FilePathTypeName = filePathTypeName;
    }
    public string Path
    {
        get => _path;
        set => this.RaiseAndSetIfChanged(ref _path, value);
    }
    
    public int Id { get; set; }
    public int FilePathTypeId { get; set; }
    public string FilePathTypeName { get; set; }
}