using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using Settings.ViewModels;
using Settings.Views;

namespace Settings;

public static class Host
{
    private static IServiceProvider? _serviceProvider;
    public static IServiceProvider Services => _serviceProvider 
                                               ?? throw new InvalidOperationException("Host not started");
    private static IServiceScope? _serviceScope;

    public static void Register()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddScoped<SettingsViewModel>();
        services.AddScoped<SettingsView>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static void Run()
    {
        _serviceScope = _serviceProvider!.CreateScope();
        var view = GetService<SettingsView>();
        view.ShowDialog();
    }
    
    public static T GetService<T>() where T : notnull => _serviceProvider!.GetRequiredService<T>();
}