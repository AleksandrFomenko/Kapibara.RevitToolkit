namespace ExporterModels.Dialogs.Instruction.Abstraction;

public interface IServiceInstruction
{
    public string GetText();
    public string GetConditionText();

    public string GetCloseButtonText();
}