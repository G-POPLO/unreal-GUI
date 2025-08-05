using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using unreal_GUI.Model;
using unreal_GUI.View;

namespace unreal_GUI.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private UIElement _currentView;
        
        // 用于页面跳转动画的 ContentControl
        public ContentControl ContentContainer { get; set; }

        public MainWindowViewModel()
        {
            // 初始化时导航到编译视图
            // 注意：在MainWindow_Loaded事件中会设置ContentContainer并调用NavigateToView
        }

        public static async Task AutoUpdate()
        {
            if (Properties.Settings.Default.AutoUpdate)
            {
                await UpdateAndExtract.CheckForUpdatesAsync(); // 检查更新
            }
        }

        // 若没有发现设置JSON文件，则弹窗提示
        public async Task InitializeJson_Async()
        {
            if (!File.Exists("settings.json"))
            {
                bool? result = await ModernDialog.ShowConfirmAsync("未检测到引擎，请先去设置引擎目录", "提示");

                if (result == true)
                {
                    ContentContainer.Content = new Settings();
                }
                else
                {
                    ContentContainer.Content = new Compile();
                }
            }
            else
            {
                ContentContainer.Content = new Compile();
            }
        }

 
        // 使用动画效果跳转到指定视图
        public void NavigateToView(UIElement view)
        {
            PageTransitionAnimation.ApplyTransition(ContentContainer, view);                    
        }

        [RelayCommand]
        private void NavigateToCompile()
        {
            NavigateToView(new Compile());
        }

        [RelayCommand]
        private void NavigateToRename()
        {
            NavigateToView(new Rename());
        }

        [RelayCommand]
        private void NavigateToQuickAccess()
        {
            NavigateToView(new QuickAccess());
        }

        [RelayCommand]
        private void NavigateToClear()
        {
            NavigateToView(new Clear());
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            NavigateToView(new Settings());
        }

        [RelayCommand]
        private void NavigateToAbout()
        {
            NavigateToView(new About());
        }
    }
}