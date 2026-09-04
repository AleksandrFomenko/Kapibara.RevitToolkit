using Kapibara.Core;
using ViewByParameter.Models;

namespace ViewByParameter.services;

public class FilterCreationService(Document document)
{
    internal ParameterFilterElement CreateFilter(
        string nameParameter,
        string value,
        FilterOption filterOption)
    {
        var categories = document.GetCategoriesByParameter(nameParameter);
        var parameterId = SearchParameter(nameParameter)
            ?? throw new InvalidOperationException($"Параметр '{nameParameter}' не найден в проекте, что странно))");

        var rule = CreateStringRule(filterOption.RevitApiMethodName, parameterId, value)
            ?? throw new ArgumentException($"Неизвестное правило: {filterOption.RevitApiMethodName}");
        
        var categoryIds = categories?.Select(cat => new ElementId(cat)).ToList();
        var uniqueName = GetUniqueFilterName(nameParameter, value);
        var filter = new ElementParameterFilter([rule]);
        return ParameterFilterElement.Create(document, uniqueName, categoryIds, filter);
    }

    private static FilterRule? CreateStringRule(string method, ElementId paramId, string value)
    {
        return method switch
        {
            "CreateContainsRule"      => Make(ParameterFilterRuleFactory.CreateContainsRule),
            "CreateNotContainsRule"   => Make(ParameterFilterRuleFactory.CreateNotContainsRule),
            "CreateBeginsWithRule"    => Make(ParameterFilterRuleFactory.CreateBeginsWithRule),
            "CreateNotBeginsWithRule" => Make(ParameterFilterRuleFactory.CreateNotBeginsWithRule),
            "CreateEndsWithRule"      => Make(ParameterFilterRuleFactory.CreateEndsWithRule),
            "CreateNotEndsWithRule"   => Make(ParameterFilterRuleFactory.CreateNotEndsWithRule),
            "CreateEqualsRule"        => Make(ParameterFilterRuleFactory.CreateEqualsRule),
            "CreateNotEqualsRule"     => Make(ParameterFilterRuleFactory.CreateNotEqualsRule),
            _ => null
        };

#if REVIT2023_OR_GREATER
        FilterRule Make(Func<ElementId, string, FilterRule> factory) =>
            factory(paramId, value);
#else
        FilterRule Make(Func<ElementId, string, bool, FilterRule> factory) =>
            factory(paramId, value, false);
#endif
    }

    private ElementId? SearchParameter(string name)
    {
        var iterator = document.ParameterBindings.ForwardIterator();

        while (iterator.MoveNext())
        {
            if (iterator.Key is InternalDefinition definition && definition.Name == name)
            {
                return definition.Id;
            }
        }

        return null;
    }

    private string GetUniqueFilterName(string parameterName, string baseName, int suffix = 0)
    {
        var newName = suffix == 0
            ? $"{parameterName}_{baseName}"
            : $"{parameterName}_{baseName}_{suffix}";

        var nameExists = new FilteredElementCollector(document)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .Any(f => f.Name == newName);

        return nameExists ? GetUniqueFilterName(parameterName, baseName, suffix + 1) : newName;
    }
}