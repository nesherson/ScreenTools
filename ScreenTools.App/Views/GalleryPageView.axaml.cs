using System;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Clowd.Clipboard;
using CommunityToolkit.Mvvm.Messaging;

namespace ScreenTools.App;

public partial class GalleryPageView : UserControl
{
    public GalleryPageView()
    {
        InitializeComponent();
        
        WeakReferenceMessenger.Default
            .Register<PreviewGalleryImageMessage>(this, HandlePreviewGalleryImageMessage);
    }
    
    private void HandlePreviewGalleryImageMessage(object recipient, PreviewGalleryImageMessage message)
    {
        var window = new Window
        {
            Width = 1280,
            Height = 720,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = new Image
            {
                Source = new Bitmap(message.GalleryImagePath)
            }
        };

        if (VisualRoot is Window windowVisual)
        {
            window.ShowDialog(windowVisual);
        }
    }
}