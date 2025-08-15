using Newtonsoft.Json.Linq;
using SevenZip;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using unreal_GUI.Properties;

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
                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
                // 从API获取最新版本信息
                var response = Properties.Settings.Default.Gitcode
                    ? await client.GetAsync("https://api.gitcode.com/api/v5/repos/C-Poplo/unreal-GUI/releases/latest/?access_token=4RszX_1zdryXuvgwHbV-Edr7")
                    : await client.GetAsync("https://api.github.com/repos/G-POPLO/unreal-GUI/releases/latest");

                release_info = JObject.Parse(await response.Content.ReadAsStringAsync());
                latestVersion = release_info["tag_name"]?.ToString();

                if (Version.Parse(latestVersion) > Version.Parse(currentVersion))
                {
                    string updateBody = release_info["body"]?.ToString() ?? Lang.msgNoUpdateContent;
                    bool? result = await ModernDialog.ShowConfirmAsync(Lang.msgCheckForUpdates,0);
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
            catch (Exception)
            {
                await ModernDialog.ShowInfoAsync(Lang.exUpdate, 3);
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

                    await ModernDialog.ShowInfoAsync(Lang.msgDownloadPath,1);



                    try
                    {

                        // 设置7zxa.dll
                        _7z.ConfigureSevenZip();
                        // 解压到download文件夹
                        SevenZipExtractor extractor = new(downloadPath);
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
                        await ModernDialog.ShowInfoAsync(Lang.exDecompression,3);
                    }
                }
                else
                {
                    await ModernDialog.ShowInfoAsync(Lang.exNoInternet,3);
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync(Lang.exDownload,3);
            }
        }
    }
}












