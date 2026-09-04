using System.ComponentModel;

namespace ImportExcelByParameter.Enums;

public enum SelectionMode
{
    [Description("Все элементы в проекте")]
    AllInProject,
    
    [Description("Все элементы на активном виде")]
    AllOnActiveView,
    
    [Description("По категории")]
    ByCategory
}