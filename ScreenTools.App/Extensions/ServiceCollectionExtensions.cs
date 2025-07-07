using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenTools.Infrastructure;
using SharpHook;

namespace ScreenTools.App;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<SimpleGlobalHook>(_ => new SimpleGlobalHook(GlobalHookType.Keyboard));
        collection.AddTransient<ScreenCaptureService>();
        collection.AddTransient<TextDetectionService>();
        collection.AddTransient<DrawingHistoryService>();
        collection.AddLogging(builder => builder.AddFile("Logs/ScreenTools-{Date}.txt"));
        
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
        
        collection.AddTransient<DrawingOverlayViewModel>();
    }
}