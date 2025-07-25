using System.Windows.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    /// <summary>
    /// Rename.xaml 的交互逻辑
    /// </summary>
    public partial class Rename : UserControl
    {
        public Rename()
        {
            InitializeComponent();
            RenameButton.IsEnabled = false;
            Input.TextChanged += ValidateInputs;
            Output.TextChanged += ValidateInputs;
        }

        private void ValidateInputs(object sender, TextChangedEventArgs e)
        {
            RenameButton.IsEnabled = !string.IsNullOrEmpty(Input.Text) && !string.IsNullOrEmpty(Output.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            // 创建FolderBrowserDialog实例
            FolderBrowserDialog folderBrowserDialog = new();
           

            // 显示对话框并检查用户是否选择了文件夹
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // 获取选择的文件夹路径
                string selectedPath = folderBrowserDialog.SelectedPath;
                Input.Text = selectedPath;
                ValidateInputs(null, null);
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
                    CreateNoWindow = false
                };
                var process = System.Diagnostics.Process.Start(processInfo);
                process.WaitForExit(); // 等待重命名进程完成

                
                msg.Text = "重命名成功！";
                msg.Visibility = Visibility.Visible;


                // 检查AutoOpen设置来决定是否打开文件夹

                if (Properties.Settings.Default.AutoOpen)
                {
                    // 更新输入框为新的路径
                    string newPath = Path.Combine(System.IO.Path.GetDirectoryName(projectPath), newName);
                    Input.Text = newPath;
                    msg.Text = "重命名成功！";
                    System.Media.SoundPlayer player = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                    player.Play();
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
                var player = new System.Media.SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }
    }
}
