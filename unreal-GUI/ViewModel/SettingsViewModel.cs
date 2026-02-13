using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using unreal_GUI.Model;
using unreal_GUI.Model.Basic;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace unreal_GUI.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<EngineInfo> _engineInfos = [];

        [ObservableProperty]
        private List<string> _enginePathsDisplay = [];

        [ObservableProperty]
        private string _tipText = "";

        [ObservableProperty]
        private bool _autoOpen;

        [ObservableProperty]
        private bool _gitcode;


        [ObservableProperty]
        private bool _autoUpdate;


        [ObservableProperty]
        private bool _fabNotification;

        [ObservableProperty]
        private DateTime _limitedTime;

        [ObservableProperty]
        private bool _autoStart;

        [ObservableProperty]
        private bool _openEpic;

        [ObservableProperty]
        private bool _headlessEnabled;

        [ObservableProperty]
        private bool _advancedMode;

        [ObservableProperty]
        private byte _backdropType;

        [ObservableProperty]
        private byte _aminateType;

        public SettingsViewModel()
        {
            // 初始化设置
            AutoOpen = Properties.Settings.Default.AutoOpen;
            Gitcode = Properties.Settings.Default.Gitcode;
            AutoUpdate = Properties.Settings.Default.AutoUpdate;
            FabNotification = Properties.Settings.Default.FabNotificationEnabled;
            // 从INI文件读取LimitedTime
            IniConfig iniConfig = new();
            LimitedTime = iniConfig.ReadDateTime("LimitedTime", Properties.Settings.Default.LimitedTime);
            AutoStart = Properties.Settings.Default.AutoStart;
            OpenEpic = Properties.Settings.Default.OpenEpic;
            HeadlessEnabled = Properties.Settings.Default.HeadlessEnabled;
            AdvancedMode = Properties.Settings.Default.AdvancedMode;
            BackdropType = Properties.Settings.Default.BackdropType;
            AminateType = Properties.Settings.Default.AminateType;

            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    EngineInfos = settings.Engines ?? [];
                    UpdateEnginePathsDisplay();
                }
                catch
                {
                    MessageBox.Show("JSON文件已损坏，请删除后再重新启动应用程序", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        [RelayCommand]
        private void AddEnginePath()
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                EngineInfos.Add(new EngineInfo { Path = folderDialog.FolderName, Version = GetEngineVersion(folderDialog.FolderName) });
                UpdateEnginePathsDisplay();
            }
        }

        [RelayCommand]
        private void RemoveEnginePath(string selectedPathDisplay)
        {
            if (!string.IsNullOrEmpty(selectedPathDisplay))
            {
                var selectedPath = selectedPathDisplay.Split('(')[0].Trim();
                EngineInfos.RemoveAll(x => x.Path == selectedPath);
                UpdateEnginePathsDisplay();
            }
        }

        [RelayCommand]
        private void AutoPath()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EpicGames\\Unreal Engine");
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        var path = subKey?.GetValue("InstalledDirectory") as string;
                        if (!string.IsNullOrEmpty(path) && !EngineInfos.Any(x => x.Path == path))
                        {
                            EngineInfos.Add(new EngineInfo { Path = path, Version = GetEngineVersion(path) });
                        }
                    }
                    UpdateEnginePathsDisplay();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("未检测到引擎，请手动设置引擎目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        [RelayCommand]
        private Task SaveSettings(JsonSerializerOptions options)
        {
            // 保存应用程序设置
            Properties.Settings.Default.AutoOpen = AutoOpen;
            Properties.Settings.Default.Gitcode = Gitcode;
            Properties.Settings.Default.AutoUpdate = AutoUpdate;
            Properties.Settings.Default.FabNotificationEnabled = FabNotification;
            Properties.Settings.Default.LimitedTime = LimitedTime;
            Properties.Settings.Default.AutoStart = AutoStart;
            Properties.Settings.Default.OpenEpic = OpenEpic;
            Properties.Settings.Default.HeadlessEnabled = HeadlessEnabled;
            Properties.Settings.Default.AdvancedMode = AdvancedMode;
            Properties.Settings.Default.BackdropType = BackdropType;
            Properties.Settings.Default.AminateType = AminateType;

            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.AutoStart)
            {
                // 设置开机自启
                unreal_GUI.Model.Features.AutoStart.SetAutoStart(AutoStart);
            }

            // 保存JSON文件
            SettingsData settings = new()
            {
                Engines = EngineInfos,
                CustomButtons = []
            };
            // 保存ini文件
            new IniConfig().CreateConfig();

            // 读取现有的自定义按钮数据
            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var existingSettings = JsonSerializer.Deserialize<SettingsData>(json);
                    settings.CustomButtons = existingSettings?.CustomButtons ?? [];
                }
                catch
                {
                    MessageBox.Show("JSON文件已损坏，请删除后再重新启动应用程序", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            File.WriteAllText("settings.json", JsonSerializer.Serialize(settings, options));

            TipText = "设置已保存";
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void OpenConfigFolder()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                var configPath = config.FilePath;
                
                if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                {
                    var configDir = Path.GetDirectoryName(configPath);
                    if (!string.IsNullOrEmpty(configDir))
                    {
                        var parentDir = Directory.GetParent(configDir);
                        if (parentDir != null)
                        {
                            var companyDir = parentDir.Parent;
                            if (companyDir != null && Directory.Exists(companyDir.FullName))
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = companyDir.FullName,
                                    UseShellExecute = true
                                });
                                return;
                            }
                        }
                    }
                }
                MessageBox.Show("未找到配置文件夹", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开配置文件夹失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateEnginePathsDisplay()
        {
            EnginePathsDisplay = [.. EngineInfos.Select(p => $"{p.Path} ({p.Version})")];
        }

        private static string GetEngineVersion(string enginePath)
        {
            try
            {
                // 尝试从路径中提取版本号
                var dirName = Path.GetFileName(enginePath.TrimEnd(Path.DirectorySeparatorChar));
                if (dirName.StartsWith("UE_"))
                {
                    return dirName.Substring(3);
                }

                // 尝试读取.version文件
                var versionFile = Path.Combine(enginePath, "Engine", "Build", "Build.version");
                if (File.Exists(versionFile))
                {
                    var versionJson = File.ReadAllText(versionFile);
                    using var versionDoc = JsonDocument.Parse(versionJson);
                    var root = versionDoc.RootElement;
                    var majorVersion = root.GetProperty("MajorVersion").GetInt32();
                    var minorVersion = root.GetProperty("MinorVersion").GetInt32();
                    var patchVersion = root.GetProperty("PatchVersion").GetInt32();
                    return $"{majorVersion}.{minorVersion}.{patchVersion}";
                }
            }
            catch { }
            return "未知版本";
        }
    }
}