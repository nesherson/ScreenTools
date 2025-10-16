using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenTools.Infrastructure;
using SharpHook;
using SharpHook.Native;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace ScreenTools.App
{
    public partial class App : Application
    {
        private SimpleGlobalHook? _hook;
        private ScreenCaptureService _screenCaptureService;
        private IServiceProvider _serviceProvider;
        private FilePathRepository _filePathRepository;
        private ILogger<App> _logger;
        private DrawingOverlay? _drawingOverlay;
        private Window? _mainWindow;
        private bool _isLeftMetaPressed;
        private bool _isDrawingOverlayHidden;
        private bool _isMainWindow;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            var collection = new ServiceCollection();
            
            collection.AddCommonServices();
            collection.AddViewModels();
            
            _serviceProvider = collection.BuildServiceProvider();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.Exit += OnExit;
            }
            
            ConfigureServices();
            
            base.OnFrameworkInitializationCompleted();
        }
        
        private void ConfigureServices()
        {
            _screenCaptureService = _serviceProvider.GetRequiredService<ScreenCaptureService>();
            _filePathRepository = _serviceProvider.GetRequiredService<FilePathRepository>();
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            _hook = _serviceProvider.GetRequiredService<SimpleGlobalHook>();
            _hook.KeyPressed += Hook_KeyPressed;
            _hook.RunAsync();
        }
        
        private async Task CaptureScreenshot()
        {
            var filePath = await _filePathRepository.GetByFilePathTypeAbrvAsync("scr-gallery");

            if (!Directory.Exists(filePath.Path))
            {
                Directory.CreateDirectory(filePath.Path);
            }
            
            _screenCaptureService.CaptureScreenToFile(
                Path.Combine(filePath.Path, $"Screenshot-{DateTime.Now:dd-MM-yyyy-hhss}.jpg"),
                ImageFormat.Jpeg);
        }

        private void ShowDrawingOverlay()
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (_drawingOverlay is not null && _isDrawingOverlayHidden)
                {
                    _drawingOverlay.Show();

                    return;
                }
                
                _drawingOverlay = ActivatorUtilities.CreateInstance<DrawingOverlay>(_serviceProvider);
                _drawingOverlay.Activated += (_, _) => _isDrawingOverlayHidden = false; 
                _drawingOverlay.Hidden += (_, _) => _isDrawingOverlayHidden = true;
                
                _drawingOverlay.Show();
            });
        }

        private async void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == KeyCode.VcF6)
            {
                try
                {
                    await CaptureScreenshot();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to capture screenshot. Exception: {ex}");
                }

                return;
            }
            
            if (e.Data.KeyCode == KeyCode.VcLeftMeta)
            {
                _isLeftMetaPressed = true;
                return;
            }

            if (!_isLeftMetaPressed) 
                return;
            
            _isLeftMetaPressed = false;
            
            switch (e.Data.KeyCode)
            {
                case 
                    KeyCode.VcF2:
                    try
                    {
                        await CaptureScreenshot();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to capture screenshot. Exception: {ex}");
                    }

                    break;
                case 
                    KeyCode.VcBackQuote:
                    ShowDrawingOverlay();
                    break;
            }
        }
        
        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            CanvasHelpers.DeleteSavedCanvas(_serviceProvider
                .GetRequiredService<IConfiguration>()["CanvasFilePath"]);
            
            _hook?.Dispose();
        }
        
        private void TrayIcon_OnClicked(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (_mainWindow is not null)
                {
                    return;
                }
                
                _mainWindow = ActivatorUtilities.CreateInstance<MainView>(_serviceProvider);
                
                _mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
                _mainWindow.Activated += (_, _) => _isDrawingOverlayHidden = false; 
                
                _mainWindow.Show();
            });
        }
        
        private void NativeMenuItem_OnClickOpenDrawingOverlay(object? sender, EventArgs e)
        {
            ShowDrawingOverlay();
        }
        
        private async void NativeMenuItem_OnClickCaptureScreenshot(object? sender, EventArgs e)
        {
            try
            {
                await Task.Delay(2000);
                await CaptureScreenshot();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to capture screenshot. Exception: {ex}");
            }
        }
        
        private void NativeMenuItem_OnClickExitApplication(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            });
        }
    }
}