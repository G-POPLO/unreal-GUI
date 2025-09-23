using System;
using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using unreal_GUI.ViewModel;

namespace unreal_GUI
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置NavigationView的默认选中项
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
            
            // 由于已切换到NavigationView，不再需要将ContentContainer传递给ViewModel
            // _viewModel.ContentContainer = ContentContainer; // 将ContentContainer传递给ViewModel
            _ = _viewModel.InitializeJson_Async();
            _ = MainWindowViewModel.AutoUpdate();
            _ = MainWindowViewModel.CheckFabAsset();
        }
        
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag.ToString())
                {
                    case "Compile":
                        ContentFrame.Content = new Compile();
                        break;
                    case "Rename":
                        ContentFrame.Content = new Rename();
                        break;
                    case "QuickAccess":
                        ContentFrame.Content = new QuickAccess();
                        break;
                    case "Clear":
                        ContentFrame.Content = new Clear();
                        break;
                    case "Settings":
                        ContentFrame.Content = new Settings();
                        break;
                    case "About":
                        ContentFrame.Content = new About();
                        break;
                }
            }
        }
    }
}

