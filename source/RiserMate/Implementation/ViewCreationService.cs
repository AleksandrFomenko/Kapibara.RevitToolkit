using RiserMate.Abstractions;

namespace RiserMate.Implementation;

public class ViewCreationService : IViewCreationService
{
    private readonly Document? _document = RevitContext.ActiveDocument;

    public View3D CreateView3D(string name, string viewTypeName)
    {
        {
            var viewType = GetViewTypeIdByName(viewTypeName);
            if (viewType == null) return null!;
            var view = View3D.CreateIsometric(_document, viewType);

            var uniqueName = GetUniqueViewName(name);
            view.Name = uniqueName;
            view.DetailLevel = ViewDetailLevel.Fine;
            view.DisplayStyle = DisplayStyle.HLR;
            if (view.CanSaveOrientation()) view.SaveOrientationAndLock();

            return view;
        }
    }
    

    private ElementId? GetViewTypeIdByName(string name)
    {
        return new FilteredElementCollector(_document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .Where(v => v.Name == name)
            .Select(v => v.Id)
            .FirstOrDefault();
    }

    private string GetUniqueViewName(string baseName, int suffix = 0)
    {
        var viewCollector = new FilteredElementCollector(_document)
            .OfClass(typeof(View3D))
            .WhereElementIsNotElementType()
            .ToElements();

        var newName = suffix == 0 ? $"{baseName}" : $"{baseName}_{suffix}";
        var nameExists = viewCollector.Any(v => v.Name == newName);

        return nameExists ? GetUniqueViewName(baseName, suffix + 1) : newName;
    }
    
}