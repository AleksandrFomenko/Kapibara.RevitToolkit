using SheetManager.Models;
namespace SheetManager.ViewModels;

public partial class CreateSheetsViewModel : ObservableObject
{
    private readonly SheetManagerModel _model;
    public List<TitleBlockItem> TitleBlocks { get; }
    public event Action<int>? Completed;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private TitleBlockItem? _selectedTitleBlock;
    [ObservableProperty] private List<string> _parameters = new();
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _selectedParameter = string.Empty;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private int _count = 1;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private int _startValue = 1;
    [ObservableProperty] private string _prefix = string.Empty;
    [ObservableProperty] private string _suffix = string.Empty;
    [ObservableProperty] private bool _isSystemParameter = true;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private bool _isUserParameter;
    [ObservableProperty] private string _message = string.Empty;

    public CreateSheetsViewModel(SheetManagerModel model)
    {
        _model = model;
        TitleBlocks = model.GetTitleBlocks();
        SelectedTitleBlock = TitleBlocks.FirstOrDefault();
        if (model.IsAvailable && TitleBlocks.Count == 0) Message = "В проекте нет рамок. Загрузите семейство основной надписи.";
    }
    partial void OnSelectedTitleBlockChanged(TitleBlockItem? value)
    {
        Parameters = new();
        SelectedParameter = string.Empty;
        if (IsUserParameter && value != null) LoadParameters();
    }
    partial void OnIsUserParameterChanged(bool value)
    {
        if (value && SelectedTitleBlock != null) LoadParameters();
    }
    private void LoadParameters()
    {
        Parameters = new();
        SelectedParameter = string.Empty;
        try
        {
            Parameters = _model.GetParameterNames(SelectedTitleBlock!);
            SelectedParameter = Parameters.FirstOrDefault() ?? string.Empty;
            Message = Parameters.Count == 0 ? "Доступные пользовательские параметры не найдены." : string.Empty;
        }
        catch (Exception exception) { Message = exception.Message; }
    }
    private bool CanCreate() => _model.IsAvailable && SelectedTitleBlock != null && Count > 0 && StartValue >= 0
        && (long)StartValue + Count - 1 <= int.MaxValue && (!IsUserParameter || !string.IsNullOrWhiteSpace(SelectedParameter));
    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        try
        {
            _model.CreateSheets(SelectedTitleBlock!, Count, StartValue, Prefix, Suffix,
                IsSystemParameter, IsUserParameter, SelectedParameter);
            Message = $"Создано листов: {Count}.";
            Completed?.Invoke(Count);
        }
        catch (Exception exception) { Message = $"Не удалось создать листы: {exception.Message}"; }
    }
}
