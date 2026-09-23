using Autodesk.Revit.UI;
using Kapibara.Core;


namespace SolidIntersection.Models;

public class SolidIntersectionModel(Document doc) : ISolidIntersectionModel
{
    public List<Project> LoadedProject()
    {
        var revitProjects = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType()
            .Select(p => new Project(p.Name, p.Id.GetValue()))
            .ToList();

        revitProjects.Add(new Project("Текущий файл", 1));

        return revitProjects;
    }

    public List<string> LoadedParameters()
    {
        return (List<string>)doc.GetProjectParameterNames();
    }


    private RevitLinkInstance? GetLink(Project project)
    {
        if (project.Id == 1)
            return null;

        return new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_RvtLinks)
            .WhereElementIsNotElementType()
            .Cast<RevitLinkInstance>()
            .FirstOrDefault(link => link.Id.GetValue() == project.Id);
    }

    public List<SelectedItems> LoadedFamilies(string name, Project project)
    {
        var link = GetLink(project);
        var document = link == null ? RevitContext.ActiveDocument : link.GetLinkDocument();

        var families = new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_GenericModel)
            .WhereElementIsNotElementType()
            .ToElements()
            .Where(element => element?.Name != null && element.Name.Contains(name))
            .ToList();
        var itemsList = families
            .Select(f => new SelectedItems(f.Name, false))
            .ToList();

        return itemsList;
    }

    private static List<XYZ> GetBoundingBoxCorners(XYZ min, XYZ max)
    {
        return
        [
            new XYZ(min.X, min.Y, min.Z),
            new XYZ(min.X, min.Y, max.Z),
            new XYZ(min.X, max.Y, min.Z),
            new XYZ(min.X, max.Y, max.Z),
            new XYZ(max.X, min.Y, min.Z),
            new XYZ(max.X, min.Y, max.Z),
            new XYZ(max.X, max.Y, min.Z),
            new XYZ(max.X, max.Y, max.Z)
        ];
    }

    private List<Element> FindIntersectingElements(string elementName, Project project)
    {
        var link = GetLink(project);
        var document = doc;
        Transform? transform = null;
        if (link != null)
        {
            document = link.GetLinkDocument();
            transform = link.GetTransform();
        }

        var element = new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_GenericModel)
            .WhereElementIsNotElementType()
            .FirstOrDefault(e => e.Name.Equals(elementName));
        if (element == null) return [];


        var srcSolid = element.GetSolid();
        if (srcSolid == null) return [];
        if (transform != null)
            srcSolid = SolidUtils.CreateTransformed(srcSolid, transform);

        var bb = element.get_BoundingBox(null);
        if (bb == null) return [];

        XYZ bbMin, bbMax;
        if (transform != null)
        {
            var corners = GetBoundingBoxCorners(bb.Min, bb.Max);
            var tCorners = corners.Select(p => transform.OfPoint(p)).ToList();
            bbMin = new XYZ(tCorners.Min(p => p.X), tCorners.Min(p => p.Y), tCorners.Min(p => p.Z));
            bbMax = new XYZ(tCorners.Max(p => p.X), tCorners.Max(p => p.Y), tCorners.Max(p => p.Z));
        }
        else
        {
            bbMin = bb.Min;
            bbMax = bb.Max;
        }

        var outline = new Outline(bbMin, bbMax);
        var bbFilter = new BoundingBoxIntersectsFilter(outline);
        var solidFilter = new ElementIntersectsSolidFilter(srcSolid);

        var seenIds = new HashSet<ElementId>();

        var result = new FilteredElementCollector(doc).WherePasses(bbFilter)
            .WherePasses(solidFilter)
            .WhereElementIsNotElementType()
            .ToElements()
            .Where(e => seenIds.Add(e.Id))
            .ToList();

        var spatialCats = new[]
        {
            BuiltInCategory.OST_Rooms,
            BuiltInCategory.OST_MEPSpaces
        };

        var spatialFilter = new ElementMulticategoryFilter(spatialCats);
        var spatialElems = new FilteredElementCollector(doc)
            .WherePasses(spatialFilter)
            .WhereElementIsNotElementType()
            .Cast<SpatialElement>()
            .ToList();

        var seCalc = new SpatialElementGeometryCalculator(doc, new SpatialElementBoundaryOptions());

        foreach (var se in spatialElems)
        {
            var seSolid = GetSpatialElementSolidSafe(seCalc, se);
            if (seSolid == null) continue;

            if (!IntersectsByBoolean(srcSolid, seSolid)) continue;
            if (seenIds.Add(se.Id))
                result.Add(se);
        }


        return result;
    }


    private static bool IntersectsByBoolean(Solid a, Solid b, double volumeEps = 1e-6)
    {
        if (a == null || b == null) return false;
        try
        {
            var inter = BooleanOperationsUtils.ExecuteBooleanOperation(a, b, BooleanOperationsType.Intersect);
            return inter != null && inter.Volume > volumeEps;
        }
        catch
        {
            return false;
        }
    }

    private static Solid? GetSpatialElementSolidSafe(SpatialElementGeometryCalculator calc, SpatialElement se)
    {
        try
        {
            var res = calc.CalculateSpatialElementGeometry(se);
            return res?.GetGeometry();
        }
        catch
        {
            return null;
        }
    }

    public void Execute(IEnumerable<SelectedItems> selectedItems, string parameterName, Project project)
    {
        try
        {
            using var t = new Transaction(doc, "Solid intersection");
            t.Start();
            foreach (var selectedItem in selectedItems)
            {
                var intersectionItems = FindIntersectingElements(selectedItem.GetName(), project);

                foreach (var elem in intersectionItems)
                    if (elem.TryGetParameterByName(parameterName, out var p))
                        p?.SetValue(selectedItem.Value);
            }

            t.Commit();
        }
        catch (Exception e)
        {
            TaskDialog.Show("Error", e.ToString());
            throw;
        }
    }

    public void Execute(IEnumerable<SelectedItems> selectedItems, string parameterName, string value, Project project)
    {
        try
        {
            using var t = new Transaction(doc, "Solid intersection");
            t.Start();
            foreach (var selectedItem in selectedItems)
            {
                var intersectionItems = FindIntersectingElements(selectedItem.GetName(), project);

                foreach (var elem in intersectionItems)
                    if (elem.TryGetParameterByName(parameterName, out var p))
                        p?.SetValue(value);
            }

            t.Commit();
        }
        catch (Exception e)
        {
            TaskDialog.Show("Error", e.ToString());
            throw;
        }
    }
}
