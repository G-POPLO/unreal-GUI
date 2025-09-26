using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using unreal_GUI.Model;


namespace unreal_GUI.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // 定义页面跳转请求事件
        public event EventHandler<string> NavigationRequested;

        // 当前选中的页面标签
        [ObservableProperty]
        private string currentPageTag = "Compile";


        // 导航历史记录
        [ObservableProperty]
        private ObservableCollection<string> navigationHistory = new();
        
        // 导航到指定页面的命令
        [RelayCommand]
        private void NavigateToPage(string pageTag)
        {
         
                CurrentPageTag = pageTag;
                NavigationRequested?.Invoke(this, pageTag);
                
                // 添加到导航历史
                if (!NavigationHistory.Contains(pageTag))
                {
                    NavigationHistory.Add(pageTag);
                }

            
        }
        
        // 返回上一页的命令
        [RelayCommand]
        private void GoBack()
        {
            if (NavigationHistory.Count > 1)
            {
                // 移除当前页面
                NavigationHistory.RemoveAt(NavigationHistory.Count - 1);
                
                // 获取上一个页面
                string previousPage = NavigationHistory[NavigationHistory.Count - 1];
                NavigateToPage(previousPage);
            }
        }

        public MainWindowViewModel()
        {
            // 初始化时添加默认页面到历史记录
            NavigationHistory.Add("Compile");
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
                else
                {
                    // 截图测试，仅Debug用
                    //await Playwright.GetPageContentAsync("https://bot.sannysoft.com/");
                }
            }
        }

        // 根据页面标签获取页面类型
        public Type GetPageTypeByTag(string tag)
        {
            return tag switch 
            {
                "Compile" => typeof(Compile),
                "Rename" => typeof(Rename),
                "QuickAccess" => typeof(QuickAccess),
                "Clear" => typeof(Clear),
                "Settings" => typeof(Settings),
                "About" => typeof(About),
                _ => typeof(Compile)
            };
        }
        
        // 处理来自视图的导航请求
        public void HandleNavigationRequest(string pageTag)
        {
            NavigateToPage(pageTag);
        }
        
        // 若没有发现设置JSON文件，则弹窗提示
        public async Task InitializeJson_Async()
        {
            
            if (!File.Exists("settings.json"))
            {
                bool? result = await ModernDialog.ShowConfirmAsync("未检测到引擎，请先去设置引擎目录", "提示");

                if (result == true)
                {
                    // 用户选择"是"，触发导航到设置页面事件
                    NavigateToPage("Settings");
                }
                else
                {
                    // 用户选择"否"，触发导航到编译页面事件
                    NavigateToPage("Compile");
                }
            }
            else
            {
                // 文件存在，触发导航到编译页面事件
                NavigateToPage("Compile");
            }
        }
    }
}