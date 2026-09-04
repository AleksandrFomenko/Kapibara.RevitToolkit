using ActiveView.Models;
using ActiveView.ViewModels;
using ActiveView.Views;
using Autodesk.Revit.Attributes;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;


namespace ActiveView.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        var services = new ServiceCollection();
        var document = RevitContext.ActiveDocument;
        if (document != null)
        {
            services.AddSingleton(document); 
        }

        services.AddScoped<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<ActiveViewModel>();
        services.AddSingleton<ActiveViewViewModel>();
        services.AddSingleton<ActiveViewView>();

        var serviceProvider = services.BuildServiceProvider();
        var themeService = serviceProvider.GetRequiredService<IThemeWatcherService>();
        var view = serviceProvider.GetRequiredService<ActiveViewView>();
        themeService.ApplyTheme();
        view.SourceInitialized += (sender, args) => themeService.ApplyTheme();
        view.Show(RevitContext.UiApplication.MainWindowHandle);
    }
}
