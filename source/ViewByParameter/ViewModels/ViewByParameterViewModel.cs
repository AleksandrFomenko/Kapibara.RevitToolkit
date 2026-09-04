using ViewByParameter.AddFilter.View;
using ViewByParameter.AddFilter.ViewModels;
using ViewByParameter.Models;

namespace ViewByParameter.ViewModels;

public sealed partial class ViewByParameterViewModel : ObservableObject
{
    private readonly IViewByParameterModel _model;
    [ObservableProperty] private List<ViewOption> _viewOptions = null!;
    [ObservableProperty] private ViewOption _viewOption = null!;
    [ObservableProperty] private List<FilterOption> _filterOptions = null!;
    [ObservableProperty] private FilterOption _filterOption = null!;
    [ObservableProperty] private List<string> _projectParameters = null!;
    [ObservableProperty] private string? _projectParameter;
    [ObservableProperty] private List<FilterFromProject> _filtersFromProject = [];
    [ObservableProperty] private FilterFromProject _filterFromProject = null!;
    [ObservableProperty] private bool _isCheckedAll = true;
    [ObservableProperty] private bool _isCheckedAllFilters = true;
    [ObservableProperty] private List<ElementsByParameter>  _elementsByParameters = null!;
    public Action? Close;
    private readonly Func<AddFilterView> _showAddFilterWindow;


    
    public ViewByParameterViewModel(IViewByParameterModel model, Func<AddFilterView?> showAddFilterWindow)
    {
        _model = model;
        _showAddFilterWindow = showAddFilterWindow!;
        ViewOptions = model.GetViewOption()!;
        ViewOption = ViewOptions.FirstOrDefault()!;
        FilterOptions = model.GetFilterOption()!;
        FilterOption = FilterOptions.FirstOrDefault()!;
        ProjectParameters = model.GetProjectParameters()!;
    }
    private void SubscribeToElementsByParameterChanges()
    {
        foreach (var elementsByParameter in ElementsByParameters)
        {
            elementsByParameter.SetCheck += value => elementsByParameter.IsChecked = value;
            elementsByParameter.CheckButton += () => ExecuteCommand.NotifyCanExecuteChanged();
        }
    }
    
    partial void OnProjectParameterChanged(string? value)
    {
        ElementsByParameters = _model.GetElementsByParameter(value);
        SubscribeToElementsByParameterChanges();
        ExecuteCommand.NotifyCanExecuteChanged();
    }
    partial void OnIsCheckedAllChanged(bool value)
    {
        foreach (var elementsByParameter in ElementsByParameters)
        {
            elementsByParameter.OnSetCheck(value);
        }
    }
    
    partial void OnFilterFromProjectChanged(FilterFromProject value)
    {
        DeleteFilterCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsCheckedAllFiltersChanged(bool value)
    {
        if (FiltersFromProject == null) return;
        foreach (var filterFromProject in FiltersFromProject)
        {
            filterFromProject.OnSetAllChecked(value);
        }
    }

    private bool CanExecute()
    {
        return ElementsByParameters != null
               && ElementsByParameters.Any(p => p is { IsChecked: true })
               && ProjectParameter != null;
    }
    
    private bool CanDeleteFilter()
    {
        return FiltersFromProject.Count != 0;
    }
    
    private void OnFiltersChanged(object sender, FilterChangedEventArgs e)
    {
        var newFilters = e.SelectedFilters
            .Where(f => FiltersFromProject.All(x => x.Name != f.Name))
            .Select(f =>
            {
                f.IsVisible = false;
                return f;
            });

        FiltersFromProject = FiltersFromProject
            .Concat(newFilters)
            .ToList();
    }

    
    [RelayCommand]
    private void AddFilter()
    {
        var view = _showAddFilterWindow.Invoke();
        
        var viewModel = view?.ViewModel;
        if (viewModel == null) return;
        
        viewModel.FiltersChanged += OnFiltersChanged;
        
        view?.ShowDialog(); 
        
        viewModel.FiltersChanged -= OnFiltersChanged;
    }
    
    [RelayCommand(CanExecute = nameof(CanDeleteFilter))]
    private void DeleteFilter()
    {
        if (FilterFromProject == null) return;
        FiltersFromProject = FiltersFromProject
            .Where(f => f.Name != FilterFromProject.Name)
            .ToList();
    }

    
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute()
    {
        var selectedElements = ElementsByParameters
            .Where(e => e.IsChecked)
            .ToList();
        var selectedFilterFromProject = FiltersFromProject
            .Where(f => f.IsChecked)
            .ToList();

        _model.Execute(selectedElements, selectedFilterFromProject, ViewOption, ProjectParameter!, FilterOption);
        Close?.Invoke();
    }
}