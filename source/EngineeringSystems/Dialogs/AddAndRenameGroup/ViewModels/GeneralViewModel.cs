using EngineeringSystems.Dialogs.AddAndRenameGroup.EventArgs;
using EngineeringSystems.services;
using EngineeringSystems.ViewModels;

namespace EngineeringSystems.Dialogs.AddAndRenameGroup.ViewModels;

public abstract partial class GeneralViewModel : ObservableObject
{
    [ObservableProperty] private string _title = null!;
    [ObservableProperty] private string _description = null!;
    
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    [ObservableProperty] private string _nameGroup = null!;
    
    [ObservableProperty] private string _buttonContent = null!;
    protected readonly GroupSystemsViewModel HostViewModel;
    
    [ObservableProperty] private InfoBarService? _infoBarService;
    
    public event EventHandler<GroupNameEventArgs>? EventGroupName;

    protected GeneralViewModel(
        string title,
        string description,
        string buttonContent,
        GroupSystemsViewModel hostViewModel,
        InfoBarService? infoBarService)
    {
        Title = title;
        Description = description;
        ButtonContent = buttonContent;
        
        // всплывающие окна
        InfoBarService = infoBarService;
        
        // вм главного окна для подписок
        HostViewModel = hostViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteSubmit))]
    protected virtual void Execute()
    {
        var name = NameGroup?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        var result = OnEventGroupName(name!);
        AfterSubmit(name!, result);
    }
    
    private bool CanExecuteSubmit() => !string.IsNullOrWhiteSpace(NameGroup);

    protected abstract Task AfterSubmit(string name, GroupNameDialogResult result);
    
    protected GroupNameDialogResult OnEventGroupName(string name)
    {
        var args = new GroupNameEventArgs(name);
        EventGroupName?.Invoke(this, args);
        return args.Result;
    }
}