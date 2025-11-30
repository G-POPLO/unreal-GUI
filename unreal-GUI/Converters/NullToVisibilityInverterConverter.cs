using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace unreal_GUI.Converters
{
    /// <summary>
    /// 空值到可见性的反向转换器
    /// </summary>
    public class NullToVisibilityInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}