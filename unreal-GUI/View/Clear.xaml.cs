using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// Clear.xaml 的交互逻辑
    /// </summary>
    public partial class Clear : UserControl
    {
        public Clear()
        {
            InitializeComponent();
            DataContext = new ClearViewModel();
        }
    }
}


