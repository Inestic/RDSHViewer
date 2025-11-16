using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RDSH_Viewer.Converters
{
    /// <summary>
    /// Converts selected index value to a boolean.
    /// </summary>
    public class IndexToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value > -1;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DependencyProperty.UnsetValue;
    }
}
