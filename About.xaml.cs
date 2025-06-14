using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
            DataContext = new AboutViewModel();
        }
    }
}


