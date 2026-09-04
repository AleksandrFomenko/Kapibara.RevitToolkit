using VentilationInstallations.Entities;
using VentilationInstallations.ViewModels;

namespace VentilationInstallations.Model;

public class FormulaModel
{
    private const string Name      = "ИОС_Формула_Наименование";
    private const string Mark      = "ИОС_Формула_Марка";
    private const string ShortName = "ИОС_Формула_Наименование краткое";
    
    private readonly Document? _doc = RevitContext.ActiveDocument;
    private readonly List<string> _warnings = [];

    public IReadOnlyList<string> Warnings => _warnings;

    private VentilationInstallationsViewModel _viewModel;
    public void Execute(VentilationInstallationsViewModel vm)
    {
        _viewModel = vm;
        _warnings.Clear();
        var map = BuildMap(vm);
        if (map.Count == 0)
        {
            _warnings.Add("Не выбраны параметры для записи результата");
            return;
        }

        var elems = GetElements(vm.Data, vm.SelectionOption);
        if (elems.Count == 0) return;

        using var t = new Transaction(_doc, "Расчёт формул");
        t.Start();

        try
        {
            foreach (var elem in elems)
                SetFormulaRecursive(elem, map);
            t.Commit();
        }
        catch (Exception e)
        {
            _warnings.Add($"Критическая ошибка: {e}");
        }
    }
    
    private static List<(string Source, string Target)> BuildMap(VentilationInstallationsViewModel vm)
    {
        var map = new List<(string, string)>(3);

        if (!string.IsNullOrWhiteSpace(vm.SelectedParameterName))
            map.Add((Name, vm.SelectedParameterName!));

        if (!string.IsNullOrWhiteSpace(vm.SelectedParameterMark))
            map.Add((Mark, vm.SelectedParameterMark!));
        
        if (!string.IsNullOrWhiteSpace(vm.SelectedParameterNameBrief))
            map.Add((Mark, vm.SelectedParameterNameBrief!));

        if (!string.IsNullOrWhiteSpace(vm.SelectedParameterFactory))
            map.Add((ShortName, vm.SelectedParameterFactory!));

        return map;
    }
    
    private void SetFormulaRecursive(Element element, List<(string Source, string Target)> map)
    {
        SetFormula(element, map);
        SetFactory(element, _viewModel.SelectedParameterFactory!, _viewModel.Factory!);

        if (element is not FamilyInstance familyInstance) return;

        foreach (var subElemId in familyInstance.GetSubComponentIds())
        {
            if (_doc!.GetElement(subElemId) is { } subElem)
            {
                SetFormulaRecursive(subElem, map);
                SetFactory(subElem, _viewModel.SelectedParameterFactory!, _viewModel.Factory!);
            }
        }
    }

    private void SetFactory(Element element, string parameterForFactory, string valueFactory)
    {
        FindParameter(element, parameterForFactory)?.Set(valueFactory);
    }

    private void SetFormula(Element element, List<(string Source, string Target)> map)
    {
        foreach (var (sourceName, targetName) in map)
        {
            var source = FindParameter(element, sourceName);
            if (source is null || source.StorageType != StorageType.String) continue;

            var formula = source.AsString();
            if (string.IsNullOrWhiteSpace(formula)) continue;

            string result;
            try
            {
                result = FormulaEvaluator.Evaluate(formula, element);
            }
            catch (Exception e)
            {
                _warnings.Add($"Id {element.Id}, '{sourceName}': {e.Message}");
                continue;
            }
            
            var target = FindParameter(element, targetName);
            if (target is null)
            {
                _warnings.Add($"Id {element.Id}: параметр '{targetName}' не найден");
                continue;
            }

            if (target.IsReadOnly || target.StorageType != StorageType.String)
            {
                _warnings.Add($"Id {element.Id}: '{targetName}' только для чтения или не текстовый");
                continue;
            }

            target.Set(result);
        }
    }
    
    private static Parameter? FindParameter(Element element, string name)
    {
        var parameter = element.LookupParameter(name);
        if (parameter is not null) return parameter;

        var typeId = element.GetTypeId();
        return typeId == ElementId.InvalidElementId ? null : element.Document.GetElement(typeId)?.LookupParameter(name);
    }

    private List<Element> GetElements(VentilationData data, SelectionOptionsEnum options) => options switch
    {
        SelectionOptionsEnum.SelectedInTable   => CollectFromTable(data),
        SelectionOptionsEnum.SelectedInProject => CollectFromSelection(),
        SelectionOptionsEnum.OnView            => Collect(_doc!.ActiveView.Id),
        SelectionOptionsEnum.All               => Collect(),
        _ => throw new ArgumentOutOfRangeException(nameof(options), options, null)
    };
    
    private List<Element> CollectFromTable(VentilationData data) =>
        data.Units
            .Where(u => u.IsChecked)
            .Select(u => _doc!.GetElement(ToElementId(u.Id)))
            .Where(e => e is not null)
            .ToList()!;
    
    private List<Element> CollectFromSelection()
    {
        var uiDoc = RevitContext.ActiveUiDocument;
        if (uiDoc is null) return [];

        var selectedIds = uiDoc.Selection.GetElementIds();
        if (selectedIds.Count == 0) return [];

        return new FilteredElementCollector(_doc, selectedIds)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .WhereElementIsNotElementType()
            .ToElements()
            .ToList();
    }

    private List<Element> Collect(ElementId? viewId = null) =>
        (viewId is null
            ? new FilteredElementCollector(_doc)
            : new FilteredElementCollector(_doc, viewId))
        .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
        .WhereElementIsNotElementType()
        .ToElements()
        .ToList();
    private static ElementId ToElementId(long id) =>
#if REVIT2024_OR_GREATER
    new ElementId(id);
#else
        new ElementId((int)id);
#endif
}