using ActiveView.Models;
using Kapibara.Core;

namespace ActiveView.ViewModels;

public sealed partial class ActiveViewViewModel : ObservableObject
{
    [ObservableProperty] private List<string> _parameters = null!;
    [ObservableProperty] private string _parameter = null!;
    [ObservableProperty] private string _value = null!;
    [ObservableProperty] private bool _boolValue = true;
    [ObservableProperty] private string _headingParameterSelection = "Выбор параметра";
    [ObservableProperty] private string _headingOptionsSelection = "Выбор опции";
    [ObservableProperty] private List<Option> _options;
    [ObservableProperty] private Option _selectionOption;
    [ObservableProperty] private string _headingValue = "Значение";
    [ObservableProperty] private string _headingNotEmpty = "Пропустить заполненное";
    [ObservableProperty] private bool _skipNotEmpty = false;
    [ObservableProperty] private bool _isTextBoxVisible = true;
    [ObservableProperty] private bool _isToggleVisible = false;
    [ObservableProperty] private string _filter = string.Empty;
    private Document Document { get; set; }

    private readonly ActiveViewModel _model;
    
    public ActiveViewViewModel(ActiveViewModel model, Document document)
    {
        Document = document;
        _model = model;
        GetParameters();
        Options =
        [
            new Option("Все на виде", true),
            new Option("Выбранные", false)
        ];
        SelectionOption = Options.FirstOrDefault() ?? new Option(string.Empty, false);
    }

    partial void OnBoolValueChanged(bool value)
    {
        Value = value ? "1" : "0";
    }

    partial void OnFilterChanged(string value) => GetParameters();
    private void GetParameters () => Parameters = _model.GetParameters().
        Where(x => x.Contains(Filter, StringComparison.OrdinalIgnoreCase)).ToList();

    
    
    partial void OnParameterChanged(string value)
    {
        var definition = Document.GetProjectParameterDefinition(value);
        if (definition == null)
        {
            IsTextBoxVisible = true;
            IsToggleVisible = false;
            return;
        }

#if REVIT2022_OR_GREATER
        var isYesNo = definition.GetDataType().Equals(SpecTypeId.Boolean.YesNo);
#else
        var isYesNo = definition.ParameterType == ParameterType.YesNo;
#endif

        if (isYesNo)
        {
            IsTextBoxVisible = false;
            IsToggleVisible = true;
            Value = "1"; 
        }
        else
        {
            IsTextBoxVisible = true;
            IsToggleVisible = false;
        }
    }

    [RelayCommand]
    private async Task Execute()
    {
        await _model.ExecuteActiveViewAsyncEvent.RaiseAsync(Parameter, Value, SkipNotEmpty, SelectionOption);
    }
}

public class Option (string name, bool isAll)
{
    public string Name { get; set; } = name;
    public bool IsAll { get; set; } =  isAll;
}