using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Ookii.Dialogs.Wpf;
using SheetManager.Models;
namespace SheetManager.ViewModels;

public partial class PrintSheetsViewModel : ObservableObject
{
    private readonly SheetManagerModel _model;
    public event Action<int>? Completed;
    [ObservableProperty] private ObservableCollection<FolderItem> _treeItems = new();
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PrintCommand))]
    private string _pathFolder = string.Empty;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PrintCommand))]
    private bool _combine;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(PrintCommand))]
    private string _fileName = "Листы";
    [ObservableProperty] private string _message = string.Empty;
    public int SelectedCount => TreeItems.SelectMany(f => f.GetSheets()).Count(s => s.IsChecked);
    public PrintSheetsViewModel(SheetManagerModel model)
    {
        _model = model;
        Refresh();
    }
    [RelayCommand]
    private void Refresh()
    {
        try
        {
            var selected = new HashSet<ElementId>(TreeItems.SelectMany(f => f.GetSheets())
                .Where(s => s.IsChecked).Select(s => s.Id));
            var items = _model.GetSheetOrganization();
            foreach (var sheet in TreeItems.SelectMany(f => f.GetSheets())) sheet.PropertyChanged -= SelectionChanged;
            TreeItems = items;
            foreach (var sheet in TreeItems.SelectMany(f => f.GetSheets()))
            {
                sheet.IsChecked = selected.Contains(sheet.Id);
                sheet.PropertyChanged += SelectionChanged;
            }
            SelectionChanged(null, new PropertyChangedEventArgs(nameof(SheetItem.IsChecked)));
            Message = _model.IsAvailable && TreeItems.Count == 0 ? "Нет листов, доступных для печати." : string.Empty;
        }
        catch (Exception exception) { Message = $"Не удалось загрузить листы: {exception.Message}"; }
    }
    private void SelectionChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName != nameof(SheetItem.IsChecked)) return;
        OnPropertyChanged(nameof(SelectedCount));
        PrintCommand.NotifyCanExecuteChanged();
    }
    [RelayCommand]
    private void SelectAll() { foreach (var folder in TreeItems) folder.SetChecked(true); }
    [RelayCommand]
    private void ClearSelection() { foreach (var folder in TreeItems) folder.SetChecked(false); }
    [RelayCommand]
    private void SelectPath()
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку для PDF", UseDescriptionForTitle = true, SelectedPath = PathFolder
        };
        if (dialog.ShowDialog() == true) PathFolder = dialog.SelectedPath;
    }
    private string PdfFileName
    {
        get
        {
            var name = FileName.Trim();
            return name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? name.Substring(0, name.Length - 4) : name;
        }
    }
    private bool CanPrint() => _model.IsAvailable && SelectedCount > 0 && Directory.Exists(PathFolder)
        && (!Combine || (!string.IsNullOrWhiteSpace(PdfFileName) && PdfFileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && PdfFileName.TrimEnd(' ', '.').Length > 0));
    [RelayCommand(CanExecute = nameof(CanPrint))]
    private void Print()
    {
        try
        {
            var sheets = TreeItems.SelectMany(f => f.GetSheets()).Where(s => s.IsChecked).Select(s => s.Id).ToList();
            _model.ExportPdf(PathFolder, sheets, Combine, PdfFileName);
            Message = $"Экспортировано листов: {sheets.Count}. Папка: {PathFolder}";
            Completed?.Invoke(sheets.Count);
        }
        catch (Exception exception) { Message = $"Ошибка экспорта PDF: {exception.Message}"; }
    }
}
