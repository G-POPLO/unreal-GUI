using HtmlAgilityPack;
using ModernWpf.Controls;
using System;
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
                // 使用Puppeteer获取页面内容
                string html = await Puppeteer.GetPageContentAsync("https://www.fab.com/limited-time-free");

                // 使用HtmlAgilityPack解析网页
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // 查找具有指定class的h2元素
                var h2Node = doc.DocumentNode.SelectSingleNode("//h2[contains(@class, 'fabkit-Typography-root') and contains(@class, 'fabkit-Typography--align-start') and contains(@class, 'fabkit-Typography--intent-primary') and contains(@class, 'fabkit-Heading--xl') and contains(@class, 'ArhVH7Um')]");

                if (h2Node != null)
                {
                    string dateString = h2Node.InnerText;
                    // 提取日期时间部分
                    var dateRegex = new Regex(@"Limited-Time Free \(Until ([A-Za-z0-9 :,AMPamp]+)\)");
                    var dateMatch = dateRegex.Match(dateString);

                    if (dateMatch.Success)
                    {
                        string dateTimeString = dateMatch.Groups[1].Value;
                        // 解析日期时间字符串
                        // 格式示例: "Aug 26 at 9:59 AM ET"
                        string[] parts = dateTimeString.Split(' ');
                        string month = parts[0];
                        int day = int.Parse(parts[1]);
                        int hour = int.Parse(parts[3].Split(':')[0]);
                        int minute = int.Parse(parts[3].Split(':')[1]);
                        string amPm = parts[4];
                        // "ET" 是 Eastern Time，需要转换为北京时间

                        // 确定年份（假设为当前年或下一年）
                        int year = DateTime.Now.Year;
                        if (DateTime.Now.Month > Array.IndexOf(new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }, month) + 1)
                        {
                            year++;
                        }

                        // 创建 Eastern Time 时间
                        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        DateTime easternTime = new DateTime(year, Array.IndexOf(new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }, month) + 1, day, hour + (amPm == "PM" ? 12 : 0), minute, 0);
                        
                        // 转换为北京时间 (后续软件多语言支持的时候可以更改成根据用户地区切换时区)
                        TimeZoneInfo chinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        DateTime chinaTime = TimeZoneInfo.ConvertTime(easternTime, easternZone, chinaZone);

                        // 保存到设置
                        Properties.Settings.Default.LimitedTime = chinaTime;
                        Properties.Settings.Default.Save();
                        //await ModernDialog.ShowInfoAsync($"{chinaTime}", "测试");
                        return chinaTime;
                    }
                }
            }
            catch (Exception ex)
            {
                // 显示错误信息
                _ = ModernDialog.ShowInfoAsync($"无法获取Fab免费资产信息: {ex.Message}", "错误");
                // 返回null
                return null;
            }

            return null;
        }
    }
}
