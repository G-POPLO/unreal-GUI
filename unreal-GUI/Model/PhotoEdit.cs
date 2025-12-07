using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace unreal_GUI.Model
{
    public class PhotoEdit
    {
        /// <summary>
        /// 检查图片是否符合3:1比例
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <returns>是否符合3:1比例</returns>
        public static bool IsCorrectRatio(string imagePath)
        {
            try
            {
                using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                // 检查宽高比是否接近3:1（允许一定误差）
                double ratio = (double)bitmap.PixelWidth / bitmap.PixelHeight;
                double targetRatio = 3.0;
                double tolerance = 0.1; // 允许10%的误差

                return Math.Abs(ratio - targetRatio) <= tolerance;
            }
            catch
            {
                // 如果无法加载图片，返回false
                return false;
            }
        }
    }
}
