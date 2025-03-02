using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;

namespace ScreenTools.App;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<SimpleGlobalHook>(_ => new SimpleGlobalHook(GlobalHookType.Keyboard));
        collection.AddTransient<WindowsToastService>();
        collection.AddTransient<ScreenCaptureService>();
        collection.AddTransient<IStorageService<string>, FileStorageService>(_ => new FileStorageService(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments ), "gallery-paths.txt")));
    }
}