using EngineeringSystems.ViewModels;
using EngineeringSystems.ViewModels.Entities;

namespace EngineeringSystems.Model;

internal class Filter
{
    private Document _doc;

    internal Filter(Document doc)
    {
        _doc = doc;
    }
    
    internal ParameterFilterElement CreateFilter(
        List <BuiltInCategory> categories,
        string nameParameter, 
        string value,
        FilterOption filterOption)
    {
        var parameterId = SearchParameter(nameParameter);
        List<FilterRule>? filterRule = filterOption.RevitApiMethodName switch
        {
#if REVIT2025_OR_GREATER
            "CreateNotContainsRule"   => [ParameterFilterRuleFactory.CreateNotContainsRule(parameterId, value)],
            "CreateNotEqualsRule"     => [ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, value)],
            "CreateNotBeginsWithRule" => [ParameterFilterRuleFactory.CreateNotBeginsWithRule(parameterId, value)],
            "Custom" =>
            [
                ParameterFilterRuleFactory.CreateNotContainsRule(parameterId, value+ ","),
                ParameterFilterRuleFactory.CreateNotEndsWithRule(parameterId, value)
            ],
#else
            "CreateNotContainsRule"   => [ParameterFilterRuleFactory.CreateNotContainsRule(parameterId, value, true)],
            "CreateNotEqualsRule"     => [ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, value, true)],
            "CreateNotBeginsWithRule" => [ParameterFilterRuleFactory.CreateNotBeginsWithRule(parameterId, value, true)],
            "Custom" =>
            [
                ParameterFilterRuleFactory.CreateNotContainsRule(parameterId, value + ",", true),
                ParameterFilterRuleFactory.CreateNotEndsWithRule(parameterId, value, true)
            ],
#endif
            _ => null
        };
        
        var categoryIds = categories.Select(cat => new ElementId(cat)).ToList();
        var uniqueName = GetUniqueFilterName(value);

        ElementFilter filter = filterRule!.Count == 1
            ? new ElementParameterFilter(filterRule[0])
            : new LogicalAndFilter(
                filterRule.Select(ElementFilter (r) => new ElementParameterFilter(r)).ToList());

        return ParameterFilterElement.Create(_doc, uniqueName, categoryIds, filter);
    }


    private ElementId? SearchParameter(string name)
    {
        var bindingMap = _doc.ParameterBindings;
        var iterator = bindingMap.ForwardIterator();
        
        while (iterator.MoveNext())
        {
            var definition = iterator.Key;
            
            if (definition != null && definition.Name == name)
            {
                return (definition as InternalDefinition)?.Id;
            }
        }
        return null; 
    }

    private string GetUniqueFilterName(string baseName, int suffix = 0)
    {
        while (true)
        {
            var filterCollector = new FilteredElementCollector(_doc).OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>();

            var newName = suffix == 0 ? baseName : $"{baseName}_{suffix}";

            var nameExists = filterCollector.Any(f => f.Name == newName);

            if (nameExists)
            {
                suffix = suffix + 1;
                continue;
            }

            return newName;
        }
    }
}