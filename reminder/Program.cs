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

            // 创建隐藏的Form以启动消息循环
            var hiddenForm = new Form
            {
                WindowState = FormWindowState.Minimized,
                ShowInTaskbar = false
            };
            hiddenForm.Load += async (sender, e) =>
            {
                await CheckFabFreeAssets();
                Application.Exit();
            };

            Application.Run(hiddenForm);
        }

        /// <summary>
        /// 检查Fab限时免费资产
        /// </summary>
        static async Task CheckFabFreeAssets()
        {
            try
            {
                Console.WriteLine("正在检查Fab限时免费资产...");

                var configReader = new IniConfig();
                bool fabReminderEnabled = configReader.ReadBool("FabNotificationEnabled", true);

                if (!fabReminderEnabled)
                {
                    Console.WriteLine("Fab提醒功能已禁用，程序将退出");
                    Environment.Exit(0);
                }

                DateTime limitedTime = configReader.ReadDateTime("LimitedTime", new DateTime(1990, 1, 1));
                DateTime system_time = DateTime.Now;

                if (system_time <= limitedTime)
                {
                    Console.WriteLine($"Fab免费资产仍在有效期内，程序退出");
                    Environment.Exit(0);
                }
                
                Console.WriteLine($"上次记录的截止时间 {limitedTime} 已过期，发送提醒通知");
                FabReminder.SendFabNotification(limitedTime);
                Console.WriteLine("正在打开浏览器获取最新截止时间...");
                var newLimitedTime = await FabReminder.GetLimitedTimeFreeEndDate();
                if (newLimitedTime.HasValue)
                {
                    Console.WriteLine($"获取到最新截止时间: {newLimitedTime.Value}");
                }
                else
                {
                    Console.WriteLine("获取最新截止时间失败");
                }
                Console.WriteLine("程序执行完毕...");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查Fab免费资产时发生错误: {ex.Message}");
                Debug.WriteLine(ex.Message);
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