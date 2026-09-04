using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using SolidIntersection.Models;
using SolidIntersection.ViewModels;
using SolidIntersection.Views;

namespace SolidIntersection.Commands;

public static class Host
{
    public static void Start()
    {
        var services = new ServiceCollection();

        var doc = RevitContext.ActiveDocument;
        if (doc == null) return;

        services.AddSingleton(doc);
        
        services.AddSingleton<ISolidIntersectionModel, SolidIntersectionModel>();
        services.AddSingleton<SolidIntersectionView>();
        services.AddSingleton<SolidIntersectionViewModel>();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        
        var serviceProvider = services.BuildServiceProvider();
        var view = serviceProvider.GetRequiredService<SolidIntersectionView>();
        var tws = serviceProvider.GetRequiredService<IThemeWatcherService>();
        view.SourceInitialized += (sender, args) => tws.ApplyTheme();
        view.ShowDialog();
    }
}