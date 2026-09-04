using EngineeringSystems.Dialogs.AddAndRenameGroup.EventArgs;
using EngineeringSystems.services;
using EngineeringSystems.ViewModels;
using Wpf.Ui.Controls;

namespace EngineeringSystems.Dialogs.AddAndRenameGroup.ViewModels;

public partial class RenameGroupViewModel : GeneralViewModel
{
    public RenameGroupViewModel(GroupSystemsViewModel hostViewModel)
        : base("Переименование группы",
            "Введите новое наименование группы",
            "Переименовать",
            hostViewModel,
            new InfoBarService())
    {
        EventGroupName += hostViewModel.HandleGroupRenameRequested;
    }
    
    protected override async Task AfterSubmit(string name, GroupNameDialogResult result)
    {
        switch (result)
        {
            case GroupNameDialogResult.Success:
                await InfoBarService!.ShowInfoAsync(InfoBarSeverity.Success,
                    "Успех",
                    $"Группа {name} переименована");
                break;

            case GroupNameDialogResult.NameIsExist:
                await InfoBarService!.ShowInfoAsync(InfoBarSeverity.Error,
                    "Ошибка", 
                    $"Группа {name} уже существует");
                break;
            
            case GroupNameDialogResult.TooShort:
                await InfoBarService!.ShowInfoAsync(InfoBarSeverity.Warning,
                    "Предупреждение", 
                    $"Добавлено, но проверьте длину имени");
                break;

            case GroupNameDialogResult.Cancelled:
            default:
                await InfoBarService!.ShowInfoAsync(InfoBarSeverity.Error, 
                    "Отменено", 
                    "Переименование отменено");
                break;
        }
    }
}