using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// QuickAccess.xaml 的交互逻辑
    /// </summary>
    public partial class QuickAccess : System.Windows.Controls.UserControl
    {
        public QuickAccess()
        {
            InitializeComponent();
            DataContext = new QuickAccessViewModel();
        }
    }
}
