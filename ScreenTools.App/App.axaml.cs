using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;
using SharpHook.Native;
using System;
using System.Drawing.Imaging;
using System.IO;

namespace ScreenTools.App
{
    public partial class App : Application
    {
        private SimpleGlobalHook? _hook;
        private WindowsToastService _toastService;
        private ScreenCaptureService _screenCaptureService;
        private IServiceProvider _serviceProvider;
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
        
        private void CaptureScreenshot()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "ScreenTools",
                "Captures");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            _screenCaptureService.CaptureScreenToFile(
                Path.Combine(path, $"Capture-{DateTime.Now:dd-MM-yyyy-hhss}.jpg"),
                ImageFormat.Jpeg);
        }

        private void ConfigureServices()
        {
            _toastService = _serviceProvider.GetRequiredService<WindowsToastService>();
            _screenCaptureService = _serviceProvider.GetRequiredService<ScreenCaptureService>();
            _hook = _serviceProvider.GetRequiredService<SimpleGlobalHook>();
            _hook.KeyPressed += Hook_KeyPressed;
            _hook.RunAsync();
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

        private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            switch (e.RawEvent.Mask)
            {
                case ModifierMask.LeftMeta when
                    e.Data.KeyCode == KeyCode.VcF2:
                    try
                    {
                        CaptureScreenshot();
                        _toastService.ShowMessage("Screenshot captured!");
                    }
                    catch (Exception)
                    {
                        _toastService.ShowMessage("An error occured!");
                    }

                    break;
                case ModifierMask.LeftMeta when
                    e.Data.KeyCode == KeyCode.VcF3:
                    if (!_isDrawingOverlayActive)
                        ShowDrawingOverlay();
                    break;
            }
        }
        
        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _hook?.Dispose();
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

        private void NativeMenuItem_OnClickOpenGallery(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var window = ActivatorUtilities.CreateInstance<GalleryView>(_serviceProvider);
                window.Show();
            });
        }
    }
}