using System.IO;
using Autodesk.Revit.UI;
using ExporterModels.Progress;

namespace ExporterModels.RevitExporters;

public class NwcExporter : RevitExporter
{
    public async Task ExportSingleAsync(
        UIApplication uiApp,
        string filePath,
        string directoryPath,
        string badNameWorkset,
        IProgress<ProgressInfo>? progress = null)
    {
        try
        {
            var doc = OpenDocumentAsDetach(filePath, badNameWorkset, false, false);
            if (doc != null)
            {
                ExportToNwc(doc, directoryPath);
                doc.Close(false);
            }

        }
        catch (Exception ex)
        { ;
            throw;
        }
    }

    private static void ExportToNwc(Document doc, string directoryPath)
    {
        if (doc == null) return;
        if (!Directory.Exists(directoryPath)) return;
        
        var exportView = GetAnySuitable3DView(doc);

        if (exportView == null) 
        {
            return;
        }
        
        var elementsInView = new FilteredElementCollector(doc, exportView.Id)
            .WhereElementIsNotElementType()
            .ToElementIds();

        if (!elementsInView.Any()) return;

        var options = new NavisworksExportOptions
        {
            ExportElementIds = true,
            Coordinates = NavisworksCoordinates.Shared,
            FacetingFactor = 1,
            ExportUrls = false,
            ConvertLights = false,
            ExportRoomAsAttribute = false,
            ConvertElementProperties = true,
            ConvertLinkedCADFormats = false,
            ExportLinks = false,
            ExportParts = true,
            FindMissingMaterials = true,
            DivideFileIntoLevels = true,
            ExportScope = NavisworksExportScope.View,
            ViewId = exportView.Id,
            ExportRoomGeometry = false
        };

        try
        {
            var docName = PrepareDocumentName(doc.Title);
            doc.Export(directoryPath, docName, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка экспорта в NWC для {doc.Title}: {ex.Message}");
        }
    }
    
    private static View3D? GetNavisworksView(Document doc)
    {
        if (doc == null) return null;
        
        var navisworksView = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Views)
            .WhereElementIsNotElementType()
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .FirstOrDefault(v => !v.IsTemplate && 
                                 v.Name != null && 
                                 v.Name.IndexOf("navisworks", StringComparison.OrdinalIgnoreCase) >= 0);

        return navisworksView;
    }
    
    private static View3D? GetAnySuitable3DView(Document doc)
    {
        if (doc == null) return null;
        
        var navisworksView = GetNavisworksView(doc);
        if (navisworksView != null) return navisworksView;
        
        var anySuitableView = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Views)
            .WhereElementIsNotElementType()
            .OfClass(typeof(View3D))
            .Cast<View3D>()
            .FirstOrDefault(v => !v.IsTemplate && v.CanBePrinted);
        
        if (anySuitableView == null && doc.ActiveView is View3D active3DView && !active3DView.IsTemplate)
        {
            return active3DView;
        }
    
        return anySuitableView;
    }

    private static string PrepareDocumentName(string title)
    {
        if (string.IsNullOrEmpty(title))
            return "unknown";

        var idxDetached = title.IndexOf("_detached", StringComparison.OrdinalIgnoreCase);
        if (idxDetached >= 0)
            return title.Substring(0, idxDetached).Trim();

        var idxRuDetached = title.IndexOf("_отсоединено", StringComparison.OrdinalIgnoreCase);
        if (idxRuDetached >= 0)
            return title.Substring(0, idxRuDetached).Trim();

        return title;
    }
}