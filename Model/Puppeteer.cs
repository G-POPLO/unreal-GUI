using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace unreal_GUI.Model
{
    /// <summary>
    /// 使用PuppeteerSharp控制无头浏览器以获取动态加载的网页内容
    /// </summary>
    internal class Puppeteer
    {
        // Edge浏览器的可执行文件路径
        private static readonly string EdgeExecutablePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

        /// <summary>
        /// 启动无头浏览器并导航到指定URL
        /// </summary>
        /// <param name="url">要导航到的URL</param>
        /// <returns>页面内容</returns>
        public static async Task<string> GetPageContentAsync(string url)
        {
            // Initialization plugin builder
            var extra = new PuppeteerExtra();

            // Use stealth plugin
            extra.Use(new StealthPlugin());

            // 启动浏览器

            var launchOptions = new LaunchOptions
            {
                Headless = true, // 设置为false可以查看浏览器操作
                ExecutablePath = EdgeExecutablePath, // 指定Edge浏览器路径
          //       Args = new[] {
          //"--disable-blink-features=AutomationControlled"  // 伪装为非Headless浏览器
          //"--no-sandbox",
          //"--window-size=1920,1080",
          //"--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36"
          //"--disable-infobars"
          //}
            };

            using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();
            
            // 导航到目标页面
            await page.GoToAsync(url);

            // 修改User-Agent和相关属性以避免被检测为自动化浏览器
            //await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
            //await page.EvaluateExpressionAsync("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
            // 等待页面加载完成，确保异步数据已加载
            // 这里使用一个通用的等待，实际项目中可能需要根据具体元素调整

            await page.WaitForSelectorAsync("body");

            // 获取页面内容
            return await page.GetContentAsync();
        }
    }
}
