using ExporterModels.Abstractions;
using ExporterModels.services;
using ExporterModels.ViewModels;
using ExporterModels.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationService = ExporterModels.services.ConfigurationService;

namespace ExporterModels;

public static class Host
{
    private static IServiceProvider? _serviceProvider;

    public static void Start()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<IWindowOwnerProvider, WindowOwnerProvider>();
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<ExporterModelsViewModel>();
        services.AddSingleton<ExporterModelsView>();

        _serviceProvider = services.BuildServiceProvider();

        var view = GetService<ExporterModelsView>();
        var windowOwnerProvider = GetService<IWindowOwnerProvider>();
        windowOwnerProvider.SetOwner(view);
        var tws = GetService<IThemeWatcherService>();
        view.SourceInitialized += (sender, args) => tws.ApplyTheme();
        view.Show(RevitContext.UiApplication.MainWindowHandle);
        
    }
    
    public static T GetService<T>() where T : class
    {
        return _serviceProvider!.GetRequiredService<T>();
    }
}