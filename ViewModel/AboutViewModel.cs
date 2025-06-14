using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace unreal_GUI.ViewModel
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _versionText;
        
        [ObservableProperty]
        private string _latestVersion;
        
        [ObservableProperty]
        private string _lastUpdateTime;
        
        [ObservableProperty]
        private string _tip;
        
        public AboutViewModel()
        {
            VersionText = "当前版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            LastUpdateTime = Properties.Settings.Default.LastUpdateTime.ToString();
            LatestVersion = Properties.Settings.Default.LatestVersion.ToString();
            Tip = $"最新可用版本：{LatestVersion}";
        }
        
        [RelayCommand]
        private static void CheckUpdate()
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/G-POPLO/unreal-GUI/releases/", UseShellExecute = true });
        }
    }
}