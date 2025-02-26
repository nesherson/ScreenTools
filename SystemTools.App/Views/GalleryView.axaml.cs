using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using System.Reactive.Concurrency;
using DynamicData;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Image = System.Drawing.Image;

namespace SystemTools.App.Views;

public partial class GalleryView : Window
{
    public GalleryView()
    {
        InitializeComponent();

        Images = [];
        
        LoadImages();
    }
    
    public ObservableCollection<Bitmap> Images { get; }

    private void LoadImages()
    {
        var items = Directory.GetFiles(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "ScreenTools",
                "Captures"));

        foreach (var item in items)
        {
            var bytes = File.ReadAllBytes(item);
            var ms = new MemoryStream(bytes);

            var bitmap = new Bitmap(ms);
            
            Images.Add(bitmap);
        }

        this.Get<ItemsControl>("ImageItems").ItemsSource = Images;
    }
}