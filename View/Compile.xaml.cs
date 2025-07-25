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
using System.Diagnostics;
using System.IO;

namespace unreal_GUI
{
    /// <summary>
    /// Compile.xaml 的交互逻辑
    /// </summary>
    public partial class Compile : System.Windows.Controls.UserControl
    {
        private List<Settings.EngineInfo> engineList = [];

        public Compile()
        {
            InitializeComponent();
            if (File.Exists("settings.json"))
            {
                engineList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Settings.EngineInfo>>(File.ReadAllText("settings.json"));
                EngineVersions.ItemsSource = engineList;
            }
            
            // 初始禁用编译按钮
            CompileButton.IsEnabled = false;
            
            // 添加输入变化事件
            Input.TextChanged += ValidateInputs;
            Output.TextChanged += ValidateInputs;
            EngineVersions.SelectionChanged += ValidateInputs;
        }
        
        private void ValidateInputs(object sender, RoutedEventArgs e)
        {
            CompileButton.IsEnabled = !string.IsNullOrWhiteSpace(Input.Text) && 
                                   !string.IsNullOrWhiteSpace(Output.Text) && 
                                   EngineVersions.SelectedItem != null;
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
            var uatPath = Path.Combine(selectedEngine.Path, "Engine", "Build", "BatchFiles", "RunUAT.bat");

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
                    if (process.ExitCode == 0) 
                    {
                        Tips.Text = "编译成功！";
                        if (Properties.Settings.Default.AutoOpen)
                        {
                            Process.Start("explorer.exe", Output.Text);
                        }
                    }
                    else
                    {
                        Tips.Text = $"编译失败，错误代码：{process.ExitCode}";
                    }
                }
                System.Media.SoundPlayer player = new(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                player.Play();
            }
            catch (Exception ex)
            {
                Tips.Text = $"编译错误：{ex.Message}";
                System.Media.SoundPlayer player = new(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
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
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Unreal Plugin Files (*.uplugin)|*.uplugin"
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                Input.Text = dialog.FileName;
                ValidateInputs(null, null);
                // 读取.uplugin文件内容              
                string pluginContent = File.ReadAllText(Input.Text);
                dynamic pluginInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(pluginContent);
                string pluginEngineVersion = pluginInfo.EngineVersion;

                // 获取选择的引擎版本
                if (EngineVersions.SelectedItem != null)
                {
                    var selectedEngine = (Settings.EngineInfo)EngineVersions.SelectedItem;
                    Tips.Text = $"即将把版本{pluginEngineVersion}的插件编译成{selectedEngine.Version}版本的插件";
                }
                else
                {
                    Tips.Text = $"读取到插件版本{pluginEngineVersion}，请先选择目标引擎版本";
                }
                Tips.Visibility = Visibility.Visible;
            }
        }
    }
}
