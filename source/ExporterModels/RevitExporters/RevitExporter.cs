using ExporterModels.services;

namespace ExporterModels.RevitExporters;

public abstract class RevitExporter
{
    private static readonly FailureProcessorService FailureProcessorService = new();

    protected Document? OpenDocumentAsDetach(string filePath, string badNameWorkset, bool deleteLinksDwg,
        bool closeAllWorset)
    {
        var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
        var app = Context.UiApplication;
        var controlledApp = app.Application;

        var openOptions = new OpenOptions();

        try
        {
            openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            var worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            if (!closeAllWorset)
            {
                IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(modelPath);
                IList<WorksetId> worksetIds;
                if (string.IsNullOrEmpty(badNameWorkset))
                    worksetIds = worksets.Select(workset => workset.Id).ToList();
                else
                    worksetIds = worksets
                        .Where(workset => !workset.Name.ToLower().Contains(badNameWorkset.ToLower()))
                        .Select(workset => workset.Id)
                        .ToList();

                worksetConfiguration.Open(worksetIds);
            }

            openOptions.SetOpenWorksetsConfiguration(worksetConfiguration);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


        try
        {
            controlledApp.FailuresProcessing += FailureProcessorService.ApplicationOnFailuresProcessing;
            app.DialogBoxShowing += FailureProcessorService.UIApplicationOnDialogBoxShowing;


            var openDoc = controlledApp.OpenDocumentFile(modelPath, openOptions);

            if (openDoc != null && openDoc.IsValidObject)
            {
                if (deleteLinksDwg) DeleteLinksAndDwg(openDoc);

                return openDoc;
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            if (FailureProcessorService != null)
            {
                controlledApp.FailuresProcessing -= FailureProcessorService.ApplicationOnFailuresProcessing;
                app.DialogBoxShowing -= FailureProcessorService.UIApplicationOnDialogBoxShowing;
            }
        }

        return null;
    }


    private static void DeleteLinksAndDwg(Document doc)
    {
        IList<ElementId> linkedElementIds = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsElementType().ToElementIds().ToList();

        IList<ElementId> dwgElementIds = new FilteredElementCollector(doc)
            .OfClass(typeof(ImportInstance))
            .ToElements()
            .Select(element => element.Id)
            .ToList();


        using (var tx = new Transaction(doc, "Удаление всех связей и DWG"))
        {
            tx.Start();

            if (linkedElementIds.Count > 0) doc.Delete(linkedElementIds);
            if (dwgElementIds.Count > 0) doc.Delete(dwgElementIds);

            tx.Commit();
        }
    }
}