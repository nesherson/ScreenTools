using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using ReactiveUI;

namespace ScreenTools.App;

public partial class OptionsView : NotifyPropertyChangedWindowBase
{
    private readonly IStorageService<string> _fileStorageService;
    private readonly WindowNotificationManager _notificationManager;

    private ObservableCollection<GalleryPath> _galleryPaths;
    
    public OptionsView(IStorageService<string> storageService)
    {
        InitializeComponent();

        _fileStorageService = storageService;
        _notificationManager = new WindowNotificationManager(GetTopLevel(this));

        RxApp.MainThreadScheduler.Schedule(LoadData);
    }

    public ObservableCollection<GalleryPath> GalleryPaths
    {
        get => _galleryPaths;
        set => SetField(ref _galleryPaths, value);
    }
    
    private async void LoadData()
    {
        var galleryPaths = new ObservableCollection<GalleryPath>();
        
        try
        {
            var text = await _fileStorageService.LoadData();
            var savedGalleryPaths = text
                .Split(';')
                .Where(x => !string.IsNullOrEmpty(x));

            foreach (var savedGalleryPath in savedGalleryPaths)
            {
                galleryPaths.Add(new GalleryPath(savedGalleryPath));
            }
            
            GalleryPaths = galleryPaths;
        }
        catch (Exception exception)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(exception);
        }
    }

    private void BtnAddPath_OnClick(object? sender, RoutedEventArgs e)
    {
        GalleryPaths.Add(new GalleryPath(string.Empty));
    }
    
    private async void BtnSavePaths_OnClick(object? sender, RoutedEventArgs e)
    {
        var galleryPaths = string.Join(';', GalleryPaths.Select(x => x.Path));

        try
        {
            await _fileStorageService.SaveData(galleryPaths);
            
            _notificationManager.Show(new Notification("Success", "Paths are successfully saved.", NotificationType.Success));
        }
        catch (Exception exception)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(exception);
        }
    }
}