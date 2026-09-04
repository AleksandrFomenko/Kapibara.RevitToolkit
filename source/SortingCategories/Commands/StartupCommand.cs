using System.Windows.Navigation;
using Autodesk.Revit.Attributes;
using Kapibara.Core;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using SortingCategories.Model;
using SortingCategories.ViewModels;
using SortingCategories.Views;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using NavigationService = Wpf.Ui.NavigationService;


namespace SortingCategories.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand: ExternalCommand
{ 
    public override void Execute()
    {
        var document = RevitContext.ActiveDocument;
        var services = new ServiceCollection();
        if(document != null) services.AddSingleton(document);
        //Window & pages
        services.AddSingleton<SortingCategoriesView>();
        services.AddSingleton<MainFamilies>();
        services.AddSingleton<SubFamilies>();
        // vm
        services.AddSingleton<SortingCategoriesViewModel>();
        services.AddSingleton<SubFamiliesViewModel>();
        //Model
        services.AddSingleton<ParametersMainFamiliesModel>();
        services.AddSingleton<SubFamiliesModel>();
        // services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        services.AddSingleton<INavigationViewPageProvider, PageService>();

        var serviceProvider = services.BuildServiceProvider();
        
        var tws = serviceProvider.GetRequiredService<IThemeWatcherService>(); 
        var view = serviceProvider.GetRequiredService<SortingCategoriesView>(); 
        var view1 = serviceProvider.GetRequiredService<MainFamilies>(); 
        var view2 = serviceProvider.GetRequiredService<SubFamilies>();

        view.SourceInitialized += (sender, args) => tws.ApplyTheme();
        
        view.ShowDialog();
    }
}
