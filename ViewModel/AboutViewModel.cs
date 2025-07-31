using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace unreal_GUI.ViewModel
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _versionText;

        public AboutViewModel()
        {
            _versionText = "当前版本：" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        [RelayCommand]
        private static void OpenUpdateUrl()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/G-POPLO/unreal-GUI/releases/",
                UseShellExecute = true
            });
        }
    }
}