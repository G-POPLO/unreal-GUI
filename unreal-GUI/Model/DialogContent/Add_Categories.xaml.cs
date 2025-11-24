using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.ComponentModel;
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
            // 监听ViewModel的PropertyChanged事件，以更新对话框按钮状态
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            // 监听DisplayNameItems集合变化
            ViewModel.DisplayNameItems.CollectionChanged += (s, e) => UpdateDialogButtonState();
        }

        // 处理ViewModel的属性变化事件
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 当任何属性变化时，更新对话框按钮状态
            UpdateDialogButtonState();
        }

        // 更新对话框按钮状态
        private void UpdateDialogButtonState()
        {
            if (Dialog != null)
            {
                // 根据ViewModel的CanSave属性启用或禁用保存按钮
                Dialog.IsPrimaryButtonEnabled = ViewModel.CanSave;
            }
        }

        // ViewModel保存请求事件处理
        private void ViewModel_OnSaveRequested(object? sender, EventArgs e)
        {
            // 通知Dialog可以关闭并保存
            Dialog?.Hide();
        }
    }
}




