using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using unreal_GUI.Model;

namespace unreal_GUI.ViewModel
{
    public class QuickAccessViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<object> engines = new ObservableCollection<object>();
        
        public QuickAccessViewModel()
        {
            LoadEngineList();
            OpenPluginDirectoryCommand = new RelayCommand<object>(OpenPluginDirectory);
            OpenWebsiteCommand = new RelayCommand<string>(OpenWebsite);
            AddCustomCommand = new RelayCommand(AddCustom);
            DeleteCustomCommand = new RelayCommand(DeleteCustom);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<object> Engines
        {
            get => engines;
            set
            {
                engines = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenPluginDirectoryCommand { get; }
        public ICommand OpenWebsiteCommand { get; }
        public ICommand AddCustomCommand { get; }
        public ICommand DeleteCustomCommand { get; }

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json);
                var engineList = settings?.Engines ?? new List<SettingsViewModel.EngineInfo>();
                Engines = new ObservableCollection<object>(engineList.Select(e => new { DisplayName = $"UE {e.Version}", Path = e.Path }));
            }
        }

        private void OpenPluginDirectory(object selectedEngine)
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

        private void OpenWebsite(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
        }

        private async void AddCustom()
        {
            await ModernDialog.ShowAddCustomDialogAsync();
        }

        private async void DeleteCustom()
        {
            await ModernDialog.ShowDeleteCustomDialogAsync();
        }

        
    }
}