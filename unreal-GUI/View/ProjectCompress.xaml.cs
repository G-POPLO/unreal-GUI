using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI.View
{
    /// <summary>
    /// ProjectCompress.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectCompress : UserControl
    {
        public ProjectCompress()
        {
            InitializeComponent();
            DataContext = new ProjectCompressViewModel();
        }
    }
}
