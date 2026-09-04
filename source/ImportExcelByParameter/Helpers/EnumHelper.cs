using System.ComponentModel;
using System.Reflection;

namespace ImportExcelByParameter.Helpers;

public static class EnumHelper
{
    public static string GetDescription(this Enum value)
    {
        return value.GetType()
            .GetField(value.ToString())
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description ?? value.ToString();
    }
}