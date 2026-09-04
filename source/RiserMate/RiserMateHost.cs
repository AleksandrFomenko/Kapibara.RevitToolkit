using System.Windows;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using RiserMate.Abstractions;
using RiserMate.Implementation;
using RiserMate.Lookups;
using RiserMate.Models.Revit;
using RiserMate.Views;
using RiserMate.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Abstractions;

namespace RiserMate;

public static class RiserMateHost
{
    private static IServiceProvider? _serviceProvider;
    public static Window? Window { get; private set; }
    
    public static void StartServices()
    {
        
        var services = new ServiceCollection();
        
        services.AddSingleton<RiserMateViewModel>();
        services.AddSingleton<RiserMateView>();
        
        services.AddSingleton<RiserCreator>();
        services.AddSingleton<RizerCreatorViewModel>();
        services.AddSingleton<IModelRiserCreator, ModelRiserMateCreator>();
        
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<INavigationViewPageProvider, PageService>();
        
        services.AddSingleton<IViewCreationService, ViewCreationService>();
        services.AddSingleton<IFilterCreationService, FilterCreationService>();
        
        services.AddSingleton(_ =>
            new PluginConfig<RiserMateConfig>("RiserMateConfig", "config.json"));
        
        _serviceProvider = services.BuildServiceProvider();
        
        var theme = _serviceProvider.GetService<IThemeWatcherService>();
        Window = _serviceProvider.GetService<RiserMateView>();
        
        theme?.ApplyTheme();
        if (Window != null) Window.SourceInitialized += (sender, args) => theme?.ApplyTheme();
    }

    public static void Show() => Window?.Show(RevitContext.UiApplication.MainWindowHandle);
}