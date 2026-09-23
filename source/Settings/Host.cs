using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using Settings.ViewModels;
using Settings.Views;
using System.Windows.Interop;

namespace Settings;

public static class Host
{
    private static IServiceProvider? _serviceProvider;
    public static IServiceProvider Services => _serviceProvider 
                                               ?? throw new InvalidOperationException("Host not started");

    public static void Register()
    {
        if (_serviceProvider is not null) return;

        var services = new ServiceCollection();

        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddScoped<SettingsViewModel>();
        services.AddScoped<SettingsView>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static void Run(IntPtr owner)
    {
        using var scope = Services.CreateScope();
        var view = scope.ServiceProvider.GetRequiredService<SettingsView>();
        new WindowInteropHelper(view).Owner = owner;
        view.ShowDialog();
    }
    
    public static T GetService<T>() where T : notnull => _serviceProvider!.GetRequiredService<T>();
}
