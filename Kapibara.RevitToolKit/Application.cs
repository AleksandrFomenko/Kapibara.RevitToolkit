using Kapibara.Core;
using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using EngineeringSystems.Commands;


namespace Kapibara.RevitToolKit;

[UsedImplicitly]
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        InitializeTheme();
        CreateRibbon(); 
        GroupSystems.StartHost();
    }

    private void CreateRibbon()
    {
        var panelSettings = Application.CreatePanel("Commands", "Kapibara");
        var panelBim = Application.CreatePanel("BIM", "Kapibara");
        var panelGeneral = Application.CreatePanel("Общие", "Kapibara");
        var panelMepGeneral = Application.CreatePanel("MEP", "Kapibara");
        
        //Settings
        panelSettings.AddPushButton<Settings.Commands.StartupCommand>("Settings")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Settings32.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/Settings32.png");
        
        //BIM
        panelBim.AddPushButton<ExporterModels.Commands.StartupCommand>("Export\nmodels")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Export models.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/Export models.png");
        panelBim.AddPushButton<ClashHub.Commands.StartupCommand>("Clash\nNavigator")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/ClashDetective.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/ClashDetective.png");
        
        var stackPanelBim = panelBim.AddStackPanel();
        stackPanelBim.AddPushButton<WorkSetLinkFiles.Commands.StartupCommand>("Worksets")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Worksets.png");
        
        //General
        panelGeneral.AddPushButton<SheetManager.Commands.StartupCommand>("Sheet\nManager")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/SheetManager.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/SheetManager.png");
        panelGeneral.AddPushButton<ImportExcelByParameter.Commands.StartupCommand>("Import\nExcel")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/ImportFromExcel.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/ImportFromExcel.png");
        panelGeneral.AddPushButton<LevelByFloor.Commands.StartupCommand>("Level\nby floor")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/LevelByFloor.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/LevelByFloor.png");

        var stackPanel1 = panelGeneral.AddStackPanel();
        stackPanel1.AddPushButton<ViewByParameter.Commands.StartupCommand>("Filter view")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Filter view.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/Filter view.png");
        stackPanel1.AddPushButton<LegendPlacer.Commands.StartupCommand>("Legend placer")
            .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Legend placer.png")
            .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/Legend placer.png");
        UpdateRibbonButton<ViewByParameter.Commands.StartupCommand>("Kapibara", "Общие");
        UpdateRibbonButton<LegendPlacer.Commands.StartupCommand>("Kapibara", "Общие");
        
        var stackPanel = panelGeneral.AddStackPanel();
           stackPanel.AddPushButton<SortingCategories.Commands.StartupCommand>("Sorting")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Sorting.png");
           stackPanel.AddPushButton<SolidIntersection.Commands.SolidIntersection>("Intersection")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Intersector.png");
           stackPanel.AddPushButton<ActiveView.Commands.StartupCommand>("Active view")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/ActiveView.png")
               .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/ActiveView.png");
            

           var stackPanelAxesLevels = panelGeneral.AddStackPanel();
           stackPanelAxesLevels.AddPushButton<Axes.Commands.StartupCommand>("Оси 2")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/AxesFirst.png");
           
           stackPanelAxesLevels.AddPushButton<AxesLevels.Commands.AxesStartupCommand>("Оси")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Axes.png");
           stackPanelAxesLevels.AddPushButton<AxesLevels.Commands.LevelsStartupCommand>("Уровни")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Levels.png");


           //MEP общие
           panelMepGeneral.AddPushButton<StartupCommandEngineeringSystems>("System\nname")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/SystemName.png")
               .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/SystemName.png");
           
           panelMepGeneral.AddPushButton<StartupCommandGroupSystems>("System\ngroup")
               .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/SystemGroup16.png")
               .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/SystemGroup32.png");
           
         panelMepGeneral.AddPushButton<RiserMate.Commands.StartupCommand>("Riser\nMate")
             .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/RiserMate16.png")
             .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/RiserMate32.png");
         panelMepGeneral.AddPushButton<Marking.Commands.StartupCommand>("Marking")
             .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Marking 16.png")
             .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/Marking 32.png");
         panelMepGeneral.AddPushButton<VentilationInstallations.Commands.StartupCommand>("Вент.\nустановки")
             .SetImage("/Kapibara.RevitToolKit;component/Resources/Icons/Cooler32.png")
             .SetLargeImage("/Kapibara.RevitToolKit;component/Resources/Icons/Cooler32.png");
    }
    
    private static void InitializeTheme()
    {
        var themeManager = new ThemeWatcherService();
        themeManager.Initialize();
    }
    
    private static void UpdateRibbonButton(string tabId, string panelName, string? commandId)
    {
        foreach (RibbonTab tab in ComponentManager.Ribbon.Tabs)
        {
            if (tab.KeyTip != null)
                continue;

            if (tab.Id == tabId)
            {
                foreach (var panel in tab.Panels)
                {
                    if (panel.Source.Name == panelName)
                    {
                        foreach (object item in panel.Source.ItemsView)
                        {
                            if (item is Autodesk.Windows.RibbonButton ribbonButton &&
                                ribbonButton.Id == $"CustomCtrl_%CustomCtrl_%{tabId}%{panelName}%{commandId}")
                            {
                                ribbonButton.Size = RibbonItemSize.Large;
                                   
                            }
                        }
                    }
                }
            }
        }
    }
    
    private static void UpdateRibbonButton<TCommand>(string tabId, string panelName) where TCommand : IExternalCommand
    {
        UpdateRibbonButton(tabId, panelName, typeof(TCommand).FullName);
    }
}
