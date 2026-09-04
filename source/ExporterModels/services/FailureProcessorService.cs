using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

namespace ExporterModels.services;

public class FailureProcessorService
{
    public void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
    {
        var accessor = e.GetFailuresAccessor();
        accessor.DeleteAllWarnings();

        try
        {
            accessor.ResolveFailures(accessor.GetFailureMessages());

            ElementId[] elementIds = accessor.GetFailureMessages()
                .SelectMany(item => item.GetFailingElementIds())
                .ToArray();

            if (elementIds.Length > 0)
            {
                accessor.DeleteElements(elementIds);
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            }
            else
            {
                e.SetProcessingResult(FailureProcessingResult.Continue);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public void UIApplicationOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
    {
        e.OverrideResult(1);
    }
}