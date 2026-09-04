using Nice3point.Revit.Toolkit.External;

namespace Axes.Models;

public partial class AxesModel : IAxesModel
{
    private readonly Document? _doc = RevitContext.ActiveDocument;
    private const double Tolerance = 0.001;

    [ExternalEvent]
    private void ChangeDatumExtent(bool isAll, bool is3D)
    {
        using var t = new Transaction(_doc, "Change datum extent");
        t.Start();

        List<Grid> grids;

        if (isAll)
        {
            grids = new FilteredElementCollector(_doc, _doc!.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .Cast<Grid>()
                .ToList();
        }
        else
        {
            var uidoc = RevitContext.UiApplication.ActiveUIDocument;
            var selectedIds = uidoc.Selection.GetElementIds();

            grids = selectedIds
                .Select(id => _doc!.GetElement(id))
                .OfType<Grid>()
                .ToList();

            if (!grids.Any()) return;
        }

        var type = is3D ? DatumExtentType.Model : DatumExtentType.ViewSpecific;

        foreach (var grid in grids)
        {
            grid.SetDatumExtentType(DatumEnds.End0, _doc!.ActiveView, type);
            grid.SetDatumExtentType(DatumEnds.End1, _doc!.ActiveView, type);
        }

        t.Commit();
    }
    [ExternalEvent]
    public void HideTopOrBottom(bool isAll, bool isTop, bool show)
    {
        var action = show ? "Show" : "Hide";
        var position = isTop ? "top" : "bottom";

        using var t = new Transaction(_doc, $"{action} {position} bubble");
        t.Start();

        List<Grid> grids;

        if (isAll)
        {
            grids = new FilteredElementCollector(_doc, _doc!.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .Cast<Grid>()
                .ToList();
        }
        else
        {
            var uidoc = RevitContext.UiApplication.ActiveUIDocument;
            var selectedIds = uidoc.Selection.GetElementIds();

            grids = selectedIds
                .Select(id => _doc!.GetElement(id))
                .OfType<Grid>()
                .ToList();

            if (!grids.Any()) return;
        }

        foreach (var grid in grids)
        {
            var curve = grid.Curve;
            var point1 = curve.GetEndPoint(0);
            var point2 = curve.GetEndPoint(1);


            if (Math.Abs(point1.X - point2.X) < Tolerance)
            {
                var endToToggle = isTop
                    ? point1.Y < point2.Y ? DatumEnds.End0 : DatumEnds.End1
                    : point1.Y > point2.Y
                        ? DatumEnds.End0
                        : DatumEnds.End1;

                if (show)
                    grid.ShowBubbleInView(endToToggle, _doc!.ActiveView);
                else
                    grid.HideBubbleInView(endToToggle, _doc!.ActiveView);
            }
        }

        t.Commit();
    }
    
    [ExternalEvent]
    private void HideRightOrLeft(bool isAll, bool isLeft, bool show)
    {
        var action = show ? "Show" : "Hide";
        var position = isLeft ? "left" : "right";

        using var t = new Transaction(_doc, $"{action} {position} bubble");
        t.Start();

        List<Grid> grids;

        if (isAll)
        {
            grids = new FilteredElementCollector(_doc, _doc!.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .WhereElementIsNotElementType()
                .Cast<Grid>()
                .ToList();
        }
        else
        {
            var uidoc = RevitContext.UiApplication.ActiveUIDocument;
            var selectedIds = uidoc.Selection.GetElementIds();

            grids = selectedIds
                .Select(id => _doc!.GetElement(id))
                .OfType<Grid>()
                .ToList();

            if (!grids.Any()) return;
        }

        foreach (var grid in grids)
        {
            var curve = grid.Curve;
            var point1 = curve.GetEndPoint(0);
            var point2 = curve.GetEndPoint(1);


            if (!(Math.Abs(point1.Y - point2.Y) < Tolerance)) continue;
            var endToToggle = isLeft
                ? point1.X > point2.X ? DatumEnds.End0 : DatumEnds.End1
                : point1.X < point2.X
                    ? DatumEnds.End0
                    : DatumEnds.End1;

            if (show)
                grid.ShowBubbleInView(endToToggle, _doc!.ActiveView);
            else
                grid.HideBubbleInView(endToToggle, _doc!.ActiveView);
        }

        t.Commit();
    }
}