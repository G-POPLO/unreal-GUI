using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using unreal_GUI.Model;

namespace unreal_GUI.ViewModel
{
    public partial class QuickAccessViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<object> engines = [];
        
        [ObservableProperty]
        private ObservableCollection<object> customButtons = [];
        
        public QuickAccessViewModel()
        {
            LoadEngineList();
          
        }

        [RelayCommand]
        private static void OpenPluginDirectory(object selectedEngine)
        {
            if (selectedEngine != null)
            {
                dynamic engine = selectedEngine;
                string pluginPath = Path.Combine(engine.Path, "Engine", "Plugins", "Marketplace");

                if (Directory.Exists(pluginPath))
                {
                    Process.Start("explorer.exe", pluginPath);
                }
                else
                {
                    _ = ModernDialog.ShowInfoAsync("目录不存在", $"未找到插件目录：\n{pluginPath}");
                }
            }
        }

        [RelayCommand]
        private static void OpenCustomDirectory(object selectedCustomButton)
        {
            if (selectedCustomButton != null)
            {
                dynamic customButton = selectedCustomButton;
                string path = customButton.Path;

                if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                else
                {
                    _ = ModernDialog.ShowInfoAsync("目录不存在", $"未找到目录：\n{path}");
                }
            }
        }

        [RelayCommand]
        private static void OpenWebsite(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
        }

        [RelayCommand]
        private static async Task AddCustom()
        {
            await ModernDialog.ShowAddCustomDialogAsync();
        }

        [RelayCommand]
        private static async Task DeleteCustom()
        {
            await ModernDialog.ShowDeleteCustomDialogAsync();
        }

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json);
                var engineList = settings?.Engines ?? [];
                Engines = new ObservableCollection<object>(engineList.Select(e => new { DisplayName = $"UE {e.Version}", e.Path }));
                
                var customButtonList = settings?.CustomButtons ?? [];
                CustomButtons = new ObservableCollection<object>(customButtonList.Select(cb => new { DisplayName = cb.Name, cb.Path }));
            }
        }

        public void LoadCustomButtons()
        {
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json);
                var customButtonList = settings?.CustomButtons ?? [];
                CustomButtons = new ObservableCollection<object>(customButtonList.Select(cb => new { DisplayName = cb.Name, cb.Path }));
            }
        }

        
    }
}