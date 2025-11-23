using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI.Model.DialogContent
{
    /// <summary>
    /// Add_Categories.xaml 的交互逻辑
    /// 用于添加模板类别
    /// </summary>
    public partial class Add_Categories : UserControl
    {
        // ViewModel属性
        public AddCategoriesViewModel ViewModel { get; private set; }

        // 对话框引用
        public ContentDialog Dialog { get; set; }

        // 构造函数
        public Add_Categories()
        {
            InitializeComponent();
            InitializeViewModel();
            DataContext = ViewModel;
        }

        // 初始化ViewModel
        private void InitializeViewModel()
        {
            ViewModel = new AddCategoriesViewModel();
            ViewModel.OnSaveRequested += ViewModel_OnSaveRequested;
        }

        // ViewModel保存请求事件处理
        private void ViewModel_OnSaveRequested(object? sender, EventArgs e)
        {
            // 通知Dialog可以关闭并保存
            Dialog?.Hide();
        }
    }
}




