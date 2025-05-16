using System;
using System.Collections.Generic;
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
using System.Windows.Forms;
using Microsoft.Win32;

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
                System.Diagnostics.Process.Start("cmd.exe", $" /c \"{exePath}\" {command.Replace("renom ", "")}");
                msg.Text = "重命名成功！";
                msg.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                msg.Text = $"重命名失败：{ex.Message}";
                msg.Visibility = Visibility.Visible;
            }
        }
    }
}
