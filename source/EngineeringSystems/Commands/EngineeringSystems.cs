using EngineeringSystems.Configuration;
using EngineeringSystems.Model;
using EngineeringSystems.Model.Abstractions;
using EngineeringSystems.ViewModels;
using EngineeringSystems.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;


namespace EngineeringSystems.Commands;

public static class EngineeringSystems
{
    public static void Start()
    {
        var services = new ServiceCollection();

        var doc = RevitContext.ActiveDocument;
        if (doc != null)
        {
            services.AddSingleton(doc);
        }
        
        services.AddSingleton<IData, Data>();
        services.AddSingleton<IEngineeringSystemsModel, EngineeringSystemsModel>();
        services.AddSingleton<SystemNameConfig>();
        services.AddSingleton<EngineeringSystemsViewModel>();
        services.AddSingleton<EngineeringSystemsView>();
        services.AddSingleton<PluginConfig<SystemNameConfig>>();
        services.AddScoped<IThemeWatcherService, ThemeWatcherService>();
        
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var view = serviceProvider.GetRequiredService<EngineeringSystemsView>();
        var tws = serviceProvider.GetRequiredService<IThemeWatcherService>();
        view.SourceInitialized += (sender, args) => tws.ApplyTheme();
        view.ShowDialog();
    }
}