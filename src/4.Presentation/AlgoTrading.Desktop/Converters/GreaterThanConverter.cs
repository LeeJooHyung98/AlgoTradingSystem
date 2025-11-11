using System.Globalization;
using System.Windows.Data;

namespace AlgoTrading.Desktop.Converters;

/// <summary>
/// Converter to check if a numeric value is greater than a parameter
/// Usage: {Binding Value, Converter={StaticResource GreaterThanConverter}, ConverterParameter=50}
/// </summary>
public class GreaterThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        try
        {
            var decimalValue = System.Convert.ToDecimal(value);
            var threshold = System.Convert.ToDecimal(parameter);
            return decimalValue > threshold;
        }
        catch
        {
            return false;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
