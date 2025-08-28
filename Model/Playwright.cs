using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace unreal_GUI.Model
{
    internal class Playwright
    {
        /// <summary>
        /// 启动浏览器并导航到指定URL获取页面内容
        /// </summary>
        /// <param name="url">要导航到的URL</param>
        /// <returns>页面内容</returns>
        public static async Task<string> GetPageContentAsync(string url)
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, // 设置为false可以查看浏览器操作
                Channel = "msedge"
            });
            var page = await browser.NewPageAsync();

            // 设置User-Agent以避免被检测为自动化浏览器
            await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
            {
                ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
            });

            // 导航到目标页面
            var response = await page.GotoAsync(url);

            // 检查响应状态码
            if (response?.Status == 403)
            {
                throw new Exception("网站返回403 Forbidden错误，可能已被反爬虫机制拦截");
            }

            // 等待页面加载完成
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 截图并保存
            await page.ScreenshotAsync(new PageScreenshotOptions()
            {
                Path = "screenshot.png"
            });

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
                Headless = true,
                Channel = "msedge"
            });
            var page = await browser.NewPageAsync();

            // 设置User-Agent以避免被检测为自动化浏览器
            await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
            {
                ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36"
            });

            // 导航到目标页面
            var response = await page.GotoAsync(url);

            // 检查响应状态码
            if (response?.Status == 403)
            {
                throw new Exception("网站返回403 Forbidden错误，可能已被反爬虫机制拦截");
            }

            // 等待页面加载完成
            //await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

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
