using HtmlAgilityPack;
using ModernWpf.Controls;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using unreal_GUI.Model;

namespace unreal_GUI.Model
{
    internal class Fab_Notification
    {
        private static readonly HttpClient client = new HttpClient();

        static Fab_Notification()
        {
            // 设置默认请求头以模拟真实浏览器访问
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36 Edg/139.0.0.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
        }

        public static async Task<DateTime?> GetLimitedTimeFreeEndDate()
        {
            try
            {
                // 创建请求消息
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.fab.com/limited-time-free");

                // 发送请求并获取响应
                var response = await client.SendAsync(request);
                string html;
                if (response.IsSuccessStatusCode)
                {
                    html = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // 处理HTTP错误状态码
                    string errorMessage = $"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}";
                    _ = ModernDialog.ShowInfoAsync($"无法获取Fab免费资产信息: {errorMessage}", "错误");
                    return null;
                }

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
                        
                        // 转换为北京时间
                        TimeZoneInfo chinaZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                        DateTime chinaTime = TimeZoneInfo.ConvertTime(easternTime, easternZone, chinaZone);

                        // 保存到设置
                        Properties.Settings.Default.LimitedTime = chinaTime;
                        Properties.Settings.Default.Save();
                        _ = ModernDialog.ShowInfoAsync($"{chinaTime}", "测试");
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
