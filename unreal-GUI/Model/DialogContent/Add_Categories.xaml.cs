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
            // 创建ViewModel实例
            ViewModel = new AddCategoriesViewModel();
            
            // 添加属性变化事件处理程序
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        // 处理ViewModel的属性变化事件
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 可以在这里添加UI响应逻辑
            // 例如，当ViewModel中的特定属性变化时，更新UI状态
        }


    }
}




