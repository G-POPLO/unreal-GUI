using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using SevenZip;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace unreal_GUI.Model
{
    class UpdateAndExtract
    {
        public static JObject release_info;
        public static string? latestVersion;

        public static async Task CheckForUpdatesAsync()
        {
            var currentVersion = Application.ResourceAssembly.GetName().Version.ToString();
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
                // 从API获取最新版本信息
                var response = Properties.Settings.Default.Gitcode
                    ? await client.GetAsync("https://api.gitcode.com/api/v5/repos/C-Poplo/unreal-GUI/releases/latest/?access_token=4RszX_1zdryXuvgwHbV-Edr7")
                    : await client.GetAsync("https://api.github.com/repos/G-POPLO/unreal-GUI/releases/latest");

                release_info = JObject.Parse(await response.Content.ReadAsStringAsync());
                latestVersion = release_info["tag_name"]?.ToString();

                if (Version.Parse(latestVersion) > Version.Parse(currentVersion))
                {
                    bool? result = await ModernDialog.ShowConfirmAsync($"发现新版本{latestVersion},是否下载？", "提示");
                    if (result == true)
                    {
                        await DownloadAndUpdateAsync();                       
                    }
                    else
                    {
                        return;
                    }


                }
            }
            catch (Exception ex) 
            {
                await ModernDialog.ShowInfoAsync($"下载失败：{ex.Message}", "提示");
            }            
        }

        public static async Task DownloadAndUpdateAsync()
        {
            //if (release_info == null) return;

            try
            {
                string? downloadUrl = release_info["assets"]
                    ?.FirstOrDefault(a => a["name"]?.ToString().EndsWith(".7z") == true)?["browser_download_url"]?.ToString();

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    var downloadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                    Directory.CreateDirectory(downloadDir);
                    var downloadPath = Path.Combine(downloadDir, $"{latestVersion}.7z");
                    using (var fileStream = File.Create(downloadPath))
                    {

                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
                        HttpResponseMessage download = await client.GetAsync(downloadUrl);
                        await download.Content.CopyToAsync(fileStream);
                    }

                    await ModernDialog.ShowInfoAsync($"已下载到：{downloadPath}", "下载完成");

                

                    try
                    {

                        // 设置7zxa.dll
                        _7z.ConfigureSevenZip();
                        // 解压到download文件夹
                        SevenZipExtractor extractor = new SevenZipExtractor(downloadPath);
                        string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                        extractor.ExtractArchive(extractPath);
                        File.Delete(downloadPath);


                        // 启动Update.bat并退出程序
                        var updateBatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.bat");
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
    
          










