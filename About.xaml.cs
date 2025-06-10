using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace unreal_GUI
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : System.Windows.Controls.UserControl
    {
        public string VersionText { get; set; }
        public string LatestVersion { get; set; }

        public About()
        {
            InitializeComponent();
           
            VersionText = "版本：" + Application.ResourceAssembly.GetName().Version.ToString();
            DataContext = this;
            //Tip.Text = $"最新可用版本：{LatestVersion}";
        }
        

               

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://github.com/G-POPLO/unreal-GUI/releases/", UseShellExecute = true });
        }

        
    }
}


