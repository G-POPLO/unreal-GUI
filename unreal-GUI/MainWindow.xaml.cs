using iNKORE.UI.WPF.Modern.Controls;
using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using System;
using System.Windows;
using System.Windows.Navigation;
using unreal_GUI.View;
using unreal_GUI.ViewModel;


namespace unreal_GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 在Loaded事件中动态设置窗口背景类型
            SetSystemBackdropType();

            // 订阅 ViewModel 的导航请求事件
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.NavigationRequested += OnNavigationRequested;
                // 订阅BackdropType属性变化事件
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                // 初始化默认导航到编译页面
                await viewModel.InitializeJson_Async();
            }

            await MainWindowViewModel.AutoUpdate();
            await MainWindowViewModel.CheckFabAsset();
        }

        private void SetSystemBackdropType()
        {
            // 根据ViewModel的BackdropType属性设置窗口背景类型
            byte backdropType;
            if (DataContext is MainWindowViewModel viewModel)
            {
                backdropType = viewModel.BackdropType;
            }
            else
            {
                // 如果ViewModel不可用，使用默认设置
                backdropType = Properties.Settings.Default.BackdropType;
            }

            // 转换为BackdropType枚举
            BackdropType backdropTypeEnum = backdropType switch
            {
                0 => BackdropType.Mica,
                1 => BackdropType.Acrylic,
                _ => BackdropType.None
            };

            // 使用依赖属性设置窗口背景类型
            SetValue(WindowHelper.SystemBackdropTypeProperty, backdropTypeEnum);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                // 获取页面类型
                string tag = item.Tag?.ToString();

                Type pageType = tag switch
                {
                    "Compile" => typeof(Compile),
                    "Rename" => typeof(Rename),
                    "QuickAccess" => typeof(QuickAccess),
                    "Clear" => typeof(Clear),
                    "Settings" => typeof(Settings),
                    "About" => typeof(About),
                    "Templates" => typeof(Templates),
                    "Terminal" => typeof(Terminal),
                    _ => typeof(Compile)
                };

                // 检查当前是否已经在目标页面上，避免重复导航
                if (ContentFrame.Content?.GetType() != pageType)
                {
                    ContentFrame.Navigate(Activator.CreateInstance(pageType));
                }
            }
        }

        // 处理回退按钮点击事件
        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        // 更新回退按钮状态
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {


            // 同步NavigationView选中项与当前页面                   
            string pageTag = ContentFrame.Content.GetType().Name;

            // 根据页面类型找到对应的菜单项并选中
            foreach (var item in NavigationView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == pageTag)
                {
                    NavigationView.SelectedItem = navItem;
                    break;
                }
            }

            NavigationView.IsBackEnabled = ContentFrame.CanGoBack;

        }

        // 处理来自 ViewModel 的导航请求
        public void OnNavigationRequested(object sender, string pageTag)
        {
            // 根据 pageTag 找到对应的菜单项并选中
            foreach (var item in NavigationView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == pageTag)
                {
                    NavigationView.SelectedItem = navItem;
                    break;
                }
            }
        }

        // 处理ViewModel属性变化事件
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.BackdropType))
            {
                SetSystemBackdropType();
            }
        }


    }
}

