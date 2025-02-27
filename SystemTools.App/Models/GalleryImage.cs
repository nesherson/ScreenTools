using Avalonia.Media.Imaging;

namespace SystemTools.App;

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