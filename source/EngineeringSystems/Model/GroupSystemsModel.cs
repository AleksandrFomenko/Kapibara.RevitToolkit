using System.Collections.ObjectModel;
using EngineeringSystems.GroupDataStorage;
using EngineeringSystems.Model.Abstractions;
using EngineeringSystems.ViewModels.Entities;
using Kapibara.Core;
using Group = EngineeringSystems.ViewModels.Entities.Group;

namespace EngineeringSystems.Model;

public class GroupSystemsModel : IGroupSystemsModel
{
    private const string SystemNameMissing = "Отсутствует";
    private const string SystemNameCutMissing = "Отсутствует";
    private readonly Document _doc = RevitContext.ActiveDocument!;

    public ObservableCollection<Group> GetGroupSystems()
    {
        var dto = GroupDataStorageService.Load(_doc);

        if (dto?.Data == null || dto.Data.Count == 0)
            return [];

        foreach (var group in dto.Data.Where(group => group.Systems == null)) 
            group.Systems = [];

        var cats = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_PipingSystem,
            BuiltInCategory.OST_DuctSystem
        };

        var existingIds = new FilteredElementCollector(_doc)
            .WherePasses(new ElementMulticategoryFilter(cats))
            .WhereElementIsNotElementType()
            .ToElementIds()
            .Select(id => id.GetValue())
            .ToHashSet();

        var hasChanges = false;

        foreach (var group in dto.Data.Where(g => g.Systems != null))
        {
            var toRemove = group?.Systems?.Where(s => s.SystemId != 0 && !existingIds.Contains(s.SystemId))
                .ToList();

            if (toRemove == null) continue;
            foreach (var system in toRemove)
            {
                group?.Systems?.Remove(system);
                hasChanges = true;
            }
        }

        if (hasChanges)
            AfterClose(new ObservableCollection<Group>(dto.Data), GetSelectedParameter());

        return new ObservableCollection<Group>(dto.Data);
    }

    public ObservableCollection<EngineeringSystem> GetProjectSystems()
    {
        var groups = GetGroupSystems();
        var usedSystemIds = new HashSet<long>(
            groups
                .Where(g => g.Systems != null)
                .SelectMany(g => g.Systems!)
                .Select(s => s.SystemId)
                .Where(id => id != 0)
        );

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
            .Where(s => s.SystemId != 0 && !usedSystemIds.Contains(s.SystemId))
            .ToList();

        var filteredResult = result
            .OrderBy(s => s.NameSystem)
            .ToList();

        return filteredResult.Count != 0
            ? new ObservableCollection<EngineeringSystem>(filteredResult)
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
    public void AfterClose(ObservableCollection<Group>? groups, string userParameter)
    {
        if (groups is null) return;

        var dto = new GroupStorageDto
        {
            Data = groups.ToList(),
            SelectedUserParameter = userParameter
        };

        using var t = new Transaction(_doc, "Сохранение групп инженерных систем");
        t.Start();
        GroupDataStorageService.Save(_doc, dto);
        t.Commit();
    }

    public List<string> GetUserParameters() =>
        _doc.GetProjectParameterNames(SpecTypeId.String.Text, ParameterBindingKind.Instance).ToList();


    public string GetSelectedParameter()
    {
        var dto = GroupDataStorageService.Load(_doc);
        return dto?.SelectedUserParameter ?? string.Empty;
    }

    public void Execute(List<Group> systemGroups, string userParameter, bool createView)
    {
        using var t = new Transaction(_doc, "System Groups");
        t.Start();
        foreach (var group in systemGroups)
        {
            if (group.Systems == null || group.Systems.Count == 0) continue;
            var systemNames = group.Systems
                .Select(s => s.NameSystem)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
            if (systemNames.Count == 0) continue;
            var elements = EngineeringSystemCore.GetElementsInSystem(systemNames, true);

            foreach (var element in elements)
            {
                var parameter = element.LookupParameter(userParameter);
                if (parameter is { IsReadOnly: false }) parameter.Set(group.Name);
                if (element is not FamilyInstance fi) continue;
                
                foreach (var subElem in fi.GetAllSubComponents())
                {

                    if (subElem.TryGetParameterByName(userParameter, out var subElementParameter))
                    {
                        subElementParameter?.SetValue(group.Name);
                    }
                }
            }

            if (createView)
            {
                var cats = GetCategoriesByParameter(userParameter);
                var view3D = new View3D(_doc);
                var filter = new Filter(_doc);
                var view = view3D.CreateView3D(group.Name);
                if (cats == null) continue;
                var filterElement = filter.CreateFilter(cats, userParameter, group.Name,
                    new FilterOption(string.Empty, "CreateNotEqualsRule"));
                view.AddFilter(filterElement.Id);
                view.SetFilterVisibility(filterElement.Id, false);
            }
        }

        t.Commit();
    }


    private List<BuiltInCategory>? GetCategoriesByParameter(string parameterName)
    {
        var bindingMap = _doc.ParameterBindings;
        var iterator = bindingMap.ForwardIterator();
        iterator.Reset();

        while (iterator.MoveNext())
        {
            var definition = iterator.Key;
            var binding = iterator.Current;

            if (definition == null || definition.Name != parameterName)
                continue;

            CategorySet? categories = null;

            if (binding is InstanceBinding instanceBinding)
                categories = instanceBinding.Categories;
            else if (binding is TypeBinding typeBinding)
                categories = typeBinding.Categories;

            if (categories == null)
                continue;

            var builtInCategories = new List<BuiltInCategory>();
            var catIterator = categories.ForwardIterator();
            catIterator.Reset();

            while (catIterator.MoveNext())
            {
                var category = catIterator.Current as Category;
                if (category == null)
                    continue;

#if REVIT2023_OR_GREATER
                try
                {
                    builtInCategories.Add(category.BuiltInCategory);
                }
                catch
                {
                    // ingored
                }
#else
            var id = category.Id.GetValue();
            var underlyingType = Enum.GetUnderlyingType(typeof(BuiltInCategory));
            var boxed = underlyingType == typeof(long) ? (object)id : (int)id;

            if (Enum.IsDefined(typeof(BuiltInCategory), boxed))
                builtInCategories.Add((BuiltInCategory)boxed);
#endif
            }

            return builtInCategories;
        }

        return null;
    }

    private string? GetCutSystemName(Element mepSystem)
    {
        var typeSystemId = mepSystem.GetTypeId();

        if (typeSystemId == ElementId.InvalidElementId) return "";

        var typeSystem = _doc.GetElement(typeSystemId);
        var par = typeSystem?.get_Parameter(BuiltInParameter.RBS_SYSTEM_ABBREVIATION_PARAM);
        return par?.AsString();
    }
}