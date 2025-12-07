using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using unreal_GUI.ViewModel;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace unreal_GUI.View.DialogContent
{
    /// <summary>
    /// ImageEdit.xaml 的交互逻辑
    /// 用于3:1比例图片剪裁
    /// </summary>
    public partial class ImageEdit : UserControl
    {
        #region 事件

        // 图片编辑完成事件
        public event EventHandler<ImageEditCompletedEventArgs> ImageEditCompleted;

        #endregion

        #region 依赖属性

        // 图片源依赖属性
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageEdit), new PropertyMetadata(null, OnImageSourceChanged));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // 图片宽度依赖属性
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageEdit), new PropertyMetadata(0.0));

        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        // 图片高度依赖属性
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageEdit), new PropertyMetadata(0.0));

        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        // 是否显示裁剪指南依赖属性
        public static readonly DependencyProperty ShowCropGuideProperty =
            DependencyProperty.Register("ShowCropGuide", typeof(bool), typeof(ImageEdit), new PropertyMetadata(true));

        public bool ShowCropGuide
        {
            get { return (bool)GetValue(ShowCropGuideProperty); }
            set { SetValue(ShowCropGuideProperty, value); }
        }

        #endregion

        #region 私有字段

        // ViewModel实例
        private readonly ImageEditViewModel _viewModel;

        #endregion

        #region 构造函数

        // 构造函数
        public ImageEdit()
        {
            InitializeComponent();
            _viewModel = new ImageEditViewModel();
            DataContext = _viewModel;

            // 初始化按钮
            InitializeButtons();
        }

        #endregion

        #region 事件处理程序

        // 浏览按钮点击事件
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建 OpenFileDialog 实例
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*"
            };

            // 显示对话框并检查用户是否选择了文件
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 创建 BitmapImage 实例并设置 UriSource
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();

                    // 设置 ImageSource 属性
                    ImageSource = bitmap;
                }
                catch (Exception ex)
                {
                    // 如果出现异常，显示错误消息
                    MessageBox.Show($"无法加载图像: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // 重置缩放按钮点击事件
        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            // 将缩放滑块值重置为1.0（即100%）
            ZoomSlider.Value = 1.0;
        }

        // 保存裁剪按钮点击事件
        private void SaveCroppedButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageSource == null)
            {
                MessageBox.Show("请先选择一张图片。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 获取原始图片路径
                string originalImagePath = "";
                if (ImageSource is BitmapImage bitmapImage && bitmapImage.UriSource != null)
                {
                    originalImagePath = bitmapImage.UriSource.LocalPath;
                }

                // 创建临时文件路径用于保存裁剪后的图片
                string tempDirectory = Path.Combine(Path.GetTempPath(), "UnrealGUI");
                Directory.CreateDirectory(tempDirectory);
                string croppedImagePath = Path.Combine(tempDirectory, $"cropped_{Guid.NewGuid()}.png");

                // 执行图片裁剪
                CropImage(originalImagePath, croppedImagePath);

                // 创建裁剪后图片的BitmapImage
                BitmapImage croppedBitmap = new();
                croppedBitmap.BeginInit();
                croppedBitmap.UriSource = new Uri(croppedImagePath);
                croppedBitmap.EndInit();

                // 触发图片编辑完成事件，传递裁剪后的图片路径
                OnImageEditCompleted(new ImageEditCompletedEventArgs { CroppedImage = croppedBitmap, CroppedImagePath = croppedImagePath });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存裁剪图片时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 缩放滑块值改变事件
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 更新缩放文本显示
            UpdateZoomText();
        }

        // 画布加载事件
        private void CroppingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置画布尺寸并初始化矩形位置
            _viewModel.SetCanvasSize(CroppingCanvas.ActualWidth, CroppingCanvas.ActualHeight);
        }

        // 拖拽边缘热区
        private void Edge_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string edge = "";
            if (sender == EdgeTop) edge = "Top";
            else if (sender == EdgeBottom) edge = "Bottom";
            else if (sender == EdgeLeft) edge = "Left";
            else if (sender == EdgeRight) edge = "Right";

            Point position = e.GetPosition(CroppingCanvas);
            _viewModel.StartEdgeDrag(edge, position.X, position.Y);
            ((System.Windows.Shapes.Rectangle)sender).CaptureMouse();
        }

        private void Edge_MouseMove(object sender, MouseEventArgs e)
        {
            if (((System.Windows.Shapes.Rectangle)sender).IsMouseCaptured)
            {
                Point position = e.GetPosition(CroppingCanvas);
                _viewModel.ResizeEdge(position.X, position.Y);
            }
        }

        private void Edge_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EndDrag();
            ((System.Windows.Shapes.Rectangle)sender).ReleaseMouseCapture();
        }

        // 拖动整个矩形
        private void ResizableRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(CroppingCanvas);
            _viewModel.StartMove(position.X, position.Y);
            ResizableRect.CaptureMouse();
        }

        private void ResizableRect_MouseMove(object sender, MouseEventArgs e)
        {
            if (ResizableRect.IsMouseCaptured)
            {
                Point position = e.GetPosition(CroppingCanvas);
                _viewModel.MoveRect(position.X, position.Y);
            }
        }

        private void ResizableRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _viewModel.EndDrag();
            ResizableRect.ReleaseMouseCapture();
        }

        #endregion

        #region 私有方法

        // 初始化按钮
        private void InitializeButtons()
        {
            // 绑定按钮事件
            if (BrowseButton != null)
                BrowseButton.Click += BrowseButton_Click;

            if (ResetZoomButton != null)
                ResetZoomButton.Click += ResetZoomButton_Click;

            if (SaveCroppedButton != null)
                SaveCroppedButton.Click += SaveCroppedButton_Click;
        }

        // 更新缩放文本显示
        private void UpdateZoomText()
        {
            if (ZoomTextBlock != null && ZoomSlider != null)
            {
                // 将滑块值转换为百分比并显示
                ZoomTextBlock.Text = $"{ZoomSlider.Value * 100:F0}%";
            }
        }

        // 当 ImageSource 改变时的回调函数
        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 获取 ImageEdit 控件实例
            ImageEdit imageEdit = (ImageEdit)d;

            // 如果新的图像源不为空
            if (e.NewValue is ImageSource imageSource)
            {
                // 更新 ViewModel 中的图像尺寸
                if (imageSource is BitmapSource bitmapSource)
                {
                    imageEdit._viewModel.ImageWidth = bitmapSource.PixelWidth;
                    imageEdit._viewModel.ImageHeight = bitmapSource.PixelHeight;
                    imageEdit.ImageWidth = bitmapSource.PixelWidth;
                    imageEdit.ImageHeight = bitmapSource.PixelHeight;
                }

                // 调用更新缩放文本方法
                imageEdit.UpdateZoomText();
            }
        }

        // 触发图片编辑完成事件
        protected virtual void OnImageEditCompleted(ImageEditCompletedEventArgs e)
        {
            ImageEditCompleted?.Invoke(this, e);
        }

        // 图片裁剪方法
        private void CropImage(string originalImagePath, string croppedImagePath)
        {
            // 检查原始图片是否存在
            if (!File.Exists(originalImagePath))
            {
                throw new FileNotFoundException("原始图片文件不存在。", originalImagePath);
            }

            // 加载图片
            using var image = Image.Load(originalImagePath);
            
            // 获取画布和矩形的实际尺寸（考虑缩放）
            double zoomFactor = ZoomSlider.Value;
            double actualCanvasWidth = _viewModel.ImageWidth * zoomFactor;
            double actualCanvasHeight = _viewModel.ImageHeight * zoomFactor;
            
            // 计算裁剪区域相对于原始图片的比例
            double cropLeft = (_viewModel.RectLeft / actualCanvasWidth) * image.Width;
            double cropTop = (_viewModel.RectTop / actualCanvasHeight) * image.Height;
            double cropWidth = (_viewModel.RectWidth / actualCanvasWidth) * image.Width;
            double cropHeight = (_viewModel.RectHeight / actualCanvasHeight) * image.Height;
            
            // 确保裁剪区域在图片范围内
            cropLeft = Math.Max(0, cropLeft);
            cropTop = Math.Max(0, cropTop);
            cropWidth = Math.Min(image.Width - cropLeft, cropWidth);
            cropHeight = Math.Min(image.Height - cropTop, cropHeight);
            
            // 执行裁剪
            var cropRectangle = new Rectangle((int)cropLeft, (int)cropTop, (int)cropWidth, (int)cropHeight);
            image.Mutate(x => x.Crop(cropRectangle));
            
            // 保存裁剪后的图片
            image.Save(croppedImagePath, new PngEncoder());
        }

        #endregion
    }

    // 图片编辑完成事件参数
    public class ImageEditCompletedEventArgs : EventArgs
    {
        public ImageSource CroppedImage { get; set; }
        public string CroppedImagePath { get; set; }
    }
}
