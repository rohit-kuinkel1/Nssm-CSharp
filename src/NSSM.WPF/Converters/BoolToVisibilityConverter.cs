using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NSSM.WPF.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility
    /// Parameter format: "Invert" to invert the logic
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Default to false if value is null
            bool visible = false;

            if (value is bool boolValue)
            {
                visible = boolValue;
            }

            bool invert = false;
            if (parameter is string paramString)
            {
                invert = paramString?.Equals("Invert", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            if (invert)
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            if (parameter is string paramString)
            {
                invert = paramString?.Equals("Invert", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            bool result = (value is Visibility visibility && visibility == Visibility.Visible);

            if (invert)
            {
                result = !result;
            }

            return result;
        }
    }
} 