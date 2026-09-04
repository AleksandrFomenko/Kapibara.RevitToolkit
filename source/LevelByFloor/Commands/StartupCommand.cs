using Autodesk.Revit.Attributes;
using Kapibara.Core;
using LevelByFloor.Models;
using Nice3point.Revit.Toolkit.External;
using LevelByFloor.ViewModels;
using LevelByFloor.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LevelByFloor.Commands;

/// <summary>
///     External command entry point invoked from the Revit interface
/// </summary>
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {

        var doc = RevitContext.ActiveDocument;
        var services = new ServiceCollection();
        if (doc != null)
        {
            services.AddSingleton(doc);
        }

        services.AddSingleton<LevelByFloorModel>();
        services.AddSingleton<LevelByFloorViewModel>();
        services.AddSingleton<LevelByFloorView>();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();

        var serviceProvider = services.BuildServiceProvider();
        var view = serviceProvider.GetRequiredService<LevelByFloorView>();
        var tws = serviceProvider.GetRequiredService<IThemeWatcherService>();

        view.SourceInitialized += (sender, args) => tws.ApplyTheme();
        view.ShowDialog();
    }
}