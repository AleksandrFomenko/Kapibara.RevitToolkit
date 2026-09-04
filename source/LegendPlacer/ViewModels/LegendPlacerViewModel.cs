using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using LegendPlacer.Models;

namespace LegendPlacer.ViewModels;

public sealed partial class LegendPlacerViewModel : ObservableObject
{
    private readonly ILegendPlacerModel? _model;
    
    [ObservableProperty] private bool _isFlyoutOpen;
    [ObservableProperty] private string _legend = string.Empty;
    [ObservableProperty] private List<string>? _legends;
    [ObservableProperty] private string _filterLegend = string.Empty;
    [ObservableProperty] private List<string?> _corners;
    [ObservableProperty] private string? _corner;
    [ObservableProperty] private ObservableCollection<FolderItem> _treeItems;
    [ObservableProperty] private int _changeX;
    [ObservableProperty] private int _changeY;
    

    public LegendPlacerViewModel(ILegendPlacerModel model)
    {
        _model = model;
        
        _legends   = _model.GetLegends(string.Empty);
        _corners   = _model.GetCorners();
        _corner    = _corners.FirstOrDefault();
        _treeItems = _model.GetSheetItem();
    }
    partial void OnFilterLegendChanged(string? value)
    {
        Legends = _model?.GetLegends(value);
    }
    
    partial void OnLegendChanged(string? oldValue, string? newValue)
    {
        ExecuteCommand.NotifyCanExecuteChanged();
    }
    private bool CanExecute()
    {
        return !string.IsNullOrWhiteSpace(Legend);
    }

    [RelayCommand]
    private void ShowFlyout()
    {
        IsFlyoutOpen = true;
    }
    
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute()
    {
        try
        {
            _model?.Execute(TreeItems, Legend, Corner, ChangeX, ChangeY);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}