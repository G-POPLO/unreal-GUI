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
using System.Diagnostics;
using System.IO;

namespace unreal_GUI
{
    /// <summary>
    /// Compile.xaml 的交互逻辑
    /// </summary>
    public partial class Compile : System.Windows.Controls.UserControl
    {
        private List<Settings.EngineInfo> engineList = new List<Settings.EngineInfo>();

        public Compile()
        {
            InitializeComponent();
            if (File.Exists("settings.json"))
            {
                engineList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Settings.EngineInfo>>(File.ReadAllText("settings.json"));
                EngineVersions.ItemsSource = engineList;
            }
        }

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            Tips.Visibility = Visibility.Visible;

            if (EngineVersions.SelectedItem == null)
            {
                Tips.Text = "请选择引擎版本";
                return;
            }

            var selectedEngine = (Settings.EngineInfo)EngineVersions.SelectedItem;
            var uatPath = System.IO.Path.Combine(selectedEngine.Path, "Engine", "Build", "BatchFiles", "RunUAT.bat");

            if (!File.Exists(uatPath))
            {
                Tips.Text = "找不到RunUAT.bat文件";
                return;
            }

            if (string.IsNullOrWhiteSpace(Input.Text) || !File.Exists(Input.Text))
            {
                Tips.Text = "插件路径无效";
                return;
            }

           

            if (string.IsNullOrWhiteSpace(Output.Text) || !Directory.Exists(Output.Text))
            {
                Tips.Text = "输出路径无效";
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = uatPath,
                    Arguments = $"BuildPlugin -plugin=\"{Input.Text}\" -package=\"{Output.Text}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    Tips.Text = process.ExitCode == 0 
                        ? "编译成功！" 
                        : $"编译失败，错误代码：{process.ExitCode}";
                }
            }
            catch (Exception ex)
            {
                Tips.Text = $"编译错误：{ex.Message}";
            }
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Output.Text = dialog.SelectedPath;
            }
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Unreal Plugin Files (*.uplugin)|*.uplugin";
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Input.Text = dialog.FileName;
                // 读取.uplugin文件内容
                string pluginContent = File.ReadAllText(Input.Text);
                dynamic pluginInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(pluginContent);
                string pluginEngineVersion = pluginInfo.EngineVersion;

                // 获取选择的引擎版本
                var selectedEngine = (Settings.EngineInfo)EngineVersions.SelectedItem;

                // 更新提示信息
                Tips.Text = $"即将把版本{pluginEngineVersion}的插件编译成{selectedEngine.Version}版本的插件";
                Tips.Visibility = Visibility.Visible;
            }
        }
    }
}
