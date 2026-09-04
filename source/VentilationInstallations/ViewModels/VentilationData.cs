using System.Collections.ObjectModel;
using VentilationInstallations.Entities;

namespace VentilationInstallations.ViewModels;

public sealed class VentilationData(IEnumerable<VentilationUnit> units)
{
    public ObservableCollection<VentilationUnit> Units { get; } = new(units);

    public IReadOnlyList<SelectionOptionsEnum> SelectionOptions { get; } =
        (SelectionOptionsEnum[])Enum.GetValues(typeof(SelectionOptionsEnum));

    private IReadOnlyList<string>? _parameters;

    public static VentilationData CreateMock() => new(GetMockUnits());
    public static VentilationData Create() => new(GetUnits());

    private const string ParameterName = "ИОС_Формула_Наименование";
    private static Document _doc = RevitContext.ActiveDocument!;

    public IReadOnlyList<string> GetTextParameterNames()
    {
        _parameters = _doc.ParameterBindings
            .EnumerateEntries()
            .Where(e => IsTextSpec(e.Definition))
            .Select(e => e.Definition.Name)
            .OrderBy(n => n)
            .ToList();
        
        return _parameters;
    }
    
    
    public string? FindParameter(string parameterName = "ADSK_Наименование") 
        => _parameters!.FirstOrDefault(p => p == parameterName) ?? _parameters!.FirstOrDefault();
    
    
    private static bool IsTextSpec(Definition definition)
    {
        var spec = definition.GetDataType();
        return spec == SpecTypeId.String.Text
               || spec == SpecTypeId.String.MultilineText;
    }
    

    private static IEnumerable<VentilationUnit> GetMockUnits() =>
    [
        new(1001, "ВУ_Приточная установка", "Этаж 1",  "П1", "Общеобменная вентиляция"),
        new(1002, "ВУ_Приточная установка", "Этаж 1",  "П2","Общеобменная вентиляция"),
        new(1003, "ВУ_Приточная установка",   "Этаж 1",  "В1","Общеобменная вентиляция"),
        new(1004, "ВУ_Приточная установка",  "Этаж 2",  "П3","Общеобменная вентиляция"),
        new(1005, "ВУ_Приточная установка",   "Этаж 2",  "В2","Общеобменная вентиляция"),
        new(1006, "ВУ_Приточная установка",   "Этаж 2",  "В3","Общеобменная вентиляция"),
        new(1007, "ВУ_Приточная установка",   "Этаж 3",  "П4","Общеобменная вентиляция"),
        new(1008, "ВУ_Приточная установка",     "Этаж 3",  "ПВ1","Общеобменная вентиляция"),
        new(1009, "ВУ_Приточная установка",   "Кровля",  "В4","Общеобменная вентиляция"),
        new(1010, "ВУ_Приточная установка",                 "Кровля",  "ДУ1","Общеобменная вентиляция"),
        new(1011, "ВУ_Приточная установка",                "Кровля",  "ПД1","Общеобменная вентиляция"),
        new(1012, "ВУ_Приточная установка", "Подвал",  "П5","Общеобменная вентиляция"),
    ];

    private static IEnumerable<VentilationUnit> GetUnits() =>
        new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .WhereElementIsNotElementType()
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .Where(fi => fi.SuperComponent is null)
            .Where(IsOurFamilyInstance)
            .Select(fi => new VentilationUnit(
                GetIdValue(fi.Id),
                fi.Symbol.FamilyName,
                GetLevelName(_doc, fi),
                GetSystemName(fi),
                GetWorksetName(_doc, fi)))
            .OrderBy(u => u.SystemName)
            .ThenBy(u => u.FamilyName)
            .ToArray();
    
    private static string GetLevelName(Document doc, Element element)
    {
        var levelId = element.LevelId;

        if (levelId is null || levelId == ElementId.InvalidElementId)
        {
            var param = element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                        ?? element.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM)
                        ?? element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
            levelId = param?.AsElementId();
        }

        if (levelId is null || levelId == ElementId.InvalidElementId)
            return "—";

        return doc.GetElement(levelId)?.Name ?? "—";
    }


    private static bool IsOurFamilyInstance(FamilyInstance instance)
    {
        foreach (var subElementId in instance.GetSubComponentIds())
        {
            var subElement = _doc.GetElement(subElementId);
            if (subElement is null) continue;

            if (subElement.LookupParameter(ParameterName) is not null)
                return true;

            if (_doc.GetElement(subElement.GetTypeId()) is FamilySymbol symbol
                && symbol.LookupParameter(ParameterName) is not null)
                return true;
        }

        return false;
    }
    

    private static string GetWorksetName(Document doc, Element element)
    {
        if (!doc.IsWorkshared) return "—";
        return doc.GetWorksetTable().GetWorkset(element.WorksetId)?.Name ?? "—";
    }

    private static string GetSystemName(FamilyInstance instance)
    {
        var param = instance.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
        var value = param?.AsString();
        if (!string.IsNullOrWhiteSpace(value)) return value!;

        var manager = instance.MEPModel?.ConnectorManager;
        if (manager is null) return "—";

        var names = manager.Connectors
            .Cast<Connector>()
            .Select(c => c.MEPSystem)
            // ReSharper disable once RedundantEnumerableCastCall
            .OfType<MEPSystem>()
            .Select(s => s.Name)
            .Distinct()
            .ToArray();

        return names.Length > 0 ? string.Join(", ", names) : "—";
    }

    private static long GetIdValue(ElementId id)
    {
#if REVIT2024_OR_GREATER
    return id.Value;
#else
        return id.IntegerValue;
#endif
    }
}