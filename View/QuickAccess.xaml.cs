using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using unreal_GUI.Model;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// QuickAccess.xaml 的交互逻辑
    /// </summary>
    public partial class QuickAccess : System.Windows.Controls.UserControl
    {
        private QuickAccessViewModel viewModel;

        public QuickAccess()
        {
            InitializeComponent();
            viewModel = new QuickAccessViewModel();
            DataContext = viewModel;
            
            // 订阅ModernDialog的导航请求事件以刷新自定义按钮
            ModernDialog.NavigationRequested += OnNavigationRequested;
        }
        
        private void OnNavigationRequested(object sender, string pageTag)
        {
            // 如果导航到QuickAccess页面，则刷新自定义按钮数据
            if (pageTag == "QuickAccess")
            {
                viewModel.LoadCustomButtons();
            }
        }

        // 在控件卸载时取消事件订阅
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ModernDialog.NavigationRequested -= OnNavigationRequested;
        }
    }
}
