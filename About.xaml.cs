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

        private string LatestVersion;
        private string LastUpdateTime;
        


        public About()
        {
            InitializeComponent();
            // 控件信息更新
            //LastUpdateTime = Properties.Settings.Default.LastUpdateTime.ToString();
            //LatestVersion = Properties.Settings.Default.LatestVersion.ToString();
            Version.Text = "当前版本：" + Application.ResourceAssembly.GetName().Version.ToString();
            DataContext = this;
            //Tip.Text = $"最新可用版本：{LatestVersion}";
            //Update_Time.Text = $"上次更新时间：{LastUpdateTime}";
        }
        

               

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://github.com/G-POPLO/unreal-GUI/releases/", UseShellExecute = true });
        }

        
    }
}


