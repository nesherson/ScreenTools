using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using ScreenTools.Core;
using ScreenTools.Infrastructure;

namespace ScreenTools.App;

public partial class OptionsView : NotifyPropertyChangedWindowBase
{
    private readonly WindowNotificationManager _notificationManager;
    private readonly GalleryPathRepository _galleryPathRepository;

    private ObservableCollection<GalleryPathObject> _galleryPaths;
    
    public OptionsView(GalleryPathRepository galleryPathRepository)
    {
        InitializeComponent();

        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        _galleryPathRepository = galleryPathRepository;

        RxApp.MainThreadScheduler.Schedule(LoadData);
    }

    public ObservableCollection<GalleryPathObject> GalleryPaths
    {
        get => _galleryPaths;
        set => SetField(ref _galleryPaths, value);
    }
    
    private async void LoadData()
    {
        try
        {
            var galleryPaths = new ObservableCollection<GalleryPathObject>();
            var result = await _galleryPathRepository
                .GetAllAsync();

            foreach (var item in result)
            {
                galleryPaths.Add(new GalleryPathObject(item.Path));
            }
            
            GalleryPaths = galleryPaths;
        }
        catch (Exception exception)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(exception);
        }
    }

    private async void BtnAddPath_OnClick(object? sender, RoutedEventArgs e)
    {
        GalleryPaths.Add(new GalleryPathObject(string.Empty));
    }
    
    private async void BtnSavePaths_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var galleryPathsToSave = GalleryPaths
                .Select(x => new GalleryPath { Path = x.Path })
                .ToArray();

            if (galleryPathsToSave.Any(x => Path.IsPathRooted(x.Path) == false))
            {
                _notificationManager.Show(new Notification("Error", "The given paths are not valid.", NotificationType.Error));
                return;
            }

            await _galleryPathRepository.DeleteAllAsync();
            await _galleryPathRepository.AddRangeAsync(galleryPathsToSave);
            await _galleryPathRepository.SaveChangesAsync();
            
            _notificationManager.Show(new Notification("Success", "Paths are successfully saved.", NotificationType.Success));
        }
        catch (Exception exception)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            Console.WriteLine(exception);
        }
    }
}