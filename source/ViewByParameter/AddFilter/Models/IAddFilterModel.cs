using ViewByParameter.Models;

namespace ViewByParameter.AddFilter.Models;

public interface IAddFilterModel
{
    public List<FilterFromProject>? GetFilterProjects(string filterByName);
}