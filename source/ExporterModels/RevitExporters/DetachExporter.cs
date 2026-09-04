using System.IO;
using Autodesk.Revit.UI;

namespace ExporterModels.RevitExporters;

public class DetachExporter : RevitExporter
{
    public async Task ExportSingleAsync(
        UIApplication uiApp,
        string filePath,
        string directoryPath,
        string badNameWorkset)
    {
        var doc = OpenDocumentAsDetach(filePath, badNameWorkset, true, false);
        if (doc != null) SaveAsCentral(doc, filePath, directoryPath);
    }

    private void SaveAsCentral(Document doc, string path, string directoryPath)
    {
        if (doc == null) return;
        if (doc.IsLinked)
        {
            doc.Close(false);
            return;
        }

        var saveAsOptions = new SaveAsOptions();
        saveAsOptions.OverwriteExistingFile = true;

        if (doc.IsWorkshared)
        {
            var worksharingSaveAsOptions = new WorksharingSaveAsOptions();
            worksharingSaveAsOptions.SaveAsCentral = true;
            saveAsOptions.SetWorksharingOptions(worksharingSaveAsOptions);
        }

        var modelName = GetModelNameFromPath(path);
        var destFilePath = Path.Combine(directoryPath, modelName + ".rvt");
        var modelSavePath = ModelPathUtils.ConvertUserVisiblePathToModelPath(destFilePath);

        doc.SaveAs(modelSavePath, saveAsOptions);
        doc.Close(false);
    }

    private static string GetModelNameFromPath(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }
}