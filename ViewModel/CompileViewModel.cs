using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using unreal_GUI.Model;

namespace unreal_GUI.ViewModel
{
    public class CompileViewModel : INotifyPropertyChanged
    {
        private List<SettingsViewModel.EngineInfo> engineList = [];
        private SettingsViewModel.EngineInfo selectedEngine;
        private string inputPath;
        private string outputPath;
        private string tipsText;
        private bool isCompileButtonEnabled;
        private Visibility tipsVisibility;

        public CompileViewModel()
        {
            LoadEngineList();
            SelectInputCommand = new RelayCommand(SelectInput);
            SelectOutputCommand = new RelayCommand(SelectOutput);
            CompileCommand = new RelayCommand(Compile, CanCompile);
            
            // 初始化属性
            TipsVisibility = Visibility.Hidden;
            IsCompileButtonEnabled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<SettingsViewModel.EngineInfo> EngineVersions { get; } = new ObservableCollection<SettingsViewModel.EngineInfo>();

        public SettingsViewModel.EngineInfo SelectedEngine
        {
            get => selectedEngine;
            set
            {
                selectedEngine = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCompileButtonEnabled));
                ((RelayCommand)CompileCommand).NotifyCanExecuteChanged();
            }
        }

        public string InputPath
        {
            get => inputPath;
            set
            {
                inputPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCompileButtonEnabled));
                ((RelayCommand)CompileCommand).NotifyCanExecuteChanged();
            }
        }

        public string OutputPath
        {
            get => outputPath;
            set
            {
                outputPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCompileButtonEnabled));
                ((RelayCommand)CompileCommand).NotifyCanExecuteChanged();
            }
        }

        public string TipsText
        {
            get => tipsText;
            set
            {
                tipsText = value;
                OnPropertyChanged();
            }
        }

        public bool IsCompileButtonEnabled
        {
            get => CanCompile();
            set
            {
                isCompileButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public Visibility TipsVisibility
        {
            get => tipsVisibility;
            set
            {
                tipsVisibility = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectInputCommand { get; }
        public ICommand SelectOutputCommand { get; }
        public ICommand CompileCommand { get; }

        private void LoadEngineList()
        {
            if (File.Exists("settings.json"))
            {
                engineList = JsonConvert.DeserializeObject<List<SettingsViewModel.EngineInfo>>(File.ReadAllText("settings.json"));
                foreach (SettingsViewModel.EngineInfo engine in engineList)
                {
                    EngineVersions.Add(engine);
                }
            }
        }

        private void SelectInput()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Unreal Plugin Files (*.uplugin)|*.uplugin"
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                InputPath = dialog.FileName;
                
                // 读取.uplugin文件内容
                string pluginContent = File.ReadAllText(InputPath);
                dynamic pluginInfo = JsonConvert.DeserializeObject(pluginContent);
                string pluginEngineVersion = pluginInfo.EngineVersion;

                // 获取选择的引擎版本
                if (SelectedEngine != null)
                {
                    TipsText = $"即将把版本{pluginEngineVersion}的插件编译成{SelectedEngine.Version}版本的插件";
                }
                else
                {
                    TipsText = $"读取到插件版本{pluginEngineVersion}，请先选择目标引擎版本";
                }
                TipsVisibility = Visibility.Visible;
            }
        }

        private void SelectOutput()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                OutputPath = dialog.SelectedPath;
            }
        }

        private bool CanCompile()
        {
            return !string.IsNullOrWhiteSpace(InputPath) &&
                   !string.IsNullOrWhiteSpace(OutputPath) &&
                   SelectedEngine != null;
        }

        private async void Compile()
        {
            TipsVisibility = Visibility.Visible;

            if (SelectedEngine == null)
            {
                TipsText = "请选择引擎版本";
                return;
            }

            var uatPath = Path.Combine(SelectedEngine.Path, "Engine", "Build", "BatchFiles", "RunUAT.bat");

            if (!File.Exists(uatPath))
            {
                TipsText = "找不到RunUAT.bat文件";
                return;
            }

            if (string.IsNullOrWhiteSpace(InputPath) || !File.Exists(InputPath))
            {
                TipsText = "插件路径无效";
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPath) || !Directory.Exists(OutputPath))
            {
                TipsText = "输出路径无效";
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = uatPath,
                    Arguments = $"BuildPlugin -plugin=\"{InputPath}\" -package=\"{OutputPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                };

                using (var process = Process.Start(startInfo))
                {
                    await Task.Run(() => process.WaitForExit());
                    if (process.ExitCode == 0)
                    {
                        TipsText = "编译成功！";
                        if (Properties.Settings.Default.AutoOpen)
                        {
                            Process.Start("explorer.exe", OutputPath);
                        }
                    }
                    else
                    {
                        TipsText = $"编译失败，错误代码：{process.ExitCode}";
                    }
                }
                
                var player = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-on.wav"));
                player.Play();
            }
            catch (Exception ex)
            {
                TipsText = $"编译错误：{ex.Message}";
                var player = new SoundPlayer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound", "ui-sound-off.wav"));
                player.Play();
            }
        }
    }
}