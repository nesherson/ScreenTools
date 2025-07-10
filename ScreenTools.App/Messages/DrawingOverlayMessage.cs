using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class DrawingOverlayMessage : ValueChangedMessage<DrawingOverlayMessageType>
{
    public DrawingOverlayMessage(DrawingOverlayMessageType type) : base(type)
    {
    }
}