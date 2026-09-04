using Axes.Models;
using Axes.ViewModels;
using Axes.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Axes.Host;

public static class Host
{
    public static void StartMock()
    {
        var services = new ServiceCollection();
        
        services.AddScoped<IThemeWatcherService, ThemeWatcherService>();
        services.AddScoped<AxesView>();
        services.AddScoped<AxesViewModel>();
        services.AddScoped<IAxesModel, AxesModelMock>();

        var sp = services.BuildServiceProvider();
        var tws = sp.GetRequiredService<IThemeWatcherService>();
        var view = sp.GetRequiredService<AxesView>();

        view.SourceInitialized += (s, e) => tws.ApplyTheme();
        view.Show();
    }
    
    public static void Start()
    {
        var services = new ServiceCollection();
        
        services.AddScoped<IThemeWatcherService, ThemeWatcherService>();
        services.AddScoped<AxesView>();
        services.AddScoped<AxesViewModel>();
        services.AddScoped<IAxesModel, AxesModel>();

        var sp = services.BuildServiceProvider();
        var tws = sp.GetRequiredService<IThemeWatcherService>();
        var view = sp.GetRequiredService<AxesView>();

        view.SourceInitialized += (s, e) => tws.ApplyTheme();
        view.Show(RevitContext.UiApplication.MainWindowHandle);
    }
}