using System;
using System.Globalization;
using System.Windows.Data;

namespace unreal_GUI.Converters
{
    /// <summary>
    /// 用于比较对象是否相等的转换器
    /// </summary>
    public class EqualityConverter : IValueConverter
    {
        /// <summary>
        /// 将源值与参数进行相等性比较
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果都是null，则相等
            if (value == null && parameter == null)
                return true;
            
            // 如果其中一个为null，则不相等
            if (value == null || parameter == null)
                return false;
            
            // 进行相等性比较
            return value.Equals(parameter);
        }

        /// <summary>
        /// 将布尔值转换回源值（如果为true则返回参数，否则返回DependencyProperty.UnsetValue）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEqual && isEqual)
            {
                return parameter;
            }
            
            return Binding.DoNothing;
        }
    }
}