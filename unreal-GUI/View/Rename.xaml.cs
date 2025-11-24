using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// Rename.xaml 的交互逻辑
    /// </summary>
    public partial class Rename : System.Windows.Controls.UserControl
    {
        public Rename()
        {
            InitializeComponent();
            DataContext = new RenameViewModel();
        }
    }
}
