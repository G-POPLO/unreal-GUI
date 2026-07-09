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
        public partial List<EngineInfo> EngineInfos { get; set; } = [];

        [ObservableProperty]
        public partial List<string> EnginePathsDisplay { get; set; } = [];

        [ObservableProperty]
        public partial string TipText { get; set; } = "";

        [ObservableProperty]
        public partial bool AutoOpen { get; set; }

        [ObservableProperty]
        public partial bool NonGithub { get; set; }

        [ObservableProperty]
        public partial bool AutoUpdate { get; set; }


        [ObservableProperty]
        public partial bool AutoClaimEnabled { get; set; }
        [ObservableProperty]
        public partial bool FabNotification { get; set; }

        [ObservableProperty]
        public partial DateTime LimitedTime { get; set; }

        [ObservableProperty]
        public partial bool AutoStart { get; set; }

        [ObservableProperty]
        public partial bool OpenEpic { get; set; }

        [ObservableProperty]
        public partial bool HeadlessEnabled { get; set; }

        [ObservableProperty]
        public partial bool AdvancedMode { get; set; }

        [ObservableProperty]
        public partial byte BackdropType { get; set; }

        [ObservableProperty]
        public partial byte AminateType { get; set; }

        [ObservableProperty]
        public partial byte BrowerType { get; set; }

        [ObservableProperty]
        public partial bool HasUsingPro { get; set; }

        [ObservableProperty]
        public partial bool RememberWindowSize { get; set; }

        public SettingsViewModel()
        {
            // 初始化设置
            AutoOpen = Properties.Settings.Default.AutoOpen;
            NonGithub = Properties.Settings.Default.NonGithub;
            AutoUpdate = Properties.Settings.Default.AutoUpdate;
            AutoClaimEnabled = Properties.Settings.Default.AutoClaimEnabled;
            // 从INI文件读取LimitedTime
            IniConfig iniConfig = new();
            LimitedTime = iniConfig.ReadDateTime("LimitedTime", Properties.Settings.Default.LimitedTime);
            AutoStart = Properties.Settings.Default.AutoStart;
            OpenEpic = Properties.Settings.Default.OpenEpic;
            HeadlessEnabled = Properties.Settings.Default.HeadlessEnabled;
            AdvancedMode = Properties.Settings.Default.AdvancedMode;
            BackdropType = Properties.Settings.Default.BackdropType;
            AminateType = Properties.Settings.Default.AminateType;
            BrowerType = Properties.Settings.Default.BrowerType;
            HasUsingPro = Properties.Settings.Default.HasUsingPro;
            RememberWindowSize = Properties.Settings.Default.RememberWindowSize;

            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    EngineInfos = settings.Engines;
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
                var launcherDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Epic", "UnrealEngineLauncher", "LauncherInstalled.dat");

                if (!File.Exists(launcherDataPath))
                {
                    MessageBox.Show("未找到LauncherInstalled.dat文件，请手动设置引擎目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var json = File.ReadAllText(launcherDataPath);
                var data = JsonSerializer.Deserialize<LauncherInstalledData>(json);

                if (data?.InstallationList == null) return;

                foreach (var entry in data.InstallationList)
                {
                    if (entry.ArtifactId?.StartsWith("UE_") == true)
                    {
                        var version = entry.ArtifactId["UE_".Length..];
                        if (!string.IsNullOrEmpty(entry.InstallLocation) && !EngineInfos.Any(x => x.Path == entry.InstallLocation))
                        {
                            EngineInfos.Add(new EngineInfo { Path = entry.InstallLocation, Version = version });
                        }
                    }
                }
                UpdateEnginePathsDisplay();
            }
            catch (Exception)
            {
                MessageBox.Show("读取引擎列表失败，请手动设置引擎目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        [RelayCommand]
        private Task SaveSettings(JsonSerializerOptions options)
        {
            // 保存应用程序设置
            Properties.Settings.Default.AutoOpen = AutoOpen;
            Properties.Settings.Default.NonGithub = NonGithub;
            Properties.Settings.Default.AutoUpdate = AutoUpdate;
            Properties.Settings.Default.AutoClaimEnabled = AutoClaimEnabled;
            Properties.Settings.Default.LimitedTime = LimitedTime;
            Properties.Settings.Default.AutoStart = AutoStart;
            Properties.Settings.Default.OpenEpic = OpenEpic;
            Properties.Settings.Default.HeadlessEnabled = HeadlessEnabled;
            Properties.Settings.Default.AdvancedMode = AdvancedMode;
            Properties.Settings.Default.BackdropType = BackdropType;
            Properties.Settings.Default.AminateType = AminateType;
            Properties.Settings.Default.BrowerType = BrowerType;
            Properties.Settings.Default.HasUsingPro = HasUsingPro;
            Properties.Settings.Default.RememberWindowSize = RememberWindowSize;

            Properties.Settings.Default.Save();

            // 设置或取消开机自启
            unreal_GUI.Model.Features.AutoStart.SetAutoStart(AutoStart);

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
            SoundFX.PlaySound(0);
            return Task.CompletedTask;
        }

        [RelayCommand]
        private static void OpenConfigFolder()
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

        [RelayCommand]
        private static void OpenLoginDataFolder()
        {
            try
            {
                string loginDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "UnrealGUI", "FabBrowserData");

                if (Directory.Exists(loginDataDir))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = loginDataDir,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("登录凭证文件夹尚未创建，请先运行 Fab 自动领取功能", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开登录凭证文件夹失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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