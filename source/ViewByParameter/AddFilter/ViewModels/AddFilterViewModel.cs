using ViewByParameter.AddFilter.Models;
using ViewByParameter.Models;

namespace ViewByParameter.AddFilter.ViewModels;

public sealed partial class AddFilterViewModel : ObservableObject
{
    private readonly IAddFilterModel _model;

    [ObservableProperty] private string _filterByName = string.Empty;
    [ObservableProperty] private List<FilterFromProject>? _filtersFromProject;
    public event Action? CloseWindow;
    
    public EventHandler<FilterChangedEventArgs>? FiltersChanged;
    
    public AddFilterViewModel(IAddFilterModel model)
    {
        _model = model;
        FiltersFromProject = _model.GetFilterProjects(string.Empty);
        SubscribeToFilters(FiltersFromProject);
    }

    partial void OnFilterByNameChanged(string value)
    {
        FiltersFromProject = _model.GetFilterProjects(value);
        SubscribeToFilters(FiltersFromProject);
    }
    
    private void SubscribeToFilters(IEnumerable<FilterFromProject>? filters)
    {
        foreach (var filter in filters!)
        {
            filter.SetAllChecked += filterIsChecked => filter.IsChecked = filterIsChecked;
            filter.CheckButton += () => ConfirmSelectionCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanConfirmSelection()
    {
        return FiltersFromProject!.Any(f => f.IsChecked);
    }

    private void OnCloseWindow()
    {
        CloseWindow?.Invoke();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var filter in FiltersFromProject!)
        {
            filter.OnSetAllChecked(true);
        }
    }
    
    [RelayCommand]
    private void CancelSelection()
    {
        foreach (var filter in FiltersFromProject!)
        {
            filter.OnSetAllChecked(false);
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanConfirmSelection))]
    private void ConfirmSelection()
    {
        if (FiltersFromProject == null) return;
        var selectedFilters = FiltersFromProject.Where(f => f.IsChecked).ToList();
        
        FiltersChanged?.Invoke(this, new FilterChangedEventArgs { SelectedFilters = selectedFilters });
        OnCloseWindow();
    }
    
}
