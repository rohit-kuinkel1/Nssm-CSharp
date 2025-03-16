using System;
using System.Globalization;
using System.Windows.Data;

namespace NSSM.WPF.Converters
{
    /// <summary>
    /// Converts a boolean value to one of two specified strings
    /// Parameter format: "FalseString|TrueString"
    /// </summary>
    public class BoolToStringConverter : IValueConverter
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
                return value?.ToString() ?? string.Empty;
            }

            string[] values = paramString.Split('|');
            if (values.Length != 2)
            {
                return value?.ToString() ?? string.Empty;
            }

            return boolValue ? values[1] : values[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string stringValue || parameter is not string paramString)
            {
                return false;
            }

            string[] values = paramString.Split('|');
            if (values.Length != 2)
            {
                return false;
            }

            if (stringValue == values[1])
            {
                return true;
            }
            else if (stringValue == values[0])
            {
                return false;
            }

            return false;
        }
    }
} 