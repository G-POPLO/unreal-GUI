using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows;
using unreal_GUI.Model;

namespace unreal_GUI.View.Update
{
    /// <summary>
    /// Update.xaml 的交互逻辑
    /// </summary>
    public partial class Update : Window
    {
        public Update()
        {
            InitializeComponent();
            Loaded += UpdateWindow_Loaded;
        }

        private async void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {

            await UpdateAndExtract.DownloadAndUpdateAsync();

        }

        [RelayCommand]
        private static void OpenSourceCode()
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/G-POPLO/unreal-GUI/", UseShellExecute = true });
        }
    }
}
