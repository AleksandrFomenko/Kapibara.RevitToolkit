using Kapibara.Core;
using RiserMate.Abstractions;

namespace RiserMate.Implementation;

public class FilterCreationService : IFilterCreationService
{
    
    private readonly Document? _document = RevitContext.ActiveDocument;
    public ParameterFilterElement CreateFilter(string nameParameter, string value)
    {
        var categories = GetCategoriesByParameter(nameParameter);
        var parameterId = SearchParameter(nameParameter);
        
        var categoryIds = categories.Select(cat => new ElementId(cat)).ToList();
        var uniqueName = GetUniqueFilterName(nameParameter, value); 
#if REVIT2025_OR_GREATER
        var filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, value));
#else
        var filter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, value, true));
#endif
        return ParameterFilterElement.Create(_document, uniqueName, categoryIds, filter);
    }
    
    private ElementId? SearchParameter(string name)
    {
        if (_document == null) return null;
        var bindingMap = _document.ParameterBindings;
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
    
    private List<BuiltInCategory> GetCategoriesByParameter(string parameterName)
        => _document?.GetCategoriesByParameter(parameterName)
           ?? throw new InvalidOperationException($"Не найдены категории параметра '{parameterName}'.");
    private string GetUniqueFilterName(string parameterName, string baseName, int suffix = 0)
    {
        var filterCollector = new FilteredElementCollector(_document)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>();
        
        var newName = suffix == 0 ? $"{parameterName}_{baseName}" : $"{parameterName}_{baseName}_{suffix}";
        
        var nameExists = filterCollector.Any(f => f.Name == newName);
        
        return nameExists ? GetUniqueFilterName(parameterName, baseName,suffix + 1) :
            newName;
    }
}
