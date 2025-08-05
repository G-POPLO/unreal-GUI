using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using unreal_GUI.Model;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

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
