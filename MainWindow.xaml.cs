using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace unreal_GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
            await CheckForUpdatesAsync();
        }
        private async Task CheckForUpdatesAsync()
        {
            if (!Properties.Settings.Default.AutoUpdate)
                return;

            try
            {
                var currentVersion = Application.ResourceAssembly.GetName().Version;
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "unreal-GUI");
                    var response = await client.GetStringAsync("https://api.github.com/repos/G-POPLO/unreal-GUI/releases/latest");
                    dynamic release = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                    var latestVersion = new Version(release.tag_name.ToString().TrimStart('v'));

                    if (latestVersion > currentVersion)
                    {
                        bool? result = await ModernDialog.ShowConfirmAsync("发现新版本 " + latestVersion + "，是否前往下载？", "版本更新");
                        if (result == true)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "https://github.com/G-POPLO/unreal-GUI/releases/latest",
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch { }
        }
        public async Task InitializeAsync()
        {
            if (!File.Exists("settings.json"))
            {
                bool? result = await ModernDialog.ShowConfirmAsync("未检测到引擎，请先去设置引擎目录", "提示");

                if (result == true)
                {
                    PageTransitionAnimation.ApplyTransition(ContentContainer, new Settings());
                }
                else
                {
                    PageTransitionAnimation.ApplyTransition(ContentContainer, new Compile());
                }
            }
            else
            {
                PageTransitionAnimation.ApplyTransition(ContentContainer, new Compile());
            }
        }


        private void CompilePlugin_Click(object sender, RoutedEventArgs e)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, new Compile());
        }
        

        private void RenameProject_Click(object sender, RoutedEventArgs e)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, new Rename());
        }

        private void QuickAccess_Click(object sender, RoutedEventArgs e)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, new QuickAccess());
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, new About());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, new Settings());
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, new Clear());
        }
    }
}