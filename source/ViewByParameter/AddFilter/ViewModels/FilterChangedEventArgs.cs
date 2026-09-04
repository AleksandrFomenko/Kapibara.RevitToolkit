using ViewByParameter.Models;

namespace ViewByParameter.AddFilter.ViewModels;
public class FilterChangedEventArgs : EventArgs
{
    public List<FilterFromProject> SelectedFilters { get; set; } = [];
}
