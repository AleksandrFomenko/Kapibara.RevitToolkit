using ExporterModels.Dialogs.Instruction.Abstraction;

namespace ExporterModels.Dialogs.Instruction.Service;

public class InstructionService : IServiceInstruction
{
    public string GetText()
    {
        return "текст инструкции аваываываыв аываыва ываываываываываываываыв аывы ваыва" +
               "текст инструкции аваываываыв аываыва ываываываываываываываыв аывы ваыва";
    }

    public string GetConditionText()
    {
        return "Прочитано";
    }

    public string GetCloseButtonText()
    {
        return "Закрыть";
    }
}