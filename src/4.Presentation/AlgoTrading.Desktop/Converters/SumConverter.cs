using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace AlgoTrading.Desktop.Converters;

/// <summary>
/// Converter to sum a property value from a collection
/// Usage: {Binding Items, Converter={StaticResource SumConverter}, ConverterParameter='PropertyName'}
/// Example: {Binding ActiveStrategies, Converter={StaticResource SumConverter}, ConverterParameter='TotalTrades'}
/// </summary>
public class SumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return 0;

        if (value is not IEnumerable collection)
            return 0;

        var propertyName = parameter.ToString();
        if (string.IsNullOrEmpty(propertyName))
            return 0;

        decimal sum = 0;

        try
        {
            foreach (var item in collection)
            {
                if (item == null)
                    continue;

                // Get property value using reflection
                var propertyInfo = item.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                    continue;

                var propertyValue = propertyInfo.GetValue(item);
                if (propertyValue == null)
                    continue;

                // Convert to decimal and add to sum
                try
                {
                    sum += System.Convert.ToDecimal(propertyValue);
                }
                catch
                {
                    // Skip if conversion fails
                    continue;
                }
            }

            return sum;
        }
        catch
        {
            return 0;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
