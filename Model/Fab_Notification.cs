using Microsoft.Playwright;
using Microsoft.Toolkit.Uwp.Notifications;

using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using unreal_GUI.Model;

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
                        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
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
                        DateTime easternTime = new DateTime(year, monthIndex, day, hour24, minute, 0);
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
                string errorMessage = ex.Message;
                if (errorMessage.Contains("403"))
                {
                    _ = ModernDialog.ShowInfoAsync($"无法获取Fab免费资产信息: 网站返回403错误，可能已被反爬虫机制拦截。请稍后再试。", "错误");
                }
                else
                {
                    _ = ModernDialog.ShowInfoAsync($"无法获取Fab免费资产信息: {ex.Message}", "错误");
                }

                return null;
            }

            return null;
        }


        /// <summary>
        /// 发送Windows通知
        /// </summary>
        private static void SendFabNotification(DateTime limitedTime)
        {
            // 使用 WindowsNotification 类显示带操作按钮的通知
            WindowsNotification.ShowNotificationWithActions(
                "Fab资产领取提醒",
                $"新的Fab免费资产可领取，截至时间:{limitedTime}",
                "是",
                "openUrl",
                "否",
                "dismiss");
        }
    }
}
