using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Core;
using ScreenTools.Infrastructure;

namespace ScreenTools.App;

public partial class PathsPageViewModel : PageViewModel
{
    private readonly FilePathRepository _filePathRepository;
    private readonly FilePathTypeRepository _filePathTypeRepository;
    private readonly ILogger<PathsPageViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<FilePathViewModel> _galleryPaths;

    public PathsPageViewModel()
    {
        PageName = ApplicationPageNames.Paths;
    }
    
    public PathsPageViewModel(FilePathRepository filePathRepository,
        FilePathTypeRepository filePathTypeRepository,
        ILogger<PathsPageViewModel> logger)
    {
        PageName = ApplicationPageNames.Paths;
        
        _filePathRepository = filePathRepository;
        _filePathTypeRepository = filePathTypeRepository;
        _logger = logger;
        
        RxApp.MainThreadScheduler.ScheduleAsync(async (_, _) => await LoadData());
    }
    
    private async Task LoadData()
    {
        try
        {
            var galleryPaths = new ObservableCollection<FilePathViewModel>();
            var result = await _filePathRepository
                .GetAllAsync("FilePathType");
    
            foreach (var item in result)
            {
                galleryPaths.Add(new FilePathViewModel(item));
            }
            
            GalleryPaths = galleryPaths;
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to load gallery paths. Exception: {ex}");
        }
    }

    [RelayCommand]
    private void AddPathOnClick()
    {
        GalleryPaths.Add(new FilePathViewModel
        {
            Id = 0, 
            Path = string.Empty
        });
    }

    [RelayCommand]
    private async Task ChoosePathOnClick()
    {
        var folders = await WeakReferenceMessenger.Default
            .Send(new ShowFolderPickerMessage
            {
                AllowMultiple = false,
                Title = "Choose path"
            });
        
        var path = folders.FirstOrDefault()?.Path.AbsolutePath;
        
        if (string.IsNullOrEmpty(path))
            return;
        
        GalleryPaths.Add(new FilePathViewModel
        {
            Id = 0,
            Path = Uri.UnescapeDataString(path.Replace("/", "\\"))
        });
    }

    [RelayCommand]
    private async Task SavePathsOnClick()
    {
        try
        {
            if (GalleryPaths.Any(x => Path.IsPathRooted(x.Path) == false))
            {
                ShowWindowNotifcation("Error", "The given paths are not valid.", NotificationType.Error);
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
            await LoadData();

            ShowWindowNotifcation("Success", "Paths are successfully saved.", NotificationType.Success);
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to save gallery paths. Exception: {ex}");
        }
    }

    [RelayCommand]
    private async Task RemovePathOnClick(FilePathViewModel filePathViewModel)
    {
        try
        {
            if (filePathViewModel.Id == 0)
            {
                await LoadData();
                
                return;
            }
            
            await _filePathRepository.DeleteByIdAsync(filePathViewModel.Id);
            await _filePathRepository.SaveChangesAsync();
            await LoadData();
            
            ShowWindowNotifcation("Success", "Path is successfully deleted.", NotificationType.Success);
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to remove gallery path. Exception: {ex}");
        }
    }
}