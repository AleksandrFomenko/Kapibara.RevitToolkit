namespace SolidIntersection.Models;

public interface ISolidIntersectionModel
{
    internal List<SelectedItems> LoadedFamilies(string name, Project project);
    public List<Project> LoadedProject();
    public List<string> LoadedParameters();
    internal void Execute(IEnumerable<SelectedItems> selectedItems, string parameterName, Project project);
    internal void Execute(IEnumerable<SelectedItems> selectedItems, string parameterName, string value, Project project);
}