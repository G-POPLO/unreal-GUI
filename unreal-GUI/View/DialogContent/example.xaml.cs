using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using unreal_GUI.ViewModel;

namespace unreal_GUI.View.DialogContent
{
    /// <summary>
    /// example.xaml 的交互逻辑
    /// </summary>
    public partial class example : UserControl
    {
        private readonly TestViewModel viewModel;

        public example()
        {
            InitializeComponent();
            viewModel = new TestViewModel();
            DataContext = viewModel;

            Loaded += MainWindow_Loaded;

            // 订阅事件
            ResizableRect.MouseLeftButtonDown += ResizableRect_MouseLeftButtonDown;
            ResizableRect.MouseMove += ResizableRect_MouseMove;
            ResizableRect.MouseLeftButtonUp += ResizableRect_MouseLeftButtonUp;

            // 边缘热区事件
            EdgeTop.MouseLeftButtonDown += Edge_MouseLeftButtonDown;
            EdgeTop.MouseMove += Edge_MouseMove;
            EdgeTop.MouseLeftButtonUp += Edge_MouseLeftButtonUp;
            EdgeBottom.MouseLeftButtonDown += Edge_MouseLeftButtonDown;
            EdgeBottom.MouseMove += Edge_MouseMove;
            EdgeBottom.MouseLeftButtonUp += Edge_MouseLeftButtonUp;
            EdgeLeft.MouseLeftButtonDown += Edge_MouseLeftButtonDown;
            EdgeLeft.MouseMove += Edge_MouseMove;
            EdgeLeft.MouseLeftButtonUp += Edge_MouseLeftButtonUp;
            EdgeRight.MouseLeftButtonDown += Edge_MouseLeftButtonDown;
            EdgeRight.MouseMove += Edge_MouseMove;
            EdgeRight.MouseLeftButtonUp += Edge_MouseLeftButtonUp;
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new(new Uri(openFileDialog.FileName));
                    BackgroundImage.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置画布尺寸并初始化矩形位置
            viewModel.SetCanvasSize(CroppingCanvas.ActualWidth, CroppingCanvas.ActualHeight);
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
            viewModel.StartEdgeDrag(edge, position.X, position.Y);
            ((Rectangle)sender).CaptureMouse();
        }

        private void Edge_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(CroppingCanvas);
            viewModel.ResizeEdge(position.X, position.Y);
        }

        private void Edge_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.EndDrag();
            ((Rectangle)sender).ReleaseMouseCapture();
        }

        // 拖动整个矩形
        private void ResizableRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(CroppingCanvas);
            viewModel.StartMove(position.X, position.Y);
            ResizableRect.CaptureMouse();
        }

        private void ResizableRect_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(CroppingCanvas);
            viewModel.MoveRect(position.X, position.Y);
        }

        private void ResizableRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.EndDrag();

            ResizableRect.ReleaseMouseCapture();
        }
    }
}
