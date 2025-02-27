using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace SystemTools.App;

public partial class GalleryView : NotifyPropertyChangedWindowBase
{
    private bool _isLoading;
    private int _loadingProgress;
    
    public GalleryView()
    {
        InitializeComponent();

        Images = [];
        _loadingProgress = 0;

        RxApp.MainThreadScheduler.Schedule(LoadImages);
    }
    
    public ObservableCollection<GalleryImage> Images { get; }

    public bool IsLoading
    {
        get => _isLoading; 
        set => SetField(ref _isLoading, value);
    }
    
    public int LoadingProgress
    {
        get => _loadingProgress; 
        set => SetField(ref _loadingProgress, value);
    }
    
    private async void LoadImages()
    {
        try
        {
            IsLoading = true;
            
            var validExtensions = new[] { "png", "jpg", "jpeg" };
            var files = Directory.EnumerateFiles(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Captures"),
                    "*.*",
                    SearchOption.AllDirectories)
                .Where(x => validExtensions.Contains(Path.GetExtension(x).TrimStart('.').ToLowerInvariant()))
                .ToArray();

            foreach (var file in files)
            {
                LoadingProgress += 100 / files.Length;
                await using var fileStream = File.OpenRead(file);
                var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(fileStream, 300));
                Images.Add(new GalleryImage(file, bitmap));
            }

            this.Get<ItemsControl>("ImageItems").ItemsSource = Images;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GalleryImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            var galleryImage = (sender as Control).DataContext as GalleryImage;
            
            if (galleryImage == null)
                return;

            Process.Start("explorer.exe", $"/select, \"{galleryImage.Path}\"");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}