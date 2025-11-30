using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace unreal_GUI.View.DialogContent
{
    /// <summary>
    /// ImageEdit.xaml 的交互逻辑
    /// 用于3:1比例图片剪裁
    /// </summary>
    public partial class ImageEdit : UserControl
    {
        // 图片源属性（用于数据绑定）
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageEdit));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // 图片尺寸属性（用于数据绑定）
        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(string), typeof(ImageEdit), new PropertyMetadata("0"));

        public string ImageWidth
        {
            get { return (string)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(string), typeof(ImageEdit), new PropertyMetadata("0"));

        public string ImageHeight
        {
            get { return (string)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        // 是否显示裁剪指南
        public static readonly DependencyProperty ShowCropGuideProperty =
            DependencyProperty.Register("ShowCropGuide", typeof(bool), typeof(ImageEdit), new PropertyMetadata(true));

        public bool ShowCropGuide
        {
            get { return (bool)GetValue(ShowCropGuideProperty); }
            set { SetValue(ShowCropGuideProperty, value); }
        }

        public ImageEdit()
        {
            InitializeComponent();
            DataContext = this;
            InitializeButtons();
        }

        private void InitializeButtons()
        {
            // 初始化按钮事件
            BrowseButton.Click += BrowseButton_Click;
            ResetZoomButton.Click += ResetZoomButton_Click;
            SaveCroppedButton.Click += SaveCroppedButton_Click;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // 这里将来会实现选择图片的逻辑
            // 目前只是占位
        }

        private void ResetZoomButton_Click(object sender, RoutedEventArgs e)
        {
            // 重置缩放
            ZoomSlider.Value = 1.0;
            UpdateZoomText(1.0);
            // 这里将来会实现图片缩放重置的逻辑
        }

        private void SaveCroppedButton_Click(object sender, RoutedEventArgs e)
        {
            // 这里将来会实现保存裁剪图片的逻辑
            // 目前只是占位
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 更新缩放文本
            UpdateZoomText(e.NewValue);
            // 这里将来会实现图片缩放的逻辑
        }

        private void UpdateZoomText(double zoomValue)
        {
            // 更新缩放百分比显示
            ZoomText.Text = $"{Math.Round(zoomValue * 100)}%";
        }
    }
}
