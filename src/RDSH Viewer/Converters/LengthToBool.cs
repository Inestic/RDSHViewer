using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RDSH_Viewer.Converters
{
    /// <summary>
    /// Convert length value to a boolean.
    /// </summary>
    public class LengthToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value > 0;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
    }
}
