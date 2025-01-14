using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using SharpHook;
using SharpHook.Native;

namespace SystemTools.App
{
    public partial class App : Application
    {
        private TaskPoolGlobalHook _hook;
        
        public App()
        {
            TestCommand = ReactiveCommand.Create(Test);
        }
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            
            _hook = new TaskPoolGlobalHook(); 
            
            _hook.KeyPressed += Hook_KeyPressed;
            
            _hook.Run();
        
            base.OnFrameworkInitializationCompleted();
        }
        
        public ReactiveCommand<Unit, Unit> TestCommand { get; }
        
        public void CaptureScreenshot()
        {
            //var screenCapture = new ScreenCapture();
            //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            //    "ScreenTools",
            //    "Captures");

            //if (!Directory.Exists(path))
            //{
            //    Directory.CreateDirectory(path);
            //}

            //screenCapture.CaptureScreenToFile(path, ImageFormat.Jpeg);
        }
        
        public void Test()
        {
            Debug.WriteLine("CLICK!!!!!");        
        }
        
        private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            //KeyCode->VcSlash
            //RawEvent.Mask->LeftShift, LeftCtrl
            Debug.WriteLine($"KeyCode -> {e.Data.KeyCode}");
            Debug.WriteLine($"RawEvent.Mask -> {e.RawEvent.Mask}");

            if (e.RawEvent.Mask == (ModifierMask.LeftShift | ModifierMask.LeftCtrl) &&
                e.Data.KeyCode == KeyCode.VcSlash)
            {
                CaptureScreenshot();
            }
        }
    }
}