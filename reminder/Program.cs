using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace reminder
{
    internal class Program
    {


        static async Task Main(string[] args)
        {
            // 注册通知激活事件处理程序
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;

            Console.WriteLine("Fab免费资产提醒程序启动...");

            // 检查Fab限时免费资产
            await CheckFabFreeAssets();

            Console.WriteLine("程序执行完毕，按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 检查Fab限时免费资产
        /// </summary>
        static async Task CheckFabFreeAssets()
        {
            try
            {
                Console.WriteLine("正在检查Fab限时免费资产...");
                
                // 读取配置确定是否启用Fab提醒功能
                var configReader = new reminder.ReadConfig();
                bool fabReminderEnabled = configReader.ReadBool("FabReminderEnabled", true);
                
                if (!fabReminderEnabled)
                {
                    Console.WriteLine("Fab提醒功能已禁用");
                    return;
                }
                
                DateTime? endDate = await Fab_Notification.GetLimitedTimeFreeEndDate();
                
                if (endDate.HasValue)
                {
                    Console.WriteLine($"发现新的Fab免费资产，截止时间: {endDate.Value}");
                }
                else
                {
                    Console.WriteLine("未找到Fab免费资产信息或获取失败");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查Fab免费资产时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 通知激活事件处理程序
        /// </summary>
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
    }
}