using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// QuickAccess.xaml 的交互逻辑
    /// </summary>
    public partial class QuickAccess : System.Windows.Controls.UserControl
    {
        public QuickAccess()
        {
            InitializeComponent();
            LoadEngineList();
        }

        private List<Settings.EngineInfo> engines = new List<Settings.EngineInfo>();

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                engines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Settings.EngineInfo>>(File.ReadAllText("settings.json"));
                EnginesList.ItemsSource = engines.Select(e => new { DisplayName = $"UE {e.Version}", Path = e.Path }).ToList();
            }
        }

        private void OpenPluginDirectory_Click(object sender, RoutedEventArgs e)
        {
            var selectedEngine = ((FrameworkElement)sender).DataContext as dynamic;
            string pluginPath = System.IO.Path.Combine(selectedEngine.Path, "Engine", "Plugins", "Marketplace");

            if (Directory.Exists(pluginPath))
            {
                Process.Start("explorer.exe", pluginPath);
            }
            else
            {
                _ = ModernDialog.ShowInfoAsync("目录不存在", $"未找到插件目录：\n{pluginPath}");
            }
        }

       
    }
}
