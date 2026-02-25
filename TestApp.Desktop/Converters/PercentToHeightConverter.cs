using System.Globalization;
using System.Windows.Data;

namespace TestApp.Desktop.Converters;

/// <summary>
/// Convierte un porcentaje (0-100) a una altura proporcional para barras de gráfico
/// </summary>
public class PercentToHeightConverter : IValueConverter
{
    private const double MaxHeight = 60; // Altura máxima en píxeles

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double percent)
        {
            // Asegurar que está en rango 0-100
            percent = Math.Max(0, Math.Min(100, percent));
            return (percent / 100.0) * MaxHeight;
        }
        return 2.0; // Altura mínima
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Convierte una tendencia numérica a texto descriptivo
/// </summary>
public class TrendToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double trend)
        {
            if (trend > 5) return "? Mejorando";
            if (trend < -5) return "? Bajando";
            return "? Estable";
        }
        return "? Sin datos";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Convierte una tendencia numérica a color
/// </summary>
public class TrendToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double trend)
        {
            if (trend > 5) return "#4CAF50"; // Verde - mejorando
            if (trend < -5) return "#F44336"; // Rojo - empeorando
            return "#FF9800"; // Naranja - estable
        }
        return "#9E9E9E"; // Gris - sin datos
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
