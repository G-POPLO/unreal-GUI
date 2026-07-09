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
        public AddCategoriesViewModel ViewModel { get; private set; }

        public Add_Categories()
        {
            InitializeComponent();
            ViewModel = new AddCategoriesViewModel();
            DataContext = ViewModel;
        }
    }
}
