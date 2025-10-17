using System.Collections.Generic;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class ShowFolderPickerMessage : AsyncRequestMessage<IReadOnlyList<IStorageFolder>>
{
    public string Title { get; set; }
    public bool AllowMultiple { get; set; }
}