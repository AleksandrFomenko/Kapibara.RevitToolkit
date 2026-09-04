using EngineeringSystems.ViewModels;
using EngineeringSystems.ViewModels.Entities;
using Kapibara.Core;

namespace EngineeringSystems.Model;

public class Data : IData
{
    private readonly Document _doc = RevitContext.ActiveDocument!;
    private const string SystemNameMissing = "Отсутствует";
    private const string SystemNameCutMissing = "Отсутствует";

    
    private string? GetCutSystemName(Element mepSystem)
    {
        var typeSystemId = mepSystem.GetTypeId();

        if (typeSystemId == ElementId.InvalidElementId)
        {
            return null;
        }

        var typeSystem = _doc.GetElement(typeSystemId);
        var par = typeSystem?.get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM);
        return par?.AsString() == "" ? null : par?.AsString();
    }
    
    public List<EngineeringSystem> GetSystems(string filter)
    {
        var cats = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_PipingSystem,
            BuiltInCategory.OST_DuctSystem
        };
        
        var catFilter = new ElementMulticategoryFilter(cats);
        
        var systems = new FilteredElementCollector(_doc)
            .WherePasses(catFilter)
            .WhereElementIsNotElementType()
            .ToElements();

        var result = systems
            .Select(f => new EngineeringSystem
            {
                NameSystem = f?.Name ?? SystemNameMissing,
                IsChecked = false,
                CutSystemName = GetCutSystemName(f!) ?? SystemNameCutMissing,
                SystemId = f?.Id?.GetValue() ?? 0
            })
            .ToList();
        var filteredResult = result
            .Where(s => s.NameSystem?.ToLower().Contains(filter.ToLower()) ?? false)
            .OrderBy(s => s.NameSystem)
            .ToList();

        return filteredResult.Any() 
            ? filteredResult 
            :
            [
                new EngineeringSystem
                {
                    NameSystem = "Не найдено подходящих систем",
                    IsChecked = false,
                    CutSystemName = string.Empty,
                    SystemId = 0
                }
            ];
    }
}