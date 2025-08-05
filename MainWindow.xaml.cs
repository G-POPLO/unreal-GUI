using System;
using System.Windows;
using unreal_GUI.ViewModel;

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
            if (ViewModel is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ContentContainer = ContentContainer;
                await mainWindowViewModel.InitializeJson_Async();
                await MainWindowViewModel.AutoUpdate();
            }
        }
    }
}

