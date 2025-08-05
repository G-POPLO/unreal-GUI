using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using unreal_GUI.Model;

namespace unreal_GUI.ViewModel
{
    public partial class ClearViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<SettingsViewModel.EngineInfo> _engineList = [];

        [ObservableProperty]
        private string _inputPath = "";

        [ObservableProperty]
        private string _tipText = "";

        [ObservableProperty]
        private string _tipText2 = "";

        [ObservableProperty]
        private string _dDCPath = "";

        [ObservableProperty]
        private string _dDCSharePath = "";

        [ObservableProperty]
        private string _totalSize = "";

        [ObservableProperty]
        private string _dDCErrorText = "";

        [ObservableProperty]
        private bool _isCleanButtonEnabled = false;

        [ObservableProperty]
        private bool _isOldDDCPanelVisible = false;

        [ObservableProperty]
        private bool _isZenPanelVisible = true;

        [ObservableProperty]
        private bool _isDerivedDataCacheChecked = false;

        [ObservableProperty]
        private bool _isSaveGameChecked = false;

        [ObservableProperty]
        private SettingsViewModel.EngineInfo _selectedEngine;

        public ClearViewModel()
        {
            // 初始化设置
            UpdatePanelVisibility();
            LoadEngineList();
            DDCPath = $"DDC全局缓存路径：{Properties.Settings.Default.DDC}";
            DDCSharePath = $"DDC共享缓存路径：{Properties.Settings.Default.DDCShare}";
            TotalSize = $"总计大小：{Properties.Settings.Default.DDCTotal:0.00} GB";
            
        }

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    EngineList = JsonConvert.DeserializeObject<List<SettingsViewModel.EngineInfo>>(File.ReadAllText("settings.json"));
                }
                catch
                {
                    // 如果JSON文件损坏，初始化为空列表
                    EngineList = [];
                }
            }
        }

        private void UpdatePanelVisibility()
        {
            IsOldDDCPanelVisible = !Properties.Settings.Default.ZenDashborad;
            IsZenPanelVisible = Properties.Settings.Default.ZenDashborad;
        }

        [RelayCommand]
        private void SelectInputPath()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InputPath = dialog.SelectedPath;
                TipText = "工程路径已设置: " + dialog.SelectedPath;
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
                    TipText = "请先设置工程路径";
                    return;
                }

                try
                {
                    if (!IsDerivedDataCacheChecked)
                        DeleteDirectoryIfExists(Path.Combine(InputPath, "DerivedDataCache"));
                }
                catch { }

                try
                {
                    if (!IsSaveGameChecked)
                        DeleteDirectoryIfExists(Path.Combine(InputPath, "Saved", "SaveGames"));
                }
                catch { }

                DeleteDirectoryIfExists(Path.Combine(InputPath, "Binaries"));
                DeleteDirectoryIfExists(Path.Combine(InputPath, "Build"));
                DeleteDirectoryIfExists(Path.Combine(InputPath, "Intermediate"));

                foreach (var file in Directory.GetFiles(InputPath, "*.sln", SearchOption.TopDirectoryOnly))
                    File.Delete(file);

                TipText = "清理完毕";
                System.Media.SoundPlayer player = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                player.Play();
                if (Properties.Settings.Default.AutoOpen)
                {
                    Process.Start("explorer.exe", Path.Combine(InputPath));
                }
            }
            catch (Exception ex)
            {
                TipText = "清理失败: " + ex.Message;
                System.Media.SoundPlayer player = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }

        [RelayCommand]
        private void FindDDCPaths()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Epic Games\\GlobalDataCachePath");
                var localPath = key?.GetValue("UE-LocalDataCachePath")?.ToString();
                var sharedPath = key?.GetValue("UE-SharedDataCachePath")?.ToString();

                if (!string.IsNullOrEmpty(localPath))
                {
                    DDCPath = $"DDC全局缓存路径：{localPath}";
                    Properties.Settings.Default.DDC = localPath;
                }

                if (!string.IsNullOrEmpty(sharedPath))
                {
                    DDCSharePath = $"DDC共享缓存路径：{sharedPath}";
                    Properties.Settings.Default.DDCShare = sharedPath;
                }

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _ = ModernDialog.ShowConfirmAsync($"找不到DDC缓存路径：" + ex.Message, "提示");
            }
        }

        [RelayCommand]
        private async Task CalculateTotalSize()
        {
            try
            {
                var ddcPath = Properties.Settings.Default.DDC;
                var sharePath = Properties.Settings.Default.DDCShare;

                if (string.IsNullOrEmpty(ddcPath) || string.IsNullOrEmpty(sharePath))
                {
                    throw new Exception("请先获取DDC缓存路径");
                }

                float totalSize = await Task.Run(() => CalculateDirectorySize(ddcPath) + CalculateDirectorySize(sharePath));
                Properties.Settings.Default.DDCTotal = totalSize;
                TotalSize = $"总计大小：{totalSize:0.00} GB";
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync("计算失败: " + ex.Message, "错误提示");
                TotalSize = $"总计大小：{Properties.Settings.Default.DDCTotal:0.00} GB";
            }
        }

        [RelayCommand]
        private void OpenZenDashboard()
        {
            try
            {
                if (SelectedEngine == null)
                {
                    DDCErrorText = "请先从下拉框中选择引擎版本";
                    return;
                }

                var zenPath = Path.Combine(SelectedEngine.Path, "Engine\\Binaries\\Win64\\ZenDashboard.exe");
                if (File.Exists(zenPath))
                {
                    Process.Start(zenPath);
                }
                else
                {
                    _ = ModernDialog.ShowInfoAsync("ZenDashboard.exe未找到，请确认引擎安装", "路径错误");
                }
            }
            catch (Exception ex)
            {
                _ = ModernDialog.ShowInfoAsync($"打开失败：{ex.Message}", "错误提示");
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

        private static float CalculateDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;

            float size = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    var info = new FileInfo(file);
                    size += info.Length;
                }
            }
            catch { }
            return size / (1024 * 1024 * 1024); // 转换为GB
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

                TipText2 = "Log清理完毕";
                System.Media.SoundPlayer player = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                player.Play();
            }
            catch (Exception ex)
            {
                TipText2 = "Log清理失败: " + ex.Message;
                System.Media.SoundPlayer player = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }

        partial void OnInputPathChanged(string value)
        {
            IsCleanButtonEnabled = !string.IsNullOrWhiteSpace(value);
        }
    }
}