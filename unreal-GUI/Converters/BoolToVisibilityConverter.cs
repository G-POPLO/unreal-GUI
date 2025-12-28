using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace unreal_GUI.Converters
{
    /// <summary>
    /// 布尔值到可见性转换器
    /// true -> Visible, false -> Collapsed
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? boolValue ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}
