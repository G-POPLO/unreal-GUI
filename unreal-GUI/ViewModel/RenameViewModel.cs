using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private async Task SelectFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择文件夹"
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                if (ValidatePath(dialog.SelectedPath, IsProjectSelected))
                {
                    InputPath = dialog.SelectedPath;
                }
                else
                {
                    string extension = IsProjectSelected ? ".uproject" : ".uplugin";
                    await ModernDialog.ShowErrorAsync($"所选路径根目录不存在{extension}文件", "路径无效");
                    InputPath = string.Empty;
                }
            }
        }

        [RelayCommand]
        private static void OpenWebLink()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/UnrealisticDev/Renom/issues/18",
                UseShellExecute = true
            });
        }

        [RelayCommand(CanExecute = nameof(CanRename))]
        private async Task Rename()
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

                // 对于C++项目，在执行重命名前提示用户备份
                if (IsProjectSelected && !IsBPSelected)
                {
                    await ModernDialog.ShowInfoAsync("在进行C++项目重命名前，请务必备份您的项目文件以防止数据丢失。", "备份提醒");
                }

                Message = "正在执行重命名操作...";


                var process = Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit(); // 等待重命名进程完成

                    if (process.ExitCode == 0)
                    {
                        // 如果是C++项目，需要额外重命名模块
                        if (IsProjectSelected && !IsBPSelected)
                        {
                            // 更新项目路径为重命名后的新路径
                            projectPath = Path.Combine(Path.GetDirectoryName(projectPath), newName);
                            string originalProjectName = Path.GetFileName(projectPath);

                            //    // 1. 重命名模块
                            //    string moduleArguments = $"rename-module --project \"{projectPath}\" --module \"{originalProjectName}\" --new-name \"{newName}\"";

                            //    var moduleProcessInfo = new ProcessStartInfo
                            //    {
                            //        FileName = exePath,
                            //        Arguments = moduleArguments,
                            //        UseShellExecute = true,
                            //        CreateNoWindow = false,
                            //        WorkingDirectory = Path.GetDirectoryName(projectPath)
                            //    };

                            //    Message = "正在重命名模块...";


                            //    var moduleProcess = Process.Start(moduleProcessInfo);
                            //    if (moduleProcess != null)
                            //    {
                            //        moduleProcess.WaitForExit();
                            //        if (moduleProcess.ExitCode == 0)
                            //        {
                            //            // 2. 重命名target
                            //            Message = "正在重命名target...";


                            //            // 重命名第一个target (<TARGET_NAME>)
                            //            string target1Arguments = $"rename-target --project \"{projectPath}\" --target \"{originalProjectName}\" --new-name \"{newName}\"";
                            //            var target1ProcessInfo = new ProcessStartInfo
                            //            {
                            //                FileName = exePath,
                            //                Arguments = target1Arguments,
                            //                UseShellExecute = true,
                            //                CreateNoWindow = false,
                            //                WorkingDirectory = Path.GetDirectoryName(projectPath)
                            //            };

                            //            var target1Process = Process.Start(target1ProcessInfo);
                            //            if (target1Process != null)
                            //            {
                            //                target1Process.WaitForExit();
                            //                if (target1Process.ExitCode == 0)
                            //                {
                            //                    // 重命名第二个target (<TARGET_NAME>Editor)
                            //                    string target2Arguments = $"rename-target --project \"{projectPath}\" --target \"{originalProjectName}Editor\" --new-name \"{newName}Editor\"";
                            //                    var target2ProcessInfo = new ProcessStartInfo
                            //                    {
                            //                        FileName = exePath,
                            //                        Arguments = target2Arguments,
                            //                        UseShellExecute = true,
                            //                        CreateNoWindow = false,
                            //                        WorkingDirectory = Path.GetDirectoryName(projectPath)
                            //                    };

                            //                    var target2Process = Process.Start(target2ProcessInfo);
                            //                    if (target2Process != null)
                            //                    {
                            //                        target2Process.WaitForExit();
                            //                        Message = target2Process.ExitCode == 0 ? "重命名成功！" : $"Editor target重命名失败：程序返回错误代码 {target2Process.ExitCode}";
                            //                    }
                            //                    else
                            //                    {
                            //                        Message = "Editor target重命名失败：无法启动Editor target重命名进程";
                            //                    }
                            //                }
                            //                else
                            //                {
                            //                    Message = $"target重命名失败：程序返回错误代码 {target1Process.ExitCode}";
                            //                }
                            //            }
                            //            else
                            //            {
                            //                Message = "target重命名失败：无法启动target重命名进程";
                            //            }
                            //        }
                            //        else
                            //        {
                            //            Message = $"模块重命名失败：程序返回错误代码 {moduleProcess.ExitCode}";
                            //        }
                            //    }
                            //    else
                            //    {
                            //        Message = "模块重命名失败：无法启动模块重命名进程";
                            //    }
                            //}
                            //else
                            //{
                            //    Message = "重命名成功！";
                            //    SoundFX.PlaySound(0);
                        }

                        // 检查AutoOpen设置来决定是否打开文件夹
                        if (Properties.Settings.Default.AutoOpen)
                        {
                            // 对于C++项目，使用更新后的路径
                            // 对于非C++项目，需要重新计算新路径
                            string finalPath;
                            if (IsProjectSelected && !IsBPSelected)
                            {
                                // C++项目：路径已经在上面的处理中更新过了
                                finalPath = projectPath;
                            }
                            else
                            {
                                // 非C++项目或非项目类型：需要重新计算路径
                                finalPath = Path.Combine(Path.GetDirectoryName(projectPath), newName);
                                InputPath = finalPath;
                            }

                            SoundFX.PlaySound(1);

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "explorer.exe",
                                Arguments = finalPath,
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


            }
            catch (Exception ex)
            {
                Message = $"重命名失败：{ex.Message}";

                SoundFX.PlaySound(1);
            }
        }

        private bool CanRename()
        {
            return !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath);
        }

        private static bool ValidatePath(string path, bool isProject)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return false;
            }

            string extension = isProject ? "*.uproject" : "*.uplugin";
            return Directory.EnumerateFiles(path, extension, SearchOption.TopDirectoryOnly).Any();
        }

        partial void OnInputPathChanged(string value)
        {
            RenameCommand.NotifyCanExecuteChanged();

            if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
            {
                _ = ValidateInputPathAsync(value);
            }
        }

        private async Task ValidateInputPathAsync(string path)
        {
            if (!ValidatePath(path, IsProjectSelected))
            {
                string extension = IsProjectSelected ? ".uproject" : ".uplugin";
                await ModernDialog.ShowErrorAsync($"所选路径根目录不存在{extension}文件", "路径无效");
                InputPath = string.Empty;
            }
        }

        partial void OnOutputPathChanged(string value)
        {

            RenameCommand.NotifyCanExecuteChanged();
        }
    }
}