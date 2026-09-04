using System.Collections.ObjectModel;
using ProjectAxes.Abstractions;
using ProjectAxes.Common;

namespace ProjectAxes.ViewModels;

public abstract partial class ToolViewModel : ObservableObject, IViewModel
{
    [ObservableProperty] private string _title = null!;
    [ObservableProperty] private string _headerSelections = null!;
    [ObservableProperty] private string _headerSelectionsDescription = null!;
    [ObservableProperty] private string _startLabel = null!;
    [ObservableProperty] private string _endLabel = null!;
    [ObservableProperty] private string _settingsDescription = null!;
    [ObservableProperty] private bool _beginningIsChecked;
    [ObservableProperty] private bool _endIsChecked;

    [ObservableProperty] private ObservableCollection<Option> _options = [];
    [ObservableProperty] private Option? _option;

    public HeaderInfo Header => new(
        Title,
        HeaderSelections,
        HeaderSelectionsDescription,
        StartLabel,
        EndLabel,
        SettingsDescription
    );

    protected ToolViewModel(HeaderInfo header)
    {
        Title = header.Title;
        HeaderSelections = header.Selections;
        HeaderSelectionsDescription = header.SelectionsDescription;
        StartLabel = header.StartLabel;
        EndLabel = header.EndLabel;
        SettingsDescription = header.SettingsDescription;

        Options =
        [
            new Option("Все на виде", OptionType.All),
            new Option("Выбранные", OptionType.Selection)
        ];
        Option = Options.FirstOrDefault();
    }
}