using Avalonia.Media.Imaging;

namespace ScreenTools.App;

public class GalleryImage
{
    public GalleryImage(string imagePath, Bitmap bitmap)
    {
        Path = imagePath;
        Bitmap = bitmap;
    }
    public string Path { get; set; }
    public Bitmap Bitmap { get; set; }
}