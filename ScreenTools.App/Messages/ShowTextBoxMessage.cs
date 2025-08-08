using System;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class ShowTextBoxMessageContent
{
    public Action<string?>? OnClosed { get; set; }
}

public class ShowTextBoxMessage : RequestMessage<ShowTextBoxMessageContent>
{
    public ShowTextBoxMessage(ShowTextBoxMessageContent content)
    {
        Content  = content;
    }
    
    public ShowTextBoxMessageContent Content { get; set; }
}