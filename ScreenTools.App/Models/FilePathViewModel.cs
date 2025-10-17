using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using ScreenTools.Core;

namespace ScreenTools.App;

public partial class FilePathViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _path;

    public FilePathViewModel()
    {
    }
    
    public FilePathViewModel(FilePath filePath)
    {
        Id = filePath.Id;
        Path = filePath.Path;
        FilePathTypeName = filePath.FilePathType.Name;
    }
    
    public int Id { get; set; }
    public int FilePathTypeId { get; set; }
    public string FilePathTypeName { get; set; }
}