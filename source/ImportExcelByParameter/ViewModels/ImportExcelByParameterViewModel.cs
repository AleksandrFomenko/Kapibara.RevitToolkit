using System.IO;
using ImportExcelByParameter.Configuration;
using ImportExcelByParameter.Enums;
using ImportExcelByParameter.Models;
using Kapibara.Core;
using Microsoft.Win32;

namespace ImportExcelByParameter.ViewModels;

public sealed partial class ImportExcelByParameterViewModel : ObservableObject
{
    private readonly ExcelByParameterModel _model;
    private PluginConfig<ImportExcelConfig> Config { get; set; }

    internal static Action? CloseWindow { get; set; }

    [ObservableProperty] private List<string> _categories = null!;
    [ObservableProperty] private List<string> _parameters = null!;
    [ObservableProperty] private List<string> _sheets = null!;
    [ObservableProperty] private string _parameterFilter = string.Empty;
    
    
    [ObservableProperty] private int _currentProgress;
    [ObservableProperty] private int _maxProgress;
    [ObservableProperty] private bool _isIndeterminate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string? _pathExcel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string? _selectedCategory;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartCommand))]
    private string? _parameter;
    
    [ObservableProperty] private SelectionMode _selectionMode = SelectionMode.AllOnActiveView;
    public IEnumerable<SelectionMode> SelectionModes =>
        Enum.GetValues(typeof(SelectionMode)).Cast<SelectionMode>();

    [ObservableProperty] private string? _sheet;
    [ObservableProperty] private int? _rowNumber;
    
    public bool IsCategoryVisible => SelectionMode == SelectionMode.ByCategory;

    public ImportExcelByParameterViewModel(PluginConfig<ImportExcelConfig> cfg, ExcelByParameterModel model)
    {
        Config = cfg;
        _model = model;

        if (cfg != null)
        {
            _pathExcel = File.Exists(Config.Data.PathStr) ? Config.Data.PathStr : "File not found";
            _selectedCategory = Config.Data.Category;
            _parameter = Config.Data.Parameter;
            _sheet = Config.Data.ListStr;
            _rowNumber = Config.Data.Number;

            if (!string.IsNullOrEmpty(Config.Data.PathStr))
            {
                try { _sheets = _model.Excel.GetWorksheetNames(Config.Data.PathStr!); }
                catch { _sheets = []; }
            }
        }
        

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            Categories = await _model.Data.LoadCategoryAsyncEvent.RaiseAsync();
            await LoadParameters();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка в InitializeAsync {e.Message}");
        }
    }
    
    partial void OnSelectionModeChanged(SelectionMode value)
    {
        OnPropertyChanged(nameof(IsCategoryVisible));
        _ = LoadParameters();
    }

    partial void OnPathExcelChanged(string? value) => Config.Data.PathStr = value;

    partial void OnSelectedCategoryChanged(string? value)
    {
        Config.Data.Category = value;
        Config.Save();;
        _ = LoadParameters();
    }
    
    partial void OnParameterFilterChanged(string value) => _ = LoadParameters();

    partial void OnParameterChanged(string? value)
    {
        Config.Data.Parameter = value;
        Config.Save();
    }

    partial void OnSheetChanged(string? value)
    {
        Config.Data.ListStr = value;
        Config.Save();
    }

    partial void OnRowNumberChanged(int? value)
    {
        Config.Data.Number = value;
        Config.Save();
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task Start()
    {
        _model.SetParameterName(Parameter!);
        _model.SetSheetName(Sheet!);
        _model.SetRowNumber(RowNumber);

        CurrentProgress = 0;

        var isFirst = true;
        var progress = new Progress<int>(value =>
        {
            if (isFirst) { MaxProgress = value; isFirst = false; }
            else CurrentProgress = value;
        });

         await _model.ExecuteAsyncEvent.RaiseAsync(PathExcel!, SelectedCategory!, SelectionMode, progress);
    }

    [RelayCommand]
    private void SelectPath()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        PathExcel = dialog.FileName;
        Config.Save();
        
        try { Sheets = _model.Excel.GetWorksheetNames(PathExcel); }
        catch { Sheets = []; }
    }

    private bool CanExecute() =>
        !string.IsNullOrEmpty(Parameter) &&
        !string.IsNullOrEmpty(PathExcel) &&
        (SelectionMode != SelectionMode.ByCategory || !string.IsNullOrEmpty(SelectedCategory));

    private async Task LoadParameters()
    {
        var all = SelectionMode == SelectionMode.ByCategory
            ? await _model.Data.LoadParametersAsyncEvent.RaiseAsync(SelectedCategory!)
            : await _model.Data.LoadAllParametersAsyncEvent.RaiseAsync();

        Parameters = string.IsNullOrEmpty(ParameterFilter)
            ? all
            : all.Where(p => p.Contains(ParameterFilter, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}