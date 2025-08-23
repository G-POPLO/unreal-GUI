using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace unreal_GUI
{

    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            
        }
        


        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 处理异常
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);

            // 设置为已处理以防止应用程序退出
            e.Handled = true;
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 记录错误信息
            Exception? ex = e.ExceptionObject as Exception;
            MessageBox.Show("An unhandled exception occurred in a non-UI thread: " + (ex?.Message ?? "Unknown error"), "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}