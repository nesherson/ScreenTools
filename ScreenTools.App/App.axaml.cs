using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenTools.Infrastructure;
using SharpHook;
using SharpHook.Native;

namespace ScreenTools.App
{
    public partial class App : Application
    {
        private SimpleGlobalHook? _hook;
        private ScreenCaptureService _screenCaptureService;
        private IServiceProvider _serviceProvider;
        private FilePathRepository _filePathRepository;
        private ILogger<App> _logger;
        
        private bool _isLeftMetaPressed;
        
        private bool _isDrawingOverlayActive;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            var collection = new ServiceCollection();
            
            collection.AddCommonServices();
            
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
                _isDrawingOverlayActive = true;
                var overlay = ActivatorUtilities.CreateInstance<DrawingOverlay>(_serviceProvider);
                overlay.Closed += (_, _) => _isDrawingOverlayActive = false; 
                overlay.Show();
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
                    if (!_isDrawingOverlayActive)
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
        
        private void NativeMenuItem_OnClickOpenGallery(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var window = ActivatorUtilities.CreateInstance<GalleryView>(_serviceProvider);
                window.Show();
            });
        }
        
        private void NativeMenuItem_OnClickOpenOptions(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var window = ActivatorUtilities.CreateInstance<OptionsView>(_serviceProvider);
                window.Show();
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