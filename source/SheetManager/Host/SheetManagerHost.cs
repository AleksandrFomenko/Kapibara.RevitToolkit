using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using SheetManager.Models;
using SheetManager.ViewModels;
using SheetManager.Views;
using Wpf.Ui.Abstractions;
using System.Windows.Interop;

namespace SheetManager.Host;

public static class SheetManagerHost
{
    private static ServiceProvider? _serviceProvider;
    private static IServiceScope? _scope;
    private static bool _isPreview;
    public static IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Host not started");
    public static IServiceScope Scope => _scope ?? throw new InvalidOperationException("Host not started");

    public static void Build(Document? document = null)
    {
        _isPreview = document == null;
        _scope?.Dispose();
        _scope = null;
        _serviceProvider?.Dispose();
        var services = new ServiceCollection();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddScoped<INavigationViewPageProvider, PageService>();
        services.AddScoped<SheetManagerView>();
        services.AddScoped<CreateSheetsView>();
        services.AddScoped<PrintSheetsView>();
        services.AddScoped<SheetManagerViewModel>();
        services.AddScoped<CreateSheetsViewModel>();
        services.AddScoped<PrintSheetsViewModel>();
        services.AddSingleton(new SheetManagerModel(document));
        _serviceProvider = services.BuildServiceProvider();
    }

    public static void Run(IntPtr owner = default)
    {
        var provider = _serviceProvider ?? throw new InvalidOperationException("Host not started");
        try
        {
            using var scope = provider.CreateScope();
            _scope = scope;
            var theme = scope.ServiceProvider.GetRequiredService<IThemeWatcherService>();
            if (_isPreview) theme.Initialize();
            var view = scope.ServiceProvider.GetRequiredService<SheetManagerView>();
            if (owner != IntPtr.Zero) new WindowInteropHelper(view).Owner = owner;
            view.SourceInitialized += (_, _) => theme.ApplyTheme();
            view.ShowDialog();
        }
        finally
        {
            _scope = null;
            provider.Dispose();
            _serviceProvider = null;
        }
    }

    public static T GetService<T>() where T : class => Scope.ServiceProvider.GetRequiredService<T>();
}
