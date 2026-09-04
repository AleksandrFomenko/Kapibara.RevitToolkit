namespace EngineeringSystems.Model;

internal class View3D
{
    private Document _doc;

    internal View3D(Document doc)
    {
        _doc = doc;
    }
    
    internal Autodesk.Revit.DB.View3D CreateView3D(string name)
    {
        var viewType = new FilteredElementCollector(_doc)
            .OfClass(typeof(Autodesk.Revit.DB.View3D))
            .WhereElementIsNotElementType()
            .Select(v => _doc.GetElement(v.GetTypeId()))
            .FirstOrDefault(e => e != null)?.Id;
        
        var view = Autodesk.Revit.DB.View3D.CreateIsometric(_doc, viewType);
        
        var uniqueName = GetUniqueViewName(name);
        view.Name = uniqueName;
        view.DetailLevel = ViewDetailLevel.Fine;
        view.Discipline = ViewDiscipline.Mechanical;
        view.DisplayStyle = DisplayStyle.HLR;
        
        var lvl = Category.GetCategory(_doc, BuiltInCategory.OST_Levels);
        var links = Category.GetCategory(_doc, BuiltInCategory.OST_RvtLinks);
        var grids = Category.GetCategory(_doc, BuiltInCategory.OST_Grids);
        
        view.SetCategoryHidden(lvl.Id, true);
        view.SetCategoryHidden(links.Id, true);
        view.SetCategoryHidden(grids.Id, true);
        
        
        return view;
    }
    
    private string GetUniqueViewName(string baseName, int suffix = 0)
    {
        var viewCollector = new FilteredElementCollector(_doc)
            .OfClass(typeof(Autodesk.Revit.DB.View3D))
            .WhereElementIsNotElementType()
            .ToElements();

        var newName = suffix == 0 ? baseName : $"{baseName}_{suffix}";
        var nameExists = viewCollector.Any(v => v.Name == newName);
        
        return nameExists ? GetUniqueViewName(baseName, suffix + 1) :
            newName;
    }
}