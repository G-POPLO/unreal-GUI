using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// Compile.xaml 的交互逻辑
    /// </summary>
    public partial class Compile : System.Windows.Controls.UserControl
    {
        public Compile()
        {
            InitializeComponent();
            DataContext = new CompileViewModel();
        }
    }
}
