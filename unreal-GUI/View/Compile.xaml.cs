using System;
using System.ComponentModel;
using System.Windows;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// Compile.xaml 的交互逻辑
    /// </summary>
    public partial class Compile : System.Windows.Controls.UserControl
    {
        public Compile()
        {
            InitializeComponent();
            DataContext = new CompileViewModel();

            // 监听 LogText 变化，自动滚动到底部以保持最新输出可见
            if (DataContext is INotifyPropertyChanged vm)
            {
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(CompileViewModel.LogText))
                    {
                        Dispatcher.BeginInvoke(new Action(() => LogTextBox.ScrollToEnd()));
                    }
                };
            }
        }
    }
}
