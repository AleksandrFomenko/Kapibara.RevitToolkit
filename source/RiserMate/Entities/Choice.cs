using System.ComponentModel;

namespace RiserMate.Entities;

public enum Choice
{   
    [Description("Обработать выбранные стояки")]
    Selected,
    [Description("Обработать все стояки")]
    All,
}