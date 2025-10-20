using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;

namespace unreal_GUI_Reminder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // 注册通知激活事件处理程序
                ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
                
                // 加载设置
                SettingsManager.Instance.LoadSettings();
                
                // 检查是否需要开机启动检查
                if (SettingsManager.Instance.Settings.AutoStart)
                {
                    Console.WriteLine("开始检查Fab免费资产...");
                    await FabNotification.CheckFabAsset();
                    Console.WriteLine("检查完成，程序将在5秒后自动退出...");
                    await Task.Delay(5000); // 等待5秒，确保通知有足够时间显示
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }
        }
        
        // 通知激活事件处理程序
        private static void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
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
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
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
    }
}
