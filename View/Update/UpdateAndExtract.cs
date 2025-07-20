using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using SevenZip;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace unreal_GUI.View.Update
{
    class UpdateAndExtract
    {
        private static JObject _releaseInfo;
        private static string? latestVersion;

        public static async Task<bool> CheckForUpdatesAsync()
        {
            var currentVersion = Application.ResourceAssembly.GetName().Version.ToString();
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");

                var response = Properties.Settings.Default.Gitcode
                    ? await client.GetAsync("https://api.gitcode.com/api/v5/repos/C-Poplo/unreal-GUI/releases/latest/?access_token=4RszX_1zdryXuvgwHbV-Edr7")
                    : await client.GetAsync("https://api.github.com/repos/G-POPLO/unreal-GUI/releases/latest");

                _releaseInfo = JObject.Parse(await response.Content.ReadAsStringAsync());
                latestVersion = _releaseInfo["tag_name"]?.ToString();

                return !string.IsNullOrEmpty(latestVersion) && 
                       Version.Parse(latestVersion) > Version.Parse(currentVersion);
            }
            catch { return false; }
        }

        public static async Task DownloadAndUpdateAsync()
        {
            if (_releaseInfo == null) return;

            try
            {
                string? downloadUrl = _releaseInfo["assets"]
                    ?.FirstOrDefault(a => a["name"]?.ToString().EndsWith(".7z") == true)?["browser_download_url"]?.ToString();

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    string downloadPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download", $"{latestVersion}.7z");
                    using (var fileStream = System.IO.File.Create(downloadPath))
                    {

                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
                        HttpResponseMessage download = await client.GetAsync(downloadUrl);
                        await download.Content.CopyToAsync(fileStream);
                    }

                    await ModernDialog.ShowInfoAsync($"已下载到：{downloadPath}", "下载完成");

                    // 解压7Z文件到download文件夹
                    try
                    {

                        // 设置7zxa.dll
                        _7z.ConfigureSevenZip();
                        // 解压到download文件夹
                        var extractor = new SevenZipExtractor(downloadPath);
                        var extractPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                        extractor.ExtractArchive(extractPath);

                        // 启动Update.bat并退出程序
                        var updateBatPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.bat");
                        System.Diagnostics.Process.Start(updateBatPath);
                        Environment.Exit(0);

                    }
                    catch (Exception ex)
                    {
                        await ModernDialog.ShowInfoAsync($"解压失败：{ex.Message}", "提示");
                    }
                }
                else
                {
                    await ModernDialog.ShowInfoAsync("下载失败，无法从服务器获取下载链接", "提示");
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"下载失败：{ex.Message}", "提示");
            }
        }
    }
}
    
          










