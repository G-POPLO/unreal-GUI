using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using unreal_GUI.Model;
using unreal_GUI.Model.Basic;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace unreal_GUI.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial ObservableCollection<EngineInfo> EngineInfos { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<string> EnginePathsDisplay { get; set; } = [];

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

        [ObservableProperty]
        public partial bool PatchUpdate { get; set; }

        [ObservableProperty]
        public partial string DefaultOutputPath { get; set; } = string.Empty;


        private static readonly string SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

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
            PatchUpdate = Properties.Settings.Default.PatchUpdate;

            // 订阅集合变化事件，自动同步显示列表
            EngineInfos.CollectionChanged += OnEngineInfosChanged;

            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<SettingsData>(json);
                    // 防御 JSON 内容为字面量 "null" 时反序列化返回 null
                    if (settings == null)
                    {
                        NotifyCorruptJson("settings.json 文件内容无效，将使用默认设置");
                        return;
                    }
                    // 逐项添加触发 OnEngineInfosChanged，避免替换集合导致订阅丢失
                    if (settings.Engines != null)
                    {
                        foreach (var engine in settings.Engines)
                        {
                            EngineInfos.Add(engine);
                        }
                    }
                    DefaultOutputPath = settings.DefaultOutputPath ?? string.Empty;
                }
                catch
                {
                    NotifyCorruptJson();
                }
            }
        }

        [RelayCommand]
        private void AddEnginePath()
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                // 防止重复添加同一引擎路径
                if (EngineInfos.Any(x => x.Path == folderDialog.FolderName))
                {
                    MessageBox.Show("该引擎路径已存在", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                EngineInfos.Add(new EngineInfo { Path = folderDialog.FolderName, Version = GetEngineVersion(folderDialog.FolderName) });
            }
        }

        [RelayCommand]
        private void RemoveEnginePath(string selectedPathDisplay)
        {
            if (!string.IsNullOrEmpty(selectedPathDisplay))
            {
                // 显示格式为 "{Path} ({Version})"，从末尾定位版本号分隔符，避免路径本身含 '(' 时被截断
                var separatorIndex = selectedPathDisplay.LastIndexOf(" (", StringComparison.Ordinal);
                var selectedPath = separatorIndex > 0
                    ? selectedPathDisplay[..separatorIndex]
                    : selectedPathDisplay;
                EngineInfos.Remove(EngineInfos.FirstOrDefault(x => x.Path == selectedPath)!);
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
            }
            catch (Exception)
            {
                MessageBox.Show("读取引擎列表失败，请手动设置引擎目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void BrowseDefaultOutput()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择默认输出文件夹",
                UseDescriptionForTitle = true
            };

            // 如果已有默认路径，设置为初始目录
            if (!string.IsNullOrWhiteSpace(DefaultOutputPath) && Directory.Exists(DefaultOutputPath))
            {
                dialog.SelectedPath = DefaultOutputPath;
            }

            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                DefaultOutputPath = dialog.SelectedPath;
            }
        }

        [RelayCommand]
        private void ClearDefaultOutput()
        {
            DefaultOutputPath = string.Empty;
        }


        [RelayCommand]
        private Task SaveSettings()
        {
            // 保存应用程序设置
            ApplyToSettings();
            Properties.Settings.Default.Save();

            // 设置或取消开机自启
            unreal_GUI.Model.Features.AutoStart.SetAutoStart(AutoStart);

            // 保存ini文件
            new IniConfig().CreateConfig();
            // LimitedTime 以 INI 为唯一真实来源，独立写入避免依赖 Properties.Settings.Default
            new IniConfig().WriteDateTime("LimitedTime", LimitedTime);

            // 保存JSON文件
            SettingsData settings = new()
            {
                Engines = [.. EngineInfos],
                CustomButtons = [],
                DefaultOutputPath = DefaultOutputPath
            };

            // 读取现有的自定义按钮数据
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var existingSettings = JsonSerializer.Deserialize<SettingsData>(json);
                    settings.CustomButtons = existingSettings?.CustomButtons ?? [];
                }
                catch
                {
                    NotifyCorruptJson();
                }
            }

            // 写入 settings.json，使用 try-catch 避免文件被占用/权限不足时崩溃
            try
            {
                var writeOptions = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings, writeOptions));
                ShowTip("设置已保存");
                SoundFX.PlaySound(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowTip("保存失败");
            }
            return Task.CompletedTask;
        }

        // 将 ViewModel 上的各偏好属性批量回写到 Properties.Settings.Default
        private void ApplyToSettings()
        {
            var s = Properties.Settings.Default;
            s.AutoOpen = AutoOpen;
            s.NonGithub = NonGithub;
            s.AutoUpdate = AutoUpdate;
            s.AutoClaimEnabled = AutoClaimEnabled;
            s.AutoStart = AutoStart;
            s.OpenEpic = OpenEpic;
            s.HeadlessEnabled = HeadlessEnabled;
            s.AdvancedMode = AdvancedMode;
            s.BackdropType = BackdropType;
            s.AminateType = AminateType;
            s.BrowerType = BrowerType;
            s.HasUsingPro = HasUsingPro;
            s.RememberWindowSize = RememberWindowSize;
            s.PatchUpdate = PatchUpdate;
        }

        // 显示提示文本并在指定时间后自动清空，避免消息长期残留
        private void ShowTip(string message)
        {
            TipText = message;
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (_, _) =>
            {
                TipText = string.Empty;
                timer.Stop();
            };
            timer.Start();
        }

        [RelayCommand]
        private static void OpenConfigFolder()
        {
            try
            {
                if (TryGetCompanyConfigDir(out var companyDir))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = companyDir,
                        UseShellExecute = true
                    });
                    return;
                }
                MessageBox.Show("未找到配置文件夹", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开配置文件夹失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 从 ConfigurationManager 提供的路径向上回溯两级，得到应用所属公司目录
        private static bool TryGetCompanyConfigDir(out string companyDirPath)
        {
            companyDirPath = string.Empty;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            var configPath = config.FilePath;
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath)) return false;

            var configDir = Path.GetDirectoryName(configPath);
            if (string.IsNullOrEmpty(configDir)) return false;

            var companyDir = Directory.GetParent(configDir)?.Parent;
            if (companyDir == null || !Directory.Exists(companyDir.FullName)) return false;

            companyDirPath = companyDir.FullName;
            return true;
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

        // 引擎列表变化时自动同步显示串，无需各命令手动调用
        private void OnEngineInfosChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            EnginePathsDisplay.Clear();
            foreach (var engine in EngineInfos)
            {
                EnginePathsDisplay.Add($"{engine.Path} ({engine.Version})");
            }
        }

        // 统一的 JSON 损坏提示，避免重复硬编码文案
        private static void NotifyCorruptJson(string? customMessage = null)
        {
            var message = customMessage ?? "JSON文件已损坏，请删除后再重新启动应用程序";
            MessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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