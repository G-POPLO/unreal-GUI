using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

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

        [ObservableProperty]
        private bool _isRenameButtonEnabled = false;

        public RenameViewModel()
        {

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
        private void Rename()
        {
            string projectPath = InputPath;
            string newName = OutputPath;
            string command;

            if (IsProjectSelected)
            {
                command = $"renom rename-project --project {projectPath} --new-name {newName}";
            }
            else
            {
                command = $"renom rename-plugin --project {projectPath} --plugin {projectPath} --new-name {newName}";
            }

            try
            {
                string exePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "App", "renom.exe");
                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = command.Replace("renom ", ""),
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                var process = Process.Start(processInfo);
                process.WaitForExit(); // 等待重命名进程完成

                Message = "重命名成功！";
                MessageVisibility = Visibility.Visible;

                // 检查AutoOpen设置来决定是否打开文件夹
                if (Properties.Settings.Default.AutoOpen)
                {
                    // 更新输入框为新的路径
                    string newPath = Path.Combine(System.IO.Path.GetDirectoryName(projectPath), newName);
                    InputPath = newPath;
                    Message = "重命名成功！";
                    System.Media.SoundPlayer player = new(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                    
                    MessageVisibility = Visibility.Visible;
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
                Message = $"重命名失败：{ex.Message}";
                MessageVisibility = Visibility.Visible;
                var player = new System.Media.SoundPlayer(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                
            }
        }

        private bool CanRename()
        {
            return !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath);
        }

        partial void OnInputPathChanged(string value)
        {
            IsRenameButtonEnabled = CanRename();
            RenameCommand.NotifyCanExecuteChanged();
        }

        partial void OnOutputPathChanged(string value)
        {
            IsRenameButtonEnabled = CanRename();
            RenameCommand.NotifyCanExecuteChanged();
        }
    }
}