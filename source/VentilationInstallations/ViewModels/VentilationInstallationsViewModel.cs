using System.Collections.ObjectModel;
using System.ComponentModel;
using VentilationInstallations.Entities;
using VentilationInstallations.Model;
using VentilationInstallations.Report;

namespace VentilationInstallations.ViewModels;

public sealed partial class VentilationInstallationsViewModel : ObservableObject
{
    public readonly VentilationData Data;
    private readonly SelectService _select;
    private readonly FormulaModel _formulaModel;
    private bool _isSyncing;

    [ObservableProperty] private IReadOnlyList<string> _stringParameters;
    
    [ObservableProperty] private string? _selectedParameterName;
    [ObservableProperty] private string? _selectedParameterMark;
    [ObservableProperty] private string? _selectedParameterNameBrief;
    [ObservableProperty] private string? _selectedParameterFactory;
    [ObservableProperty] private string? _factory;

    public VentilationInstallationsViewModel(VentilationData data, SelectService select,  FormulaModel formulaModel)
    {
        Data = data;
        _select = select;
        _formulaModel = formulaModel;
        StringParameters = data.GetTextParameterNames();
        SelectedParameterName = data.FindParameter();
        SelectedParameterMark = data.FindParameter("ADSK_Марка");
        SelectedParameterNameBrief = data.FindParameter("ADSK_Наименование краткое");
        SelectedParameterFactory = data.FindParameter("ADSK_Завод-изготовитель");
        Factory = "Русклимат";
        
        foreach (var unit in Data.Units)
            unit.CheckedChanged += OnUnitCheckedChanged;

        SyncHeaderState();
    }

    public ObservableCollection<VentilationUnit> Units => Data.Units;
    public IReadOnlyList<SelectionOptionsEnum> SelectionOptions => Data.SelectionOptions;

    [ObservableProperty]
    private SelectionOptionsEnum _selectionOption;

    [ObservableProperty]
    private bool? _isAllChecked = true;
    

    public IReadOnlyList<VentilationUnit> CheckedUnits =>
        Data.Units.Where(x => x.IsChecked).ToArray();
    
    partial void OnIsAllCheckedChanged(bool? value)
    {
        if (_isSyncing) return;
        if (value is not { } state) return;

        _isSyncing = true;
        foreach (var unit in Data.Units)
            unit.IsChecked = state;
        _isSyncing = false;

        OnPropertyChanged(nameof(CheckedUnits));
    }
    
    private void OnUnitCheckedChanged(object? sender, EventArgs e)
    {
        if (_isSyncing) return;

        SyncHeaderState();
        OnPropertyChanged(nameof(CheckedUnits));
    }

    private void SyncHeaderState()
    {
        _isSyncing = true;
        IsAllChecked = Data.Units.Count(x => x.IsChecked) switch
        {
            0 => false,
            var n when n == Data.Units.Count => true,
            _ => null
        };
        _isSyncing = false;
    }

    [RelayCommand]
    private void Select(VentilationUnit unit) => _select.Select(unit.Id);

    [RelayCommand]
    private void SelectChecked() => _select.Select(CheckedUnits.Select(u => u.Id));

    [RelayCommand]
    private void Execute()
    {
        _formulaModel.Execute(this);
        WarningsReportService.ShowReportInBackground(_formulaModel.Warnings);
    }
}