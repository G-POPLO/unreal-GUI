using System;
using System.Threading.Tasks;

namespace reminder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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
    }
}