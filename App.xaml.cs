using Microsoft.Toolkit.Uwp.Notifications;
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
            
            // 注册通知激活事件处理程序
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
            
           
        }
        // 通知激活事件处理程序
        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            // 获取通知参数
            var args = ToastArguments.Parse(e.Argument);
            
            // 根据参数执行相应操作
            if (args.Contains("action"))
            {
                string action = args["action"];
                
                switch (action)
                {
                    case "openUrl":
                        // 从参数中获取URL，如果不存在则使用默认URL
                        string url = args.Contains("url") ? args["url"] : "https://www.fab.com/limited-time-free";
                        // 打开网站
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        break;
                    case "dismiss":
                        // 忽略通知，什么都不做
                        break;
                    default:
                        // 未来可以添加更多操作类型
                        break;
                }
            }
        }


        // 全局异常处理
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
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("An unhandled exception occurred in a non-UI thread: " + (ex?.Message ?? "Unknown error"), "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}