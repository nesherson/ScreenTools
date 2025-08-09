using CommunityToolkit.Mvvm.Messaging.Messages;
using ScreenTools.Core;

namespace ScreenTools.App;

public class DrawingOverlayMessage : ValueChangedMessage<DrawingOverlayMessageType>
{
    public DrawingOverlayMessage(DrawingOverlayMessageType type) : base(type)
    {
    }
}