using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace TestApp.Desktop.Converters;

public class CollectionCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable enumerable)
        {
            var count = 0;
            foreach (var item in enumerable)
            {
                if (item is Core.Models.QuestionFile file)
                {
                    count += file.Questions.Count;
                }
            }
            return count.ToString();
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
