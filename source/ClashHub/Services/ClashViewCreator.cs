using Nice3point.Revit.Toolkit.External;


namespace ClashHub.Services;

public sealed partial class ClashViewCreator(Document doc, IElementColorizer colorizer) : IClashViewCreator
{
    [ExternalEvent]
    private void CreateView(long firstElementId, long secondElementId)
    {
        var first = FindElement(firstElementId.ToElementId(), out var firstLink);
        var second = FindElement(secondElementId.ToElementId(), out var secondLink);
        if (first is null && second is null) return;

        var box1 = GetTransformedBox(first, firstLink);
        var box2 = GetTransformedBox(second, secondLink);

        var box = Union(box1, box2);
        if (box == null) return;

        View3D view;
        using (var t = new Transaction(doc, "Создание вида коллизии"))
        {
            t.Start();

            view = doc.ActiveView as View3D ?? Create3DView();
            view.SetSectionBox(box);
            view.IsSectionBoxActive = true;

            if ((firstLink == null && first != null) || (secondLink == null && second != null))
                colorizer.Colorize(view, firstElementId, secondElementId);

            t.Commit();
        }

        OpenAndZoom(view);
    }

    private Element? FindElement(ElementId id, out RevitLinkInstance? link)
    {
        link = null;
        var el = doc.GetElement(id);
        if (el != null) return el;

        foreach (var li in new FilteredElementCollector(doc)
                     .OfClass(typeof(RevitLinkInstance))
                     .Cast<RevitLinkInstance>())
        {
            var linkDoc = li.GetLinkDocument();
            var linkedEl = linkDoc?.GetElement(id);
            if (linkedEl != null)
            {
                link = li;
                return linkedEl;
            }
        }

        return null;
    }

    private static BoundingBoxXYZ? GetTransformedBox(Element? element, RevitLinkInstance? link)
    {
        var box = element?.get_BoundingBox(null);
        if (box == null) return null;
        if (link == null) return box;

        var transform = link.GetTotalTransform();
        
        var corners = new[]
            {
                new XYZ(box.Min.X, box.Min.Y, box.Min.Z),
                new XYZ(box.Max.X, box.Min.Y, box.Min.Z),
                new XYZ(box.Min.X, box.Max.Y, box.Min.Z),
                new XYZ(box.Max.X, box.Max.Y, box.Min.Z),
                new XYZ(box.Min.X, box.Min.Y, box.Max.Z),
                new XYZ(box.Max.X, box.Min.Y, box.Max.Z),
                new XYZ(box.Min.X, box.Max.Y, box.Max.Z),
                new XYZ(box.Max.X, box.Max.Y, box.Max.Z)
            }
            .Select(transform.OfPoint)
            .ToArray();

        return new BoundingBoxXYZ
        {
            Min = new XYZ(corners.Min(p => p.X), corners.Min(p => p.Y), corners.Min(p => p.Z)),
            Max = new XYZ(corners.Max(p => p.X), corners.Max(p => p.Y), corners.Max(p => p.Z))
        };
    }

    private static BoundingBoxXYZ? Union(BoundingBoxXYZ? box1, BoundingBoxXYZ? box2)
    {
        if (box1 == null && box2 == null) return null;
        if (box1 == null) return Expand(box2!, 0.5);
        if (box2 == null) return Expand(box1, 0.5);

        const double offset = 1.0;
        return new BoundingBoxXYZ
        {
            Min = new XYZ(
                Math.Min(box1.Min.X, box2.Min.X) - offset,
                Math.Min(box1.Min.Y, box2.Min.Y) - offset,
                Math.Min(box1.Min.Z, box2.Min.Z) - offset),
            Max = new XYZ(
                Math.Max(box1.Max.X, box2.Max.X) + offset,
                Math.Max(box1.Max.Y, box2.Max.Y) + offset,
                Math.Max(box1.Max.Z, box2.Max.Z) + offset)
        };
    }

    private static BoundingBoxXYZ Expand(BoundingBoxXYZ box, double offset) => new()
    {
        Min = new XYZ(box.Min.X - offset, box.Min.Y - offset, box.Min.Z - offset),
        Max = new XYZ(box.Max.X + offset, box.Max.Y + offset, box.Max.Z + offset)
    };

    private static void OpenAndZoom(View3D view)
    {
        var uiDoc = RevitContext.ActiveUiDocument;
        if (uiDoc == null) return;

        if (uiDoc.ActiveView.Id != view.Id)
            uiDoc.ActiveView = view;

        var uiView = uiDoc.GetOpenUIViews()
            .FirstOrDefault(v => v.ViewId == view.Id);

        uiView?.ZoomToFit();
    }

    private View3D Create3DView()
    {
        var viewFamilyType = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

        var view = View3D.CreateIsometric(doc, viewFamilyType.Id);
        view.Name = GetUniqueViewName("Collisions");
        view.DetailLevel = ViewDetailLevel.Fine;
        view.DisplayStyle = DisplayStyle.ShadingWithEdges;
        return view;
    }

    private string GetUniqueViewName(string baseName)
    {
        var existingNames = new FilteredElementCollector(doc)
            .OfClass(typeof(View))
            .Cast<View>()
            .Where(v => !v.IsTemplate)
            .Select(v => v.Name)
            .ToHashSet();

        if (!existingNames.Contains(baseName)) return baseName;

        var i = 1;
        while (existingNames.Contains($"{baseName} {i}")) i++;
        return $"{baseName} {i}";
    }

    public void CreateViewAsync(long firstElementId, long secondElementId)
        => CreateViewAsyncEvent.RaiseAsync(firstElementId, secondElementId);
}