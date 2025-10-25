using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenTools.Core;
using ScreenTools.Infrastructure;
using SharpHook;

namespace ScreenTools.App;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<SimpleGlobalHook>(_ => new SimpleGlobalHook(GlobalHookType.Keyboard));
        collection.AddSingleton<IPageFactory, PageFactory>();
        collection.AddSingleton<DialogService>();
        collection.AddTransient<ScreenCaptureService>();
        collection.AddTransient<TextDetectionService>();
        collection.AddTransient<DrawingHistoryService>();
        collection.AddLogging(builder => builder.AddFile("Logs/ScreenTools-{Date}.txt"));
        
        collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(
            sp => pageName => pageName switch
            {
                ApplicationPageNames.Unknown => sp.GetRequiredService<HomePageViewModel>(),
                ApplicationPageNames.Home => sp.GetRequiredService<HomePageViewModel>(),
                ApplicationPageNames.Gallery => sp.GetRequiredService<GalleryPageViewModel>(),
                ApplicationPageNames.Paths => sp.GetRequiredService<PathsPageViewModel>(),
                ApplicationPageNames.Settings => sp.GetRequiredService<SettingsPageViewModel>(),
                ApplicationPageNames.CoordinatePlane => sp.GetRequiredService<CoordinatePlanePageViewModel>(),
                _ => sp.GetRequiredService<HomePageViewModel>()
            });
        
        collection.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build());

        collection.AddDbContext<ScreenToolsDbContext>((sp, opt) =>
        {
            opt.UseSqlite(sp
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("ScreenToolsConnection"));
        });

        collection.AddTransient<FilePathRepository>();
        collection.AddTransient<FilePathTypeRepository>();
    }

    public static void AddViewModels(this IServiceCollection collection)
    {
        collection.AddSingleton<MainViewModel>();
        collection.AddTransient<HomePageViewModel>();
        collection.AddTransient<PathsPageViewModel>();
        collection.AddTransient<DrawingOverlayViewModel>();
        collection.AddTransient<GalleryPageViewModel>();
        collection.AddTransient<SettingsPageViewModel>();
        collection.AddTransient<CoordinatePlanePageViewModel>();
    }
}