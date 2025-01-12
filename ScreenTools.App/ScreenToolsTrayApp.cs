using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ScreenTools.App
{
    public class ScreenToolsTrayApp : IDisposable
    {
        private const string trayIconPath = "pack://application:,,,/ScreenTools.App;component/Icons/trayIcon.ico";
        private const string settingsIconPath = "pack://application:,,,/ScreenTools.App;component/Icons/settings.ico";
        private const string exitIconPath = "pack://application:,,,/ScreenTools.App;component/Icons/exit.ico";

        private readonly Application _application;

        private TaskbarIcon? _trayIcon;

        public ScreenToolsTrayApp(Application application)
        {
            _application = application;
        }

        public void Start()
        {
            _trayIcon = new TaskbarIcon
            {
                ToolTip = "Screen tools",
                Icon = new BitmapImage(new Uri(trayIconPath)).ToIcon(),
                ContextMenu = CreateContextMenu()
            };
        }

        private ContextMenu CreateContextMenu()
        {
            var optionsIcon = new BitmapImage(new Uri(settingsIconPath)).ToIcon();
            var exitIcon = new BitmapImage(new Uri(exitIconPath)).ToIcon();

            return new ContextMenu
            {
                ItemsSource = new List<MenuItem>
                {
                    new() { Header = "Options", Icon = optionsIcon, Command = new RelayCommand(OpenOptions) },
                    new() { Header = "Exit", Icon = exitIcon, Command = new RelayCommand(ExitApplication)}
                }
            };
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
        }

        private void ExitApplication()
        {
            _application.Shutdown();
        }

        private void OpenOptions()
        {
            _application.MainWindow = new MainWindow();
            _application.MainWindow.Show();
        }
    }
}