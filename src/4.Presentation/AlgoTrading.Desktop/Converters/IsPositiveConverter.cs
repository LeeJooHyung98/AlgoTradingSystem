using System.Globalization;
using System.Windows.Data;

namespace AlgoTrading.Desktop.Converters;

/// <summary>
/// Converter to check if a numeric value is positive (> 0)
/// </summary>
public class IsPositiveConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        try
        {
            var decimalValue = System.Convert.ToDecimal(value);
            return decimalValue > 0;
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
