using KapibaraUI.Services.Appearance;
using Microsoft.Extensions.DependencyInjection;
using VentilationInstallations.Model;
using VentilationInstallations.Views;
using VentilationInstallations.ViewModels;

namespace VentilationInstallations;

/// <summary>
///     Provides a host for the application's services and manages their lifetimes
/// </summary>
public static class Host
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    ///     Starts the host and configures the application's services
    /// </summary>
    public static void StartMock()
    {
        var services = new ServiceCollection();
        // services 
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        //MVVM
        services.AddTransient<VentilationInstallationsViewModel>();
        services.AddTransient<VentilationInstallationsView>();
        services.AddTransient<VentilationData>(_ => VentilationData.CreateMock());
        services.AddTransient<FormulaModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    public static void Start()
    {
        var services = new ServiceCollection();
        // services 
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        //MVVM
        services.AddTransient<VentilationInstallationsViewModel>();
        services.AddTransient<VentilationInstallationsView>();
        services.AddTransient<VentilationData>(_ => VentilationData.Create());
        services.AddSingleton<SelectService>(_ => new SelectService(RevitContext.ActiveUiDocument!));
        services.AddTransient<FormulaModel>();
        

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    ///     Get service of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of service object to get</typeparam>
    /// <exception cref="System.InvalidOperationException">There is no service of type <typeparamref name="T"/></exception>
    public static T GetService<T>() where T : class
    {
        return _serviceProvider!.GetRequiredService<T>();
    }
}