using System.Windows;

namespace ScreenTools.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ScreenToolsTrayApp? _trayApp;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayApp = new ScreenToolsTrayApp(this);

            await _trayApp.StartAsync();  
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayApp?.Dispose();

            base.OnExit(e);
        }
    }
}