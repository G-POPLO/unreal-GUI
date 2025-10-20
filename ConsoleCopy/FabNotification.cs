using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace unreal_GUI_Reminder
{
    internal class FabNotification
    {
        public static async Task<DateTime?> GetLimitedTimeFreeEndDate()
        {
            try
            {
                // 使用HeadlessBrower直接查找具有指定class的h2元素
                string dateString = await HeadlessBrower.GetH2ElementTextAsync(
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
                        int hour24 = hour % 12 + (amPm.ToUpper() == "PM" ? 12 : 0);
                        DateTime easternTime = new(year, monthIndex, day, hour24, minute, 0);
                        TimeZoneInfo chinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        DateTime chinaTime = TimeZoneInfo.ConvertTime(easternTime, easternZone, chinaZone);

                        // 保存到设置
                        SettingsManager.Instance.Settings.LimitedTime = chinaTime;
                        SettingsManager.Instance.SaveSettings();

                        // 发送通知
                        SendFabNotification(chinaTime);
                        return chinaTime;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法获取Fab免费资产信息: {ex.Message}");
                return null;
            }

            return null;
        }

        /// <summary>
        /// 发送Windows通知
        /// </summary>
        private static void SendFabNotification(DateTime limitedTime)
        {
            // 显示带操作按钮的通知
            ShowNotificationWithUrls(
                "Fab资产领取提醒",
                $"新的Fab免费资产可领取，截至时间:{limitedTime}",
                "是",
                "openUrl",
                "https://www.fab.com/limited-time-free",
                "否",
                "dismiss");
        }

        /// <summary>
        /// 显示带操作按钮的通知
        /// </summary>
        public static void ShowNotificationWithUrls(string title, string message, string button1Text, string button1Action, string button1Url, string button2Text, string button2Action)
        {
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddButton(new ToastButton()
                    .SetContent(button1Text)
                    .AddArgument("action", button1Action)
                    .AddArgument("url", button1Url))
                .AddButton(new ToastButton()
                    .SetContent(button2Text)
                    .AddArgument("action", button2Action))
                .GetToastContent();

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        /// <summary>
        /// 检查是否需要获取Fab资产信息
        /// </summary>
        public static async Task CheckFabAsset()
        {
            if (SettingsManager.Instance.Settings.FabNotificationEnabled)
            {
                // 只有当本机时间大于LimitedTime或LimitedTime为空时才调用
                DateTime limitedTime = SettingsManager.Instance.Settings.LimitedTime;
                DateTime defaultTime = new(1990, 1, 1);

                if (DateTime.Now > limitedTime || limitedTime == defaultTime)
                {
                    await GetLimitedTimeFreeEndDate();
                }
            }
        }
    }
}
