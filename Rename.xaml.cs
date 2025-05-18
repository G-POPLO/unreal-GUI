using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            // 创建FolderBrowserDialog实例
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
           

            // 显示对话框并检查用户是否选择了文件夹
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取选择的文件夹路径
                string selectedPath = folderBrowserDialog.SelectedPath;
                Input.Text = selectedPath;
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            string projectPath = Input.Text;
            string newName = Output.Text;
            string command;

            if (rbProject.IsChecked == true)
            {
                command = $"renom rename-project --project {projectPath} --new-name {newName}";
            }
            else
            {
                command = $"renom rename-plugin --project {projectPath} --plugin {projectPath} --new-name {newName}";
            }

            try
            {
                string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App", "renom.exe");
                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = command.Replace("renom ", ""),
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(processInfo);


                
                msg.Text = "重命名成功！";
                msg.Visibility = Visibility.Visible;


                // 检查AutoOpen设置来决定是否打开文件夹
                if (Properties.Settings.Default.AutoOpen)
                {
                    // 更新输入框为新的路径
                    string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(projectPath), newName);
                    Input.Text = newPath;
                    msg.Text = "重命名成功！";
                    msg.Visibility = Visibility.Visible;
                    Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = newPath,
                    UseShellExecute = true
                });
                }
            }
            catch (Exception ex)
            {
                msg.Text = $"重命名失败：{ex.Message}";
                msg.Visibility = Visibility.Visible;               
                var player = new System.Media.SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }
    }
}
