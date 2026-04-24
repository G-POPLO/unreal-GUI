using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI.View.DialogContent
{
    public partial class Tutorial : UserControl
    {
        public Tutorial()
        {
            InitializeComponent();
            DataContext = new TutorialViewModel();
        }
    }
}
