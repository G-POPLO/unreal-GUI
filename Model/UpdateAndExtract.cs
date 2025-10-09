using Microsoft.Toolkit.Uwp.Notifications;
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

        /// <summary>
        /// 将字节数格式化为更易读的格式（如KB, MB等）
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化后的字符串</returns>
        private static string FormatBytes(double bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes /= 1024;
            }
            return $"{bytes:0.##} {sizes[order]}";
        }

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
                await ModernDialog.ShowErrorAsync($"获取更新失败：{ex.Message}", "提示");
            }
        }



        public static async Task DownloadAndUpdateAsync()
        {
            try
            {
                string? downloadUrl = release_info["assets"]
                    ?.FirstOrDefault(a => a["name"]?.ToString().EndsWith(".7z") == true)?["browser_download_url"]?.ToString();

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    var downloadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                    Directory.CreateDirectory(downloadDir);
                    var downloadPath = Path.Combine(downloadDir, $"{latestVersion}.7z");

                    // 获取文件大小
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
                    var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    // Toast 通知初始化（带进度条）
                    string toastTag = "download-progress";
                    string toastGroup = "update-group";
                    string fileSizeText = totalBytes != -1 ? $"文件大小: {FormatBytes(totalBytes)}" : "文件大小: 未知";
                    var toastContent = new ToastContentBuilder()
                        .AddText($"正在下载更新包 {latestVersion}")
                        .AddText(fileSizeText) // 显示文件大小
                        .AddVisualChild(new AdaptiveProgressBar()
                        {
                            Title = "下载进度",
                            Value = new BindableProgressBarValue("progressValue"),
                            ValueStringOverride = new BindableString("progressText"),
                            Status = new BindableString("downloadSpeed") // 绑定到下载速度
                        })
                        .GetToastContent();

                    // 创建通知并设置初始数据
                    var toast = new ToastNotification(toastContent.GetXml())
                    {
                        Tag = toastTag,
                        Group = toastGroup,
                        Data = new NotificationData()
                        {
                            Values =
                            {
                                ["progressValue"] = "0.00",
                                ["progressText"] = "0%",
                                ["downloadSpeed"] = "计算中..."
                            },
                            SequenceNumber = 1
                        }
                    };

                    ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);

                    // 在后台线程执行下载操作，避免阻塞UI线程
                    await Task.Run(async () =>
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = File.Create(downloadPath))
                        {
                            var buffer = new byte[81920];
                            long totalRead = 0;
                            int read;
                            uint sequenceNumber = 1; // 添加序列号以确保更新顺序正确
                            DateTime lastUpdateTime = DateTime.Now;
                            long lastTotalRead = 0;
                            string downloadSpeed = "计算中...";
                            
                            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;
                                
                                // 计算下载速度
                                DateTime now = DateTime.Now;
                                TimeSpan timeSpan = now - lastUpdateTime;
                                if (timeSpan.TotalSeconds >= 1) // 每秒更新一次速度
                                {
                                    long bytesDownloaded = totalRead - lastTotalRead;
                                    double speed = bytesDownloaded / timeSpan.TotalSeconds;
                                    downloadSpeed = $"下载速度: {FormatBytes(speed)}/s";
                                    lastTotalRead = totalRead;
                                    lastUpdateTime = now;
                                }
                                
                                if (canReportProgress)
                                {
                                    double progress = totalRead / (double)totalBytes;
                                    int percent = (int)(progress * 100);
                                    // 每次读取都实时更新进度条
                                    var data = new NotificationData
                                    {
                                        SequenceNumber = sequenceNumber++ // 递增序列号
                                    };
                                    data.Values["progressValue"] = progress.ToString("F2");
                                    data.Values["progressText"] = $"{percent}%";
                                    data.Values["downloadSpeed"] = downloadSpeed; // 更新下载速度
                                    
                                    // 在UI线程上更新Toast通知
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        ToastNotificationManagerCompat.CreateToastNotifier().Update(data, toastTag, toastGroup);
                                    });
                                }
                            }
                        }
                        
                        // 下载完成后更新通知状态为"正在解压文件"
                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    uint sequenceNumber = 1; // 添加序列号以确保更新顺序正确
                        //    var data = new NotificationData
                        //    {
                        //        SequenceNumber = sequenceNumber++
                        //    };
                        //    data.Values["progressValue"] = "1.00";
                        //    data.Values["progressText"] = "100%";
                        //    data.Values["downloadSpeed"] = "正在解压文件";
                        //    ToastNotificationManagerCompat.CreateToastNotifier().Update(data, toastTag, toastGroup);
                        //});
                    });


                    // 设置7zxa.dll
                    _7z.ConfigureSevenZip();
                    
                    try
                    {
                        // 解压到download文件夹
                        string extractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                        
                        // 在后台线程执行解压操作，避免阻塞UI线程
                        await Task.Run(() =>
                        {
                            using (var extractor = new SevenZipExtractor(downloadPath))
                            {
                                extractor.ExtractArchive(extractPath);
                            }
                            
                            // 解压完成后关闭通知
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ToastNotificationManagerCompat.History.Remove(toastTag, toastGroup);
                            });
                        });
                        
                        File.Delete(downloadPath); // 删除压缩包

                        // 启动Update.bat并退出程序
                        var updateBatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.bat");
                        System.Diagnostics.Process.Start(updateBatPath);
                        Environment.Exit(0);

                    }
                    catch (Exception ex)
                    {
                        await ModernDialog.ShowErrorAsync($"解压失败：{ex.Message}", "提示");
                    }
                }
                else
                {
                    await ModernDialog.ShowErrorAsync("下载失败，无法从服务器获取下载链接", "提示");
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"下载失败：{ex}", "提示");
            }
        }
    }
}