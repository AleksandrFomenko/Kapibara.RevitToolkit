using ViewByParameter.Models;

namespace ViewByParameter.AddFilter.Models;

public class AddFilterModel(Document document) : IAddFilterModel
{
    public List<FilterFromProject>? GetFilterProjects(string filterByName)
    {
        return new FilteredElementCollector(document)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .Where(f => string.IsNullOrEmpty(filterByName) || 
                        (f.Name != null && f.Name.ToLower().Contains(filterByName.ToLower())))
            .Select(f => new FilterFromProject(f.Name))
            .ToList();
    }
}