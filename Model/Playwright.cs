using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using unreal_GUI.Properties;

namespace unreal_GUI.Model
{
    internal class Playwright
    {
        /// <summary>
        /// 启动浏览器并导航到指定URL并截图获取结果(Debug Only)
        /// </summary>
        /// <param name="url">要导航到的URL</param>
        /// <returns>页面内容</returns>
        public static async Task<string> GetPageContentAsync(string url)
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Channel = "msedge",
                Args =
                [
                    "--disable-blink-features=AutomationControlled",
                    "--start-maximized"
                ]
            });

            // 设置 UserAgent
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0",                          
            });

            var page = await context.NewPageAsync();

            // 注入脚本隐藏常见自动化特征
            //await page.AddInitScriptAsync(@"Object.defineProperty(navigator, 'webdriver', { get: () => undefined });");
            //await page.AddInitScriptAsync(@"window.chrome = { runtime: {} };");
            // 实测：add_init_script 设置为 false才能更好隐藏特征


            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
            });      

            // 截图保存到本地
            string screenshotPath = "screenshot.png";
            await page.ScreenshotAsync(new PageScreenshotOptions()
            {
                Path = screenshotPath,
                FullPage = true // 截取完整网页
            });

            // 复制到桌面
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string destPath = Path.Combine(desktopPath, "screenshot.png");
            File.Copy(screenshotPath, destPath, true);

            // 获取页面内容
            return await page.ContentAsync();
        }

        /// <summary>
        /// 在指定URL页面中查找具有特定class的h2元素
        /// </summary>
        /// <param name="url">要导航到的URL</param>
        /// <param name="classSelectors">class选择器数组</param>
        /// <returns>h2元素的文本内容</returns>
        public static async Task<string> GetH2ElementTextAsync(string url, params string[] classSelectors)
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Properties.Settings.Default.HeadlessEnabled, // 根据设置决定是否使用无头模式
                Channel = "msedge",
                Args = new[]
                {
                    "--disable-blink-features=AutomationControlled",
                    "--start-maximized"
                }
            });
            
            // 设置User-Agent以避免被检测为自动化浏览器
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0",
            });
            var page = await browser.NewPageAsync();
            // 导航到目标页面
            var response = await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,               
            });

            // 构建CSS选择器
            string selector = "h2";
            if (classSelectors != null && classSelectors.Length > 0)
            {
                selector += "." + string.Join(".", classSelectors);
            }

            // 查找元素
            var element = await page.QuerySelectorAsync(selector);

            // 返回元素文本内容
            return element != null ? await element.InnerTextAsync() : string.Empty;
        }
    }
}
