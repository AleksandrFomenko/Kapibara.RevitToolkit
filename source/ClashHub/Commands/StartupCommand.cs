using Autodesk.Revit.Attributes;
using ClashHub.Services;
using ClashHub.Views;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using ClashDetectiveViewModel = ClashHub.ViewModels.ClashDetectiveViewModel;


namespace ClashHub.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        var document = RevitContext.ActiveDocument;
        var services = new ServiceCollection();

        if (document != null) services.AddSingleton(document);
        services.AddSingleton<IPickerElements, PickerElements>();
        services.AddSingleton<IElementColorizer, ElementColorizer>();
        services.AddSingleton<IClashViewCreator, ClashViewCreator>();
        services.AddSingleton<ClashDetectiveViewModel>();
        services.AddSingleton<ClashDetectiveView>();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();

        var serviceProvider = services.BuildServiceProvider();
        var view = serviceProvider.GetRequiredService<ClashDetectiveView>();
        var tws = serviceProvider.GetRequiredService<IThemeWatcherService>();
        view.SourceInitialized += (sender, args) => tws.ApplyTheme();
        view.Show(RevitContext.UiApplication.MainWindowHandle);
    }
}