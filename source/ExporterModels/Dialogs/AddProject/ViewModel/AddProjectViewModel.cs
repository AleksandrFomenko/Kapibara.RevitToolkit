using ExporterModels.Abstractions;
using ExporterModels.Entities;
using Wpf.Ui.Controls;

namespace ExporterModels.Dialogs.AddProject.ViewModel;

public partial class AddProjectViewModel : ObservableObject
{
    [ObservableProperty] private string _enableButton;
    [ObservableProperty] private string _projectDescription;
    [ObservableProperty] private string _projectName;

    public AddProjectViewModel(IInfoBarService infoBarService)
    {
        InfoBarService = infoBarService;
        InfoBarService.ShowInfoAsync(
            InfoBarSeverity.Informational,
            "Input data",
            "Введите имя и описание проекта");
    }

    private IInfoBarService InfoBarService { get; }
    public event Action<Project> AddProjectEvent;

    partial void OnProjectDescriptionChanged(string value)
    {
        AddProjectCommand.NotifyCanExecuteChanged();
    }

    partial void OnProjectNameChanged(string value)
    {
        AddProjectCommand.NotifyCanExecuteChanged();
    }


    private bool CheckExecute()
    {
        return !string.IsNullOrWhiteSpace(ProjectDescription) &&
               !string.IsNullOrWhiteSpace(ProjectName);
    }

    [RelayCommand(CanExecute = nameof(CheckExecute))]
    private void AddProject()
    {
        OnAddProjectEvent(new Project(ProjectName, ProjectDescription, []));
        InfoBarService.ShowInfoAsync(InfoBarSeverity.Success, "Success", "Добавлено");
    }

    protected virtual void OnAddProjectEvent(Project obj)
    {
        AddProjectEvent?.Invoke(obj);
    }
}