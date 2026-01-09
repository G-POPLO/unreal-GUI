using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using unreal_GUI.Model;
using unreal_GUI.Model.Basic;

namespace unreal_GUI.ViewModel
{
    public partial class ClearViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<EngineInfo> _engineList = [];

        [ObservableProperty]
        private string _inputPath = "";

        [ObservableProperty]
        private string _tipClearCache = "";

        [ObservableProperty]
        private string _tipClearLog = "";

        [ObservableProperty]
        private string _tipZen = "";

        [ObservableProperty]
        private bool _isCleanButtonEnabled = false;

        [ObservableProperty]
        private bool _isSaveChecked = false;

        [ObservableProperty]
        private bool _isDerivedDataCacheChecked = false;


        [ObservableProperty]
        private EngineInfo _selectedEngine;

        public ClearViewModel()
        {
            LoadEngineList();
        }

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    EngineList = settings?.Engines ?? [];
                }
                catch
                {
                    // 如果JSON文件损坏，初始化为空列表
                    _ = ModernDialog.ShowInfoAsync("JSON文件已损坏，已重置为默认状态");
                    EngineList = [];
                }
            }
        }


        [RelayCommand]
        private void SelectInputPath()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "选择需要清理的项目目录",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // 获取选择的文件夹路径
                InputPath = dialog.SelectedPath;
                TipClearCache = "工程路径已设置: " + InputPath;
                IsCleanButtonEnabled = !string.IsNullOrWhiteSpace(InputPath);
            }
        }

        [RelayCommand]
        private void CleanCache()
        {
            try
            {
                if (string.IsNullOrEmpty(InputPath))
                {
                    TipClearCache = "请先设置工程路径";
                    return;
                }

                try
                {
                    if (IsDerivedDataCacheChecked)
                        DeleteDirectoryIfExists(Path.Combine(InputPath, "DerivedDataCache"));
                }
                catch { }

                try
                {
                    if (IsSaveChecked)
                        DeleteDirectoryIfExists(Path.Combine(InputPath, "Saved", "SaveGames"));
                }
                catch { }

                DeleteDirectoryIfExists(Path.Combine(InputPath, "Binaries"));
                DeleteDirectoryIfExists(Path.Combine(InputPath, "Build"));
                DeleteDirectoryIfExists(Path.Combine(InputPath, "Intermediate"));
                DeleteDirectoryIfExists(Path.Combine(InputPath, ".vs"));

                foreach (var file in Directory.GetFiles(InputPath, "*.sln", SearchOption.TopDirectoryOnly))
                    File.Delete(file);
                File.Delete(Path.Combine(InputPath, ".vsconfig"));
                TipClearCache = "清理完毕";
                SoundFX.PlaySound(0);

                if (Properties.Settings.Default.AutoOpen)
                {
                    Process.Start("explorer.exe", Path.Combine(InputPath));
                }
            }
            catch (Exception ex)
            {
                TipClearCache = "清理失败: " + ex.Message;
                SoundFX.PlaySound(1);
            }
        }


        [RelayCommand]
        private void OpenZenDashboard()
        {
            try
            {
                if (SelectedEngine == null)
                {
                    TipZen = "请先从下拉框中选择引擎版本";
                    return;
                }

                string zenPath = Path.Combine(SelectedEngine.Path, "Engine\\Binaries\\Win64\\ZenDashboard.exe");
                if (File.Exists(zenPath))
                {
                    Process.Start(zenPath);
                }
                else
                {
                    _ = ModernDialog.ShowErrorAsync("ZenDashboard.exe未找到，请确认引擎安装", "路径错误");
                }
            }
            catch (Exception ex)
            {
                _ = ModernDialog.ShowErrorAsync($"打开失败：{ex.Message}", "错误提示");
            }
        }

        [RelayCommand]
        private static void OpenDocumentation()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://dev.epicgames.com/documentation/unreal-engine/zen-storage-server-for-unreal-engine",
                UseShellExecute = true
            });
        }

        private static void DeleteDirectoryIfExists(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        [RelayCommand]
        private void CleanLog()
        {
            try
            {
                // 清理 AutomationTool\Logs 目录下的所有内容
                string automationToolLogsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unreal Engine", "AutomationTool", "Logs");
                if (Directory.Exists(automationToolLogsPath))
                {
                    Directory.Delete(automationToolLogsPath, true);
                    // 重新创建空目录
                    Directory.CreateDirectory(automationToolLogsPath);
                }

                // 清理 Local\UnrealEngine 目录下的 XmlConfigCache.bin文件
                string localUnrealEnginePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UnrealEngine");
                if (Directory.Exists(localUnrealEnginePath))
                {
                    // 删除 .bin 文件
                    foreach (var file in Directory.GetFiles(localUnrealEnginePath, "*.bin", SearchOption.TopDirectoryOnly))
                    {
                        File.Delete(file);
                    }
                }

                TipClearLog = "Log清理完毕";
                SoundFX.PlaySound(0);

            }
            catch (Exception ex)
            {
                TipClearLog = "Log清理失败: " + ex.Message;
                SoundFX.PlaySound(1);

            }
        }

    }
}