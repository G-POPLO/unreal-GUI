using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using unreal_GUI.Model.Basic;

namespace unreal_GUI.ViewModel
{
    public partial class RenameViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _inputPath;

        [ObservableProperty]
        private string _outputPath;

        [ObservableProperty]
        private bool _isProjectSelected = true;

        [ObservableProperty]
        private bool _isBPSelected = true;

        [ObservableProperty]
        private string _message;

        [ObservableProperty]
        private Visibility _messageVisibility = Visibility.Hidden;

        [ObservableProperty]
        private Visibility _projectTypePanelVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility _infoBarVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private bool _isRenameButtonEnabled = true;

        partial void OnIsProjectSelectedChanged(bool value)
        {
            ProjectTypePanelVisibility = value ? Visibility.Visible : Visibility.Collapsed;
        }


        partial void OnIsBPSelectedChanged(bool value)
        {
            InfoBarVisibility = !value ? Visibility.Visible : Visibility.Collapsed;
        }



        [RelayCommand]
        private void SelectFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择文件夹"
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                InputPath = dialog.SelectedPath;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private void WriteLog(string logMessage)
        {
            try
            {
                string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "renom_debug.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] {logMessage}{Environment.NewLine}";

                File.AppendAllText(logFilePath, logEntry);
            }
            catch
            {
                // 静默处理日志写入失败，避免影响主要功能
            }
        }

        private async Task<string> ExecuteRenomCommand(string arguments, string stepDescription, string projectPath)
        {
            WriteLog($"=== 开始执行: {stepDescription} ===");
            WriteLog($"命令参数: {arguments}");

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App", "renom.exe");

            if (!File.Exists(exePath))
            {
                string error = $"错误：找不到renom.exe工具文件";
                WriteLog(error);
                return error;
            }

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Path.GetDirectoryName(projectPath)
                };

                WriteLog($"工作目录: {Path.GetDirectoryName(projectPath)}");

