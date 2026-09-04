using System.Collections.ObjectModel;
using SortingCategories.Model;

namespace SortingCategories.ViewModels;

public partial class SubFamiliesViewModel : ObservableObject
{
    private readonly SubFamiliesModel _model;

    [ObservableProperty] private List<string>? _projectParameters;
    [ObservableProperty] private string? _parameterForSort;
    [ObservableProperty] private string? _parameterForGroup;
    [ObservableProperty] private string? _groupValue;
    [ObservableProperty] private ObservableCollection<Algorithm>? _algorithms;
    [ObservableProperty] private Algorithm? _algorithm;

    public SubFamiliesViewModel(SubFamiliesModel model)
    {
        _model = model;
        GetData();
    }

    private void GetData()
    {
        ProjectParameters = _model.GetParameters();
        Algorithms = _model.GetAlgorithms();
    }
    
    [RelayCommand]
    private void Execute()
    {
        Algorithm?.Execute(ParameterForSort!, ParameterForGroup!, GroupValue!);
    }
}