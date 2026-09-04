#nullable enable
using EngineeringSystems.Model;
using EngineeringSystems.Model.Abstractions;
using EngineeringSystems.Model.Mock;
using EngineeringSystems.services;
using EngineeringSystems.ViewModels;
using EngineeringSystems.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace EngineeringSystems.Commands;

public static class GroupSystems
{
    private static IServiceProvider? _serviceProvider;
    private static IServiceScope? _scope;

    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("Host not started");

    public static IServiceProvider ScopeServices =>
        _scope?.ServiceProvider ?? throw new InvalidOperationException("Scope not created");

    public static void StartHostMock()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<WindowProvider>();
        services.AddScoped<GroupSystemsView>();
        services.AddScoped<GroupSystemsViewModel>();
        services.AddScoped<IGroupSystemsModel, GroupSystemsMockModel>();

        _serviceProvider = services.BuildServiceProvider();
    }
    
    public static void StartHost()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<WindowProvider>();
        services.AddScoped<GroupSystemsView>();
        services.AddScoped<GroupSystemsViewModel>();
        services.AddScoped<IGroupSystemsModel, GroupSystemsModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static void StartMock()
    {
        if (_serviceProvider is null)
            throw new InvalidOperationException("Host not started");

        _scope?.Dispose();
        _scope = _serviceProvider.CreateScope();

        var sp = _scope.ServiceProvider;

        var tws = sp.GetRequiredService<IThemeWatcherService>();
        var view = sp.GetRequiredService<GroupSystemsView>();
        var windowProvider = sp.GetRequiredService<WindowProvider>();

        windowProvider.SetWindowOwner(view);

        view.Show();

        var theme = ApplicationTheme.Dark;
        var backdrop = WindowBackdropType.Tabbed;

        ApplicationThemeManager.Apply(theme, backdrop);
        WindowBackgroundManager.UpdateBackground(view, theme, backdrop);

        view.Closed += (_, _) =>
        {
            _scope?.Dispose();
            _scope = null;
        };
    }
    
    public static void Start()
    {
        if (_serviceProvider is null)
            throw new InvalidOperationException("Host not started");

        
        _scope = _serviceProvider.CreateScope();
        
        var sp = _scope.ServiceProvider;

        var tws = sp.GetRequiredService<IThemeWatcherService>();
        var view = sp.GetRequiredService<GroupSystemsView>();
        var windowProvider = sp.GetRequiredService<WindowProvider>();

        windowProvider.SetWindowOwner(view);

        view.SourceInitialized += (_, _) => tws.ApplyTheme();
        view.Closed += (_, _) =>
        {
            _scope?.Dispose();
        };

        view.ShowDialog();
    }
}