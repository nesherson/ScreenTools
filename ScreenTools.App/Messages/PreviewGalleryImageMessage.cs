using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScreenTools.App;

public class PreviewGalleryImageMessage : RequestMessage<bool>
{
    public PreviewGalleryImageMessage(string galleryImagePath)
    {
        GalleryImagePath = galleryImagePath;
    }
    
    public string GalleryImagePath { get; }
}