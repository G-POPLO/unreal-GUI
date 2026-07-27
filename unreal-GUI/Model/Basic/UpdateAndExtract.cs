using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Notifications;

namespace unreal_GUI.Model.Basic
{
    class UpdateAndExtract
    {
        public static JsonDocument release_info;
        public static string latestVersion;

        // Shared HttpClient to avoid socket exhaustion from repeated instantiation (P2).
        private static readonly HttpClient SharedHttpClient = new HttpClient();

        static UpdateAndExtract()
        {
            SharedHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("unreal-GUI");
        }

        /// <summary>
        /// 将字节数格式化为更易读的格式（如KB, MB等）
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化后的字符串</returns>
        private static string FormatBytes(double bytes)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
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
                // 从GitHub API获取最新版本信息
                var response = await SharedHttpClient.GetAsync("https://api.github.com/repos/G-POPLO/unreal-GUI/releases/latest");
                response.EnsureSuccessStatusCode();

                release_info = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                // 检查是否是限流响应
                if (release_info.RootElement.TryGetProperty("message", out var msgElement) &&
                    !release_info.RootElement.TryGetProperty("tag_name", out _))
                {
                    throw new Exception($"GitHub API 返回错误: {msgElement.GetString()}");
                }

                if (!release_info.RootElement.TryGetProperty("tag_name", out var tagNameElement))
                {
                    throw new Exception("GitHub API 响应中未找到 tag_name 字段");
                }
                latestVersion = tagNameElement.GetString();

                if (Version.Parse(latestVersion) > Version.Parse(currentVersion))
                {
                    string updateBody = release_info.RootElement.TryGetProperty("body", out JsonElement bodyElement) ? bodyElement.GetString() ?? "无更新内容" : "无更新内容";
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
                bool usePatch = Properties.Settings.Default.PatchUpdate;
                string assetName = usePatch ? "acsync_patch.7z" : "unreal_setup.exe";
                string downloadUrl = null;
                if (release_info.RootElement.TryGetProperty("assets", out JsonElement assetsArray))
                {
                    foreach (var asset in assetsArray.EnumerateArray())
                    {
                        if (asset.TryGetProperty("name", out JsonElement nameElement))
                        {
                            var name = nameElement.GetString();
                            if (name?.Equals(assetName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(downloadUrl))
                {
                    // 当启用第三方更新源时，使用gh-proxy加速下载
                    if (Properties.Settings.Default.NonGithub)
                    {
                        downloadUrl = $"https://gh-proxy.org/{downloadUrl}";
                    }

                    var downloadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
                    Directory.CreateDirectory(downloadDir);
                    var downloadPath = Path.Combine(downloadDir, assetName);

                    // 获取文件大小
                    var response = await SharedHttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
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
                        using var stream = await response.Content.ReadAsStreamAsync();
                        using var fileStream = File.Create(downloadPath);
                        var buffer = new byte[81920];
                        long totalRead = 0;
                        int read;
                        uint sequenceNumber = 1; // 添加序列号以确保更新顺序正确
                        DateTime lastUpdateTime = DateTime.Now;
                        long lastTotalRead = 0;
                        string downloadSpeed = "计算中...";
                        // P1: throttle Toast updates — at most every 200ms OR on each full percent advance.
                        DateTime lastNotifyTime = DateTime.MinValue;
                        int lastPercent = -1;

                        while ((read = await stream.ReadAsync(buffer)) > 0)
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, read));
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

                                bool shouldNotify = (now - lastNotifyTime).TotalMilliseconds >= 200
                                    || percent > lastPercent;
                                if (!shouldNotify)
                                {
                                    continue;
                                }

                                lastNotifyTime = now;
                                lastPercent = percent;
                                var data = new NotificationData
                                {
                                    SequenceNumber = sequenceNumber++ // 递增序列号
                                };
                                data.Values["progressValue"] = progress.ToString("F2");
                                data.Values["progressText"] = $"{percent}%";
                                data.Values["downloadSpeed"] = downloadSpeed; // 更新下载速度

                                // P1: BeginInvoke avoids blocking the download thread on the UI pump.
                                // Discard (_) suppresses CS4014 — fire-and-forget is intentional.
                                _ = Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    ToastNotificationManagerCompat.CreateToastNotifier().Update(data, toastTag, toastGroup);
                                }));
                            }
                        }
                    });

                    if (usePatch)
                    {
                        await ApplyPatchAndRestartAsync(downloadPath, toastTag, toastGroup);
                    }
                    else
                    {
                        try
                        {
                            // 下载完成后关闭通知
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ToastNotificationManagerCompat.History.Remove(toastTag, toastGroup);
                            });

                            // 启动unreal_setup.exe并退出程序
                            System.Diagnostics.Process.Start(downloadPath);
                            Environment.Exit(0);

                        }
                        catch (Exception ex)
                        {
                            await ModernDialog.ShowErrorAsync($"启动安装程序失败：{ex.Message}", "提示");
                        }
                    }
                }
                else
                {
                    string errorMsg = usePatch
                        ? "下载失败，无法从服务器获取增量更新包"
                        : "下载失败，无法从服务器获取下载链接";
                    await ModernDialog.ShowErrorAsync(errorMsg, "提示");
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"下载失败：{ex}", "提示");
            }
        }

        private static async Task ApplyPatchAndRestartAsync(string patchPath, string toastTag, string toastGroup)
        {
            var installDir = AppDomain.CurrentDomain.BaseDirectory;
            var sevenZipPath = Path.Combine(installDir, "App", "7za.exe");

            // 更新 Toast 状态为"正在应用补丁"
            Application.Current.Dispatcher.Invoke(() =>
            {
                var data = new NotificationData
                {
                    SequenceNumber = 1
                };
                data.Values["progressValue"] = "0.50";
                data.Values["progressText"] = "50%";
                data.Values["downloadSpeed"] = "正在应用补丁...";
                ToastNotificationManagerCompat.CreateToastNotifier().Update(data, toastTag, toastGroup);
            });

            //try
            //{
            //    await Task.Run(() =>
            //    {
            //        SyncPatch.ApplyPatch(
            //            targetPath: installDir,
            //            patchFile: patchPath,
            //            excludeExtensions: [".json", ".ini"],
            //            sevenZipPath: sevenZipPath);
            //    });

            //    // 更新 Toast 状态为"更新完成"
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        var data = new NotificationData
            //        {
            //            SequenceNumber = 2
            //        };
            //        data.Values["progressValue"] = "1.00";
            //        data.Values["progressText"] = "100%";
            //        data.Values["downloadSpeed"] = "更新完成，正在重启...";
            //        ToastNotificationManagerCompat.CreateToastNotifier().Update(data, toastTag, toastGroup);
            //    });

            //    // 短暂停留以便用户看到"更新完成"状态
            //    await Task.Delay(2000);

            //    // 关闭通知
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        ToastNotificationManagerCompat.History.Remove(toastTag, toastGroup);
            //    });

            //    // 重启应用
            //    var exePath = Path.Combine(installDir, "Unreal-GUI.exe");
            //    if (File.Exists(exePath))
            //    {
            //        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            //        {
            //            FileName = exePath,
            //            UseShellExecute = true
            //        });
            //    }
            //    Environment.Exit(0);
            //}
            //catch (Exception ex)
            //{
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        ToastNotificationManagerCompat.History.Remove(toastTag, toastGroup);
            //    });
            //    await ModernDialog.ShowErrorAsync($"应用补丁失败：{ex.Message}", "提示");
            //}
        }
    }
}