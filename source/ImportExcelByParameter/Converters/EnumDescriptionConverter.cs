using System.Globalization;
using System.Windows.Data;
using ImportExcelByParameter.Helpers;

namespace ImportExcelByParameter.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Enum e ? e.GetDescription() : value?.ToString() ?? string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}