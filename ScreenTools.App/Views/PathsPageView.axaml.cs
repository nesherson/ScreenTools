using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;

namespace ScreenTools.App;

public partial class PathsPageView : UserControl
{
    public PathsPageView()
    {
        InitializeComponent();
        
        WeakReferenceMessenger.Default
            .Register<ShowFolderPickerMessage>(this, HandleShowFolderPickerMessage);
    }
    
    private void HandleShowFolderPickerMessage(object recipient, ShowFolderPickerMessage message)
    {
        var topLevel = TopLevel.GetTopLevel(VisualRoot as Window);
    
        if (topLevel is null)
            return;
        
        var task = topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = message.Title,
            AllowMultiple = message.AllowMultiple
        });
        
        message.Reply(task);
        
    }
}