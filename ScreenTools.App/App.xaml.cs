using System.Windows;

namespace ScreenTools.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ScreenToolsTrayApp? trayApp;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            trayApp = new ScreenToolsTrayApp(this);

            trayApp.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayApp?.Dispose();

            base.OnExit(e);
        }
    }
}