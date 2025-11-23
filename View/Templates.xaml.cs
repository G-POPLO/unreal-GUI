using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI.View
{
    /// <summary>
    /// Templates.xaml 的交互逻辑
    /// </summary>
    public partial class Templates : UserControl
    {
        public Templates()
        {
            InitializeComponent();
            this.DataContext = new TemplatesViewModel();
        }
    }
}
