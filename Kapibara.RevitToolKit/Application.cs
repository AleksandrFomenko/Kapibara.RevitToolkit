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
        var panelInfo = Application.CreatePanel("Разное", "Kapibara");
        
        //Settings
        panelSettings.AddPushButton<Settings.Commands.StartupCommand>("Settings")
            .SetImage("/KapibaraV2;component/Resources/Icons/Settings32.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/Settings32.png");
        
        //BIM
        panelBim.AddPushButton<ExporterModels.Commands.StartupCommand>("Export\nmodels")
            .SetImage("/KapibaraV2;component/Resources/Icons/ExportModels.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/ExportModels.png");
        panelBim.AddPushButton<ClashHub.Commands.StartupCommand>("Clash\nNavigator")
            .SetImage("/KapibaraV2;component/Resources/Icons/ClashDetective.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/ClashDetective.png");
        
        var stackPanelBim = panelBim.AddStackPanel();
        stackPanelBim.AddPushButton<WorkSetLinkFiles.Commands.StartupCommand>("Worksets")
            .SetImage("/KapibaraV2;component/Resources/Icons/WorksetLinkFiles.png");
        
        //General
        panelGeneral.AddPushButton<ImportExcelByParameter.Commands.StartupCommand>("Import\nExcel")
            .SetImage("/KapibaraV2;component/Resources/Icons/ImportExcel.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/ImportExcel.png");
        panelGeneral.AddPushButton<LevelByFloor.Commands.StartupCommand>("Level\nby floor")
            .SetImage("/KapibaraV2;component/Resources/Icons/LevelByFloor.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/LevelByFloor.png");

        var stackPanel1 = panelGeneral.AddStackPanel();
        stackPanel1.AddPushButton<ViewByParameter.Commands.StartupCommand>("Filter view")
            .SetImage("/KapibaraV2;component/Resources/Icons/ViewByParameter.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/ViewByParameter.png");
        stackPanel1.AddPushButton<LegendPlacer.Commands.StartupCommand>("Legend placer")
            .SetImage("/KapibaraV2;component/Resources/Icons/LedendPlacer.png")
            .SetLargeImage("/KapibaraV2;component/Resources/Icons/LedendPlacer.png");
        UpdateRibbonButton<ViewByParameter.Commands.StartupCommand>("Kapibara", "Общие");
        UpdateRibbonButton<LegendPlacer.Commands.StartupCommand>("Kapibara", "Общие");
        
        var stackPanel = panelGeneral.AddStackPanel();
           stackPanel.AddPushButton<SortingCategories.Commands.StartupCommand>("Sorting")
               .SetImage("/KapibaraV2;component/Resources/Icons/Sort.png");
           stackPanel.AddPushButton<SolidIntersection.Commands.SolidIntersection>("Intersection")
               .SetImage("/KapibaraV2;component/Resources/Icons/intersector.png");
           stackPanel.AddPushButton<ActiveView.Commands.StartupCommand>("Active view")
               .SetImage("/KapibaraV2;component/Resources/Icons/ActiveView.png");
           //stackPanel.AddPushButton<ColorsByParameters.Commands.StartupCommand>("Цвета")
               //.SetImage("/KapibaraV2;component/Resources/Icons/SystemName.png");

           var stackPanelAxesLevels = panelGeneral.AddStackPanel();
           stackPanelAxesLevels.AddPushButton<Axes.Commands.StartupCommand>("Оси Ахмата")
               .SetImage("/KapibaraV2;component/Resources/Icons/Axes.png");;
           
           stackPanelAxesLevels.AddPushButton<AxesLevels.Commands.AxesStartupCommand>("Оси")
               .SetImage("/KapibaraV2;component/Resources/Icons/Axes.png");;
           stackPanelAxesLevels.AddPushButton<AxesLevels.Commands.LevelsStartupCommand>("Уровни")
               .SetImage("/KapibaraV2;component/Resources/Icons/Levels.png");;


           //MEP общие
           panelMepGeneral.AddPushButton<StartupCommandEngineeringSystems>("System\nname")
               .SetImage("/KapibaraV2;component/Resources/Icons/SystemName.png")
               .SetLargeImage("/KapibaraV2;component/Resources/Icons/SystemName.png");
           
           panelMepGeneral.AddPushButton<StartupCommandGroupSystems>("System\ngroup")
               .SetImage("/KapibaraV2;component/Resources/Icons/GroupSystems16.png")
               .SetLargeImage("/KapibaraV2;component/Resources/Icons/GroupSystems32.png");
           
         panelMepGeneral.AddPushButton<RiserMate.Commands.StartupCommand>("Riser\nMate")
             .SetImage("/KapibaraV2;component/Resources/Icons/RizerMate16.png")
             .SetLargeImage("/KapibaraV2;component/Resources/Icons/RizerMate32.png");
         panelMepGeneral.AddPushButton<Marking.Commands.StartupCommand>("Marking")
             .SetImage("/KapibaraV2;component/Resources/Icons/Mark16.png")
             .SetLargeImage("/KapibaraV2;component/Resources/Icons/Mark32.png");
         panelMepGeneral.AddPushButton<VentilationInstallations.Commands.StartupCommand>("Вент.\nустановки")
             .SetImage("/KapibaraV2;component/Resources/Icons/Cooler32.png")
             .SetLargeImage("/KapibaraV2;component/Resources/Icons/Cooler32.png");
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