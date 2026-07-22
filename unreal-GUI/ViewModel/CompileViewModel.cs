using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private string? pluginName;

        [ObservableProperty]
        public partial EngineInfo SelectedEngine { get; set; } = null!;

        [ObservableProperty]
        public partial string InputPath { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string OutputPath { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string TipsText { get; set; } = string.Empty;

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

            if (result != DialogResult.OK)
            {
                return;
            }

            InputPath = dialog.FileName;

            // 读取 .uplugin 文件内容并解析
            try
            {
                string pluginContent = File.ReadAllText(InputPath);
                JsonNode pluginInfo = JsonNode.Parse(pluginContent);
                string? pluginEngineVersion = pluginInfo["EngineVersion"]?.ToString() ?? "未知";
                pluginName = pluginInfo["FriendlyName"]?.ToString()?.Replace(" ", "")
                    ?? Path.GetFileNameWithoutExtension(InputPath);

                // 获取选择的引擎版本
                TipsText = SelectedEngine != null
                    ? $"即将把版本{pluginEngineVersion}的插件编译成{SelectedEngine.Version}版本的插件"
                    : $"读取到插件版本{pluginEngineVersion}，请先选择目标引擎版本";
            }
            catch (Exception ex)
            {
                pluginName = null;
                TipsText = $"无法解析插件文件：{ex.Message}";
                SoundFX.PlaySound(2);
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

        // 统一刷新编译按钮的可用性
        private void RefreshCompileState()
        {
            OnPropertyChanged(nameof(IsCompileButtonEnabled));
            CompileCommand.NotifyCanExecuteChanged();
        }

        partial void OnInputPathChanged(string value) => RefreshCompileState();

        partial void OnOutputPathChanged(string value) => RefreshCompileState();

        partial void OnSelectedEngineChanged(EngineInfo value) => RefreshCompileState();

        [RelayCommand(CanExecute = nameof(CanCompile))]
        private async Task Compile()
        {
            if (!ValidateInputs(out string terminalPath, out string actualOutputPath))
            {
                return;
            }

            // 如果用户取消，则不执行编译
            if (!await ConfirmOverwriteIfNeeded(actualOutputPath))
            {
                return;
            }

            await RunBuildProcess(terminalPath, actualOutputPath);
        }

        // 验证输入参数并计算实际输出路径，不修改 OutputPath
        private bool ValidateInputs(out string terminalPath, out string actualOutputPath)
        {
            terminalPath = string.Empty;
            actualOutputPath = string.Empty;

            if (SelectedEngine == null)
            {
                TipsText = "请选择引擎版本";
                return false;
            }

            string candidateTerminal = Path.Combine(
                SelectedEngine.Path, "Engine", "Build", "BatchFiles", "RunUAT.bat");

            if (!File.Exists(candidateTerminal))
            {
                TipsText = "找不到RunUAT.bat文件";
                return false;
            }

            if (string.IsNullOrWhiteSpace(InputPath) || !File.Exists(InputPath))
            {
                TipsText = "插件路径无效";
                return false;
            }

            if (string.IsNullOrWhiteSpace(OutputPath) || !Directory.Exists(OutputPath))
            {
                TipsText = "输出路径无效";
                return false;
            }

            if (string.IsNullOrWhiteSpace(pluginName))
            {
                TipsText = "无法获取插件名称";
                return false;
            }

            terminalPath = candidateTerminal;
            actualOutputPath = Path.Combine(OutputPath, pluginName);

            // 确保子文件夹存在（不修改 OutputPath，避免污染用户输入）
            if (!Directory.Exists(actualOutputPath))
            {
                Directory.CreateDirectory(actualOutputPath);
            }

            return true;
        }

        // 仅在目标目录非空时询问用户
        private async Task<bool> ConfirmOverwriteIfNeeded(string actualOutputPath)
        {
            if (Directory.GetFileSystemEntries(actualOutputPath).Length == 0)
            {
                return true;
            }

            bool? confirmResult = await ModernDialog.ShowConfirmAsync(
                $"输出文件夹 '{actualOutputPath}' 不为空，编译过程将删除该文件夹中的所有内容。是否继续？",
                "确认编译？");

            if (confirmResult == true)
            {
                return true;
            }

            TipsText = "编译操作已取消";
            SoundFX.PlaySound(1);
            return false;
        }

        // 启动 RunUAT.bat 并等待编译完成
        private async Task RunBuildProcess(string terminalPath, string actualOutputPath)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = terminalPath,
                    Arguments = $"BuildPlugin -plugin=\"{InputPath}\" -package=\"{actualOutputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = startInfo };
                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) outputBuilder.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) errorBuilder.AppendLine(e.Data);
                };

                if (!process.Start())
                {
                    TipsText = "无法启动编译进程";
                    SoundFX.PlaySound(2);
                    return;
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    TipsText = "编译成功！";
                    SoundFX.PlaySound(4);
                    if (Properties.Settings.Default.AutoOpen)
                    {
                        Process.Start("explorer.exe", actualOutputPath);
                    }
                }
                else
                {
                    SoundFX.PlaySound(2);
                    TipsText = process.ExitCode switch
                    {
                        6 => "编译失败：引擎API更改，请手动创建新的C++工程进行编译",
                        -1 => "编译失败：用户取消了操作",
                        _ => $"编译失败，错误代码：{process.ExitCode}"
                    };

                    // 将日志写入软件根目录，并询问用户是否查看
                    string logPath = WriteCompileLog(process.ExitCode, outputBuilder, errorBuilder);
                    await PromptToViewLog(logPath);
                }
            }
            catch (Exception ex)
            {
                SoundFX.PlaySound(2);
                TipsText = $"编译错误：{ex.Message}";
            }
        }

        // 将编译日志写入软件根目录的文本文件
        private string WriteCompileLog(int exitCode, StringBuilder output, StringBuilder error)
        {
            string fileName = $"CompileLog_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            var content = new StringBuilder();
            content.AppendLine("=== 编译日志 ===");
            content.AppendLine($"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            content.AppendLine($"退出码：{exitCode}");
            content.AppendLine($"输入插件：{InputPath}");
            content.AppendLine();
            content.AppendLine("--- 标准输出 ---");
            content.Append(output);
            content.AppendLine("--- 错误输出 ---");
            content.Append(error);

            File.WriteAllText(logPath, content.ToString(), Encoding.UTF8);
            return logPath;
        }

        // 询问用户是否打开日志文件
        private static async Task PromptToViewLog(string logPath)
        {
            bool? confirm = await ModernDialog.ShowConfirmAsync(
                $"编译失败，日志已保存到：\n{logPath}\n\n是否打开日志查看详情？",
                "查看编译日志");

            if (confirm == true)
            {
                // 用系统默认关联程序打开（通常是记事本）
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
        }

        private void LoadEngineList()
        {
            if (!File.Exists("settings.json"))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText("settings.json");
                var settings = JsonSerializer.Deserialize<SettingsData>(json);
                foreach (var engine in settings?.Engines ?? [])
                {
                    EngineVersions.Add(engine);
                }

                // 若未手动选择输出路径且默认路径存在，则自动填充
                if (string.IsNullOrWhiteSpace(OutputPath)
                    && !string.IsNullOrWhiteSpace(settings?.DefaultOutputPath)
                    && Directory.Exists(settings.DefaultOutputPath))
                {
                    OutputPath = settings.DefaultOutputPath;
                }
            }
            catch
            {
                // 与 ClearViewModel/SettingsViewModel 保持一致：JSON 损坏时提示用户并以空列表继续
                _ = ModernDialog.ShowInfoAsync("settings.json 文件已损坏，已重置为默认状态", "提示");
            }
        }
    }
}
