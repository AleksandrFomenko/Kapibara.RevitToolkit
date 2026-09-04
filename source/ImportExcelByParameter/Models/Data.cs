using Kapibara.Core;
using Nice3point.Revit.Toolkit.External;

namespace ImportExcelByParameter.Models;

public sealed partial  class Data
{
    private readonly Document? _doc;

    internal Data(Document? doc)
    {
        _doc = doc;
    }
    
    [ExternalEvent]
    private List<string> LoadCategory()
    {
        var categories = _doc?.Settings.Categories;
        return categories!
            .Cast<Category>()
            .Where(i => i.IsVisibleInUI)
            .Where(i => i.AllowsBoundParameters)
            .Where(i => i.CategoryType == CategoryType.Model)
            .Select(c => c.Name)
            .ToList();
    }
    
    [ExternalEvent]
    private List<string> LoadAllParameters() => _doc?.GetProjectParameterNames().ToList() ?? [];
    
    [ExternalEvent]
    private List<string> LoadParameters(string categoryName)
    {
        if (categoryName == null) return [];
        var parameters = new HashSet<string>();
        var category = _doc?.Settings.Categories
            .Cast<Category>()
            .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        var cat = GetBuiltInCategory(category!);
        var types = new FilteredElementCollector(_doc)
            .OfCategory(cat)
            .WhereElementIsElementType()
            .ToList();
        var elements = new FilteredElementCollector(_doc)
            .OfCategory(cat)
            .WhereElementIsNotElementType()
            .ToList();
        foreach (var type in types.Distinct()) 
        {
            if (type != null)
            {
                foreach (var param in type.GetParametersCore()
                             .Select(p => p.Definition.Name)) 
                {
                    parameters.Add(param); 
                }
            }
            var elem = elements.FirstOrDefault(e => e.Name == type!.Name);
            if (elem == null) continue;
            {
                foreach (var param in elem.GetParametersCore()
                             .Select(p => p.Definition.Name))
                {
                    parameters.Add(param); 
                }
            }
        }
        return parameters.ToList();
    }

    internal static BuiltInCategory GetBuiltInCategory(Category category)
    {
        if (category == null || !Enum.IsDefined(typeof(BuiltInCategory),
                category.Id.GetValue())) return BuiltInCategory.INVALID;
        var builtInCategory = (BuiltInCategory)category.Id.GetValue();
        return builtInCategory;
    }
}