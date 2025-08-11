using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ScreenTools.App;

public partial class GalleryPageView : UserControl
{
    public GalleryPageView()
    {
        InitializeComponent();
    }
    
    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {   
        var menuItem = sender as MenuItem;

        if (menuItem is null)
            return;

        var galleryImage = menuItem.DataContext as GalleryImage;
            
        if (galleryImage is null)
            return;

        switch (menuItem.Name)
        {
            case "Preview":
                HandlePreview(galleryImage);
                break;
            case "ShowInExplorer":
                HandleShowInExplorer(galleryImage);
                break;
            case "CopyToClipboard":
                HandleCopyToClipBoard(galleryImage);
                break;
        }
    }

    private async void HandleCopyToClipBoard(GalleryImage galleryImage)
    {
        // try
        // {
        //     var bitmap = new Bitmap(galleryImage.Path);
        //
        //     await ClipboardAvalonia.SetImageAsync(bitmap);
        // }
        // catch (Exception ex)
        // {
        //     _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
        //     _logger.LogError($"Failed to copy image to the clipboard. Exception: {ex}");
        // }
    }

    private void HandleShowInExplorer(GalleryImage galleryImage)
    {
        // try
        // {
        //     ProcessHelpers.ShowFileInFileExplorer(galleryImage.Path);
        // }
        // catch (Exception ex)
        // {
        //     _notificationManager.Show(new Notification("Error", "An error occured.", NotificationType.Error));
        //     _logger.LogError($"Failed to show image in explorer. Exception: {ex}");
        // }
    }

    private void HandlePreview(GalleryImage galleryImage)
    {
        // var window = new Window
        // {
        //     Width = 1280,
        //     Height = 720,
        //     WindowStartupLocation = WindowStartupLocation.CenterScreen,
        //     Content = new Image
        //     {
        //         Source = new Bitmap(galleryImage.Path)
        //     }
        // };
        //
        // window.ShowDialog(this);
    }
}