using Microsoft.Toolkit.Uwp.Notifications;

namespace ScreenTools.App;

public class WindowsToastService
{
    private int _conversationIdCounter = 0;
    public void ShowMessage(string message)
    {
        new ToastContentBuilder()
            .AddArgument("conversationId", _conversationIdCounter++)
            .AddText(message)
            .Show();
    }
}