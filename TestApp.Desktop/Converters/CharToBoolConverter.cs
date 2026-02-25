using System.Globalization;
using System.Windows.Data;

namespace TestApp.Desktop.Converters;

public class CharToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is char charValue && parameter is string paramString && paramString.Length > 0)
        {
            return charValue == paramString[0];
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter is string paramString && paramString.Length > 0)
        {
            return paramString[0];
        }
        return Binding.DoNothing;
    }
}