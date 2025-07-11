using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class ShowContextMenuMessageContent
{
    public bool IsPasteEnabled { get; set; }
    public Action? OnPaste { get; set; }
}

public class ShowContextMenuMessage : RequestMessage<ShowContextMenuMessageContent>
{
    public ShowContextMenuMessage(ShowContextMenuMessageContent content)
    {
        Content = content;
    }
    
    public ShowContextMenuMessageContent Content { get; set; }
}