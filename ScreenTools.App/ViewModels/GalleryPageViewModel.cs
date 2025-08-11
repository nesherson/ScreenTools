using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Infrastructure;

namespace ScreenTools.App;

public partial class GalleryPageViewModel : PageViewModel
{
    private readonly FilePathRepository _filePathRepository;
    private readonly ILogger<GalleryPageViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<GalleryImage> _galleryImages;
    [ObservableProperty]
    private bool _isLoading;
    [ObservableProperty]
    private int _loadingProgress;
    [ObservableProperty]
    private bool _hasData;
    
    public GalleryPageViewModel(FilePathRepository filePathRepository,
        ILogger<GalleryPageViewModel> logger)
    {
        PageName = ApplicationPageNames.Gallery;
        
        _filePathRepository = filePathRepository;
        _logger = logger;
        
        _loadingProgress = 0;
        HasData = IsLoading == false;

        RxApp.MainThreadScheduler.Schedule(LoadImages);
    }
    
    private async void LoadImages()
    {
        try
        {
            IsLoading = true;

            var validExtensions = new[] { "png", "jpg", "jpeg" };
            var galleryPaths = await _filePathRepository.GetAllAsync();
            var files = galleryPaths.SelectMany(gp => Directory.EnumerateFiles(
                        gp.Path,
                        "*.*",
                        SearchOption.AllDirectories)
                    .Where(x => validExtensions.Contains(Path.GetExtension(x).TrimStart('.').ToLowerInvariant())))
                .ToArray();

            var galleryImages = new ObservableCollection<GalleryImage>();

            foreach (var file in files)
            {
                LoadingProgress += Convert.ToInt32(Math.Ceiling(100.0 / files.Length));
                await using var fileStream = File.OpenRead(file);
                var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(fileStream, 640));
                galleryImages.Add(new GalleryImage(file, bitmap));
            }

            HasData = files.Length != 0;

            GalleryImages = galleryImages;
        }
        catch (Exception ex)
        {
            // _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            _logger.LogError($"Failed to load images to the gallery. Exception: {ex}");
            
        }
        finally
        {
            IsLoading = false;
        }
    }
}