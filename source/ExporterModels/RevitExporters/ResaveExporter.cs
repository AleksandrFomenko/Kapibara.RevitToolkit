using System.IO;
using Autodesk.Revit.UI;

namespace ExporterModels.RevitExporters;

public class ResaveExporter : RevitExporter
{
    public async Task ExportSingleAsync(
        UIApplication uiApp,
        string filePath,
        string directoryPath)
    {
        var modelName = GetModelNameFromPath(filePath);
        var destFilePath = Path.Combine(directoryPath, modelName + ".rvt");

        Resaving(filePath, destFilePath);
        UpdateTransmissionData(destFilePath);
    }

    private string GetModelNameFromPath(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    private void UpdateTransmissionData(string modelFilePath)
    {
        var localModelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(modelFilePath);
        var transData = TransmissionData.ReadTransmissionData(localModelPath);

        if (transData == null) return;

        var externalReferences = transData.GetAllExternalFileReferenceIds();
        foreach (var refId in externalReferences)
        {
            var extRef = transData.GetLastSavedReferenceData(refId);
            if (extRef.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink)
                transData.SetDesiredReferenceData(refId, extRef.GetPath(), extRef.PathType, false);
        }

        transData.IsTransmitted = true;
        TransmissionData.WriteTransmissionData(localModelPath, transData);
    }

    private static void Resaving(string modelPathStr, string destFilepath)
    {
        var app = RevitApiContext.Application;
        var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(modelPathStr);
        app.CopyModel(modelPath, destFilepath, true);
    }
}