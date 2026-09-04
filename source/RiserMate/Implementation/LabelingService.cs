using Autodesk.Revit.DB.Plumbing;
using RiserMate.Abstractions;


namespace RiserMate.Implementation;

public class LabelingService(View3D view) : ILabelingService
{
    private readonly View3D _view = view ?? throw new ArgumentNullException(nameof(view));
    private readonly Document? _document = RevitContext.ActiveDocument;

    public void MarkHeatDevice(string marksHeatDevice)
    {
        if (_document == null) return;

        var heatDevices = new FilteredElementCollector(_document, _view.Id)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .WhereElementIsNotElementType()
            .ToElements();

        var mark = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipmentTags)
            .WhereElementIsElementType()
            .FirstOrDefault(m => m.Name == marksHeatDevice);

        if (mark == null) return;

        var up = _view.UpDirection.Normalize();
        const double horizontalOffset = 10.0;
        const double verticalOffset = 5.0;

        foreach (var heatDevice in heatDevices)
        {
            var reference = new Reference(heatDevice);
            if (heatDevice.Location is not LocationPoint) continue;

            var leaderEnd = GetElbowPoint(heatDevice);
            if (leaderEnd == null) continue;

            var connectionPoint = GetConnectionPoint(heatDevice);
            var direction = connectionPoint != null
                ? GetScreenHorizontalAwayFromConnection(heatDevice, connectionPoint)
                : _view.RightDirection.Normalize();

            var tagPoint = leaderEnd
                           + direction * horizontalOffset
                           + up * verticalOffset;

            var tag = IndependentTag.Create(
                _document,
                mark.Id,
                _view.Id,
                reference,
                true,
                TagOrientation.Horizontal,
                leaderEnd
            );

            if (tag != null)
            {
                tag.LeaderEndCondition = LeaderEndCondition.Free;
                tag.TagHeadPosition = tagPoint;
                tag.SetLeaderEnd(reference, leaderEnd);
            }

            _document.Regenerate();

            try
            {
                CreateSpotElevation(heatDevice);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }


    public void MarkPipeAccessory(string markPipeAccessory)
    {
        if (_document == null) return;

        var pipeAccessories = new FilteredElementCollector(_document, _view.Id)
            .OfCategory(BuiltInCategory.OST_PipeAccessory)
            .WhereElementIsNotElementType()
            .Cast<FamilyInstance>()
            .Where(p => p.SuperComponent == null)
            .ToList();

        var mark = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_PipeAccessoryTags)
            .WhereElementIsElementType()
            .FirstOrDefault(m => m.Name == markPipeAccessory);

        if (mark == null) return;

        var viewRight = _view.RightDirection;
        var viewUp = _view.UpDirection;


        const double offsetX = 6;
        const double offsetY = 2;


        var createdTags = new List<IndependentTag>();

        foreach (var pipeAccessory in pipeAccessories)
        {
            if (pipeAccessory.Location is not LocationPoint location) continue;

            var reference = new Reference(pipeAccessory);
            var hostPoint = location.Point;

            var tagHeadWorld = hostPoint
                               - offsetX * viewRight
                               - offsetY * viewUp;

            var leaderEnd = hostPoint;

            try
            {
                var tag = IndependentTag.Create(
                    _document, mark.Id, _view.Id,
                    reference, true, TagOrientation.Horizontal,
                    tagHeadWorld);

                tag.LeaderEndCondition = LeaderEndCondition.Free;
                tag.TagHeadPosition = tagHeadWorld;
                tag.SetLeaderEnd(reference, leaderEnd);

                createdTags.Add(tag);
                _document.Regenerate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //RemoveIntersectingTags(createdTags);
    }


    private void RemoveIntersectingTags(List<IndependentTag> tags)
    {
        var tagsToDelete = new HashSet<ElementId>();

        for (var i = 0; i < tags.Count; i++)
        {
            if (tagsToDelete.Contains(tags[i].Id)) continue;

            var bbI = tags[i].get_BoundingBox(_view);
            if (bbI == null) continue;

            for (var j = i + 1; j < tags.Count; j++)
            {
                if (tagsToDelete.Contains(tags[j].Id)) continue;

                var bbJ = tags[j].get_BoundingBox(_view);
                if (bbJ == null) continue;

                if (BoundingBoxesIntersectXy(bbI, bbJ))
                    tagsToDelete.Add(tags[j].Id);
            }
        }

        foreach (var id in tagsToDelete)
            _document?.Delete(id);
    }

    private static bool BoundingBoxesIntersectXy(BoundingBoxXYZ a, BoundingBoxXYZ b)
    {
        return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X &&
               a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y;
    }

    public void MarkPipe(string markPipe)
    {
        if (_document == null) return;

        var pipes = new FilteredElementCollector(_document, _view.Id)
            .OfCategory(BuiltInCategory.OST_PipeCurves)
            .WhereElementIsNotElementType()
            .Cast<Pipe>()
            .ToList();

        var mark = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_PipeTags)
            .WhereElementIsElementType()
            .FirstOrDefault(m => m.Name == markPipe);

        if (mark == null) return;

        var verticalPipes = pipes.Where(p =>
        {
            if (p.Location is not LocationCurve location) return false;
            var curve = location.Curve;
            var point1 = curve.GetEndPoint(0);
            var point2 = curve.GetEndPoint(1);
            var deltaZ = Math.Abs(point2.Z - point1.Z);
            var deltaXy = Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
            return deltaZ > deltaXy;
        }).ToList();

        var pipesBySystem = verticalPipes
            .GroupBy(p => p.MEPSystem?.Id)
            .Where(g => g.Key != null);

        var deviceCenters = new FilteredElementCollector(_document, _view.Id)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .WhereElementIsNotElementType()
            .Select(GetElementCenter)
            .OfType<XYZ>()
            .ToList();

        var up = _view.UpDirection.Normalize();
        const double horizontalOffsetMm = 1.0;
        const double verticalOffsetMm = 5.0;

        var horizontalOffset = horizontalOffsetMm / 304.8 * _view.Scale;
        var verticalOffset = verticalOffsetMm / 304.8 * _view.Scale;


        foreach (var systemGroup in pipesBySystem)
        {
            var validPipes = new List<Pipe>();
            var sortedPipes = systemGroup.OrderBy(p => GetMidPipe(p).Z).ToList();

            if (sortedPipes.Count == 0) continue;

            var direction = GetPipeTagDirection(sortedPipes[0], deviceCenters);

            var currentMinZ = GetMidPipe(sortedPipes[0]).Z;
            var lengthIgnore = 1.0 * 304.8;

            for (var i = 0; i < sortedPipes.Count; i++)
            {
                var pipe = sortedPipes[i];

                if (i % 2 != 0) continue;

                if (pipe.Location is not LocationCurve location) continue;

                var curve = location.Curve;
                var length = curve.Length;

                if (length < 1500 / 304.8) continue;

                var pipeZ = GetMidPipe(pipe).Z;

                if (pipeZ >= currentMinZ && pipeZ < currentMinZ + lengthIgnore)
                {
                    validPipes.Add(pipe);
                }
                else if (pipeZ >= currentMinZ + lengthIgnore)
                {
                    currentMinZ = pipeZ;
                    validPipes.Add(pipe);
                }
            }

            foreach (var pipe in validPipes)
            {
                var reference = new Reference(pipe);
                var midPipe = GetMidPipe(pipe);

                var tag = IndependentTag.Create(
                    _document,
                    mark.Id,
                    _view.Id,
                    reference,
                    true,
                    TagOrientation.Horizontal,
                    midPipe
                );

                if (tag == null) continue;

                tag.LeaderEndCondition = LeaderEndCondition.Free;

                var desiredHead = midPipe
                                  + direction * horizontalOffset
                                  - up * verticalOffset;

                tag.TagHeadPosition = desiredHead;

                _document.Regenerate();

                AdjustTagClearance(tag, midPipe, direction, desiredHead);
            }

            break;
        }
    }

    private void AdjustTagClearance(IndependentTag tag, XYZ pipePoint, XYZ direction, XYZ currentHead)
    {
        var bb = tag.get_BoundingBox(_view);
        if (bb == null) return;

        var t = bb.Transform;
        var corners = new[]
        {
            t.OfPoint(new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z)),
            t.OfPoint(new XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z)),
            t.OfPoint(new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z)),
            t.OfPoint(new XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z)),
            t.OfPoint(new XYZ(bb.Min.X, bb.Min.Y, bb.Max.Z)),
            t.OfPoint(new XYZ(bb.Max.X, bb.Min.Y, bb.Max.Z)),
            t.OfPoint(new XYZ(bb.Min.X, bb.Max.Y, bb.Max.Z)),
            t.OfPoint(new XYZ(bb.Max.X, bb.Max.Y, bb.Max.Z))
        };


        var nearestEdge = corners.Min(c => c.DotProduct(direction));
        var pipeProjection = pipePoint.DotProduct(direction);

        var clearance = 5.0 / 304.8 * _view.Scale;

        var shortfall = pipeProjection + clearance - nearestEdge;
        if (shortfall <= 0) return;

        tag.TagHeadPosition = currentHead + direction * shortfall;
    }


    private XYZ GetPipeTagDirection(Pipe pipe, List<XYZ> deviceCenters)
    {
        var right = _view.RightDirection.Normalize();
        var pipeMid = GetMidPipe(pipe);

        const double searchRadius = 3000 / 304.8;

        var nearbyCenters = deviceCenters
            .Where(c =>
            {
                var dx = c.X - pipeMid.X;
                var dy = c.Y - pipeMid.Y;
                return Math.Sqrt(dx * dx + dy * dy) < searchRadius;
            })
            .ToList();

        if (nearbyCenters.Count == 0) return right;

        var avgCenter = nearbyCenters.Aggregate(XYZ.Zero, (acc, p) => acc + p) / nearbyCenters.Count;

        var delta = avgCenter.DotProduct(right) - pipeMid.DotProduct(right);

        return delta >= 0 ? right.Negate() : right;
    }

    private XYZ GetMidPipe(Pipe pipe)
    {
        if (pipe.Location is not LocationCurve location) return XYZ.Zero;

        var curve = location.Curve;

        var point1 = curve.GetEndPoint(0);
        var point2 = curve.GetEndPoint(1);

        return (point1 + point2) / 2.0;
    }

    private void CreateSpotElevation(Element element)
    {
        if (_document == null || element.Location is not LocationPoint) return;

        var connectionPoint = GetConnectionPoint(element);
        if (connectionPoint == null) return;

        var (reference, point) = GetFarthestLowestEdgePoint(element, connectionPoint);
        if (reference == null || point == null) return;

        var direction = GetScreenHorizontalAwayFromConnection(element, connectionPoint);

        const double leaderLength = 3.0;
        const double shelfLength = 2.0;
        var dropOffset = 0.001 / 304.8 * _view.Scale;

        var drop = -_view.UpDirection.Normalize() * dropOffset;

        var bendPoint = point + direction * leaderLength + drop;
        var endPoint = bendPoint + direction * shelfLength;

        var spotDimension = _document.Create.NewSpotElevation(
            _view,
            reference,
            point,
            bendPoint,
            endPoint,
            point,
            true
        );
    }

    private XYZ GetScreenHorizontalAwayFromConnection(Element element, XYZ connectionPoint)
    {
        var right = _view.RightDirection.Normalize();

        var center = GetElementCenter(element);
        if (center == null) return right;

        var delta = center.DotProduct(right) - connectionPoint.DotProduct(right);

        return delta >= 0 ? right : right.Negate();
    }


    private XYZ? GetElementCenter(Element element)
    {
        var bbox = element.get_BoundingBox(null);
        if (bbox == null) return null;

        var localCenter = (bbox.Min + bbox.Max) / 2.0;
        return bbox.Transform.OfPoint(localCenter);
    }


    private static XYZ? GetConnectionPoint(Element element)
    {
        if (element is not FamilyInstance familyInstance) return null;

        var connectors = familyInstance.MEPModel?.ConnectorManager?.Connectors;
        if (connectors == null) return null;

        var points = connectors
            .Cast<Connector>()
            .Where(c => c.IsConnected)
            .Select(c => c.Origin)
            .ToList();

        if (points.Count == 0)
            points = connectors.Cast<Connector>().Select(c => c.Origin).ToList();

        if (points.Count == 0) return null;

        var sum = points.Aggregate(XYZ.Zero, (acc, p) => acc + p);
        return sum / points.Count;
    }

    private (Reference? Reference, XYZ? Point) GetFarthestLowestEdgePoint(Element element, XYZ connectionPoint)
    {
        var options = new Options
        {
            ComputeReferences = true,
            View = _view
        };

        var geom = element.get_Geometry(options);
        if (geom == null) return (null, null);

        var allSolids = new List<(Solid Solid, Transform Transform)>();

        foreach (var obj in geom)
            switch (obj)
            {
                case Solid solid when solid.Volume > 0:
                    allSolids.Add((solid, Transform.Identity));
                    break;
                case GeometryInstance instance:
                {
                    var transform = instance.Transform;
                    foreach (var symObj in instance.GetSymbolGeometry())
                        if (symObj is Solid symSolid && symSolid.Volume > 0)
                            allSolids.Add((symSolid, transform));
                    break;
                }
            }

        if (allSolids.Count == 0) return (null, null);

        var maxVolume = allSolids.Max(s => s.Solid.Volume);
        const double volumeTolerance = 0.0001;

        var largestSolids = allSolids
            .Where(s => Math.Abs(s.Solid.Volume - maxVolume) < volumeTolerance)
            .ToList();

        var edges = new List<(Edge Edge, Transform Transform)>();

        foreach (var (solid, transform) in largestSolids)
        foreach (Edge edge in solid.Edges)
            if (edge != null)
                edges.Add((edge, transform));

        if (edges.Count == 0) return (null, null);

        var minZ = double.MaxValue;

        foreach (var (edge, transform) in edges)
        {
            var curve = edge.AsCurve();
            if (curve == null) continue;

            var z0 = transform.OfPoint(curve.GetEndPoint(0)).Z;
            var z1 = transform.OfPoint(curve.GetEndPoint(1)).Z;

            minZ = Math.Min(minZ, Math.Min(z0, z1));
        }

        const double zTolerance = 0.01;

        Reference? bestReference = null;
        XYZ? bestPoint = null;
        var maxDistance = double.MinValue;

        foreach (var (edge, transform) in edges)
        {
            var curve = edge.AsCurve();
            if (curve == null || edge.Reference == null) continue;

            for (var i = 0; i < 2; i++)
            {
                var worldPoint = transform.OfPoint(curve.GetEndPoint(i));

                if (worldPoint.Z > minZ + zTolerance) continue;

                var distance = worldPoint.DistanceTo(connectionPoint);
                if (distance <= maxDistance) continue;

                maxDistance = distance;
                bestReference = edge.Reference;
                bestPoint = worldPoint;
            }
        }

        return (bestReference, bestPoint);
    }


    private XYZ? GetElbowPoint(Element element)
    {
        var bbox = element.get_BoundingBox(_view);

        if (bbox != null)
        {
            var center = (bbox.Min + bbox.Max) / 2.0;
            return center;
        }

        return null;
    }

    private static (double x, double y, double z) ProjectToView(
        XYZ worldPoint, XYZ viewOrigin, XYZ viewRight, XYZ viewUp, XYZ viewDir, int viewScale)
    {
        var delta = worldPoint - viewOrigin;
        return (
            delta.DotProduct(viewRight) / viewScale,
            delta.DotProduct(viewUp) / viewScale,
            delta.DotProduct(viewDir) / viewScale
        );
    }
}