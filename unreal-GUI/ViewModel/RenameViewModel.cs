using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
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
        private string _message;

        [ObservableProperty]
        private Visibility _messageVisibility = Visibility.Hidden;



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
        private void Rename()
        {
            string projectPath = InputPath;
            string newName = OutputPath;
            string arguments;

            if (IsProjectSelected)
            {
                // 重命名项目：rename-project --project <PROJECT> --new-name <NEW_NAME>
                arguments = $"rename-project --project \"{projectPath}\" --new-name \"{newName}\"";
            }
            else
            {
                // 重命名插件：rename-plugin --project <PROJECT> --plugin <PLUGIN> --new-name <NEW_NAME>             
                string pluginName = Path.GetFileName(projectPath);
                arguments = $"rename-plugin --project \"{projectPath}\" --plugin \"{pluginName}\" --new-name \"{newName}\"";
            }

            try
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App", "renom.exe");

                // 检查renom.exe是否存在
                if (!File.Exists(exePath))
                {
                    Message = $"错误：找不到renom.exe工具文件";
                    MessageVisibility = Visibility.Visible;
                    return;
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WorkingDirectory = Path.GetDirectoryName(projectPath)
                };

                Message = "正在执行重命名操作...";
                MessageVisibility = Visibility.Visible;

                var process = Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit(); // 等待重命名进程完成

                    if (process.ExitCode == 0)
                    {
                        Message = "重命名成功！";

                        // 检查AutoOpen设置来决定是否打开文件夹
                        if (Properties.Settings.Default.AutoOpen)
                        {
                            // 更新输入框为新的路径
                            string newPath = Path.Combine(Path.GetDirectoryName(projectPath), newName);
                            InputPath = newPath;

                            SoundFX.PlaySound(1);


                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = newPath,
                                UseShellExecute = true
                            });
                        }
                    }
                    else
                    {
                        Message = $"重命名失败：程序返回错误代码 {process.ExitCode}";
                    }
                }
                else
                {
                    Message = "重命名失败：无法启动重命名进程";
                }

                MessageVisibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Message = $"重命名失败：{ex.Message}";
                MessageVisibility = Visibility.Visible;
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