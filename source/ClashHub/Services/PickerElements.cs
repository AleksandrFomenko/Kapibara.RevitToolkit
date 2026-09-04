namespace ClashHub.Services;

public class PickerElements : IPickerElements
{
    public void PickElement(long id) => PickElements([id]);

    public void PickElements(IEnumerable<long> ids)
    {
        var uiDoc = RevitContext.ActiveUiDocument;
        if (uiDoc == null) return;

        var doc = uiDoc.Document;

        var requested = ids.ToList();
        var validIds = requested
            .Where(id => id > 0)
            .Select(id => id.ToElementId())
            .Where(elementId => doc.GetElement(elementId) != null)
            .ToList();
        

        if (validIds.Count == 0) return;

        uiDoc.Selection.SetElementIds(validIds);
        uiDoc.ShowElements(validIds);
    }
}