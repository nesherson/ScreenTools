using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class ShowContextMenuMessageContent
{
    
}

public class ShowContextMenuMessage : RequestMessage<ShowContextMenuMessageContent>
{
    public bool IsPasteEnabled { get; set; }
    public Action? OnPaste { get; set; }
}