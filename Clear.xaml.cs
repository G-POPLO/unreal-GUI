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

        
    }
}
