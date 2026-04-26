using Microsoft.Playwright;

namespace reminder
{
    internal class Playwright
    {
        /// <summary>
        /// 用户数据目录，用于持久化浏览器状态（Cookie、登录状态等）
        /// </summary>
        private static readonly string UserDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UnrealGUI", "FabBrowserData");

        /// <summary>
        /// Fab限时免费页面URL
        /// </summary>
        private const string FabLimitedTimeFreeUrl = "https://www.fab.com/limited-time-free?lang=en";

        /// <summary>
        /// 启动浏览器并导航到指定URL并截图获取结果(Debug Only)
        /// </summary>
        /// <param name="url">要导航到的URL</param>
        /// <returns>页面内容</returns>
        public static async Task<string> GetPageContentAsync(string url)
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            IniConfig iniConfig = new();
            byte browerType = (byte)iniConfig.ReadInt("BrowerType", 0);
            string channel = browerType == 1 ? "chrome" : "msedge";
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Channel = channel,
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
#if DEBUG
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
#endif
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
            IniConfig iniConfig = new();
            byte browerType = (byte)iniConfig.ReadInt("BrowerType", 0);
            string channel = browerType == 1 ? "chrome" : "msedge";
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, // 默认使用无头模式
                Channel = channel,
                Args =
                [
                    "--disable-blink-features=AutomationControlled",
                    "--start-maximized"
                ]
            });

            // 设置User-Agent和Locale以避免被检测为自动化浏览器，并强制使用英语
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0",

            });
            var page = await browser.NewPageAsync();
            // 导航到目标页面，不等待整个DOM加载完成
            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.Commit, // 只等待导航请求提交
            });

            // 构建CSS选择器
            string selector = "h2";
            if (classSelectors != null && classSelectors.Length > 0)
            {
                selector += "." + string.Join(".", classSelectors);
            }

            try
            {
                // 等待目标元素出现，设置适当的超时时间（15秒）
                var element = await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    Timeout = 15000, // 超时时间设置为15秒
                    State = WaitForSelectorState.Visible // 等待元素可见
                });

                // 返回元素文本内容
                return element != null ? await element.InnerTextAsync() : string.Empty;

            }
            catch (TimeoutException)
            {
#if DEBUG
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
#endif

                return string.Empty;

            }
        }

        /// <summary>
        /// 自动化领取Fab免费资产
        /// </summary>
        /// <returns>领取结果</returns>
        public static async Task<bool> AutoClaimFabAssetsAsync()
        {
            using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            IniConfig iniConfig = new();
            byte browerType = (byte)iniConfig.ReadInt("BrowerType", 0);
            string channel = browerType == 1 ? "chrome" : "msedge";

            // 确保用户数据目录存在
            Directory.CreateDirectory(UserDataDir);

            // 使用持久化上下文以保存登录状态
            await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(
                UserDataDir,
                new BrowserTypeLaunchPersistentContextOptions
                {
                    Headless = false, // 必须使用有头模式以便用户手动登录
                    Channel = channel,
                    Args =
                    [
                        "--disable-blink-features=AutomationControlled",
                        "--start-maximized"
                    ],
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36 Edg/140.0.0.0",

                });

            var page = await browser.NewPageAsync();

            try
            {
                // 步骤1：导航到Fab限时免费页面
                Console.WriteLine("[步骤1] 正在访问Fab限时免费页面...");
                await page.GotoAsync(FabLimitedTimeFreeUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                });

                // 等待页面加载完成
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // 步骤2：检查是否已登录
                Console.WriteLine("[步骤2] 检查登录状态...");
                bool isLoggedIn = await CheckIsLoggedInAsync(page);

                if (!isLoggedIn)
                {
                    Console.WriteLine("未检测到登录状态，请手动登录...");
                    // 等待用户手动登录
                    await WaitForUserLoginAsync(page);
                }

                // 步骤3：获取所有资产卡片
                Console.WriteLine("[步骤3] 获取资产卡片列表...");
                int assetCount = await GetAssetCardCountAsync(page);
                Console.WriteLine($"发现 {assetCount} 个资产卡片");

                if (assetCount == 0)
                {
                    Console.WriteLine("未找到任何资产卡片");
                    return false;
                }

                // 步骤4：遍历每个资产卡片并添加到购物车
                Console.WriteLine("[步骤4] 开始添加资产到购物车...");
                for (int i = 0; i < assetCount; i++)
                {
                    Console.WriteLine($"正在处理第 {i + 1}/{assetCount} 个资产...");
                    bool success = await AddAssetToCartAsync(page, i);
                    if (!success)
                    {
                        Console.WriteLine($"第 {i + 1} 个资产添加到购物车失败");
                    }
                }

                // 步骤5：点击下单按钮
                Console.WriteLine("[步骤5] 正在提交订单...");
                bool orderSuccess = await PlaceOrderAsync(page);

                if (orderSuccess)
                {
                    Console.WriteLine("[完成] 订单提交成功！");
                }
                else
                {
                    Console.WriteLine("[完成] 订单提交失败或无需下单");
                }

                return orderSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自动化领取过程中发生错误: {ex.Message}");
                return false;
            }
            finally
            {
                // 关闭页面
                await page.CloseAsync();
            }
        }

        /// <summary>
        /// 获取资产卡片数量
        /// </summary>
        private static async Task<int> GetAssetCardCountAsync(IPage page)
        {
            try
            {
                // 使用 Playwright 原生方法获取资产卡片数量
                var cards = await page.QuerySelectorAllAsync(".oeSuy4_9.vL3jJySf");
                return cards.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取资产卡片数量时出错: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 检查用户是否已登录
        /// </summary>
        private static async Task<bool> CheckIsLoggedInAsync(IPage page)
        {
            try
            {
                // 使用 Playwright 原生方法检查页面上是否存在"添加到购物车"按钮
                var cards = await page.QuerySelectorAllAsync(".oeSuy4_9.vL3jJySf");
                if (cards.Count == 0)
                {
                    return false;
                }

                var addButton = await cards[0].QuerySelectorAsync(".fabkit-Button-root.fabkit-Button--icon.fabkit-Button--rounded.fabkit-Button--sm.fabkit-Button--blurry");
                return addButton != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查登录状态时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 等待用户手动登录
        /// </summary>
        private static async Task WaitForUserLoginAsync(IPage page)
        {
            Console.WriteLine("请在浏览器中完成登录，登录成功后程序将自动继续...");

            // 循环检查登录状态，最多等待5分钟
            const int maxWaitMs = 300000; // 5分钟
            const int checkIntervalMs = 2000; // 每2秒检查一次
            int elapsedMs = 0;

            while (elapsedMs < maxWaitMs)
            {
                await Task.Delay(checkIntervalMs);
                elapsedMs += checkIntervalMs;

                bool isLoggedIn = await CheckIsLoggedInAsync(page);
                if (isLoggedIn)
                {
                    Console.WriteLine("检测到登录状态，继续执行...");
                    return;
                }

                // 每30秒提示一次
                if (elapsedMs % 30000 == 0)
                {
                    Console.WriteLine($"已等待 {elapsedMs / 1000} 秒，请完成登录...");
                }
            }

            throw new TimeoutException("等待登录超时（5分钟），请重新运行程序");
        }

        /// <summary>
        /// 将指定索引的资产添加到购物车
        /// </summary>
        private static async Task<bool> AddAssetToCartAsync(IPage page, int index)
        {
            try
            {
                // 步骤4.1：点击"添加到购物车"按钮
                var addButton = await page.QuerySelectorAsync($".oeSuy4_9.vL3jJySf:nth-child({index + 1}) .fabkit-Button-root.fabkit-Button--icon.fabkit-Button--rounded.fabkit-Button--sm.fabkit-Button--blurry");

                if (addButton == null)
                {
                    Console.WriteLine($"  [步骤4.1] 第 {index + 1} 个资产未找到添加到购物车按钮，可能已添加或无需添加");
                    return false;
                }

                await addButton.ClickAsync();
                Console.WriteLine($"  [步骤4.1] 已点击第 {index + 1} 个资产的添加到购物车按钮");

                // 步骤4.2：等待许可证选择对话框出现
                Console.WriteLine($"  [步骤4.2] 等待许可证选择对话框...");
                await page.WaitForSelectorAsync("input[name=\"License\"]", new PageWaitForSelectorOptions
                {
                    Timeout = 10000,
                    State = WaitForSelectorState.Visible
                });

                // 步骤4.3：根据设置选择许可证类型
                Console.WriteLine($"  [步骤4.3] 选择许可证类型...");
                await SelectLicenseAsync(page);

                // 步骤4.4：点击"添加至购物车"确认按钮
                Console.WriteLine($"  [步骤4.4] 确认添加到购物车...");
                var confirmButton = await page.QuerySelectorAsync(".fabkit-Button-root.fabkit-Button--md.fabkit-Button--primary");
                if (confirmButton != null)
                {
                    await confirmButton.ClickAsync();
                    Console.WriteLine($"  [步骤4.4] 第 {index + 1} 个资产已确认添加到购物车");
                }

                // 步骤4.5：等待对话框关闭
                Console.WriteLine($"  [步骤4.5] 等待对话框关闭...");
                try
                {
                    await page.WaitForSelectorAsync("input[name=\"License\"]", new PageWaitForSelectorOptions
                    {
                        Timeout = 10000,
                        State = WaitForSelectorState.Hidden
                    });
                }
                catch (TimeoutException)
                {
                    // 对话框可能已经关闭或不存在，继续执行
                    Console.WriteLine($"  [步骤4.5] 许可证对话框已关闭或无需等待");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [错误] 添加第 {index + 1} 个资产到购物车时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 根据配置选择许可证类型
        /// </summary>
        private static async Task SelectLicenseAsync(IPage page)
        {
            try
            {
                IniConfig iniConfig = new();
                // 读取许可证设置，true=个人，false=专业
                bool hasUsingPro = iniConfig.ReadBool("HasUsingPro", false);

                // 使用 Playwright 原生方法选择许可证
                var licenseInputs = await page.QuerySelectorAllAsync("input[name=\"License\"]");

                if (licenseInputs.Count >= 2)
                {
                    // 个人许可证通常是第一个选项
                    int targetIndex = hasUsingPro ? 0 : 1;
                    await licenseInputs[targetIndex].ClickAsync();
                    Console.WriteLine($"已选择{(hasUsingPro ? "个人" : "专业")}许可证");
                }
                else if (licenseInputs.Count == 1)
                {
                    await licenseInputs[0].ClickAsync();
                    Console.WriteLine("已选择唯一可用的许可证选项");
                }
                else
                {
                    Console.WriteLine("未找到许可证选项");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"选择许可证时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 点击下单按钮完成订单
        /// </summary>
        private static async Task<bool> PlaceOrderAsync(IPage page)
        {
            try
            {
                // 步骤5.1：点击主页购物车按钮查看购物车
                Console.WriteLine("  [步骤5.1] 打开购物车...");
                var cartButton = await page.QuerySelectorAsync(".fabkit-Button-root.fabkit-Button--icon.fabkit-Button--sm.fabkit-Button--ghost.fabkit-MegaMenu-iconButton");
                if (cartButton != null)
                {
                    await cartButton.ClickAsync();
                    Console.WriteLine("  [步骤5.1] 已打开购物车");
                }

                // 步骤5.2：等待页面加载
                Console.WriteLine("  [步骤5.2] 等待购物车页面加载...");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(2000); // 额外等待2秒确保购物车页面加载完成

                // 步骤5.3：查找并点击下单按钮
                Console.WriteLine("  [步骤5.3] 查找下单按钮...");
                IElementHandle? orderButton = null;

                // 尝试不同的选择器
                orderButton = await page.QuerySelectorAsync(".fabkit-StickyElement-root.fabkit-StickyElement--top-right.fabkit-StickyElement--show");
                orderButton ??= await page.QuerySelectorAsync("button:has-text(\"Place Order\")");
                orderButton ??= await page.QuerySelectorAsync("button:has-text(\"下单\")");
                orderButton ??= await page.QuerySelectorAsync("button:has-text(\"Place order\")");
                orderButton ??= await page.QuerySelectorAsync("[aria-label*=\"Place Order\"]");
                orderButton ??= await page.QuerySelectorAsync("[aria-label*=\"下单\"]");

                if (orderButton != null)
                {
                    await orderButton.ClickAsync();
                    Console.WriteLine("  [步骤5.3] 已点击下单按钮");

                    // 步骤5.4：等待交易完成
                    Console.WriteLine("  [步骤5.4] 等待交易完成...");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    await Task.Delay(5000); // 额外等待5秒确保交易完成

                    return true;
                }
                else
                {
                    Console.WriteLine("  [步骤5.3] 未找到下单按钮，购物车可能为空或已下单");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [错误] 下单时出错: {ex.Message}");
                return false;
            }
        }
    }
}
