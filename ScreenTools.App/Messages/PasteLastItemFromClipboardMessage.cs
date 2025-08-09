using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class PasteLastItemFromClipboardMessage : AsyncRequestMessage<string?>;