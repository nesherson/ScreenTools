using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Infrastructure;

namespace ScreenTools.App;

public partial class GalleryPageViewModel : PageViewModel
{
    private readonly FilePathRepository _filePathRepository;
    private readonly ILogger<GalleryPageViewModel> _logger;

    [ObservableProperty] private ObservableCollection<GalleryImageViewModel> _galleryImages;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _loadingProgress;
    [ObservableProperty] private bool _hasData;

    public GalleryPageViewModel()
    {
        PageName = ApplicationPageNames.Gallery;
    }

    public GalleryPageViewModel(FilePathRepository filePathRepository,
        ILogger<GalleryPageViewModel> logger)
    {
        PageName = ApplicationPageNames.Gallery;

        _filePathRepository = filePathRepository;
        _logger = logger;

        _loadingProgress = 0;
        HasData = IsLoading == false;

        RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) => await LoadImages());
    }

    private async Task LoadImages()
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

            var galleryImages = new ObservableCollection<GalleryImageViewModel>();

            foreach (var file in files)
            {
                LoadingProgress += Convert.ToInt32(Math.Ceiling(100.0 / files.Length));
                await using var fileStream = File.OpenRead(file);
                var bitmap = await Task.Run(() => Bitmap.DecodeToWidth(fileStream, 640));
                galleryImages.Add(new GalleryImageViewModel { Bitmap = bitmap, Path = file });
            }

            HasData = files.Length != 0;

            GalleryImages = galleryImages;
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to load images to the gallery. Exception: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void PreviewImage(GalleryImageViewModel imageViewModel)
    {
        WeakReferenceMessenger.Default
            .Send(new PreviewGalleryImageMessage(imageViewModel.Path));
    }
    
    [RelayCommand]
    private void ShowImageInExplorer(GalleryImageViewModel imageViewModel)
    {
        try
        {
            ProcessHelpers.ShowFileInFileExplorer(imageViewModel.Path);
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to show image in explorer. Exception: {ex}");
        }
    }
    
    [RelayCommand]
    private async Task CopyImageToClipboard(GalleryImageViewModel imageViewModel)
    {
        try
        {
            var bitmap = new Bitmap(imageViewModel.Path);

            await ClipboardAvalonia.SetImageAsync(bitmap);
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to copy image to the clipboard. Exception: {ex}");
        }
    }
}