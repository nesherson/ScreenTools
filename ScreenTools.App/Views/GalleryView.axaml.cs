using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using ReactiveUI;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace ScreenTools.App;

public partial class GalleryView : NotifyPropertyChangedWindowBase
{
    private readonly WindowNotificationManager _notificationManager;

    private ObservableCollection<GalleryImage> _galleryImages;
    private bool _isLoading;
    private int _loadingProgress;
    private bool _hasData;
    
    public GalleryView()
    {
        InitializeComponent();
        
        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        
        _loadingProgress = 0;
        HasData = IsLoading == false;

        RxApp.MainThreadScheduler.Schedule(LoadImages);
    }

    public ObservableCollection<GalleryImage> GalleryImages
    {
        get => _galleryImages;
        set => SetField(ref _galleryImages, value); 
    }

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
    
    public bool HasData
    {
        get => _hasData; 
        set => SetField(ref _hasData, value);
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

            var galleryImages = new ObservableCollection<GalleryImage>();

            foreach (var file in files)
            {
                LoadingProgress += 100 / files.Length;
                await using var fileStream = File.OpenRead(file);
                var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(fileStream, 640));
                galleryImages.Add(new GalleryImage(file, bitmap));
            }

            HasData = files.Length != 0;

            GalleryImages = galleryImages;
        }
        catch (Exception ex)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {   
        var menuItem = sender as MenuItem;

        if (menuItem is null)
            return;

        var galleryImage = menuItem.DataContext as GalleryImage;
            
        if (galleryImage is null)
            return;

        switch (menuItem.Name)
        {
            case "Preview":
                HandlePreview(galleryImage);
                break;
            case "ShowInExplorer":
                HandleShowInExplorer(galleryImage);
                break;
        }
    }
    
    private void HandleShowInExplorer(GalleryImage galleryImage)
    {
        try
        {
            Process.Start("explorer.exe", $"/select, \"{galleryImage.Path}\"");
        }
        catch (Exception ex)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(ex);
        }
    }

    private void HandlePreview(GalleryImage galleryImage)
    {
      var window = new Window
      {
        Width = 1280,
        Height = 720,
        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        Content = new Image
        {
            Source = new Bitmap(galleryImage.Path)
        }
      };

      window.ShowDialog(this);
    }
}