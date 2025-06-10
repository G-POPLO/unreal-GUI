using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using ModernWpf.Controls;
using Newtonsoft.Json.Linq;


namespace unreal_GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        public string latestVersion;

        //private async Task CheckForUpdatesAsync()
        //{
        //    try
        //    {
        //        using var client = new System.Net.Http.HttpClient();
        //        client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
        //        var response = await client.GetAsync("https://api.github.com/repos/G-POPLO/unreal-GUI/releases/latest");
        //        // 获取最新版本号latestVersion
        //        latestVersion = JObject.Parse(await response.Content.ReadAsStringAsync())["tag_name"]?.ToString();

        //        // 通过事件通知更新UI
                

                
        //        var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        //        if (Version.Parse(latestVersion) <= Version.Parse(currentVersion))
        //        {

        //            var result = await ModernDialog.ShowCustomAsync("发现新版本 " + latestVersion, "更新可用", "立即下载");
        //            if (result == ContentDialogResult.Primary)
        //            {
        //                Process.Start(new ProcessStartInfo("https://github.com/G-POPLO/unreal-GUI/releases/latest") { UseShellExecute = true });
        //            }
        //        }

        //        Properties.Settings.Default.LastUpdateTime = DateTime.Now;
        //        Properties.Settings.Default.Save();
        //    }
        //    catch {
        //        // 错误逻辑
        //    }
        //}

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeAsync();
            //await CheckForUpdatesAsync(); // 添加更新检查
        }

        // 若没有发现设置JSON文件，则弹窗提示
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



        // 页面跳转

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
            //var aboutPage = ContentContainer.Content as About;
            //    if(aboutPage != null)
            //    {
            //    aboutPage.LatestVersion = latestVersion;
            //    aboutPage.Tip.Text = $"最新可用版本：{latestVersion}";
            //    }
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

