using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using SevenZip;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Notifications;

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
                    string updateBody = release_info["body"]?.ToString() ?? "无更新内容";
                    bool? result = await ModernDialog.ShowMarkdownAsync($"发现新版本{latestVersion}\n\n更新内容:\n{updateBody}\n\n是否下载？", "提示");
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
                await ModernDialog.ShowInfoAsync($"获取更新失败：{ex.Message}", "提示");
            }
        }

        //private static void SendDownloadNotification()
        //{
        //    // TBD
        //}

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
                    // 创建 Toast 通知用于显示进度
                    var toastContentBuilder = new ToastContentBuilder()
                        .AddText("正在下载更新...")
                        .AddVisualChild(new AdaptiveProgressBar()
                        {
                            Value = new BindableProgressBarValue("progressValue"),
                            ValueStringOverride = new BindableString("progressValueString"),
                            Status = "正在下载..."
                        });

                    var toastNotification = new ToastNotification(toastContentBuilder.GetXml());
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotification);

                    using (var fileStream = File.Create(downloadPath))
                    {
                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");

                        // 使用 HttpRequestMessage 来支持进度报告
                        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
                        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var canReportProgress = totalBytes != -1;
                        var totalBytesRead = 0L;
                        var buffer = new byte[8192];
                        var bytesRead = 0;

                        using (var downloadStream = await response.Content.ReadAsStreamAsync())
                        {
                            while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                if (canReportProgress)
                                {
                                    var progress = (double)totalBytesRead / totalBytes;
                                    // 更新 Toast 通知的进度
                                    toastNotification.Data = new NotificationData();
                                    toastNotification.Data.Values["progressValue"] = progress.ToString();
                                    toastNotification.Data.Values["progressValueString"] = $"{progress:P0}";
                                    ToastNotificationManager.CreateToastNotifier().Update(toastNotification.Data, toastNotification.Tag, toastNotification.Group);
                                }
                            }
                        }
                    }

                    // 下载完成后更新 Toast 通知
                    toastContentBuilder = new ToastContentBuilder()
                        .AddText("下载完成")
                        .AddText("正在解压资源，请稍后...");
                    toastNotification = new ToastNotification(toastContentBuilder.GetXml());
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotification);


                    try
                    {

                        // 设置7zxa.dll
                        _7z.ConfigureSevenZip();
                        // 解压到download文件夹
                        SevenZipExtractor extractor = new(downloadPath);
                        string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                        extractor.ExtractArchive(extractPath);
                        File.Delete(downloadPath); // 删除压缩包


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












