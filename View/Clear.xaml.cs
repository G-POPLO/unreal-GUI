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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace unreal_GUI
{
    /// <summary>
    /// Clear.xaml 的交互逻辑
    /// </summary>
    public partial class Clear : UserControl
    {
        private List<Settings.EngineInfo> engineList = new List<Settings.EngineInfo>();

        private void DeleteDirectoryIfExists(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private void UpdatePanelVisibility()
        {
            if (Properties.Settings.Default.ZenDashborad)
            {
                OldDDC_Panel.Visibility = Visibility.Collapsed;
                Zen_Panel.Visibility = Visibility.Visible;
            }
            else
            {
                OldDDC_Panel.Visibility = Visibility.Visible;
                Zen_Panel.Visibility = Visibility.Collapsed;
            }
        }

        public Clear()
        {
            InitializeComponent();
            
            if (File.Exists("settings.json"))
            {
                engineList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Settings.EngineInfo>>(File.ReadAllText("settings.json"));
                EngineVersions.ItemsSource = engineList;
            }
            // 控件UI初始化
            UpdatePanelVisibility();  
            DDC.Text = $"DDC全局缓存路径：{Properties.Settings.Default.DDC}";
            DDCshare.Text = $"DDC共享缓存路径：{Properties.Settings.Default.DDCShare}";
            Total.Text = $"总计大小：{Properties.Settings.Default.DDCTotal:0.00} GB";
            
            // 初始禁用清理按钮
            CleanButton.IsEnabled = false;
            
            // 添加输入变化事件
            Input.TextChanged += ValidateInputs;
        }
        
        private void ValidateInputs(object sender, RoutedEventArgs e)
        {
            CleanButton.IsEnabled = !string.IsNullOrWhiteSpace(Input.Text);
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Input.Text = dialog.SelectedPath;
                ValidateInputs(null, null);
                Tip.Text = "工程路径已设置: " + dialog.SelectedPath;
                Tip.Visibility = Visibility.Visible;
            }
        }

        private void CleanButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string projectPath = Input.Text;
                if (string.IsNullOrEmpty(projectPath))
                {
                    Tip.Text = "请先设置工程路径";
                    Tip.Visibility = Visibility.Visible;
                    return;
                }

                try
                {
                    if (!DerivedDataCache.IsChecked ?? false)
                        Directory.Delete(Path.Combine(projectPath, "DerivedDataCache"), true);
                }
                catch { }

                try
                {
                    if (!SaveGame.IsChecked ?? false)
                        Directory.Delete(Path.Combine(projectPath, "Saved", "SaveGames"), true);
                }
                catch { }

                DeleteDirectoryIfExists(Path.Combine(projectPath, "Binaries"));
                DeleteDirectoryIfExists(Path.Combine(projectPath, "Build"));
                DeleteDirectoryIfExists(Path.Combine(projectPath, "Intermediate"));
                
                foreach (var file in Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly))
                File.Delete(file);

                Tip.Text = "清理完毕";
                Tip.Visibility = Visibility.Visible;
                var player = new System.Media.SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                player.Play();
                if (Properties.Settings.Default.AutoOpen)
                {
                    System.Diagnostics.Process.Start("explorer.exe", Path.Combine(Input.Text));
                }
            }
            catch (Exception ex)
            {
                Tip.Text = "清理失败: " + ex.Message;
                Tip.Visibility = Visibility.Visible;
                var player = new System.Media.SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }

        private void DDCButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\GlobalDataCachePath"))
                {
                    var localPath = key?.GetValue("UE-LocalDataCachePath")?.ToString();
                    var sharedPath = key?.GetValue("UE-SharedDataCachePath")?.ToString();

                    if (!string.IsNullOrEmpty(localPath))
                    {
                        DDC.Text = $"DDC全局缓存路径：{Properties.Settings.Default.DDC}";
                        Properties.Settings.Default.DDC = localPath;
                    }

                    if (!string.IsNullOrEmpty(sharedPath))
                    {
                        DDCshare.Text = $"DDC共享缓存路径：{Properties.Settings.Default.DDCShare}";
                        Properties.Settings.Default.DDCShare = sharedPath;
                    }

                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {

                _ = ModernDialog.ShowConfirmAsync($"找不到DDC缓存路径：" + ex.Message, "提示");
            }
        }

        private float CalculateDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            
            float size = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(file);
                    size += info.Length;
                }
            }
            catch { }
            return size / (1024 * 1024 * 1024); // 转换为GB
        }

        private async void TotalButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            try
            {
                button.IsEnabled = false;
                Total.Text = "正在计算...";

                await Task.Run(() =>
                {
                    var ddcPath = Properties.Settings.Default.DDC;
                    var sharePath = Properties.Settings.Default.DDCShare;

                    if (string.IsNullOrEmpty(ddcPath) || string.IsNullOrEmpty(sharePath))
                    {
                        throw new Exception("请先获取DDC缓存路径");
                    }

                    float totalSize = CalculateDirectorySize(ddcPath) + CalculateDirectorySize(sharePath);
                    Properties.Settings.Default.DDCTotal = totalSize;
                    Dispatcher.Invoke(() =>
                    {
                        Total.Text = $"总计大小：{totalSize:0.00} GB";
                    });
                    Properties.Settings.Default.Save();
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.Invoke(async () =>
                {
                    await ModernDialog.ShowInfoAsync("计算失败: " + ex.Message, "错误提示");
                    Total.Text = $"总计大小：{Properties.Settings.Default.DDCTotal:0.00} GB";
                });
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private void ZenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EngineVersions.SelectedItem == null)
                {
                    DDC_error.Text = "请先从下拉框中选择引擎版本"; 
                    DDC_error.Visibility = Visibility.Visible;
                    return;
                }
                var selectedEngine = EngineVersions.SelectedItem as Settings.EngineInfo;
                if (selectedEngine != null)
                {
                    var zenPath = Path.Combine(selectedEngine.Path, "Engine\\Binaries\\Win64\\ZenDashboard.exe");
                    if (File.Exists(zenPath))
                    {
                        System.Diagnostics.Process.Start(zenPath);
                    }
                    else
                    {
                        _ = ModernDialog.ShowInfoAsync("ZenDashboard.exe未找到，请确认引擎安装", "路径错误");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ModernDialog.ShowInfoAsync($"打开失败：{ex.Message}", "错误提示");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://dev.epicgames.com/documentation/unreal-engine/zen-storage-server-for-unreal-engine", UseShellExecute = true });
        }
    }
}


