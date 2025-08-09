using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public struct WindowSize
{
    public double Width;
    public double Height;
}

public class GetWindowSizeMessage : AsyncRequestMessage<WindowSize>;