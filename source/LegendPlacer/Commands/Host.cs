using Kapibara.Core;
using LegendPlacer.Models;
using LegendPlacer.Services;
using LegendPlacer.ViewModels;
using LegendPlacer.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LegendPlacer.Commands;

public static class Host
{
    public static void Start()
    {
        var services = new ServiceCollection();
        var doc = RevitContext.ActiveDocument;
        if (doc != null)
        {
            services.AddSingleton(doc);
        }
        services.AddSingleton<ILegendPlacerModel, LegendPlacerModel>();
        services.AddSingleton<IThemeWatcherService,ThemeWatcherService>();
        services.AddSingleton<SheetOrganizationService>();
        services.AddSingleton<LegendPlacerViewModel>();
        services.AddSingleton<LegendPlacerView>();

        
        var serviceProvider = services.BuildServiceProvider();

        var view = serviceProvider.GetService<LegendPlacerView>();
        var tws = serviceProvider.GetService<IThemeWatcherService>();
        view!.SourceInitialized += (sender, args) => tws?.ApplyTheme();
        view.ShowDialog();
    }
    
    public static void StartTestUi()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILegendPlacerModel, LegendPlacerModelMock>();
        services.AddSingleton<IThemeWatcherService,ThemeWatcherService>();
        services.AddSingleton<LegendPlacerViewModel>();
        services.AddSingleton<LegendPlacerView>();
        
        var serviceProvider = services.BuildServiceProvider();

        var view = serviceProvider.GetService<LegendPlacerView>();
        view?.Show();
    }
}