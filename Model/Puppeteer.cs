using PuppeteerSharp;
using System;
using System.Threading.Tasks;

namespace unreal_GUI.Model
{
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
            // 启动浏览器
            var launchOptions = new LaunchOptions
            {
                Headless = true, // 设置为false可以查看浏览器操作
                ExecutablePath = EdgeExecutablePath // 指定Edge浏览器路径
            };

            using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();
            
            // 导航到目标页面
            await page.GoToAsync(url);
            
            // 等待页面加载完成，确保异步数据已加载
            // 这里使用一个通用的等待，实际项目中可能需要根据具体元素调整
           
            await page.WaitForSelectorAsync("body");

            // 获取页面内容
            return await page.GetContentAsync();
        }
    }
}
