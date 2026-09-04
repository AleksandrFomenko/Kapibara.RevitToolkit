using Kapibara.Core;
using Marking.Models;
using Microsoft.Extensions.DependencyInjection;
using Marking.Views;
using Marking.ViewModels;

namespace Marking;


public static class Host
{
    private static IServiceProvider? _serviceProvider;
    
    public static void Start()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddTransient<MarkingViewModel>();
        services.AddTransient<MarkingView>();
        services.AddTransient<MarkingModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    public static T GetService<T>() where T : class
    {
        return _serviceProvider!.GetRequiredService<T>();
    }
}