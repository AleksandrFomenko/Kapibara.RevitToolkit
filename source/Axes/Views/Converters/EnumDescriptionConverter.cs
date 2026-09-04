using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

namespace Axes.Views.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var field = value.GetType().GetField(value.ToString()!);
        var attr = field?.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}