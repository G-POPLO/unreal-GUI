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
        }
        
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ContentContainer = ContentContainer; // ContentContainer����Ϊ��ʱ��ֱ���л���ͼ
            }
        }
    }
}

