using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TestApp.Desktop.Converters;

public class CorrectAnswerColorConverter : IValueConverter
{
    private static readonly Brush GreenBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
    private static readonly Brush BlackBrush = Brushes.Black;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not char correctAnswer || parameter is not string optionLetter)
            return BlackBrush;

        return correctAnswer.ToString().Equals(optionLetter, StringComparison.OrdinalIgnoreCase)
            ? GreenBrush
            : BlackBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}