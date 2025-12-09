using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace unreal_GUI.Model
{
    internal class Fab_Notification
    {
        public static async Task<DateTime?> GetLimitedTimeFreeEndDate()
        {
            try
            {
                // 使用Playwright直接查找具有指定class的h2元素
                // fabkit-Typography-root fabkit-Typography--align-start fabkit-Typography--intent-primary fabkit-Heading--xl ArhVH7Um
                string dateString = await Playwright.GetH2ElementTextAsync(
                    "https://www.fab.com/limited-time-free",
                    "fabkit-Typography-root",
                    "fabkit-Typography--align-start",
                    "fabkit-Typography--intent-primary",
                    "fabkit-Heading--xl",
                    "ArhVH7Um");

                if (!string.IsNullOrEmpty(dateString))
                {
                    // 提取日期时间部分
                    var dateRegex = new Regex(@"Limited-Time Free \(Until ([A-Za-z0-9 :,AMPamp]+)\)");
                    var dateMatch = dateRegex.Match(dateString);

                    if (dateMatch.Success)
                    {
                        string dateTimeString = dateMatch.Groups[1].Value;
                        // 解析日期时间字符串
                        // 格式示例: "Aug 26 at 9:59 AM ET"
                        string[] parts = dateTimeString.Split(' ');
                        // 替换原有月份数组和解析逻辑
                        string[] monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                        string month = parts[0];
                        // 兼容 "Sept" 为 "Sep"
                        if (month.Equals("Sept", StringComparison.OrdinalIgnoreCase))
                        {
                            month = "Sep";
                        }
                        int monthIndex = Array.IndexOf(monthNames, month) + 1;
                        if (monthIndex < 1 || monthIndex > 12)
                            throw new ArgumentException($"无法识别的月份: {month}");

                        int day = int.Parse(parts[1]);
                        int hour = int.Parse(parts[3].Split(':')[0]);
                        int minute = int.Parse(parts[3].Split(':')[1]);
                        string amPm = parts[4];
                        int year = DateTime.Now.Year;
                        if (DateTime.Now.Month > monthIndex)
                        {
                            year++;
                        }

                        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        int hour24 = hour % 12 + (amPm.Equals("PM", StringComparison.CurrentCultureIgnoreCase) ? 12 : 0);
                        DateTime easternTime = new(year, monthIndex, day, hour24, minute, 0);
                        TimeZoneInfo chinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        DateTime chinaTime = TimeZoneInfo.ConvertTime(easternTime, easternZone, chinaZone);

                        // 保存到设置
                        Properties.Settings.Default.LimitedTime = chinaTime;
                        Properties.Settings.Default.Save();
                        //await ModernDialog.ShowInfoAsync($"{chinaTime}", "测试");

                        // 发送通知
                        SendFabNotification(chinaTime);
                        return chinaTime;
                    }
                }
            }
            catch (Exception ex)
            {
                // 显示错误信息                
                _ = ModernDialog.ShowErrorAsync($"无法获取Fab免费资产信息: {ex.Message}", "错误");

                return null;
            }

            return null;
        }

        /// <summary>
        /// 发送Windows通知
        /// </summary>
        private static void SendFabNotification(DateTime limitedTime)
        {

            if (Properties.Settings.Default.OpenEpic)
            {
                WindowsNotification.ShowNotificationWithUrls(
    "Fab资产领取提醒",
    $"新的Fab免费资产可领取，截至时间:{limitedTime}",
    "是",
    "openUrl",
    "com.epicgames.launcher://fab", // com.epicgames.launcher://fab/limited-time-free无法使用，会显示错误页面
    "否",
    "dismiss");
            }
            else
            {
                WindowsNotification.ShowNotificationWithUrls(
    "Fab资产领取提醒",
    $"新的Fab免费资产可领取，截至时间:{limitedTime}",
    "是",
    "openUrl",
    "https://www.fab.com/limited-time-free", // 使用浏览器链接
    "否",
    "dismiss");
            }
        }
    }
}
