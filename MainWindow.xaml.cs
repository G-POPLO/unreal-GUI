using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

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
    }
}