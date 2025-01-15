using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;
using SharpHook.Native;

namespace SystemTools.App
{
    public partial class App : Application
    {
        private SimpleGlobalHook? _hook;
        private WindowsToastService _toastService;
        private ScreenCaptureService _screenCaptureService;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            var collection = new ServiceCollection();
            
            collection.AddCommonServices();
            
            var serviceProvider = collection.BuildServiceProvider();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.Exit += OnExit;
            }
            
            ConfigureServices(serviceProvider);
        
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

        private void ConfigureServices(IServiceProvider serviceProvider)
        {
            _toastService = serviceProvider.GetRequiredService<WindowsToastService>();
            _screenCaptureService = serviceProvider.GetRequiredService<ScreenCaptureService>();
            _hook = serviceProvider.GetRequiredService<SimpleGlobalHook>();
            _hook.KeyPressed += Hook_KeyPressed;
            _hook.RunAsync();
        }

        private void ShowOverlay()
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var overlay = new DrawingOverlay();
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
                    ShowOverlay();
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
    }
}