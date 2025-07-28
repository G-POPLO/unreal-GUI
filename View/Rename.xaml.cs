using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
