using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using unreal_GUI.Properties;

namespace unreal_GUI.ViewModel
{
    public partial class RenameViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _inputPath;
        
        [ObservableProperty]
        private string _outputName;
        
        [ObservableProperty]
        private string _message;
        
        [ObservableProperty]
        private bool _isProjectSelected = true;
        
        [RelayCommand]
        private void SelectFolder()
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                InputPath = folderBrowserDialog.SelectedPath;
            }
        }
        
        [RelayCommand]
        private void Rename()
        {
            try
            {
                string command = IsProjectSelected 
                    ? $"renom rename-project --project {InputPath} --new-name {OutputName}"
                    : $"renom rename-plugin --project {InputPath} --plugin {InputPath} --new-name {OutputName}";

                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App", "renom.exe");
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = command.Replace("renom ", ""),
                    UseShellExecute = true,
                    CreateNoWindow = true
                });

                Message = "重命名成功！";
     


                if (Properties.Settings.Default.AutoOpen)
                {
                    string originalPath = InputPath;
                    string newPath = Path.Combine(Path.GetDirectoryName(InputPath), OutputName);
                    
                    // 先执行重命名命令
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = command.Replace("renom ", ""),
                        UseShellExecute = true,
                        CreateNoWindow = true
                    });

                    // 等待重命名完成后再打开新路径
                    OnPropertyChanged(InputPath);
                    InputPath = newPath;
                    
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
            }
        }
    }
}