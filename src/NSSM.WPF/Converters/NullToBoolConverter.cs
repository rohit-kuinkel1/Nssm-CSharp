using System;
using System.Globalization;
using System.Windows.Data;

namespace NSSM.WPF.Converters
{
    /// <summary>
    /// Converts a null value to a bool (true if not null, false if null)
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 