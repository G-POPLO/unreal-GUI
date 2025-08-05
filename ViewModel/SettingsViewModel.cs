using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using unreal_GUI.Model;

namespace unreal_GUI.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<EngineInfo> _engineInfos = new List<EngineInfo>();

        [ObservableProperty]
        private List<string> _enginePathsDisplay = new List<string>();

        [ObservableProperty]
        private string _tipText = "";

        [ObservableProperty]
        private bool _autoOpen;

        [ObservableProperty]
        private bool _gitcode;

        [ObservableProperty]
        private bool _zenDashborad;

        [ObservableProperty]
        private bool _autoUpdate;

        public SettingsViewModel()
        {
            // 初始化设置
            AutoOpen = Properties.Settings.Default.AutoOpen;
            Gitcode = Properties.Settings.Default.Gitcode;
            ZenDashborad = Properties.Settings.Default.ZenDashborad;
            AutoUpdate = Properties.Settings.Default.AutoUpdate;

            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var settings = JsonConvert.DeserializeObject<SettingsData>(json);
                    EngineInfos = settings.Engines ?? new List<EngineInfo>();
                    UpdateEnginePathsDisplay();
                }
                catch
                {
                    // 如果JSON文件损坏，初始化为空列表
                    EngineInfos = new List<EngineInfo>();
                }
            }
        }

        [RelayCommand]
        private void AddEnginePath()
        {
            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    EngineInfos.Add(new EngineInfo { Path = folderDialog.SelectedPath, Version = GetEngineVersion(folderDialog.SelectedPath) });
                    UpdateEnginePathsDisplay();
                }
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
                using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EpicGames\\Unreal Engine"))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                var path = subKey?.GetValue("InstalledDirectory") as string;
                                if (!string.IsNullOrEmpty(path) && !EngineInfos.Any(x => x.Path == path))
                                {
                                    EngineInfos.Add(new EngineInfo { Path = path, Version = GetEngineVersion(path) });
                                }
                            }
                        }
                        UpdateEnginePathsDisplay();
                    }
                }
            }
            catch (Exception)
            {
                _ = ModernDialog.ShowConfirmAsync("未检测到引擎，请手动设置引擎目录", "提示");
            }
        }

        [RelayCommand]
        private void SavePaths()
        {
            // 保存应用程序设置
            Properties.Settings.Default.AutoOpen = AutoOpen;
            Properties.Settings.Default.Gitcode = Gitcode;
            Properties.Settings.Default.ZenDashborad = ZenDashborad;
            Properties.Settings.Default.AutoUpdate = AutoUpdate;
            Properties.Settings.Default.Save();

            // 保存JSON文件
            var settings = new SettingsData
            {
                Engines = EngineInfos,
                CustomButtons = new List<CustomButton>()
            };
            
            // 读取现有的自定义按钮数据
            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var existingSettings = JsonConvert.DeserializeObject<SettingsData>(json);
                    settings.CustomButtons = existingSettings?.CustomButtons ?? new List<CustomButton>();
                }
                catch
                {
                    // 如果JSON文件损坏，初始化为空列表
                    settings.CustomButtons = new List<CustomButton>();
                }
            }
            
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));

            TipText = "设置已保存";
        }

        private void UpdateEnginePathsDisplay()
        {
            EnginePathsDisplay = EngineInfos.Select(p => $"{p.Path} ({p.Version})").ToList();
        }

        private static string GetEngineVersion(string enginePath)
        {
            try
            {
                // 尝试从路径中提取版本号
                var dirName = Path.GetFileName(enginePath.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                if (dirName.StartsWith("UE_"))
                {
                    return dirName.Substring(3);
                }

                // 尝试读取.version文件
                var versionFile = System.IO.Path.Combine(enginePath, "Engine", "Build", "Build.version");
                if (File.Exists(versionFile))
                {
                    dynamic versionInfo = JsonConvert.DeserializeObject(File.ReadAllText(versionFile));
                    return $"{versionInfo.MajorVersion}.{versionInfo.MinorVersion}.{versionInfo.PatchVersion}";
                }
            }
            catch { }
            return "未知版本";
        }

        public partial class EngineInfo : ObservableObject
        {
            [ObservableProperty]
            private string _path;

            [ObservableProperty]
            private string _version;
        }

        public partial class SettingsData : ObservableObject
        {
            [ObservableProperty]
            private List<EngineInfo> _engines;

            [ObservableProperty]
            private List<CustomButton> _customButtons;
        }

        public partial class CustomButton : ObservableObject
        {
            [ObservableProperty]
            private string _name;

            [ObservableProperty]
            private string _path;
        }

    }
}