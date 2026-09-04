using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ExporterModels.Abstractions;
using ExporterModels.Entities;
using Wpf.Ui.Controls;

namespace ExporterModels.Dialogs.RemoveModel.ViewModel;

public partial class RemoveModelViewModel : ObservableObject
{
    public static Action? CloseWindow;

    [ObservableProperty] private bool _deleteModelEnable;
    [ObservableProperty] private double _heightWindow;
    [ObservableProperty] private ObservableCollection<Model>? _selectedModels;

    public RemoveModelViewModel(IInfoBarService infoBarService)
    {
        InfoBarService = infoBarService;
        HeightWindow = 200;
        _ = ShowInfoThenResizeAsync();
    }

    public Action<ObservableCollection<Model>?> RemoveModel { get; set; } = _ => { };

    public IInfoBarService InfoBarService { get; }

    partial void OnSelectedModelsChanged(ObservableCollection<Model>? value)
    {
        if (_selectedModels != null)
            _selectedModels.CollectionChanged -= OnSelectedModelsCollectionChanged;

        if (value != null)
            value.CollectionChanged += OnSelectedModelsCollectionChanged;

        DeleteModelCommand.NotifyCanExecuteChanged();
    }

    private void OnSelectedModelsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DeleteModelCommand.NotifyCanExecuteChanged();
    }

    private bool CanDelete()
    {
        return SelectedModels is { Count: > 0 };
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void DeleteModel()
    {
        if (SelectedModels != null)
            RemoveModel?.Invoke(SelectedModels);
    }

    [RelayCommand]
    private void Close()
    {
        CloseWindow?.Invoke();
    }

    private async Task ShowInfoThenResizeAsync()
    {
        await InfoBarService.ShowInfoAsync(
            InfoBarSeverity.Informational,
            "",
            "Выберите модель из списка");

        HeightWindow = 125;
    }
}