using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScreenTools.Infrastructure;
using SharpHook;

namespace ScreenTools.App;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<SimpleGlobalHook>(_ => new SimpleGlobalHook(GlobalHookType.Keyboard));
        collection.AddTransient<WindowsToastService>();
        collection.AddTransient<ScreenCaptureService>();
        collection.AddTransient<ImageProcessingService>();
        
        collection.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build());

        collection.AddDbContext<ScreenToolsDbContext>((sp, opt) =>
        {
            opt.UseSqlite(sp
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("ScreenToolsConnection"));
        });

        collection.AddTransient<GalleryPathRepository>();
    }
}