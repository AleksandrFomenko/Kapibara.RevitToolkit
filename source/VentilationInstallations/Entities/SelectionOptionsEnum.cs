using System.ComponentModel;

namespace VentilationInstallations.Entities;

public enum SelectionOptionsEnum
{
    [Description("Выбранные в таблице")]
    SelectedInTable,

    [Description("Выбранные в проекте")]
    SelectedInProject,

    [Description("Видимые на виде")]
    OnView,
    
    [Description("Все")]
    All,
}