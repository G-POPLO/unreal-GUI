using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.IO;

namespace unreal_GUI.ViewModel
{
    public partial class ImageEditViewModel : ObservableObject
    {
        // 图片相关属性
        [ObservableProperty]
        private BitmapSource? imageSource;

        [ObservableProperty]
        private int imageWidth;

        [ObservableProperty]
        private int imageHeight;

        // 剪裁矩形相关属性（绑定到 XAML 中的 Rectangle）
        [ObservableProperty]
        private double rectLeft;

        [ObservableProperty]
        private double rectTop;

        [ObservableProperty]
        private double rectWidth;

        [ObservableProperty]
        private double rectHeight;

        // 边框热区属性
        [ObservableProperty]
        private double edgeTopLeft;

        [ObservableProperty]
        private double edgeTopTop;

        [ObservableProperty]
        private double edgeTopWidth;

        [ObservableProperty]
        private double edgeTopHeight;

        [ObservableProperty]
        private double edgeBottomLeft;

        [ObservableProperty]
        private double edgeBottomTop;

        [ObservableProperty]
        private double edgeBottomWidth;

        [ObservableProperty]
        private double edgeBottomHeight;

        [ObservableProperty]
        private double edgeLeftLeft;

        [ObservableProperty]
        private double edgeLeftTop;

        [ObservableProperty]
        private double edgeLeftWidth;

        [ObservableProperty]
        private double edgeLeftHeight;

        [ObservableProperty]
        private double edgeRightLeft;

        [ObservableProperty]
        private double edgeRightTop;

        [ObservableProperty]
        private double edgeRightWidth;

        [ObservableProperty]
        private double edgeRightHeight;

        // 缩放相关属性
        [ObservableProperty]
        private double zoomLevel = 1.0;

        [ObservableProperty]
        private bool enableCompress = false;

        private BitmapSource? originalImage;
        private System.Windows.Rect cropRect;
        private bool isDragging = false;
        private string? currentImagePath;
        
        // 画布尺寸
        private double canvasWidth;
        private double canvasHeight;

        public ImageEditViewModel()
        {
            // 设置默认画布大小（如果需要的话）
            SetCanvasSize(800, 600);
            
            // 初始化默认剪裁区域（3:1比例，居中显示）
            InitializeRectPosition();
        }

        /// <summary>
        /// 设置画布尺寸并初始化矩形位置
        /// </summary>
        /// <param name="width">画布宽度</param>
        /// <param name="height">画布高度</param>
        public void SetCanvasSize(double width, double height)
        {
            canvasWidth = width;
            canvasHeight = height;
            InitializeRectPosition();
        }

        private void InitializeRectPosition()
        {
            if (canvasWidth > 0 && canvasHeight > 0)
            {
                // 设置默认的300x100矩形，居中显示
                double rectWidth = 300;
                double rectHeight = 100;
                
                RectLeft = (canvasWidth - rectWidth) / 2;
                RectTop = (canvasHeight - rectHeight) / 2;
                RectWidth = rectWidth;
                RectHeight = rectHeight;
                
                UpdateEdgeHotspots();
            }
        }

        /// <summary>
        /// 初始化默认的3:1剪裁矩形
        /// </summary>
        private void InitializeDefaultCropRect()
        {
            if (originalImage != null)
            {
                double imageWidth = originalImage.Width;
                double imageHeight = originalImage.Height;
                
                // 计算3:1比例的剪裁区域
                double cropWidth = Math.Min(imageWidth, imageHeight * 3);
                double cropHeight = cropWidth / 3;
                
                // 居中显示 - 使用生成的属性
                RectLeft = (imageWidth - cropWidth) / 2;
                RectTop = (imageHeight - cropHeight) / 2;
                RectWidth = cropWidth;
                RectHeight = cropHeight;
                
                UpdateEdgeHotspots();
            }
        }

        /// <summary>
        /// 更新边框热区位置
        /// </summary>
        private void UpdateEdgeHotspots()
        {
            // 上边框 - 使用生成的属性
            EdgeTopLeft = RectLeft;
            EdgeTopTop = RectTop - 5;
            EdgeTopWidth = RectWidth;
            EdgeTopHeight = 10;

            // 下边框 - 使用生成的属性
            EdgeBottomLeft = RectLeft;
            EdgeBottomTop = RectTop + RectHeight - 5;
            EdgeBottomWidth = RectWidth;
            EdgeBottomHeight = 10;

            // 左边框 - 使用生成的属性
            EdgeLeftLeft = RectLeft - 5;
            EdgeLeftTop = RectTop;
            EdgeLeftWidth = 10;
            EdgeLeftHeight = RectHeight;

            // 右边框 - 使用生成的属性
            EdgeRightLeft = RectLeft + RectWidth - 5;
            EdgeRightTop = RectTop;
            EdgeRightWidth = 10;
            EdgeRightHeight = RectHeight;
        }