                using var process = new Process { StartInfo = processInfo, EnableRaisingEvents = true };
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                        WriteLog($"[标准输出] {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                        WriteLog($"[错误输出] {e.Data}");
                    }
                };

                WriteLog("启动进程...");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                WriteLog("等待进程完成...");
                await Task.Run(() => process.WaitForExit());

                WriteLog($"进程完成，退出代码: {process.ExitCode}");

                if (process.ExitCode == 0)
                {
                    WriteLog($"{stepDescription} 执行成功");
                    return "Success";
                }
                else
                {
                    string error = $"{stepDescription}失败：程序返回错误代码 {process.ExitCode}";
                    WriteLog(error);
                    if (errorBuilder.Length > 0)
                    {
                        WriteLog($"错误详情: {errorBuilder}");
                    }
                    return error;
                }
            }
            catch (Exception ex)
            {
                string error = $"{stepDescription}执行异常: {ex.Message}";
                WriteLog(error);
                WriteLog($"异常详情: {ex}");
                return error;
            }
        }

        [RelayCommand]
        private async Task Rename()
        {
            WriteLog("=== 开始重命名操作 ===");

            string projectPath = InputPath;
            string newName = OutputPath;

            try
            {
                // 1. 执行主要重命名操作（项目或插件）
                string arguments;
                if (IsProjectSelected)
                {
                    // 重命名项目：rename-project --project <PROJECT> --new-name <NEW_NAME>
                    arguments = $"rename-project --project \"{projectPath}\" --new-name \"{newName}\"";
                    WriteLog("执行项目重命名操作");
                }
                else
                {
                    // 重命名插件：rename-plugin --project <PROJECT> --plugin <PLUGIN> --new-name <NEW_NAME>             
                    string pluginName = Path.GetFileName(projectPath);
                    arguments = $"rename-plugin --project \"{projectPath}\" --plugin \"{pluginName}\" --new-name \"{newName}\"";
                    WriteLog("执行插件重命名操作");
                }

                Message = "正在执行重命名操作...";
                MessageVisibility = Visibility.Visible;

                string result = await ExecuteRenomCommand(arguments, "主要重命名操作", projectPath);

                if (result != "Success")
                {
                    Message = result;
                    WriteLog($"主要重命名操作失败: {result}");
                    SoundFX.PlaySound(1);
                    return;
                }

                WriteLog("主要重命名操作成功完成");

                // 2. 如果是C++项目，需要额外重命名模块和target
                if (IsProjectSelected && !IsBPSelected)
                {
                    WriteLog("检测到C++项目，开始额外重命名步骤");

                    // 先保存原项目名称，再更新项目路径
                    string originalProjectName = Path.GetFileName(projectPath);
                    WriteLog($"原始项目名称: {originalProjectName}");

                    // 更新项目路径为重命名后的新路径
                    string newProjectPath = Path.Combine(Path.GetDirectoryName(projectPath), newName);
                    WriteLog($"更新后的项目路径: {newProjectPath}");
                    projectPath = newProjectPath;

                    // 2.1 重命名模块
                    Message = "正在重命名模块...";
                    WriteLog("开始重命名模块操作");

                    string moduleArguments = $"rename-module --project \"{projectPath}\" --module \"{originalProjectName}\" --new-name \"{newName}\"";
                    string moduleResult = await ExecuteRenomCommand(moduleArguments, "模块重命名", projectPath);

                    if (moduleResult != "Success")
                    {
                        Message = moduleResult;
                        WriteLog($"模块重命名失败: {moduleResult}");
                        SoundFX.PlaySound(1);
                        return;
                    }

                    WriteLog("模块重命名成功完成");

                    // 2.2 重命名第一个target (<TARGET_NAME>)
                    Message = "正在重命名target...";
                    WriteLog("开始重命名第一个target");

                    string target1Arguments = $"rename-target --project \"{projectPath}\" --target \"{originalProjectName}\" --new-name \"{newName}\"";
                    string target1Result = await ExecuteRenomCommand(target1Arguments, "第一个target重命名", projectPath);

                    if (target1Result != "Success")
                    {
                        Message = target1Result;
                        WriteLog($"第一个target重命名失败: {target1Result}");
                        SoundFX.PlaySound(1);
                        return;
                    }

                    WriteLog("第一个target重命名成功完成");

                    // 2.3 重命名第二个target (<TARGET_NAME>Editor)
                    WriteLog("开始重命名第二个target (Editor)");

                    string target2Arguments = $"rename-target --project \"{projectPath}\" --target \"{originalProjectName}Editor\" --new-name \"{newName}Editor\"";
                    string target2Result = await ExecuteRenomCommand(target2Arguments, "第二个target重命名", projectPath);

                    if (target2Result != "Success")
                    {
                        Message = target2Result;
                        WriteLog($"第二个target重命名失败: {target2Result}");
                        SoundFX.PlaySound(1);
                        return;
                    }

                    WriteLog("第二个target重命名成功完成");
                }

                // 3. 所有操作成功
                Message = "重命名成功！";
                MessageVisibility = Visibility.Visible;
                WriteLog("=== 所有重命名操作成功完成 ===");

                // 4. 检查AutoOpen设置来决定是否打开文件夹
                if (Properties.Settings.Default.AutoOpen)
                {
                    WriteLog("AutoOpen功能已启用，准备打开文件夹");

                    // 对于C++项目，使用更新后的路径
                    // 对于非C++项目，需要重新计算新路径
                    string finalPath;
                    if (IsProjectSelected && !IsBPSelected)
                    {
                        // C++项目：路径已经在上面的处理中更新过了
                        finalPath = projectPath;
                        WriteLog($"C++项目使用更新后的路径: {finalPath}");
                    }
                    else
                    {
                        // 非C++项目或非项目类型：需要重新计算路径
                        finalPath = Path.Combine(Path.GetDirectoryName(projectPath), newName);
                        InputPath = finalPath;
                        WriteLog($"非C++项目重新计算的路径: {finalPath}");
                    }

                    SoundFX.PlaySound(1);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = finalPath,
                        UseShellExecute = true
                    });

                    WriteLog($"已打开文件夹: {finalPath}");
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"重命名失败：{ex.Message}";
                Message = errorMessage;
                MessageVisibility = Visibility.Visible;
                WriteLog($"异常错误: {errorMessage}");
                WriteLog($"异常详情: {ex}");
                SoundFX.PlaySound(1);
            }
        }

        private bool CanRename()
        {
            return !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath);
        }

        partial void OnInputPathChanged(string value)
        {
            RenameCommand.NotifyCanExecuteChanged();
        }

        partial void OnOutputPathChanged(string value)
        {
            RenameCommand.NotifyCanExecuteChanged();
        }
    }
}