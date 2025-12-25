using System;
using System.IO;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace unreal_GUI.Model.Features
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

        /// <summary>
        /// 使用ImageSharp压缩PNG图片
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

                // 使用ImageSharp加载、处理并保存图片
                using var image = Image.Load(inputPath);
                
                // 配置PNG编码器
                var pngEncoder = new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    ColorType = PngColorType.RgbWithAlpha
                };

                // 保存压缩后的图片
                image.Save(outputPath, pngEncoder);

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误
                Console.WriteLine($"PNG压缩失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 使用ImageSharp压缩PNG图片并调整尺寸
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

                // 使用ImageSharp加载图片
                using var image = Image.Load(inputPath);

                // 如果需要调整尺寸
                if (maxWidth > 0 || maxHeight > 0)
                {
                    int newWidth = image.Width;
                    int newHeight = image.Height;

                    // 计算新尺寸，保持宽高比
                    if (maxWidth > 0 && image.Width > maxWidth)
                    {
                        newWidth = maxWidth;
                        newHeight = (int)Math.Round((double)image.Height * newWidth / image.Width);
                    }

                    if (maxHeight > 0 && newHeight > maxHeight)
                    {
                        newHeight = maxHeight;
                        newWidth = (int)Math.Round((double)image.Width * newHeight / image.Height);
                    }

                    // 调整图片尺寸
                    image.Mutate(x => x.Resize(newWidth, newHeight));
                }

                // 配置PNG编码器
                var pngEncoder = new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression,
                    ColorType = PngColorType.RgbWithAlpha
                };

                // 保存压缩后的图片
                image.Save(outputPath, pngEncoder);

                return true;
            }
            catch (Exception ex)
            {
                // 记录错误
                Console.WriteLine($"PNG压缩和调整尺寸失败：{ex.Message}");
                return false;
            }
        }
    }
}
