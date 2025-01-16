using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;

namespace SystemTools.App;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<SimpleGlobalHook>(_ => new SimpleGlobalHook(GlobalHookType.Keyboard));
        collection.AddTransient<WindowsToastService>();
        collection.AddTransient<ScreenCaptureService>();
    }
}