using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using unreal_GUI.ViewModel;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace unreal_GUI.View.DialogContent
{
    /// <summary>
    /// ImageEdit.xaml 的交互逻辑
    /// 图片剪裁工具，用于将图片剪裁为3:1比例
    /// </summary>
    public partial class ImageEdit : UserControl
    {
        public ImageEditViewModel ViewModel { get; private set; }

        // 剪裁后的图片路径
        public string? CroppedImagePath { get; private set; }

        // 原始图片路径
        private string? originalImagePath;

        // 图片编辑完成事件
        public event EventHandler<string>? ImageEditCompleted;

        public ImageEdit()
        {
            InitializeComponent();

            // 设置 DataContext
            ViewModel = new ImageEditViewModel();
            DataContext = ViewModel;

            // 订阅鼠标事件 - 为新插入的UserControl1内容
            Loaded += ImageEdit_Loaded;
        }

        private void ImageEdit_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置画布尺寸并初始化矩形位置
            if (FindName("CroppingCanvas") is Canvas croppingCanvas)
            {
                ViewModel.SetCanvasSize(croppingCanvas.ActualWidth, croppingCanvas.ActualHeight);
            }
        }

        /// <summary>
        /// 设置图片
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        public void SetImage(string imagePath)
        {
            try
            {
                originalImagePath = imagePath;

                using var image = SixLabors.ImageSharp.Image.Load(imagePath);

                // 转换为 BitmapSource
                var bitmapSource = ConvertImageToBitmapSource(image);
                ViewModel.ImageSource = bitmapSource;
                ViewModel.ImageWidth = bitmapSource.PixelWidth;
                ViewModel.ImageHeight = bitmapSource.PixelHeight;

                // 设置BackgroundImage显示图片
                var bitmapImage = new BitmapImage(new Uri(imagePath));
                BackgroundImage.Source = bitmapImage;

                // 更新剪裁矩形
                UpdateCropRect();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 将 ImageSharp Image 转换为 WPF BitmapSource
        /// </summary>
        /// <param name="image">ImageSharp 图片对象</param>
        /// <returns>WPF BitmapSource</returns>
        private static BitmapSource ConvertImageToBitmapSource(SixLabors.ImageSharp.Image image)
        {
            using var memoryStream = new MemoryStream();
            image.SaveAsPng(memoryStream);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        /// <summary>
        /// 更新剪裁矩形位置
        /// </summary>
        private void UpdateCropRect()
        {
            if (ViewModel.ImageSource != null)
            {
                var imageWidth = ViewModel.ImageSource.PixelWidth;
                var imageHeight = ViewModel.ImageSource.PixelHeight;

                // 计算3:1比例的剪裁区域
                var cropWidth = Math.Min(imageWidth, imageHeight * 3);
                var cropHeight = cropWidth / 3;

                // 居中显示
                ViewModel.RectLeft = (imageWidth - cropWidth) / 2;
                ViewModel.RectTop = (imageHeight - cropHeight) / 2;
                ViewModel.RectWidth = cropWidth;
                ViewModel.RectHeight = cropHeight;
            }
        }

        /// <summary>
        /// 选择图片按钮点击事件
        /// </summary>
        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件 (*.*)|*.*",
                Title = "选择要剪裁的图片"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SetImage(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// 重置缩放按钮点击事件
        /// </summary>
        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ImageSource != null)
            {
                UpdateCropRect();
            }
        }

        /// <summary>
        /// 保存剪裁图片按钮点击事件
        /// </summary>
        private void SaveCroppedButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ImageSource == null)
            {
                MessageBox.Show("请先选择一张图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG 图片 (*.png)|*.png|JPEG 图片 (*.jpg)|*.jpg|所有文件 (*.*)|*.*",
                Title = "保存剪裁后的图片"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveCroppedImage(saveFileDialog.FileName);
                    MessageBox.Show("图片保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 保存剪裁后的图片
        /// </summary>
        /// <param name="outputPath">输出文件路径</param>
        private void SaveCroppedImage(string outputPath)
        {
            if (ViewModel.ImageSource == null || string.IsNullOrEmpty(originalImagePath))
                return;

            try
            {
                // 重新加载原始图片进行剪裁
                using var originalImage = SixLabors.ImageSharp.Image.Load(originalImagePath);

                // 创建剪裁矩形
                var cropRectangle = new SixLabors.ImageSharp.Rectangle(
                    (int)ViewModel.RectLeft,
                    (int)ViewModel.RectTop,
                    (int)ViewModel.RectWidth,
                    (int)ViewModel.RectHeight);

                // 执行剪裁
                originalImage.Mutate(ctx => ctx.Crop(cropRectangle));

                // 保存剪裁后的图片
                var extension = System.IO.Path.GetExtension(outputPath).ToLower();
                if (extension == ".jpg" || extension == ".jpeg")
                {
                    originalImage.SaveAsJpeg(outputPath);
                }
                else
                {
                    originalImage.SaveAsPng(outputPath);
                }

                CroppedImagePath = outputPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存剪裁图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 鼠标按下事件 - 开始拖拽剪裁矩形
        /// </summary>
        private void CropRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 可以添加拖拽剪裁矩形的逻辑
            e.Handled = true;
        }

        /// <summary>
        /// 鼠标移动事件 - 拖拽剪裁矩形
        /// </summary>
        private void CropRect_MouseMove(object sender, MouseEventArgs e)
        {
            // 可以添加拖拽剪裁矩形的逻辑
        }

        /// <summary>
        /// 鼠标释放事件 - 结束拖拽
        /// </summary>
        private void CropRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 可以添加结束拖拽的逻辑
        }

        /// <summary>
        /// 上边框热区鼠标事件
        /// </summary>
        private void EdgeTop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 可以添加调整大小的逻辑
            e.Handled = true;
        }

        /// <summary>
        /// 下边框热区鼠标事件
        /// </summary>
        private void EdgeBottom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 可以添加调整大小的逻辑
            e.Handled = true;
        }

        /// <summary>
        /// 左边框热区鼠标事件
        /// </summary>
        private void EdgeLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 可以添加调整大小的逻辑
            e.Handled = true;
        }

        /// <summary>
        /// 右边框热区鼠标事件
        /// </summary>
        private void EdgeRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 可以添加调整大小的逻辑
            e.Handled = true;
        }

        #region UserControl1 测试内容的鼠标事件处理

        /// <summary>
        /// 拖拽边缘热区
        /// </summary>
        private void Edge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle edgeRect)
            {
                var point = e.GetPosition(this);
                ViewModel.StartEdgeDrag(point.X, point.Y, edgeRect.Name);
                edgeRect.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Edge_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle edgeRect && e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(this);
                ViewModel.ResizeEdge(point.X, point.Y, edgeRect.Name);
                e.Handled = true;
            }
        }

        private void Edge_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle edgeRect)
            {
                edgeRect.ReleaseMouseCapture();
                ViewModel.EndDrag();
                e.Handled = true;
            }
        }

        private void ResizableRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle rect)
            {
                var point = e.GetPosition(this);
                ViewModel.StartMove(point.X, point.Y);
                rect.CaptureMouse();
                e.Handled = true;
            }
        }

        private void ResizableRect_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle && e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition(this);
                ViewModel.MoveRect(point.X, point.Y);
                e.Handled = true;
            }
        }

        private void ResizableRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle rect)
            {
                rect.ReleaseMouseCapture();
                ViewModel.EndDrag();
                e.Handled = true;
            }
        }

        #endregion
        /// <summary>
        /// 完成剪裁按钮点击事件
        /// </summary>
        private void FinishCropButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ImageSource == null)
            {
                MessageBox.Show("请先选择一张图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 创建临时目录保存剪裁后的图片
            string tempDir = Path.Combine(Path.GetTempPath(), "UnrealGUI");
            Directory.CreateDirectory(tempDir);

            string tempFileName = $"cropped_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string tempFilePath = Path.Combine(tempDir, tempFileName);

            try
            {
                // 保存剪裁后的图片
                SaveCroppedImage(tempFilePath);

                // 设置剪裁后的图片路径
                CroppedImagePath = tempFilePath;

                // 触发图片编辑完成事件
                ImageEditCompleted?.Invoke(this, tempFilePath);

                MessageBox.Show("图片剪裁完成！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存剪裁图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}