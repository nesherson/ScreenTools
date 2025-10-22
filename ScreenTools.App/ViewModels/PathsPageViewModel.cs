using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ScreenTools.Infrastructure;

namespace ScreenTools.App;

public partial class PathsPageViewModel : PageViewModel
{
    private readonly FilePathRepository _filePathRepository;
    private readonly FilePathTypeRepository _filePathTypeRepository;
    private readonly ILogger<PathsPageViewModel> _logger;
    private readonly DialogService _dialogService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private ObservableCollection<FilePathViewModel> _galleryPaths;

    [ObservableProperty] 
    private FilePathViewModel? _selectedFilePath;

    public PathsPageViewModel()
    {
        PageName = ApplicationPageNames.Paths;
    }
    
    public PathsPageViewModel(FilePathRepository filePathRepository,
        FilePathTypeRepository filePathTypeRepository,
        ILogger<PathsPageViewModel> logger,
        DialogService dialogService,
        MainViewModel mainViewModel)
    {
        PageName = ApplicationPageNames.Paths;
        
        _filePathRepository = filePathRepository;
        _filePathTypeRepository = filePathTypeRepository;
        _logger = logger;
        _dialogService = dialogService;
        _mainViewModel  = mainViewModel;
        
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
    private async Task OpenAddPathDialog()
    {
        var addFilePathDialogViewModel = new AddFilePathDialogViewModel(_filePathTypeRepository,
            _filePathRepository, _logger)
        {
            DialogWidth = 520
        };
        
        await _dialogService.ShowDialog(_mainViewModel, addFilePathDialogViewModel);
        await LoadData();
    }
    
    [RelayCommand]
    private async Task OpenEditPathDialog()
    {
        if (SelectedFilePath is null)
            return;
        
        var addFilePathDialogViewModel = new AddFilePathDialogViewModel(_filePathTypeRepository, 
            _filePathRepository, _logger, SelectedFilePath.Id);
        
        await _dialogService.ShowDialog(_mainViewModel, addFilePathDialogViewModel);
        await LoadData();
    }
    
    
    [RelayCommand]
    private async Task DeletePath()
    {
        if (SelectedFilePath == null)
        {
            return;
        }

        var confirmDialogViewModel = new ConfirmDialogViewModel();
        
        await _dialogService
            .ShowDialog(_mainViewModel, confirmDialogViewModel);

        if (!confirmDialogViewModel.Confirmed)
            return;
        
        try
        {
            await _filePathRepository.DeleteByIdAsync(SelectedFilePath.Id);
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