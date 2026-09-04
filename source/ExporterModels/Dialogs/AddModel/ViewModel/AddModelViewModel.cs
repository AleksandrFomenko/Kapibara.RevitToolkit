using System.Collections.ObjectModel;
using ExporterModels.Abstractions;
using ExporterModels.Dialogs.AddModel.Abstractions;
using ExporterModels.Dialogs.AddModel.Entities;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ExporterModels.Dialogs.AddModel.ViewModel;

public partial class AddModelViewModel : ObservableObject
{
    private readonly IAddedModel _model;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private ObservableCollection<Option> _options;
    [ObservableProperty] private string _pathFromFolder;
    [ObservableProperty] private Option _selectedOption;

    [ObservableProperty] private ObservableCollection<ServerItem> _treeItems = [];
    [ObservableProperty] private bool _visibilitySelectModelFromFolder;
    [ObservableProperty] private bool _visibilitySelectModelRevitServer;

    public AddModelViewModel(IAddedModel model, IInfoBarService infoBarService)
    {
        _model = model;
        InfoBarService = infoBarService;
        Options = _model.GetOptionAsync();
        SelectedOption = Options?.FirstOrDefault() ?? new Option();
        _ = LoadAsync();
    }

    public IInfoBarService InfoBarService { get; }

    public event Action<ObservableCollection<ExporterModels.Entities.Model>> AddModelEvent;

    partial void OnSelectedOptionChanged(Option value)
    {
        VisibilitySelectModelFromFolder = !value.VisibleThreeList;
        VisibilitySelectModelRevitServer = value.VisibleThreeList;
    }

    private async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            TreeItems = await _model.GetServersTreeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private void SelectModelFromFolder()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Revit files (*.rvt)|*.rvt"
        };
        if (openFileDialog.ShowDialog() == true) PathFromFolder = openFileDialog.FileName;
    }

    [RelayCommand]
    private void AddModel()
    {
        if (SelectedOption is not { VisibleThreeList: true })
        {
            var models = new ObservableCollection<ExporterModels.Entities.Model>
            {
                new(0,
                        "",
                        PathFromFolder,
                        "")
                    { IsSelected = true }
            };
            OnAddModelEvent(models);
            InfoBarService.ShowInfoAsync(
                InfoBarSeverity.Success,
                "Success",
                "Добавил");
        }
        else
        {
            if (TreeItems is null || TreeItems.Count == 0)
            {
                InfoBarService.ShowInfoAsync(
                    InfoBarSeverity.Informational,
                    "Info",
                    "Revit server еще не загрузился или отсуствует.");
                return;
            }

            var checkedSheets = new List<SheetItem>();

            foreach (var server in TreeItems)
            {
                foreach (var s in server.Sheets)
                    if (s.IsChecked)
                        checkedSheets.Add(s);

                foreach (var f in server.SubFolders)
                    CollectCheckedSheetsFromFolder(f, checkedSheets);
            }

            if (checkedSheets.Count == 0)
            {
                InfoBarService.ShowInfoAsync(
                    InfoBarSeverity.Error,
                    "Error",
                    "Модели не выбраны");
                return;
            }

            var models = new ObservableCollection<ExporterModels.Entities.Model>();
            foreach (var sh in checkedSheets)
            {
                var path = string.IsNullOrWhiteSpace(sh.RsnPath) ? sh.PipePath : sh.RsnPath;
                models.Add(new ExporterModels.Entities.Model(0, "", path, "") { IsSelected = true });
            }

            OnAddModelEvent(models);
            InfoBarService.ShowInfoAsync(
                InfoBarSeverity.Success,
                "Success",
                "Добавил");
        }
    }

    private static void CollectCheckedSheetsFromFolder(FolderItem folder, List<SheetItem> acc)
    {
        foreach (var s in folder.Sheets)
            if (s.IsChecked)
                acc.Add(s);

        foreach (var sub in folder.SubFolders)
            CollectCheckedSheetsFromFolder(sub, acc);
    }

    protected virtual void OnAddModelEvent(ObservableCollection<ExporterModels.Entities.Model> obj)
    {
        AddModelEvent?.Invoke(obj);
    }
}