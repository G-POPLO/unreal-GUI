using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace unreal_GUI.ViewModel
{
    public class RenameViewModel : INotifyPropertyChanged
    {
        private string _inputPath;
        private string _outputPath;
        private bool _isProjectSelected = true;
        private string _message;
        private Visibility _messageVisibility = Visibility.Hidden;
        private bool _canRename = false;

        public string InputPath
        {
            get => _inputPath;
            set
            {
                _inputPath = value;
                OnPropertyChanged(nameof(InputPath));
                CheckCanRename();
            }
        }

        public string OutputPath
        {
            get => _outputPath;
            set
            {
                _outputPath = value;
                OnPropertyChanged(nameof(OutputPath));
                CheckCanRename();
            }
        }

        public bool CanRename
        {
            get => _canRename;
            set
            {
                _canRename = value;
                OnPropertyChanged(nameof(CanRename));
            }
        }

        public bool IsProjectSelected
        {
            get => _isProjectSelected;
            set
            {
                _isProjectSelected = value;
                OnPropertyChanged(nameof(IsProjectSelected));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        public Visibility MessageVisibility
        {
            get => _messageVisibility;
            set
            {
                _messageVisibility = value;
                OnPropertyChanged(nameof(MessageVisibility));
            }
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand RenameCommand { get; }

        public RenameViewModel()
        {
            SelectFolderCommand = new RelayCommand(SelectFolder);
            RenameCommand = new RelayCommand(Rename, CanRename);
        }

        private void SelectFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InputPath = dialog.SelectedPath;
            }
        }

        private bool CanRename()
        {
            return !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath);
        }

        private void CheckCanRename()
        {
            CanRename = !string.IsNullOrEmpty(InputPath) && !string.IsNullOrEmpty(OutputPath);
            if (RenameCommand is RelayCommand relayCommand)
            {
                relayCommand.NotifyCanExecuteChanged();
            }
        }

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

                Message = "重命名成功！";
                MessageVisibility = Visibility.Visible;

                // 检查AutoOpen设置来决定是否打开文件夹
                if (Properties.Settings.Default.AutoOpen)
                {
                    // 更新输入框为新的路径
                    string newPath = Path.Combine(System.IO.Path.GetDirectoryName(projectPath), newName);
                    InputPath = newPath;
                    Message = "重命名成功！";
                    System.Media.SoundPlayer player = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                    player.Play();
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
                var player = new System.Media.SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}