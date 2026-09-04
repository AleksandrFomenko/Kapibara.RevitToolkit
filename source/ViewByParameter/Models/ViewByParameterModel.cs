using Kapibara.Core;
using ViewByParameter.services;

namespace ViewByParameter.Models;

public class ViewByParameterModel(
    Document document, 
    FilterCreationService filterCreationService, 
    ViewCreationService viewCreationService) : IViewByParameterModel
{
    public List<ViewOption?> GetViewOption()
    {
        return
        [
            GetViewOption("3D вид", ViewFamily.ThreeDimensional),
            //GetViewOption("План", ViewFamily.FloorPlan),
        ];
    }

    private ViewOption GetViewOption(string name, ViewFamily viewFamily)
    {
        var types = new FilteredElementCollector(document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .Where(v => v.ViewFamily == viewFamily)
            .Select(v => new Type(v.Name))
            .ToList();

        return new ViewOption(name, types);
    }
    
    public List<FilterOption?> GetFilterOption()
    {
        return
        [
            new FilterOption("Не равно", "CreateNotEqualsRule"),
            new FilterOption("Равно", "CreateEqualsRule"),
            new FilterOption("Не содержит", "CreateNotContainsRule"),
            new FilterOption("Содержит", "CreateContainsRule"),
            new FilterOption("Начинается с", "CreateBeginsWithRule"),
            new FilterOption("Не начинается с", "CreateNotBeginsWithRule"),
            new FilterOption("Заканчивается на", "CreateEndsWithRule"),
            new FilterOption("Не заканчивается на", "CreateNotEndsWithRule")
        ];
    }
    public List<FilterFromProject?> GetFiltersFromProject()
    {
        return [..new List<FilterFromProject?>(new List<FilterFromProject?>())];
    }
    public List<string> GetProjectParameters()
    {
        return document.ParameterBindings
            .EnumerateEntries()
            .Select(entry => entry.Definition.Name)
            .ToList();
    }
    public List<ElementsByParameter> GetElementsByParameter(string? parameterName)
    {
        List<ElementsByParameter> result = [];
        var dict = GetParameterValueStatistics(parameterName);

        result.AddRange(dict.Select(person => new ElementsByParameter(person.Key, person.Value)));
        return result;
    }
    
    private Dictionary<string, int> GetParameterValueStatistics(string? parameterName)
    {
        var parameterValueCounts = new Dictionary<string, int>();

        var projectElements = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .ToList();

        foreach (var projectElement in projectElements)
        {
            
            if (projectElement.TryGetParameterByName(parameterName!, out var parameter))
            {
                var parameterResult = parameter?.AsValueString();
                if (string.IsNullOrEmpty(parameterResult))
                {
                    continue;
                }

                IncrementCount(parameterValueCounts, parameterResult);
            }
        }

        return parameterValueCounts;
    }

    private void IncrementCount(Dictionary<string, int> dict, string key)
    {
        if (dict.TryGetValue(key, out var count))
            dict[key] = count + 1;
        else
            dict[key] = 1;
    }

    private ParameterFilterElement? GetFilterProject(string name)
    {
        return new FilteredElementCollector(document)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .FirstOrDefault(f => f.Name == name);
    }
    
    public void Execute(
        List<ElementsByParameter> elementsByParameters,
        List<FilterFromProject> filtersFromProject,
        ViewOption viewOption, 
        string parameterName,
        FilterOption filterOption)
    {
        using var t = new Transaction(document, "View by filters");
        t.Start();
            
        foreach (var elementsByParameter in elementsByParameters)
        {
            var view = viewCreationService.CreateView3D(parameterName,elementsByParameter.Value, viewOption);
            var filter = filterCreationService.CreateFilter(parameterName, elementsByParameter.Value, filterOption);
            view.AddFilter(filter.Id);
            view.SetFilterVisibility(filter.Id, elementsByParameter.FilterVisible);

            foreach (var filterFromProject in filtersFromProject)
            {
                var filterProject = GetFilterProject(filterFromProject.Name);
                view.AddFilter(filterProject?.Id);
                view.SetFilterVisibility(filterProject?.Id, filterFromProject.IsVisible);
                view.SetIsFilterEnabled(filterProject?.Id, filterFromProject.IsEnabled);
            }
        }
        t.Commit();
    }
}