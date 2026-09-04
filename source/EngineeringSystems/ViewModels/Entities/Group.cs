using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;


namespace EngineeringSystems.ViewModels.Entities;

public partial class Group : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isChecked;
    [ObservableProperty] private ObservableCollection<EngineeringSystem>? _systems;
    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isDeleted;

    public Group(string name, ObservableCollection<EngineeringSystem> systems)
    {
        _name = name;
        _systems = systems ?? [];
        _isExpanded = false;
    }
}