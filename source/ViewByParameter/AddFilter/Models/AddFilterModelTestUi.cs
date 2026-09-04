using ViewByParameter.Models;

namespace ViewByParameter.AddFilter.Models;

public class AddFilterModelTestUi : IAddFilterModel
{
    public List<FilterFromProject>? GetFilterProjects(string filterByName)
    {
        List<FilterFromProject>? filtersProject = [];
        for (var i = 0; i < 20; i++)
        {
            filtersProject.Add(new FilterFromProject(i.ToString() + " " + "фильтр"));
        }
        
        if (string.IsNullOrEmpty(filterByName))
        {
            return filtersProject;
        }

        return filtersProject.Where(f => f.Name.ToLower().Contains(filterByName.ToLower())).ToList();
    }
}