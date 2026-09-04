using ExporterModels.Dialogs.Instruction.Abstraction;

namespace ExporterModels.Dialogs.Instruction.ViewModel;

public partial class InstructionViewModel(IServiceInstruction instructionService) : ObservableObject
{
    public static Action? CloseWindow;
    [ObservableProperty] private string _closeButtonContent = instructionService.GetCloseButtonText();
    [ObservableProperty] private string _conditionText = instructionService.GetConditionText();
    [ObservableProperty] private bool _isRead;
    [ObservableProperty] private string _text = instructionService.GetText();

    partial void OnIsReadChanged(bool value)
    {
        CloseCommand.NotifyCanExecuteChanged();
    }

    private bool CanClose()
    {
        return IsRead;
    }


    [RelayCommand(CanExecute = nameof(CanClose))]
    private void Close()
    {
        CloseWindow?.Invoke();
    }
}