using AxesLevels.Models;
using AxesLevels.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using ProjectAxes.Factories;
using ProjectAxes.ViewModels;
using ProjectAxes.Views;
using ProjectAxesModel = AxesLevels.Models.ProjectAxesModel;
using ProjectLevelsModel = AxesLevels.Models.ProjectLevelsModel;
using ProjectToolViewModel = AxesLevels.ViewModels.ProjectToolViewModel;

namespace AxesLevels.Commands;

public static class Host
{
    public static void StartAxes() => Bootstrap.StartTool<ProjectAxesModel>();
    public static void StartLevels() => Bootstrap.StartTool<ProjectLevelsModel>();
    public static void StartMock() => Bootstrap.StartTool<MockModel>();
}


public static class Bootstrap
{
    public static void StartTool<TModel>()
        where TModel : class, IModel
    {
        var services = new ServiceCollection();

        services.AddScoped<IThemeWatcherService, ThemeWatcherService>();
        if (typeof(TModel) != typeof(MockModel))
        {
            var doc = RevitContext.ActiveDocument 
                      ?? throw new InvalidOperationException("Нет активного документа Revit.");
            services.AddSingleton(doc);
        }

        services.AddTransient<TModel>();
        services.AddTransient<ProjectToolViewModel>();

        services.AddSingleton<IViewModelFactory, ViewModelFactory>();

        services.AddScoped<ProjectAxesView>();

        var sp = services.BuildServiceProvider();

        var factory = sp.GetRequiredService<IViewModelFactory>();
        var model   = sp.GetRequiredService<TModel>();
        var vm      = factory.Create(model);

        var view = sp.GetRequiredService<ProjectAxesView>();
        view.DataContext = vm;

        var tws = sp.GetRequiredService<IThemeWatcherService>();
        view.SourceInitialized += (s, e) => tws.ApplyTheme();

        view.Show(RevitContext.UiApplication.MainWindowHandle);
    }
}