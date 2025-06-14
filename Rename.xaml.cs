using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// Rename.xaml 的交互逻辑
    /// </summary>
    public partial class Rename : UserControl
    {
        public Rename()
        {
            InitializeComponent();
            DataContext = new RenameViewModel();
        }
    }
}

