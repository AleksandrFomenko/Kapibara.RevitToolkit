using System.Collections.ObjectModel;
using SortingCategories.Model;
namespace SortingCategories.ViewModels;

public sealed partial class SortingCategoriesViewModel : ObservableObject
{
    private readonly ParametersMainFamiliesModel _model;
    
    [ObservableProperty] private bool _isAllChecked;
    [ObservableProperty] private ObservableCollection<RevitCategory> _revitCategories;
    [ObservableProperty] private RevitCategory? _revitCategory;
    [ObservableProperty] private List<string>? _projectParameters;
    [ObservableProperty] private List<Option>? _options;
    [ObservableProperty] private Option? _option;
    [ObservableProperty] private string? _parameterForSort;
    [ObservableProperty] private string? _parameterForGroup;
    [ObservableProperty] private bool _checkSubComponents = true;
    private readonly List<Category> _projectCategory;
    
    
    public SortingCategoriesViewModel(ParametersMainFamiliesModel model)
    {
        _model = model;
        _revitCategories = [];
        _model.RevitCategories = _revitCategories;
        ProjectParameters = _model.GetParameters();
        _projectCategory = _model.GetCategory();
        Options = ParametersMainFamiliesModel.GetOptions();
        Option = Options.FirstOrDefault() ?? throw new InvalidOperationException();
    }
    
    partial void OnIsAllCheckedChanged(bool value)
    {
        foreach (var item in RevitCategories!)
        {
            item.IsChecked = value;
        }
    }
    partial void OnRevitCategoriesChanged(ObservableCollection<RevitCategory> value) => _model.RevitCategories = value;
    
    [RelayCommand]
    private void Add() 
        =>  RevitCategories?.Add(new RevitCategory() {IsChecked = true, Categories = _projectCategory});

    [RelayCommand]
    private void Delete()
        => RevitCategories?.Remove((RevitCategory ?? RevitCategories.LastOrDefault())!);

    
    [RelayCommand]
    private void DuctPattern()
    {
        RevitCategories?.Clear();
        RevitCategories = _model.GetPattern(1, _projectCategory);
    }
    
    [RelayCommand]
    private void PipelineHotPattern()
    {
        RevitCategories?.Clear();
        RevitCategories = _model.GetPattern(2, _projectCategory);
    }
    [RelayCommand]
    private void PipelineWaterPattern()
    {
        RevitCategories?.Clear();
        RevitCategories = _model.GetPattern(3, _projectCategory);
    }
    
    
    [RelayCommand]
    private void Clear()  => RevitCategories?.Clear();


    [RelayCommand]
    private void Execute()
    {
        _model.Execute(ParameterForSort!, ParameterForGroup!, Option!.IsActiveView, CheckSubComponents);
    }
}
