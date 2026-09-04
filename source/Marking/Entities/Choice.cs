using System.ComponentModel;

namespace Marking.Entities;

public enum Choice
{
    [Description("Маркировать тип-основу")] Selected,
    [Description("Маркировать все типы у основы")] All,
}