using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class HideWindowMessage(bool isHidden) : ValueChangedMessage<bool>(isHidden);