using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using unreal_GUI.ViewModel;
using unreal_GUI.Model;
using System.Reflection;
using System.Windows.Navigation;


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

            // 订阅 ViewModel 的导航请求事件
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.NavigationRequested += OnNavigationRequested;
                // 初始化默认导航到编译页面
                await viewModel.InitializeJson_Async();
            }

            await MainWindowViewModel.AutoUpdate();
            MainWindowViewModel.CheckFabAsset();
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
        
        
    }
}

