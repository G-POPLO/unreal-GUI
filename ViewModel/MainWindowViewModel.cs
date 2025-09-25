using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using unreal_GUI.Model;

namespace unreal_GUI.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private UIElement _currentView;

        // 用于页面跳转动画的 ContentControl
        // public ContentControl ContentContainer { get; set; }

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

        public static async Task CheckFabAsset()
        {
            if (Properties.Settings.Default.FabNotificationEnabled)
            {
                // 只有当本机时间大于LimitedTime或LimitedTime为空时才调用
                DateTime limitedTime = Properties.Settings.Default.LimitedTime;
                DateTime defaultTime = new DateTime(1990, 1, 1);
                
                if (DateTime.Now > limitedTime || limitedTime == defaultTime)
                {
                    await Fab_Notification.GetLimitedTimeFreeEndDate();
                }
            }
        }

        // 若没有发现设置JSON文件，则弹窗提示
        public Task InitializeJson_Async()
        {
            // 由于已切换到NavigationView，此方法中的ContentContainer相关代码已不再需要
            if (!File.Exists("settings.json"))
            {
                // bool? result = await ModernDialog.ShowConfirmAsync("未检测到引擎，请先去设置引擎目录", "提示");
                // 
                // if (result == true)
                // {
                //     ContentContainer.Content = new Settings();
                // }
                // else
                // {
                //     ContentContainer.Content = new Compile();
                // }
            }

            return Task.CompletedTask;
            // else
            // {
            //     ContentContainer.Content = new Compile();
            // }
        }


        // 跳转到指定视图
        // 由于NavigationView自带页面切换动画，因此注释掉自定义动画效果
        /*
        public void NavigateToView(UIElement view)
        {
            if (Properties.Settings.Default.AmimateEnabled)
            {
                PageTransitionAnimation.ApplyTransition(ContentContainer, view);
            }
            else
            {
                ContentContainer.Content = view;
            }

        }
        */

        // 由于已切换到NavigationView，不再需要这些命令
        /*
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
        */

    }
}