using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Core;
using ScreenTools.Infrastructure;

namespace ScreenTools.App;

public partial class OptionsView : NotifyPropertyChangedWindowBase
{
    private readonly WindowNotificationManager _notificationManager;
    private readonly FilePathRepository _filePathRepository;
    private readonly FilePathTypeRepository _filePathTypeRepository;
    private readonly ILogger<OptionsView> _logger;

    private ObservableCollection<FilePathModel> _galleryPaths;
    
    public OptionsView(FilePathRepository filePathRepository,
        FilePathTypeRepository filePathTypeRepository,
        ILogger<OptionsView> logger)
    {
        InitializeComponent();

        _notificationManager = new WindowNotificationManager(GetTopLevel(this));
        _filePathRepository = filePathRepository;
        _filePathTypeRepository = filePathTypeRepository;
        _logger = logger;

        RxApp.MainThreadScheduler.Schedule(LoadData);
    }

    public ObservableCollection<FilePathModel> GalleryPaths
    {
        get => _galleryPaths;
        set => SetField(ref _galleryPaths, value);
    }
    
    private async void LoadData()
    {
        try
        {
            var galleryPaths = new ObservableCollection<FilePathModel>();
            var result = await _filePathRepository
                .GetAllAsync("FilePathType");

            foreach (var item in result)
            {
                galleryPaths.Add(new FilePathModel(item.Id, item.Path, item.FilePathType.Name));
            }
            
            GalleryPaths = galleryPaths;
        }
        catch (Exception ex)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            _logger.LogError(ex.Message);
        }
    }

    private void BtnAddPath_OnClick(object? sender, RoutedEventArgs e)
    {
        GalleryPaths.Add(new FilePathModel(0, string.Empty));
    }
    
    private async void BtnChoosePath_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);

        if (topLevel is null)
            return;
        
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Choose path",
            AllowMultiple = false
        });
        
        var path = folders.FirstOrDefault()?.Path.AbsolutePath;
        
        if (string.IsNullOrEmpty(path))
            return;
        
        GalleryPaths.Add(new FilePathModel(0, Uri.UnescapeDataString(path.Replace("/", "\\"))));
    }
    
    private async void BtnSavePaths_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (GalleryPaths.Any(x => Path.IsPathRooted(x.Path) == false))
            {
                _notificationManager.Show(new Notification("Error", "The given paths are not valid.", NotificationType.Error));
                return;
            }
            
            var galleryPathsToSave = GalleryPaths
                .Select(x => new FilePath { Path = x.Path, FilePathTypeId = x.FilePathTypeId })
                .ToArray();
            
            var filePathType = await _filePathTypeRepository.FindByAbrv("scr-gallery");

            foreach (var galleryPath in galleryPathsToSave)
            {
                galleryPath.FilePathTypeId = filePathType.Id;
            }

            await _filePathRepository.DeleteAllAsync();
            await _filePathRepository.AddRangeAsync(galleryPathsToSave);
            await _filePathRepository.SaveChangesAsync();
            
            _notificationManager.Show(new Notification("Success", "Paths are successfully saved.", NotificationType.Success));
        }
        catch (Exception ex)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            _logger.LogError(ex.Message);
        }
    }

    private async void BtnRemovePath_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var galleryPathToRemove = (sender as Control)?.DataContext as FilePathModel;

            if (galleryPathToRemove == null)
                return;

            if (galleryPathToRemove.Id == 0)
            {
                LoadData();
                
                return;
            }
            
            await _filePathRepository.DeleteByIdAsync(galleryPathToRemove.Id);
            await _filePathRepository.SaveChangesAsync();
            LoadData();
            
            _notificationManager.Show(new Notification("Success", "Path is successfully deleted.", NotificationType.Success));
        }
        catch (Exception ex)
        {
            _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
            _logger.LogError(ex.Message);
        }
    }
}