using Nice3point.Revit.Toolkit.External;

namespace AxesLevels.Models;

public partial class ProjectLevelsModel(Document doc) : IModel
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
        var levels = new FilteredElementCollector(doc, doc.ActiveView.Id)
            .OfCategory(BuiltInCategory.OST_Levels)
            .WhereElementIsNotElementType()
            .Cast<Level>()
            .ToList();

        foreach (var level in levels)
        {
            if (!beginCheck)
                level.HideBubbleInView(DatumEnds.End0, doc.ActiveView);
            else
                level.ShowBubbleInView(DatumEnds.End0, doc.ActiveView);
            if (!endCheck)
                level.HideBubbleInView(DatumEnds.End1, doc.ActiveView);
            else
                level.ShowBubbleInView(DatumEnds.End1, doc.ActiveView);
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

        var levels = selectedIds
            .Select(id => doc.GetElement(id))
            .OfType<Level>()
            .ToList();

        foreach (var level in levels)
        {
            if (!beginCheck)
                level.HideBubbleInView(DatumEnds.End0, doc.ActiveView);
            else
                level.ShowBubbleInView(DatumEnds.End0, doc.ActiveView);

            if (!endCheck)
                level.HideBubbleInView(DatumEnds.End1, doc.ActiveView);
            else
                level.ShowBubbleInView(DatumEnds.End1, doc.ActiveView);
        }

        t.Commit();
    }
}