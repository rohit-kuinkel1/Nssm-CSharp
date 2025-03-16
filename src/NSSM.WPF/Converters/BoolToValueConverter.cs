using System;
using System.Globalization;
using System.Windows.Data;

namespace NSSM.WPF.Converters
{
    /// <summary>
    /// Converts a boolean value to one of two specified values based on the parameter
    /// Parameter format: "FalseValue|TrueValue"
    /// </summary>
    public class BoolToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Default to false if value is null
            bool boolValue = false;
            
            if (value is bool bVal)
            {
                boolValue = bVal;
            }

            if (parameter is not string paramString)
            {
                return value ?? string.Empty;
            }

            string[] values = paramString.Split('|');
            if (values.Length != 2)
            {
                return value ?? string.Empty;
            }

            string result = boolValue ? values[1] : values[0];

            // Try to convert the result to the target type if needed
            if (targetType == typeof(string))
            {
                return result;
            }
            else if (targetType == typeof(bool))
            {
                return bool.TryParse(result, out bool boolResult) ? boolResult : boolValue;
            }
            else if (targetType == typeof(int))
            {
                return int.TryParse(result, out int intResult) ? intResult : 0;
            }
            else if (targetType == typeof(double))
            {
                return double.TryParse(result, out double doubleResult) ? doubleResult : 0.0;
            }
            else if (targetType == typeof(System.Windows.Media.Brush))
            {
                try
                {
                    var brush = new System.Windows.Media.BrushConverter().ConvertFromString(result);
                    return brush ?? System.Windows.Media.Brushes.Black;
                }
                catch
                {
                    return System.Windows.Media.Brushes.Black;
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string paramString)
            {
                return false;
            }

            string[] values = paramString.Split('|');
            if (values.Length != 2)
            {
                return false;
            }

            if (value != null && value.ToString() == values[1])
            {
                return true;
            }
            else if (value != null && value.ToString() == values[0])
            {
                return false;
            }

            return false;
        }
    }
} 