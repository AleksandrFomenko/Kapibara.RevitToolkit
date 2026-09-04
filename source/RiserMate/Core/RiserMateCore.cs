using Autodesk.Revit.DB.Plumbing;
using Kapibara.Core;

namespace RiserMate.Core;

public static class RiserMateCore
{
    private static Document? Document => RevitContext.ActiveDocument;


    public static List<Pipe> GetBottomPipesByRiser(string riserName, string parameterName)
    {
        var pipes = new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_PipeCurves)
            .WhereElementIsNotElementType()
            .Cast<Pipe>()
            .Where(pipe => pipe.LookupParameter(parameterName)?.AsString() == riserName);

        var pipeHeights = new List<(Pipe Pipe, double MinZ)>();

        foreach (var pipe in pipes)
        {
            var connectors = pipe.ConnectorManager?.Connectors;
            if (connectors == null)
                continue;

            var minZ = double.PositiveInfinity;

            foreach (Connector connector in connectors)
            {
                var z = connector.Origin.Z;
                if (z < minZ)
                    minZ = z;
            }

            if (minZ < double.PositiveInfinity)
                pipeHeights.Add((pipe, minZ));
        }

        return pipeHeights
            .OrderBy(t => t.MinZ)
            .Take(2)
            .Select(t => t.Pipe)
            .ToList();
    }

    private static Connector? GetTopConnector(Element pipe)
    {
        if (pipe is not Pipe revitPipe)
            return null;

        var connectors = revitPipe.ConnectorManager?.Connectors;
        if (connectors == null)
            return null;

        Connector? topConnector = null;
        var maxZ = double.NegativeInfinity;

        foreach (Connector connector in connectors)
        {
            var cs = connector.CoordinateSystem;
            if (cs == null)
                continue;

            var z = cs.Origin.Z;
            if (z > maxZ)
            {
                maxZ = z;
                topConnector = connector;
            }
        }

        return topConnector;
    }

    private static IList<Element> GetElementsFromTopConnector(
        Pipe startPipe,
        HashSet<ElementId> visited,
        ElementId? stopId,
        out HashSet<ElementId> excludedIds)
    {
        excludedIds = [];

        var connectors = startPipe.ConnectorManager?.Connectors;
        if (connectors == null || connectors.Size == 0)
            return new List<Element>();

        var result = new List<Element>();

        if (visited.Add(startPipe.Id))
            result.Add(startPipe);

        var connectorList = connectors.Cast<Connector>().ToList();

        var bottomConnector = connectorList
            .OrderBy(c => c.Origin.Z)
            .FirstOrDefault();
        
        if (bottomConnector != null)
        {
            foreach (Connector refC in bottomConnector.AllRefs)
            {
                if (refC.Owner != null && refC.Owner is not MEPSystem)
                    excludedIds.Add(refC.Owner.Id);
            }
        }

        foreach (var c in connectorList)
        {
            if (bottomConnector != null &&
                c.Id == bottomConnector.Id &&
                c.Owner.Id == bottomConnector.Owner.Id)
                continue;

            TraverseFromConnector(c, visited, result, stopId);
        }

        return result;
    }

    private static void TraverseFromConnector(
        Connector connector,
        HashSet<ElementId> visited,
        IList<Element> result,
        ElementId? stopId)
    {
        foreach (Connector refConnector in connector.AllRefs)
        {
            var owner = refConnector.Owner;
            if (owner == null)
                continue;

            if (owner is MEPSystem)
                continue;

            if (owner.Id == stopId)
            {
                if (visited.Add(owner.Id))
                    result.Add(owner);
                continue;
            }

            if (!visited.Add(owner.Id))
                continue;

            result.Add(owner);

            foreach (var ownerConnector in GetAllConnectors(owner))
                TraverseFromConnector(ownerConnector, visited, result, stopId);
        }
    }


    private static IEnumerable<Connector> GetAllConnectors(Element element)
    {
        switch (element)
        {
            case MEPCurve mepCurve:
            {
                var cm = mepCurve.ConnectorManager;
                if (cm != null && cm.Connectors != null)
                    foreach (Connector c in cm.Connectors)
                        yield return c;
                break;
            }
            case FamilyInstance fi:
            {
                var mepModel = fi.MEPModel;
                var cm = mepModel?.ConnectorManager;
                if (cm?.Connectors == null)
                    yield break;

                foreach (Connector c in cm.Connectors)
                    yield return c;
                break;
            }
        }
    }

    private static BoundingBoxXYZ? GetPipeBoundingBox(Pipe pipe)
    {
        var bb = pipe.get_BoundingBox(null);
        if (bb == null)
            return null;

        var min = bb.Min;
        var max = bb.Max;

        bool IsFinite(XYZ p)
        {
            return !(double.IsNaN(p.X) || double.IsNaN(p.Y) || double.IsNaN(p.Z) ||
                     double.IsInfinity(p.X) || double.IsInfinity(p.Y) || double.IsInfinity(p.Z));
        }

        if (!IsFinite(min) || !IsFinite(max))
            return null;

        var result = new BoundingBoxXYZ
        {
            Min = min,
            Max = max
        };

        return result;
    }
    private static bool BoxesIntersect(BoundingBoxXYZ a, BoundingBoxXYZ b, double tol = 1e-6)
    {
        var aMin = a.Min;
        var aMax = a.Max;
        var bMin = b.Min;
        var bMax = b.Max;

        return aMin.X <= bMax.X + tol && aMax.X + tol >= bMin.X &&
               aMin.Y <= bMax.Y + tol && aMax.Y + tol >= bMin.Y &&
               aMin.Z <= bMax.Z + tol && aMax.Z + tol >= bMin.Z;
    }
    private static List<Element> GetIntersectingElementsForEach(
        IList<Element> connectedElements,
        HashSet<ElementId> excludedIds)
    {
        var resultIds = new HashSet<ElementId>();
        var result = new List<Element>();
        var familyInstances = new FilteredElementCollector(Document)
            .OfClass(typeof(FamilyInstance))
            .WhereElementIsNotElementType()
            .Cast<FamilyInstance>()
            .ToList();

        var famBoxes = new List<(FamilyInstance Fi, BoundingBoxXYZ Box)>();

        foreach (var fi in familyInstances)
        {
            var bb = fi.get_BoundingBox(null);
            if (bb != null)
                famBoxes.Add((fi, bb));
        }

        foreach (var el in connectedElements)
        {
            if (el is not Pipe pipe)
                continue;

            var pipeBox = GetPipeBoundingBox(pipe);
            if (pipeBox == null) continue;

            foreach (var (fi, box) in famBoxes)
            {
                if (excludedIds.Contains(fi.Id))
                    continue;

                if (!BoxesIntersect(pipeBox, box))
                    continue;

                if (resultIds.Add(fi.Id))
                    result.Add(fi);
            }
        }
        return result;
    }
    public static void Execute(List<Pipe> pipes, string riserName, string parameterName)
    {
        if (pipes == null || pipes.Count == 0)
            return;

        var visited = new HashSet<ElementId>();
        var startPipe = pipes[0];
        var stopId = pipes.Count > 1 ? pipes[1].Id : null;
   

        var connectedElements = GetElementsFromTopConnector(startPipe, visited, stopId, out var excludedIds);

        foreach (var el in connectedElements)
        {
            if (el.TryGetParameterByName(parameterName, out var parameter))
            {
                parameter?.SetValue(riserName);
            }

            if (el is not FamilyInstance fi)
                continue;

            foreach (var subElem in fi.GetAllSubComponents())
            {
                if (subElem.TryGetParameterByName(parameterName, out var subPar))
                {
                    subPar?.SetValue(riserName);
                }
            }
        }

        var intersected = GetIntersectingElementsForEach(connectedElements, excludedIds);
        
        foreach (var el in intersected)
        {
            if (el.TryGetParameterByName(parameterName, out var intersectingElementParameter))
            {
                intersectingElementParameter?.SetValue(riserName);
            }
            
            if (el is not FamilyInstance fi)
                continue;

            foreach (var sub in fi.GetAllSubComponents())
            {
                if (sub.TryGetParameterByName(parameterName, out var subPar))
                {
                    subPar?.SetValue(riserName);
                }
            }
        }
    }
}