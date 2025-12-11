using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Windows.Controls;
using ProgressBar = iNKORE.UI.WPF.Modern.Controls.ProgressBar;

namespace unreal_GUI.View.DialogContent
{
    /// <summary>
    /// CompressInfo.xaml 的交互逻辑
    /// </summary>
    public partial class CompressInfo : UserControl
    {
        public ContentDialog Dialog { get; set; }

        public CompressInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 更新压缩进度
        /// </summary>
        /// <param name="progress">进度值（0-100）</param>
        public void UpdateProgress(int progress)
        {
            // 确保在UI线程上更新
            this.Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = progress;
                ProgressText.Text = $"{progress}%";
            });
        }

        /// <summary>
        /// 更新压缩信息
        /// </summary>
        /// <param name="message">要添加的信息</param>
        public void UpdateCompressInfo(string message)
        {
            // 确保在UI线程上更新
            this.Dispatcher.Invoke(() =>
            {
                CompressText.Text += message + Environment.NewLine;
                // 滚动到最新内容
                CompressText.ScrollToEnd();
            });
        }

        /// <summary>
        /// 更新当前操作
        /// </summary>
        /// <param name="operation">当前操作信息</param>
        public void UpdateCurrentOperation(string operation)
        {
            // 确保在UI线程上更新
            this.Dispatcher.Invoke(() =>
            {
                CurrentOperationText.Text = operation;
            });
        }
    }
}