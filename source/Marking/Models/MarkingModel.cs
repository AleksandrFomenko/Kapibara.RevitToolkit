using Autodesk.Revit.UI.Selection;
using Kapibara.Core;
using Marking.Abstractions;
using Marking.Entities;
using Nice3point.Revit.Toolkit.External;

namespace Marking.Models;

public partial class MarkingModel : IMarkingModel
{
    public event EventHandler<CustomEventArgs>? SendName;
    public event Action? SendMaximize;
    public event Action<int>? SendQuantity;
    private Choice? _selectedChoice;
    private Document? _doc = RevitContext.ActiveDocument;
    private int _pickResult;

    public void SetSelectedChoice(Choice? choice)
    {
        _selectedChoice = choice;
    }

    public async Task ExecuteMarkingAsync(int elementId) => await ExecuteMarkingAsyncEvent.RaiseAsync(elementId);
    

    public int PickMark()
    {
        var uiDoc = RevitContext.ActiveUiDocument;
        if (uiDoc == null) return 0;
        try
        {
            var refObj = uiDoc.Selection.PickObject(
                ObjectType.Element,
                new ElementTypesSelectionFilter(),
                "Выберите марку"
            );

#if REVIT2025_OR_GREATER
            _pickResult = (int)refObj.ElementId.Value;
#else
            _pickResult = refObj.ElementId.IntegerValue;
#endif
            return _pickResult;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            return 0;
        }
        finally
        {
            var name = _doc?.GetElement(new ElementId(_pickResult))?.Name;
            SendName?.Invoke(this, new CustomEventArgs(name ?? "ошибка"));
            SendQuantity?.Invoke(GetTargetElements(_pickResult)?.Count ?? 0);
            SendMaximize?.Invoke();
        }
    }
    [ExternalEvent]
    private void ExecuteMarking(int elementId)
    {
        using var t = new Transaction(_doc, "Duplicate Tags");
        try
        {
            t.Start();
            Start(elementId);
            t.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (t.GetStatus() == TransactionStatus.Started) t.RollBack();
        }
    }

