using ExporterModels.Abstractions;
using ExporterModels.Entities;
using Wpf.Ui.Controls;

namespace ExporterModels.Dialogs.RemoveProject.ViewModel;

public partial class RemoveProjectViewModel : ObservableObject
{
    public static Action? CloseAction;

    [ObservableProperty] private double _heightWindow;


    [ObservableProperty] private Project? _selectedProject;


    public RemoveProjectViewModel(IInfoBarService infoBarService)
    {
        HeightWindow = 200;
        InfoBarService = infoBarService;
        _ = ShowInfoThenResizeAsync();
    }

    public IInfoBarService InfoBarService { get; }
    public event Action<Project>? RemoveProject;

    partial void OnSelectedProjectChanged(Project? value)
    {
        DeleteProjectCommand.NotifyCanExecuteChanged();
    }

    private async Task ShowInfoThenResizeAsync()
    {
        await InfoBarService.ShowInfoAsync(
            InfoBarSeverity.Informational,
            "",
            "Выберите проект из списка");

        HeightWindow = 125;
    }

    private bool CanDelete()
    {
        return SelectedProject is not null;
    }


    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void DeleteProject()
    {
        if (SelectedProject != null) OnRemoveProject(SelectedProject);
    }

    [RelayCommand]
    private void Close()
    {
        CloseAction?.Invoke();
    }


    private void OnRemoveProject(Project obj)
    {
        RemoveProject?.Invoke(obj);
    }
}