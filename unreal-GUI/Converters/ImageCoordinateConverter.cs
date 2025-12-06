using System;
using System.Globalization;
using System.Windows.Data;

namespace unreal_GUI.Converters
{
    /// <summary>
    /// 图片坐标转换器
    /// 将原始图片坐标转换为显示坐标
    /// 参数顺序：
    /// 1. 原始坐标值 (X, Y, Width, 或 Height)
    /// 2. 原始图片宽度或高度
    /// 3. 显示区域宽度或高度
    /// </summary>
    public class ImageCoordinateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3 || 
                !(values[0] is double originalValue) || 
                !(values[1] is double originalDimension) || 
                !(values[2] is double displayDimension))
            {
                return 0.0;
            }

            // 计算缩放比例
            double scale = displayDimension / originalDimension;
            
            // 返回转换后的值
            return originalValue * scale;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}