    private List<FamilyInstance>? GetTargetElements(int elementId)
    {
        if (_doc?.ActiveView == null) return null;

        var element = _doc.GetElement(new ElementId(elementId));
        if (element is not IndependentTag originalTag) return null;

        var taggedElements = originalTag.GetTaggedLocalElements().ToList();
        if (taggedElements.Count == 0) return null;

        var originalHost = taggedElements[0];
        if (originalHost is not FamilyInstance originalInstance) return null;

        var symbolId = originalInstance.GetTypeId();
        if (symbolId == ElementId.InvalidElementId) return null;

        if (_selectedChoice != Choice.All)
            return new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e => e.GetTypeId() == symbolId && e.Id != originalHost.Id)
                .ToList();
        {
            var symbol = _doc.GetElement(symbolId) as FamilySymbol;
            var familyId = symbol?.Family.Id;

            return new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e => e.Symbol.Family.Id == familyId && e.Id != originalHost.Id)
                .ToList();
        }
    }

    private static (double x, double y, double z) ProjectToView(XYZ worldPoint, XYZ viewOrigin,
        XYZ viewRightDir, XYZ viewUpDir, XYZ viewDir, int viewScale)
    {
        var delta = worldPoint - viewOrigin;
        return (
            delta.DotProduct(viewRightDir) / viewScale,
            delta.DotProduct(viewUpDir) / viewScale,
            delta.DotProduct(viewDir) / viewScale
        );
    }

    private static XYZ ProjectToWorld(double viewX, double viewY, double viewZ, XYZ viewOrigin,
        XYZ viewRightDir, XYZ viewUpDir, XYZ viewDir, int viewScale)
    {
        return viewOrigin
               + viewX * viewScale * viewRightDir
               + viewY * viewScale * viewUpDir
               + viewZ * viewScale * viewDir;
    }

    private void Start(int elementId)
    {
        if (_doc?.ActiveView == null) return;

        var element = _doc.GetElement(new ElementId(elementId));
        if (element is not IndependentTag originalTag) return;

        var taggedElements = originalTag.GetTaggedLocalElements().ToList();
        if (taggedElements.Count == 0) return;

        var originalHost = taggedElements[0];
        if (originalHost is not FamilyInstance originalInstance) return;
        if (originalInstance.Location is not LocationPoint originalLocation) return;

        var targetElements = GetTargetElements(elementId);
        if (targetElements == null || targetElements.Count == 0) return;

        var tagTypeId = originalTag.GetTypeId();
        var viewId = _doc.ActiveView.Id;
        var hasLeader = originalTag.HasLeader;
        var orientation = originalTag.TagOrientation;
        var leaderCondition = originalTag.LeaderEndCondition;

        var originalHostPoint = originalLocation.Point;
        var originalHeadPos = originalTag.TagHeadPosition;

        var viewOrigin = _doc.ActiveView.Origin;
        var viewScale = _doc.ActiveView.Scale;
        var viewRightDir = _doc.ActiveView.RightDirection;
        var viewUpDir = _doc.ActiveView.UpDirection;
        var viewDir = _doc.ActiveView.ViewDirection;

        var (hostX, hostY, hostZ) = ProjectToView(originalHostPoint, viewOrigin,
            viewRightDir, viewUpDir, viewDir, viewScale);

        var (headX, headY, headZ) = ProjectToView(originalHeadPos, viewOrigin,
            viewRightDir, viewUpDir, viewDir, viewScale);

        var headOffsetX = headX - hostX;
        var headOffsetY = headY - hostY;
        var headOffsetZ = headZ - hostZ;

        XYZ? leaderEndOffset = null;
        XYZ? elbowOffset = null;

        if (hasLeader)
        {
            var refLink = new Reference(originalHost);

            if (leaderCondition == LeaderEndCondition.Free)
                try
                {
                    var leaderEndRaw = originalTag.GetLeaderEnd(refLink);
                    var (endX, endY, endZ) = ProjectToView(leaderEndRaw, viewOrigin,
                        viewRightDir, viewUpDir, viewDir, viewScale);
                    leaderEndOffset = new XYZ(endX - hostX, endY - hostY, endZ - hostZ);
                }
                catch
                {
                }

            try
            {
                var elbowRaw = originalTag.GetLeaderElbow(refLink);
                var (elbowX, elbowY, elbowZ) = ProjectToView(elbowRaw, viewOrigin,
                    viewRightDir, viewUpDir, viewDir, viewScale);
                elbowOffset = new XYZ(elbowX - hostX, elbowY - hostY, elbowZ - hostZ);
            }
            catch
            {
            }
        }

        foreach (var target in targetElements)
        {
            if (target.Location is not LocationPoint targetLocation) continue;

            var targetPoint = targetLocation.Point;
            var (targetX, targetY, targetZ) = ProjectToView(targetPoint, viewOrigin,
                viewRightDir, viewUpDir, viewDir, viewScale);

            var newHeadPos = ProjectToWorld(
                targetX + headOffsetX,
                targetY + headOffsetY,
                targetZ + headOffsetZ,
                viewOrigin, viewRightDir, viewUpDir, viewDir, viewScale);

            try
            {
                var reference = new Reference(target);

                var newTag = IndependentTag.Create(
                    _doc, tagTypeId, viewId,
                    reference,
                    hasLeader, orientation, newHeadPos);

                newTag.TagHeadPosition = newHeadPos;
                newTag.LeaderEndCondition = leaderCondition;

                if (hasLeader && leaderCondition == LeaderEndCondition.Free && leaderEndOffset != null)
                {
                    var newLeaderEnd = ProjectToWorld(
                        targetX + leaderEndOffset.X,
                        targetY + leaderEndOffset.Y,
                        targetZ + leaderEndOffset.Z,
                        viewOrigin, viewRightDir, viewUpDir, viewDir, viewScale);

                    newTag.SetLeaderEnd(reference, newLeaderEnd);
                }

                if (hasLeader && elbowOffset != null)
                {
                    var newElbow = ProjectToWorld(
                        targetX + elbowOffset.X,
                        targetY + elbowOffset.Y,
                        targetZ + elbowOffset.Z,
                        viewOrigin, viewRightDir, viewUpDir, viewDir, viewScale);

                    try
                    {
                        newTag.SetLeaderElbow(reference, newElbow);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

public class ElementTypesSelectionFilter : ISelectionFilter
{
    public bool AllowElement(Element elem)
    {
        return elem is IndependentTag;
    }

    public bool AllowReference(Reference reference, XYZ position)
    {
        return false;
    }
}