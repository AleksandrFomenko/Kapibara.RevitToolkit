using Nice3point.Revit.Toolkit.External;

namespace AxesLevels.Models;

public partial class ProjectAxesModel(Document doc) : IModel
{
    public Task DoAllAsync(bool beginCheck, bool endCheck)
        => DoAllAsyncEvent.RaiseAsync(beginCheck, endCheck);

    public Task DoSelectionAsync(bool beginCheck, bool endCheck)
        => DoSelectionAsyncEvent.RaiseAsync(beginCheck, endCheck);

    [ExternalEvent]
    private void DoAll(bool beginCheck, bool endCheck)
    {
        using var t = new Transaction(doc, "Set HideBubble axes");
        t.Start();
        var grids = new FilteredElementCollector(doc, doc.ActiveView.Id)
            .OfCategory(BuiltInCategory.OST_Grids)
            .WhereElementIsNotElementType()
            .Cast<Grid>()
            .ToList();

        foreach (var grid in grids)
        {
            if (!beginCheck)
                grid.HideBubbleInView(DatumEnds.End0, doc.ActiveView);
            else
                grid.ShowBubbleInView(DatumEnds.End0, doc.ActiveView);
            if (!endCheck)
                grid.HideBubbleInView(DatumEnds.End1, doc.ActiveView);
            else
                grid.ShowBubbleInView(DatumEnds.End1, doc.ActiveView);
        }

        t.Commit();
    }

    [ExternalEvent]
    private void DoSelection(bool beginCheck, bool endCheck)
    {
        using var t = new Transaction(doc, "Set HideBubble axes (Selection)");
        t.Start();

        var uidoc = RevitContext.UiApplication.ActiveUIDocument;
        var selectedIds = uidoc.Selection.GetElementIds();

        var grids = selectedIds
            .Select(id => doc.GetElement(id))
            .OfType<Grid>()
            .ToList();

        foreach (var grid in grids)
        {
            if (!beginCheck)
                grid.HideBubbleInView(DatumEnds.End0, doc.ActiveView);
            else
                grid.ShowBubbleInView(DatumEnds.End0, doc.ActiveView);
            if (!endCheck)
                grid.HideBubbleInView(DatumEnds.End1, doc.ActiveView);
            else
                grid.ShowBubbleInView(DatumEnds.End1, doc.ActiveView);
        }

        t.Commit();
    }
}