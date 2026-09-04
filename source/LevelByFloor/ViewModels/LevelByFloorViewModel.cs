using System.Windows;
using LevelByFloor.Models;
using Options = LevelByFloor.Models.Options;

namespace LevelByFloor.ViewModels;

public partial class LevelByFloorViewModel : ObservableObject
{
    private readonly LevelByFloorModel _model;
    internal static Action? Close;
    
    [ObservableProperty]
    private IReadOnlyList<string>? _parameters;
    
    [ObservableProperty]
    private string _parameter = null!;
    
    [ObservableProperty]
    private List<Options>? _options;
    
    [ObservableProperty]
    private Options? _option;
    
    [ObservableProperty]
    private string _prefix = null!;
    
    [ObservableProperty]
    private string _suffix = null!;
    
    [ObservableProperty]
    private string _indent = "0";

    public LevelByFloorViewModel(Document doc, LevelByFloorModel model)
    {
        _model = model;
        Parameters = _model.LoadParameters();
        Options =
        [
            new Options("Элементы на активном виде", new FilteredElementCollector(doc, doc.ActiveView.Id)),
            new Options("Все элементы в проекте", new FilteredElementCollector(doc))
        ];
        Option = Options.FirstOrDefault();
    }
    partial void OnParameterChanged(string value)
    {
        ExecuteCommand.NotifyCanExecuteChanged();
    }
    private bool CanExecuteCommand()
    {
        return Parameter != null;
    }
    
    [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
    private void Execute(Window window)
    {
        _model.SetOpt(Option!);
        _model.Execute(Parameter,Suffix,Prefix, Indent);
        Close?.Invoke();
    }
}