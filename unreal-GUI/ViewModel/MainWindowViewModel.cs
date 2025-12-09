using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
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

        // 高级模式设置
        [ObservableProperty]
        private bool _advancedMode;

        // 窗口背景类型设置
        [ObservableProperty]
        private byte _backdropType;

        // 导航历史记录
        [ObservableProperty]
        private ObservableCollection<string> navigationHistory = [];

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
                string previousPage = NavigationHistory[^1];
                NavigateToPage(previousPage);
            }
        }

        public MainWindowViewModel()
        {
            AdvancedMode = Properties.Settings.Default.AdvancedMode;
            BackdropType = Properties.Settings.Default.BackdropType;
            
            // 订阅设置变化事件
            Properties.Settings.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        // 处理设置变化事件
        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.BackdropType))
            {
                BackdropType = Properties.Settings.Default.BackdropType;
            }
            else if (e.PropertyName == nameof(Properties.Settings.Default.AdvancedMode))
            {
                AdvancedMode = Properties.Settings.Default.AdvancedMode;
            }
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
                DateTime defaultTime = new(1990, 1, 1);

                if (DateTime.Now > limitedTime || limitedTime == defaultTime)
                {
                    await Fab_Notification.GetLimitedTimeFreeEndDate();
                }
                else
                {
                    // 截图测试，仅Debug用
                    //await Fab_Notification.GetLimitedTimeFreeEndDate();
                    //await Playwright.GetPageContentAsync("https://bot.sannysoft.com/");
                }
            }
        }

        //// 根据页面标签获取页面类型
        //public static Type GetPageTypeByTag(string tag)
        //{
        //    return tag switch
        //    {
        //        "Compile" => typeof(Compile),
        //        "Rename" => typeof(Rename),
        //        "QuickAccess" => typeof(QuickAccess),
        //        "Clear" => typeof(Clear),
        //        "Settings" => typeof(Settings),
        //        "About" => typeof(About),
        //        _ => typeof(Compile)
        //    };
        //}

        //// 处理来自视图的导航请求
        //public void HandleNavigationRequest(string pageTag)
        //{
        //    NavigateToPage(pageTag);
        //}

        // 若没有发现设置JSON文件，则弹窗提示
        public async Task InitializeJson_Async()
        {

            if (!File.Exists("settings.json"))
            {
                var result = await ModernDialog.ShowConfirmAsync("未检测到引擎，请先去设置引擎目录", "提示");

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