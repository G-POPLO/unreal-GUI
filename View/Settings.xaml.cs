using Microsoft.Win32;
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
using unreal_GUI.Model;
using static unreal_GUI.Settings;

namespace unreal_GUI
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : System.Windows.Controls.UserControl
    {
        private List<EngineInfo> engineInfos = new List<EngineInfo>();
       

    public class EngineInfo
    {
        public string Path { get; set; }
        public string Version { get; set; }
        }
        

        public Settings()
        {
            InitializeComponent();
            // 初始化UI页面
            Button0.IsChecked = Properties.Settings.Default.AutoOpen;
            Button1.IsChecked = Properties.Settings.Default.Gitcode;
            Button2.IsChecked = Properties.Settings.Default.ZenDashborad;
            Button3.IsChecked = Properties.Settings.Default.AutoUpdate;

            if (File.Exists("settings.json"))
            {
                engineInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EngineInfo>>(File.ReadAllText("settings.json"));
                EnginePathsList.ItemsSource = null;
                EnginePathsList.ItemsSource = engineInfos.Select(p => $"{p.Path} ({p.Version})").ToList();
                
            }
        }

        

        private string GetEngineVersion(string enginePath)
        {
            try
            {
                // 尝试从路径中提取版本号
                var dirName = Path.GetFileName(enginePath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                if (dirName.StartsWith("UE_"))
                {
                    return dirName.Substring(3);
                }

                // 尝试读取.version文件
                var versionFile = System.IO.Path.Combine(enginePath, "Engine", "Build", "Build.version");
                if (File.Exists(versionFile))
                {
                    dynamic versionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(versionFile));
                    return $"{versionInfo.MajorVersion}.{versionInfo.MinorVersion}.{versionInfo.PatchVersion}";
                }
            }
            catch { }
            return "未知版本";
        }
        // 自动设定引擎路径
        private void AutoPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EpicGames\\Unreal Engine"))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                var path = subKey?.GetValue("InstalledDirectory") as string;
                                if (!string.IsNullOrEmpty(path) && !engineInfos.Any(x => x.Path == path))
                                {
                                    engineInfos.Add(new EngineInfo { Path = path, Version = GetEngineVersion(path) });
                                }
                            }
                        }
                        EnginePathsList.ItemsSource = null;
                        EnginePathsList.ItemsSource = engineInfos.Select(p => $"{p.Path} ({p.Version})").ToList();
            
                    }
                }
            }
            catch (Exception)
            {
                _ = ModernDialog.ShowConfirmAsync("未检测到引擎，请手动设置引擎目录", "提示");
            }
        }

        private void AddEnginePath_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    engineInfos.Add(new EngineInfo { Path = folderDialog.SelectedPath, Version = GetEngineVersion(folderDialog.SelectedPath) });
                    EnginePathsList.ItemsSource = null;
                    EnginePathsList.ItemsSource = engineInfos.Select(p => $"{p.Path} ({p.Version})").ToList();
            
                }
            }
        }

        private void RemoveEnginePath_Click(object sender, RoutedEventArgs e)
        {
            if (EnginePathsList.SelectedItem != null)
            {
                var selectedPath = EnginePathsList.SelectedItem.ToString().Split('(')[0].Trim();
                engineInfos.RemoveAll(x => x.Path == selectedPath);
                EnginePathsList.ItemsSource = null;
                EnginePathsList.ItemsSource = engineInfos.Select(p => $"{p.Path} ({p.Version})").ToList();
            
            }
        }

        private void SavePaths_Click(object sender, RoutedEventArgs e)
        {
            // 保存应用程序设置
            Properties.Settings.Default.AutoOpen = Button0.IsChecked.Value;
            Properties.Settings.Default.Gitcode = Button1.IsChecked.Value;
            Properties.Settings.Default.ZenDashborad = Button2.IsChecked.Value;
            Properties.Settings.Default.AutoUpdate = Button3.IsChecked.Value;
            Properties.Settings.Default.Save();
            // 保存JSON文件
            File.WriteAllText("settings.json", Newtonsoft.Json.JsonConvert.SerializeObject(engineInfos));


            Tip.Text = "设置已保存";
            Tip.Visibility = System.Windows.Visibility.Visible;
            
        }



        private void Button0_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoOpen  = (bool)Button0.IsChecked;
        }

        private void Button1_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Gitcode = (bool)Button1.IsChecked;
        }

        private void Button2_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ZenDashborad = (bool)Button2.IsChecked;
        }

        private void Button3_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoUpdate = (bool)Button3.IsChecked;
        }
    }
}
