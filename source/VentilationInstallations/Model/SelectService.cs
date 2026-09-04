using Autodesk.Revit.UI;

namespace VentilationInstallations.Model;

public sealed class SelectService(UIDocument uiDoc)
{
    public void Select(long id) => Select([ToElementId(id)]);

    public void Select(IEnumerable<long> ids) =>
        Select(ids.Select(ToElementId).ToArray());

    public void Select(ElementId id) => Select([id]);

    private void Select(ICollection<ElementId> ids)
    {
        var valid = ids
            .Where(id => id != ElementId.InvalidElementId)
            .Where(id => uiDoc.Document.GetElement(id) is not null)
            .ToArray();

        if (valid.Length == 0) return;

        uiDoc.Selection.SetElementIds(valid);

        try
        {
            uiDoc.ShowElements(valid);
        }
        catch (Autodesk.Revit.Exceptions.ApplicationException)
        {
        }
    }

    private static ElementId ToElementId(long value)
    {
#if REVIT2024_OR_GREATER
        return new ElementId(value);
#else
        return new ElementId((int)value);
#endif
    }
}