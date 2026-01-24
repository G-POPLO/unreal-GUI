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

        // 请求导航到图片编辑页面的事件
        //public event EventHandler<string> NavigateToImageEditRequested;

        // 构造函数
        public Add_Categories()
        {
            InitializeComponent();
            ViewModel = new AddCategoriesViewModel();
            DataContext = ViewModel;
        }

        // 初始化ViewModel
        //private void InitializeViewModel()
        //{

        //    // 订阅图片编辑请求事件
        //    //ViewModel.RequestImageEdit += ViewModel_RequestImageEdit;

        //    //// 添加属性变化事件处理程序
        //    //ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        //}

        // 处理图片编辑请求事件
        //private void ViewModel_RequestImageEdit(object? sender, string imagePath)
        //{
        //    // 触发导航事件，通知父级需要切换到图片编辑页面
        //    NavigateToImageEditRequested?.Invoke(this, imagePath);
        //}

        // 处理ViewModel的属性变化事件
        //private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    // 可以在这里添加UI响应逻辑
        //    // 例如，当ViewModel中的特定属性变化时，更新UI状态
        //}
    }
}




