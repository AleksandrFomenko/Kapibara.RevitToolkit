using ViewByParameter.Models;

namespace ViewByParameter.services;

public class ViewCreationService(Document document)
{
    internal View3D CreateView3D(string parameterName, string name, ViewOption viewOption)
    {
        var viewType = GetViewTypeIdByName(viewOption?.Type?.Name ?? string.Empty);
        if (viewType == null) return null!;
        var view = View3D.CreateIsometric(document, viewType);
        
        var uniqueName = GetUniqueViewName(parameterName, name);
        view.Name = uniqueName;
        view.DetailLevel = ViewDetailLevel.Fine;
        view.DisplayStyle = DisplayStyle.HLR;
        
        return view;
    }
    
    private ElementId? GetViewTypeIdByName(string name)
    {
        return new FilteredElementCollector(document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .Where(v => v.Name == name)
            .Select(v => v.Id)
            .FirstOrDefault();
    }
    
    private string GetUniqueViewName(string parameterName, string baseName, int suffix = 0)
    {
        var viewCollector = new FilteredElementCollector(document)
            .OfClass(typeof(View3D))
            .WhereElementIsNotElementType()
            .ToElements();

        var newName = suffix == 0 ? $"{parameterName}_{baseName}" : $"{parameterName}_{baseName}_{suffix}";
        var nameExists = viewCollector.Any(v => v.Name == newName);
        
        return nameExists ? GetUniqueViewName(parameterName,baseName, suffix + 1) :
            newName;
    }
}