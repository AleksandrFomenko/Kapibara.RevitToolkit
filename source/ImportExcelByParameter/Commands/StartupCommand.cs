using Autodesk.Revit.Attributes;
using ImportExcelByParameter.Configuration;
using ImportExcelByParameter.Models;
using Nice3point.Revit.Toolkit.External;
using ImportExcelByParameter.ViewModels;
using ImportExcelByParameter.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;



namespace ImportExcelByParameter.Commands;


[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        var services = new ServiceCollection();
        var doc = RevitContext.ActiveDocument;
        if (doc != null) services.AddSingleton(doc);

        services.AddSingleton<PluginConfig<ImportExcelConfig>>();
        services.AddSingleton<ExcelByParameterModel>();
        services.AddSingleton<ImportExcelByParameterViewModel>();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<ImportExcelByParameterView>();

        var sp = services.BuildServiceProvider();
        var view = sp.GetRequiredService<ImportExcelByParameterView>();
        var tws = sp.GetRequiredService<IThemeWatcherService>();
        tws.ApplyTheme();
        view.SourceInitialized += (_, _) => tws.ApplyTheme();
        view.Show(RevitContext.UiApplication.MainWindowHandle);
    }
}