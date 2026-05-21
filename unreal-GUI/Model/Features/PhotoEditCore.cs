using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace unreal_GUI.Model.Features
{
    public class PhotoEditCore
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

        /// <summary>
        /// 自动从图片中心裁剪为3:1比例
        /// </summary>
        /// <param name="inputPath">输入图片路径</param>
        /// <param name="outputPath">输出图片路径</param>
        /// <returns>是否裁剪成功</returns>
        public static bool AutoCropTo3to1Ratio(string inputPath, string outputPath)
        {
            try
            {
                // 确保输出目录存在
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // 使用System.Drawing加载图片
                using var originalImage = Image.FromFile(inputPath);

                int originalWidth = originalImage.Width;
                int originalHeight = originalImage.Height;
                double originalRatio = (double)originalWidth / originalHeight;
                double targetRatio = 3.0;

                int cropX, cropY, cropWidth, cropHeight;

                if (originalRatio > targetRatio)
                {
                    // 图片太宽，需要裁剪宽度
                    cropWidth = (int)Math.Round(originalHeight * targetRatio);
                    cropHeight = originalHeight;
                    cropX = (originalWidth - cropWidth) / 2;
                    cropY = 0;
                }
                else
                {
                    // 图片太高，需要裁剪高度
                    cropWidth = originalWidth;
                    cropHeight = (int)Math.Round(originalWidth / targetRatio);
                    cropX = 0;
                    cropY = (originalHeight - cropHeight) / 2;
                }

                // 确保裁剪区域不超出图片边界
                if (cropX < 0) cropX = 0;
                if (cropY < 0) cropY = 0;
                if (cropX + cropWidth > originalWidth) cropWidth = originalWidth - cropX;
                if (cropY + cropHeight > originalHeight) cropHeight = originalHeight - cropY;

                // 创建裁剪后的图片
                using var croppedBitmap = new Bitmap(cropWidth, cropHeight);
                using (var graphics = Graphics.FromImage(croppedBitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    // 从原图裁剪指定区域
                    graphics.DrawImage(originalImage, new Rectangle(0, 0, cropWidth, cropHeight), cropX, cropY, cropWidth, cropHeight, GraphicsUnit.Pixel);
                }

                // 保存为PNG
                croppedBitmap.Save(outputPath, ImageFormat.Png);

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误
                Debug.WriteLine($"自动裁剪失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用System.Drawing压缩PNG图片
        /// </summary>
        /// <param name="inputPath">输入图片路径</param>
        /// <param name="outputPath">输出图片路径</param>
        /// <param name="quality">压缩质量（0-100，0最低质量，100最高质量）</param>
        /// <returns>是否压缩成功</returns>
        public static bool CompressPng(string inputPath, string outputPath, int quality = 80)
        {
            try
            {
                // 验证质量参数
                if (quality < 0 || quality > 100)
                {
                    quality = 80; // 默认值
                }

                // 确保输出目录存在
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // 使用System.Drawing加载图片
                using var image = Image.FromFile(inputPath);

                // 保存为PNG（System.Drawing的PNG压缩是自动的，不支持自定义压缩级别）
                image.Save(outputPath, ImageFormat.Png);

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误
                Debug.WriteLine($"PNG压缩失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用System.Drawing压缩PNG图片并调整尺寸
        /// </summary>
        /// <param name="inputPath">输入图片路径</param>
        /// <param name="outputPath">输出图片路径</param>
        /// <param name="maxWidth">最大宽度（0表示不限制）</param>
        /// <param name="maxHeight">最大高度（0表示不限制）</param>
        /// <param name="quality">压缩质量（0-100，0最低质量，100最高质量）</param>
        /// <returns>是否压缩成功</returns>
        public static bool CompressPngAndResize(string inputPath, string outputPath, int maxWidth = 0, int maxHeight = 0, int quality = 80)
        {
            try
            {
                // 验证质量参数
                if (quality < 0 || quality > 100)
                {
                    quality = 80; // 默认值
                }

                // 确保输出目录存在
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // 使用System.Drawing加载图片
                using var originalImage = Image.FromFile(inputPath);
                int newWidth = originalImage.Width;
                int newHeight = originalImage.Height;

                // 如果需要调整尺寸
                if (maxWidth > 0 || maxHeight > 0)
                {
                    // 计算新尺寸，保持宽高比
                    if (maxWidth > 0 && originalImage.Width > maxWidth)
                    {
                        newWidth = maxWidth;
                        newHeight = (int)Math.Round((double)originalImage.Height * newWidth / originalImage.Width);
                    }

                    if (maxHeight > 0 && newHeight > maxHeight)
                    {
                        newHeight = maxHeight;
                        newWidth = (int)Math.Round((double)originalImage.Width * newHeight / originalImage.Height);
                    }

                    // 创建调整尺寸后的图片
                    using var resizedBitmap = new Bitmap(newWidth, newHeight);
                    using (var graphics = Graphics.FromImage(resizedBitmap))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);
                    }

                    // 保存为PNG
                    resizedBitmap.Save(outputPath, ImageFormat.Png);
                }
                else
                {
                    // 不需要调整尺寸，直接保存
                    originalImage.Save(outputPath, ImageFormat.Png);
                }

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误
                Debug.WriteLine($"PNG压缩和调整尺寸失败：{ex.Message}");
                return false;
            }
        }
    }
}