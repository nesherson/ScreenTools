using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
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

public partial class AddFilePathDialogViewModel : DialogViewModelBase
{
    private readonly FilePathTypeRepository _filePathTypeRepository;
    private readonly FilePathRepository _filePathRepository;
    private readonly ILogger<PathsPageViewModel>  _logger;
    private readonly int? _filePathId;
    
    [ObservableProperty] 
    private string _title;
    [ObservableProperty] 
    private string _description;
    [ObservableProperty]
    private ObservableCollection<FilePathTypeViewModel>? _filePathTypes;
    [ObservableProperty]
    private int? _selectedFileTypePathId;
    [ObservableProperty] 
    private string _filePath;
    [ObservableProperty] 
    private bool _isEdit;

    public AddFilePathDialogViewModel()
    {
    }
    
    public AddFilePathDialogViewModel(FilePathTypeRepository filePathTypeRepository,
        FilePathRepository filePathRepository,
        ILogger<PathsPageViewModel> logger, 
        int? filePathId = null)
    {
        _filePathTypeRepository = filePathTypeRepository;
        _filePathRepository = filePathRepository;
        _logger = logger;
        
        _filePathId = filePathId;
        _isEdit = filePathId != null;
        
        Title = !IsEdit ? "Create file path" : "Edit file path";
        Description = !IsEdit ? "Create a new file path" : "Create file path";
        
        RxApp.MainThreadScheduler
            .ScheduleAsync(async (_, _) => await LoadFileTypesAsync());

        if (_isEdit)
        {
            RxApp.MainThreadScheduler
                .ScheduleAsync(async (_, _) => await LoadFilePathAsync());
        }
    }

    private async Task LoadFilePathAsync()
    {
        if (_filePathId is null)
            return;
        
        var result = await _filePathRepository.GetByIdAsync(_filePathId.Value);

        if (result is null)
            return;

        FilePath = result.Path;
        SelectedFileTypePathId = result.FilePathTypeId;
    }
    private async Task LoadFileTypesAsync()
    {
        var result = await _filePathTypeRepository
            .GetAll();
        FilePathTypes = result.Select(x => new FilePathTypeViewModel
        {
            Id = x.Id,
            Name = x.Name,
        })
        .ToObservable();
    }
    
    [RelayCommand]
    private async Task PickPath()
    {
        var folders = await WeakReferenceMessenger.Default
            .Send(new ShowFolderPickerMessage
            {
                AllowMultiple = false,
                Title = "Choose path"
            });
        
        var pickedPath = folders.FirstOrDefault()?.Path.AbsolutePath;
        
        if (string.IsNullOrEmpty(pickedPath))
            return;

        FilePath = Uri.UnescapeDataString(pickedPath.Replace("/", "\\"));
    }
    
    [RelayCommand]
    private async Task Save()
    {
        if (!Path.IsPathRooted(FilePath))
        {
            ShowWindowNotifcation("Error", "The given paths are not valid.", NotificationType.Error);
            
            return;
        }

        if (SelectedFileTypePathId is null)
        {
            ShowWindowNotifcation("Error", "File path type is not selected.", NotificationType.Error);
            
            return;
        }
        
        try
        {
            if (IsEdit)
            {
                var existingFilePath = await _filePathRepository
                    .GetByIdAsync(_filePathId!.Value);
                
                existingFilePath.Path = FilePath;
                existingFilePath.FilePathTypeId = SelectedFileTypePathId.Value;
                
                _filePathRepository.Update(existingFilePath);
            }
            else
            {
                var newFilePath = new FilePath
                {
                    Path = FilePath,
                    FilePathTypeId = SelectedFileTypePathId.Value
                };
            
                await _filePathRepository.AddAsync(newFilePath);
            }
            
            await _filePathRepository.SaveChangesAsync();
            ShowWindowNotifcation("Success", $"Path is successfully {(!IsEdit ? "created" : "updated!")}.", NotificationType.Success); 
        }
        catch (Exception ex)
        {
            ShowWindowNotifcation("Error", "An error occured.", NotificationType.Error);
            _logger.LogError($"Failed to {(!IsEdit ? "create" : "update")} file path. Exception: {ex}");
        }
        
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }
}