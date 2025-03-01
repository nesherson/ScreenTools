using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using Avalonia.Controls;
using ReactiveUI;

namespace ScreenTools.App;

public partial class OptionsView : NotifyPropertyChangedWindowBase
{
    private ObservableCollection<string> _galleryPaths;
    
    public OptionsView()
    {
        InitializeComponent();


       

        RxApp.MainThreadScheduler.Schedule(LoadData);
    }

    public ObservableCollection<string> GalleryPaths
    {
        get => _galleryPaths;
        set => SetField(ref _galleryPaths, value);
    }

    private void LoadData()
    {
        GalleryPaths = new ObservableCollection<string>()
        {
            "Test 1",
            "Test 2"
        };
        this.Get<ListBox>("TestListBox").ItemsSource = GalleryPaths;
       
    }
    
}