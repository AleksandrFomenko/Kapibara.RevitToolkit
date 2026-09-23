using System.Windows;
using EngineeringSystems.Configuration;
using EngineeringSystems.Model;
using EngineeringSystems.Model.Abstractions;
using EngineeringSystems.ViewModels.Entities;
using Kapibara.Core;
using Options = EngineeringSystems.ViewModels.Entities.Options;
using SystemParameters = EngineeringSystems.ViewModels.Entities.SystemParameters;

namespace EngineeringSystems.ViewModels;

public sealed partial class EngineeringSystemsViewModel : ObservableObject
{
    private readonly IData _data;
    private readonly IEngineeringSystemsModel _model;
    private readonly PluginConfig<SystemNameConfig> _config;

    internal static Action? Close;

    [ObservableProperty] private GridLength _firstColumnWidth;
    [ObservableProperty] private GridLength _secondColumnWidth;
    [ObservableProperty] private double _windowWidth;
    [ObservableProperty] private double _windowHeight;

    [ObservableProperty] private List<SystemParameters> _systemParameters = null!;
    [ObservableProperty] private SystemParameters _systemParameter = null!;

    [ObservableProperty] private List<string> _userParameters = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string? _userParameter;

    [ObservableProperty] private List<Options> _options = null!;
    [ObservableProperty] private Options _option = null!;

    [ObservableProperty] private List<FilterOption> _filterOptions = null!;
    [ObservableProperty] private FilterOption _filterOption = null!;

    [ObservableProperty] private List<EngineeringSystem> _engineeringSystems = null!;
    [ObservableProperty] private EngineeringSystem _engineeringSystem = null!;

    [ObservableProperty] private string _filterByName = string.Empty;
    [ObservableProperty] private bool _isCheckedAllSystems;

    [ObservableProperty] private bool _createView;
    [ObservableProperty] private bool _toggleButtonEnabled;

    public EngineeringSystemsViewModel(
        IData data,
        IEngineeringSystemsModel model,
        PluginConfig<SystemNameConfig> config)
    {
        _data = data;
        _model = model;
        _config = config;
        StartWindow();
    }

    partial void OnUserParameterChanged(string? value)
    {
        _config.Data.UserParameter = value;
        _config.Save();
    }

    partial void OnOptionChanged(Options value) => CheckOptions();

    partial void OnFilterOptionChanged(FilterOption value) => CheckOptions();

    partial void OnFilterByNameChanged(string value) => ReloadEngineeringSystems();

    partial void OnEngineeringSystemChanged(EngineeringSystem value)
        => value.IsChecked = !value.IsChecked;

    partial void OnIsCheckedAllSystemsChanged(bool value)
    {
        foreach (var system in EngineeringSystems)
            system.IsChecked = value;
    }
    

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Start()
    {
        var flag = SystemParameter.Name == "Имя системы";
        var systemString = GetCheckedSystemNames(flag);
        _model.Execute(systemString, UserParameter!, flag, CreateView, Option, FilterOption);
        Close?.Invoke();
    }

    private bool CanExecute() => UserParameter != null;
    

    private void StartWindow()
    {
        FirstColumnWidth = new GridLength(1, GridUnitType.Star);
        SecondColumnWidth = new GridLength(0, GridUnitType.Pixel);

        Options =
        [
            new Options("Выбрать систему", 1100, 750,
                new GridLength(0.5, GridUnitType.Star),
                new GridLength(1, GridUnitType.Star),
                false),
            new Options("Записать в элементы на активном виде", 500, 750,
                new GridLength(1, GridUnitType.Star),
                new GridLength(0, GridUnitType.Pixel),
                true)
        ];
        Option = Options.FirstOrDefault()!;
        FilterOptions =
        [
            new FilterOption("Не содержит", "CreateNotContainsRule"),
            new FilterOption("Не равно", "CreateNotEqualsRule"),
            new FilterOption("Не начинается с", "CreateNotBeginsWithRule")
        ];
        FilterOption = FilterOptions.First();
        ReloadEngineeringSystems();

        SystemParameters =
        [
            new SystemParameters("Имя системы"),
            new SystemParameters("Сокращение системы")
        ];
        SystemParameter = SystemParameters[0];

        UserParameters = _model.GetUserParameters().OrderBy(s => s).ToList();

        UserParameter = _config.Data.UserParameter;
    }

    private void ReloadEngineeringSystems()
        => EngineeringSystems = _data.GetSystems(FilterByName);

    private void CheckOptions()
    {
        WindowWidth = Option.Width;
        WindowHeight = Option.Height;
        FirstColumnWidth = Option.FirstColumnWidth;
        SecondColumnWidth = Option.SecondColumnWidth;
        ToggleButtonEnabled = Option?.NameOpt != "Записать в элементы на активном виде";
        if (Option?.NameOpt == "Записать в элементы на активном виде")
            CreateView = false;
    }

    private List<string> GetCheckedSystemNames(bool flag)
        => EngineeringSystems
            .Where(s => s.IsChecked)
            .Select(s => flag ? s.NameSystem : s.CutSystemName)
            .ToList();
}