        /// <summary>
        /// 选择图片命令
        /// </summary>
        [RelayCommand]
        private void BrowseImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "图片文件 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件 (*.*)|*.*",
                Title = "选择要剪裁的图片"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LoadImage(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 重置缩放命令
        /// </summary>
        [RelayCommand]
        private void ResetZoom()
        {
            ZoomLevel = 1.0;
        }

        /// <summary>
        /// 保存剪裁图片命令
        /// </summary>
        [RelayCommand]
        private void SaveCropped()
        {
            if (ImageSource == null)
            {
                System.Windows.MessageBox.Show("请先选择一张图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    System.Windows.MessageBox.Show("图片保存成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"保存图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        private void LoadImage(string imagePath)
        {
            currentImagePath = imagePath;
            
            // 使用 ImageSharp 加载图片
            using var image = Image.Load(imagePath);
            
            // 转换为 BitmapSource
            var bitmapSource = ConvertImageToBitmapSource(image);
            ImageSource = bitmapSource;
            originalImage = bitmapSource;
            
            // 更新图片尺寸
            ImageWidth = image.Width;
            ImageHeight = image.Height;
            
            // 初始化剪裁区域
            InitializeDefaultCropRect();
        }

        /// <summary>
        /// 将 ImageSharp Image 转换为 WPF BitmapSource
        /// </summary>
        /// <param name="image">ImageSharp 图片对象</param>
        /// <returns>WPF BitmapSource</returns>
        private BitmapSource ConvertImageToBitmapSource(Image image)
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
        /// 更新剪裁矩形
        /// </summary>
        /// <param name="newRect">新的剪裁矩形</param>
        public void UpdateCropRect(System.Windows.Rect newRect)
        {
            if (originalImage == null) return;
            
            // 保持3:1比例
            double aspectRatio = 3.0;
            double width = newRect.Width;
            double height = width / aspectRatio;
            
            // 调整位置以确保不超出图片边界
            double left = Math.Max(0, Math.Min(newRect.Left, originalImage.Width - width));
            double top = Math.Max(0, Math.Min(newRect.Top, originalImage.Height - height));
            
            // 确保高度不超出边界
            if (top + height > originalImage.Height)
            {
                top = originalImage.Height - height;
            }
            
            RectLeft = left;
            RectTop = top;
            RectWidth = width;
            RectHeight = height;
            
            UpdateEdgeHotspots();
        }

        /// <summary>
        /// 保存剪裁后的图片
        /// </summary>
        /// <param name="outputPath">输出文件路径</param>
        private void SaveCroppedImage(string outputPath)
        {
            if (originalImage == null || string.IsNullOrEmpty(currentImagePath))
                return;
                
            using var originalImageSharp = Image.Load(currentImagePath);
            
            // 创建剪裁后的图片
            var cropRectangle = new Rectangle(
                (int)RectLeft, 
                (int)RectTop, 
                (int)RectWidth, 
                (int)RectHeight);
            
            using var croppedImage = originalImageSharp.Clone(ctx => ctx.Crop(cropRectangle));
            
            // 保存图片
            var extension = Path.GetExtension(outputPath).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    croppedImage.SaveAsJpeg(outputPath);
                    break;
                case ".png":
                default:
                    croppedImage.SaveAsPng(outputPath);
                    break;
            }
        }

        /// <summary>
        /// 处理鼠标事件（需要在后台代码中调用）
        /// </summary>
        public void HandleMouseDown(System.Windows.Point mousePosition)
        {
            // 检查鼠标是否在剪裁矩形内
            if (mousePosition.X >= RectLeft && mousePosition.X <= RectLeft + RectWidth &&
                mousePosition.Y >= RectTop && mousePosition.Y <= RectTop + RectHeight)
            {
                isDragging = true;
                cropRect = new System.Windows.Rect(RectLeft, RectTop, RectWidth, RectHeight);
            }
        }

        /// <summary>
        /// 处理鼠标移动事件
        /// </summary>
        public void HandleMouseMove(System.Windows.Point mousePosition)
        {
            if (isDragging && originalImage != null)
            {
                double newLeft = mousePosition.X - cropRect.Width / 2;
                double newTop = mousePosition.Y - cropRect.Height / 2;
                
                // 边界检查
                newLeft = Math.Max(0, Math.Min(newLeft, originalImage.Width - RectWidth));
                newTop = Math.Max(0, Math.Min(newTop, originalImage.Height - RectHeight));
                
                RectLeft = newLeft;
                RectTop = newTop;
                
                UpdateEdgeHotspots();
            }
        }

        /// <summary>
        /// 处理鼠标释放事件
        /// </summary>
        public void HandleMouseUp()
        {
            isDragging = false;
        }
        
        // 拖拽相关属性
        [ObservableProperty]
        private bool isDraggingRect = false;

        [ObservableProperty]
        private string? dragEdge = null;

        [ObservableProperty]
        private double clickPositionX;

        [ObservableProperty]
        private double clickPositionY;

        // 开始拖拽边缘
        public void StartEdgeDrag(double posX, double posY, string edgeName)
        {
            IsDraggingRect = true;
            DragEdge = edgeName;
            ClickPositionX = posX;
            ClickPositionY = posY;
        }

        // 拖拽边缘调整大小
        public void ResizeEdge(double posX, double posY, string edgeName)
        {
            if (!IsDraggingRect || string.IsNullOrEmpty(DragEdge))
                return;

            double deltaX = posX - ClickPositionX;
            double deltaY = posY - ClickPositionY;

            // 根据边缘名称决定调整方向
            switch (edgeName)
            {
                case "EdgeTop":
                    ResizeFromTop(deltaY);
                    break;
                case "EdgeBottom":
                    ResizeFromBottom(deltaY);
                    break;
                case "EdgeLeft":
                    ResizeFromLeft(deltaX);
                    break;
                case "EdgeRight":
                    ResizeFromRight(deltaX);
                    break;
            }

            ClickPositionX = posX;
            ClickPositionY = posY;
        }

        private void ResizeFromTop(double deltaY)
        {
            double newHeight = RectHeight - deltaY;
            double newWidth = newHeight * 3.0;
            double newTop = RectTop + (RectHeight - newHeight);
            double newLeft = RectLeft + (RectWidth - newWidth) / 2;

            if (newHeight > 0 && newWidth > 0 && canvasHeight >= newHeight && canvasWidth >= newWidth)
            {
                RectTop = newTop;
                RectLeft = newLeft;
                RectHeight = newHeight;
                RectWidth = newWidth;
                UpdateEdgeHotspots();
            }
        }

        private void ResizeFromBottom(double deltaY)
        {
            double newHeight = RectHeight + deltaY;
            double newWidth = newHeight * 3.0;
            double newLeft = RectLeft + (RectWidth - newWidth) / 2;

            if (newHeight > 0 && newWidth > 0 && canvasHeight >= newHeight && canvasWidth >= newWidth)
            {
                RectHeight = newHeight;
                RectWidth = newWidth;
                RectLeft = newLeft;
                UpdateEdgeHotspots();
            }
        }

        private void ResizeFromLeft(double deltaX)
        {
            double newWidth = RectWidth - deltaX;
            double newHeight = newWidth / 3.0;
            double newLeft = RectLeft + (RectWidth - newWidth);
            double newTop = RectTop + (RectHeight - newHeight) / 2;

            if (newWidth > 0 && newHeight > 0 && canvasHeight >= newHeight && canvasWidth >= newWidth)
            {
                RectLeft = newLeft;
                RectTop = newTop;
                RectWidth = newWidth;
                RectHeight = newHeight;
                UpdateEdgeHotspots();
            }
        }

        private void ResizeFromRight(double deltaX)
        {
            double newWidth = RectWidth + deltaX;
            double newHeight = newWidth / 3.0;
            double newTop = RectTop + (RectHeight - newHeight) / 2;

            if (newWidth > 0 && newHeight > 0 && canvasHeight >= newHeight && canvasWidth >= newWidth)
            {
                RectWidth = newWidth;
                RectHeight = newHeight;
                RectTop = newTop;
                UpdateEdgeHotspots();
            }
        }

        // 结束拖拽
        public void EndDrag()
        {
            IsDraggingRect = false;
            DragEdge = null;
        }

        // 开始移动整个矩形
        public void StartMove(double posX, double posY)
        {
            IsDraggingRect = true;
            DragEdge = "Move";
            ClickPositionX = posX;
            ClickPositionY = posY;
        }

        // 移动整个矩形
        public void MoveRect(double posX, double posY)
        {
            if (IsDraggingRect && DragEdge == "Move")
            {
                double offsetX = posX - ClickPositionX;
                double offsetY = posY - ClickPositionY;

                double newLeft = RectLeft + offsetX;
                double newTop = RectTop + offsetY;

                // 边界检查
                if (newLeft >= 0 && newTop >= 0 && 
                    newLeft + RectWidth <= canvasWidth && 
                    newTop + RectHeight <= canvasHeight)
                {
                    RectLeft = newLeft;
                    RectTop = newTop;
                    UpdateEdgeHotspots();

                    ClickPositionX = posX;
                    ClickPositionY = posY;
                }
            }
        }
    }
}