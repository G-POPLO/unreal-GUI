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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace unreal_GUI
{
    /// <summary>
    /// Clear.xaml 的交互逻辑
    /// </summary>
    public partial class Clear : UserControl
    {
        private void DeleteDirectoryIfExists(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public Clear()
        {
            InitializeComponent();
            DDC.Text = $"DDC全局缓存路径：{Properties.Settings.Default.DDC}";
            DDCshare.Text = $"DDC共享缓存路径：{Properties.Settings.Default.DDCShare}";
            Total.Text = $"总计大小：{Properties.Settings.Default.DDCTotal:0.00} GB";
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Input.Text = dialog.SelectedPath;
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

                if (!DerivedDataCache.IsChecked ?? false && Directory.Exists(Path.Combine(projectPath, "DerivedDataCache")))
                    Directory.Delete(Path.Combine(projectPath, "DerivedDataCache"), true);
                
                if (!SaveGame.IsChecked ?? false && Directory.Exists(Path.Combine(projectPath, "Saved", "SaveGames")))
                    Directory.Delete(Path.Combine(projectPath, "Saved", "SaveGames"), true);
                
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
    }
}


