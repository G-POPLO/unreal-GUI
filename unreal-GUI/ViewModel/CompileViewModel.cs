using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;
using unreal_GUI.Model;
using unreal_GUI.Model.Basic;
namespace unreal_GUI.ViewModel
{
    public partial class CompileViewModel : ObservableObject
    {
        private List<EngineInfo> engineList = [];

        [ObservableProperty]
        private EngineInfo selectedEngine;

        [ObservableProperty]
        private string inputPath;

        [ObservableProperty]
        private string outputPath;

        [ObservableProperty]
        private string tipsText;

        public CompileViewModel()
        {
            LoadEngineList();
        }

        public ObservableCollection<EngineInfo> EngineVersions { get; } = [];

        public bool IsCompileButtonEnabled => CanCompile();

        [RelayCommand]
        private void SelectInput()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Unreal Plugin Files (*.uplugin)|*.uplugin"
            };

            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                InputPath = dialog.FileName;

                // 读取.uplugin文件内容
                string pluginContent = File.ReadAllText(InputPath);
                JsonNode pluginInfo = JsonNode.Parse(pluginContent);
                string pluginEngineVersion = pluginInfo["EngineVersion"].ToString();

                // 获取选择的引擎版本
                TipsText = SelectedEngine != null
                    ? $"即将把版本{pluginEngineVersion}的插件编译成{SelectedEngine.Version}版本的插件"
                    : $"读取到插件版本{pluginEngineVersion}，请先选择目标引擎版本";

            }
        }

        [RelayCommand]
        private void SelectOutput()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择输出文件夹",
                UseDescriptionForTitle = true
            };

            // 如果已有输出路径，设置为初始目录
            if (!string.IsNullOrWhiteSpace(OutputPath) && Directory.Exists(OutputPath))
            {
                dialog.SelectedPath = OutputPath;
            }

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                OutputPath = dialog.SelectedPath;
            }
        }

        private bool CanCompile()
        {
            return !string.IsNullOrWhiteSpace(InputPath) &&
                   !string.IsNullOrWhiteSpace(OutputPath) &&
                   SelectedEngine != null;
        }

        partial void OnInputPathChanged(string value)
        {
            OnPropertyChanged(nameof(IsCompileButtonEnabled));
            CompileCommand.NotifyCanExecuteChanged();
        }

        partial void OnOutputPathChanged(string value)
        {
            OnPropertyChanged(nameof(IsCompileButtonEnabled));
            CompileCommand.NotifyCanExecuteChanged();
        }

        partial void OnSelectedEngineChanged(EngineInfo value)
        {
            OnPropertyChanged(nameof(IsCompileButtonEnabled));
            CompileCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanCompile))]
        private async Task Compile()
        {


            if (SelectedEngine == null)
            {
                TipsText = "请选择引擎版本";
                return;
            }

            var TerminalPath = Path.Combine(SelectedEngine.Path, "Engine", "Build", "BatchFiles", "RunTerminal.bat");

            if (!File.Exists(TerminalPath))
            {
                TipsText = "找不到RunTerminal.bat文件";
                return;
            }

            if (string.IsNullOrWhiteSpace(InputPath) || !File.Exists(InputPath))
            {
                TipsText = "插件路径无效";
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPath) || !Directory.Exists(OutputPath))
            {
                TipsText = "输出路径无效";
                return;
            }

            // 检查输出文件夹是否为空
            if (Directory.GetFileSystemEntries(OutputPath).Length > 0)
            {
                // 显示确认对话框
                bool? confirmResult = await ModernDialog.ShowConfirmAsync(
                    $"输出文件夹 '{OutputPath}' 不为空，编译过程将删除该文件夹中的所有内容。是否继续？",
                    "确认编译？");

                // 如果用户取消，则不执行编译
                if (confirmResult != true)
                {
                    TipsText = "编译操作已取消";
                    SoundFX.PlaySound(1);
                    return;
                }
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = TerminalPath,
                    Arguments = $"BuildPlugin -plugin=\"{InputPath}\" -package=\"{OutputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                };

                using (var process = Process.Start(startInfo))
                {
                    await Task.Run(process.WaitForExit);
                    if (process.ExitCode == 0)
                    {
                        TipsText = "编译成功！";
                        if (Properties.Settings.Default.AutoOpen)
                        {
                            Process.Start(OutputPath);
                        }
                    }
                    else
                    {
                        TipsText = $"编译失败，错误代码：{process.ExitCode}";
                    }
                }

                SoundFX.PlaySound(0);

            }
            catch (Exception ex)
            {
                TipsText = $"编译错误：{ex.Message}";
                SoundFX.PlaySound(1);

            }
        }

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var settings = JsonSerializer.Deserialize<SettingsData>(json);
                engineList = settings?.Engines ?? [];
                foreach (EngineInfo engine in engineList)
                {
                    EngineVersions.Add(engine);
                }
            }
        }
    }
}