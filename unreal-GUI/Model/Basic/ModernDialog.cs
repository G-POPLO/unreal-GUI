using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using unreal_GUI.Model.DialogContent;
using unreal_GUI.View.DialogContent;


namespace unreal_GUI.Model.Basic
{
    public static class ModernDialog
    {
        public static event EventHandler<string> NavigationRequested;

        /// <summary>
        /// 显示一个带 Yes/No 按钮的对话框
        /// </summary>
        public static async Task<bool?> ShowConfirmAsync(string message, string title = "提示")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "是",
                CloseButtonText = "否",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        /// <summary>
        /// 显示一个带 Yes/No 按钮的对话框，但消息内容支持 Markdown 格式
        /// </summary>
        public static async Task<bool?> ShowMarkdownAsync(string message, string title)
        {
            var markdownViewer = new Markdig.Wpf.MarkdownViewer
            {
                Markdown = message
            };
            markdownViewer.Document.FontFamily = new FontFamily("Microsoft YaHei UI");
            //markdownViewer.Document.Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"];
            var dialog = new ContentDialog
            {
                Title = title,
                Content = markdownViewer,
                PrimaryButtonText = "是",
                CloseButtonText = "否",
                DefaultButton = ContentDialogButton.Primary
            };
            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        /// <summary>
        /// 显示一个仅 OK 按钮的对话框，用于信息提示
        /// </summary>
        public static async Task ShowInfoAsync(string message, string title = "提示")
        {
            // 创建包含图标和文本内容的StackPanel
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // 添加提示图标
            var icon = new FontIcon
            {
                Glyph = "\uF167", // InfoSolid图标
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 20,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 直接使用TextBlock，支持文本换行和自适应大小
            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 400 // 限制最大宽度以确保可读性
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(textBlock);

            var dialog = new ContentDialog
            {
                Title = title,
                Content = stackPanel,
                CloseButtonText = "确定",
                // 启用自适应大小
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// 显示一个带错误信息的对话框，包含"复制信息到剪切板"和"确认"按钮
        /// </summary>
        public static async Task ShowErrorAsync(string message, string title = "错误")
        {
            // 创建包含图标和文本内容的StackPanel
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // 添加错误图标
            var icon = new FontIcon
            {
                Glyph = "\uEA39", // ErrorBadge图标
                FontFamily = new FontFamily("Segoe Fluent Icons"),
                FontSize = 20,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // 直接使用TextBlock，支持文本换行和自适应大小
            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 400 // 限制最大宽度以确保可读性
            };

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(textBlock);

            var dialog = new ContentDialog
            {
                Title = title,
                Content = stackPanel,
                PrimaryButtonText = "复制信息到剪切板",
                DefaultButton = ContentDialogButton.Close,
                CloseButtonText = "确认",
                // 启用自适应大小
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // 复制信息到剪切板
                Clipboard.SetText(message);
            }
        }

        /// <summary>
        /// 显示可自定义按钮的对话框
        /// </summary>

        public static async Task<ContentDialogResult> ShowCustomAsync
            (
            string message,
            string title,
            string primaryButton,
            string closeButton,
            string secondaryButton
             )

        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButton,
                SecondaryButtonText = secondaryButton,
                CloseButtonText = closeButton,
                DefaultButton = ContentDialogButton.Primary
            };

            return await dialog.ShowAsync();
        }

        public static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions { WriteIndented = true };
        }

        /// <summary>
        /// 显示"添加自定义按钮"的对话框
        /// </summary>
        public static async Task<ContentDialogResult> ShowAddCustomDialogAsync(JsonSerializerOptions options)
        {
            var content = new Add_DialogContent();
            var dialog = new ContentDialog
            {
                Title = "添加自定义按钮",
                PrimaryButtonText = "添加",
                CloseButtonText = "取消",
                Content = content,
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = false // 初始禁用添加按钮
            };

            // 设置对话框引用
            content.Dialog = dialog;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var buttonName = content.ButtonNameTextBox.Text;
                var folderPath = content.FolderPathTextBox.Text;
                if (!string.IsNullOrWhiteSpace(buttonName) && !string.IsNullOrWhiteSpace(folderPath))
                {
                    // 读取现有设置
                    var settings = new SettingsData();
                    if (File.Exists("settings.json"))
                    {
                        var json = File.ReadAllText("settings.json");
                        settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                    }

                    // 若不存在配置，则初始化列表
                    if (settings.CustomButtons == null)
                    {
                        settings.CustomButtons = [];
                    }
                    settings.CustomButtons.Add(new CustomButton { Name = buttonName, Path = folderPath });
                    // 保存更新后的设置
                    var jsonSettings = JsonSerializer.Serialize(settings, options);
                    File.WriteAllText("settings.json", jsonSettings);
                }
            }

            if (result == ContentDialogResult.Primary)
            {
                // 通知QuickAccess页面刷新
                NavigationRequested?.Invoke(null, "QuickAccess");
                return result;
            }

            return result;
        }



        /// <summary>
        /// 显示"删除自定义按钮"的对话框
        /// </summary>
        public static async Task<ContentDialogResult> ShowDeleteCustomDialogAsync()
        {
            var content = new Delete_DialogContent();

            // 读取现有设置并更新 ListBox
            if (File.Exists("settings.json"))
            {
                var json = File.ReadAllText("settings.json");
                var settings = JsonSerializer.Deserialize<SettingsData>(json);

                if (settings?.CustomButtons != null)
                {
                    content.ButtonsListBox.ItemsSource = settings.CustomButtons;
                }
            }

            // 为删除按钮添加删除完成后的刷新事件
            content.DeleteCompleted += (sender, args) =>
            {
                // 删除完成后立即通知QuickAccess页面刷新
                NavigationRequested?.Invoke(null, "QuickAccess");
            };

            ContentDialog dialog = new()
            {
                Title = "删除自定义文件夹",
                PrimaryButtonText = "关闭",
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };
            var result = await dialog.ShowAsync();

            return result;
        }


        /// <summary>
        /// 显示添加模板类别对话框
        /// </summary>
        /// <param name="enginePath">引擎安装路径，用于定位TemplateCategories.ini文件</param>
        /// <summary>
        /// 将文本附加到指定文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="text">要附加的文本</param>
        /// <returns>是否成功附加</returns>
        private static async Task<bool> AppendTextToFile(string filePath, string text)
        {
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using StreamWriter writer = new(filePath, true);
                // 添加空行分隔不同类别（如果文件已有内容）
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    await writer.WriteLineAsync();
                }
                await writer.WriteLineAsync(text);

                return true;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("保存类别失败：" + ex.Message, "错误");
                return false;
            }
        }

        public static async Task<ContentDialogResult> ShowCategoriesDialogAsync(string enginePath)
        {
            var content = new Add_Categories();

            ContentDialog dialog = new()
            {
                Title = "添加模板类别",
                PrimaryButtonText = "保存",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };

            var result = await dialog.ShowAsync();

            // 处理保存逻辑
            if (result == ContentDialogResult.Primary && content.ViewModel != null)
            {
                try
                {
                    // 获取TemplateCategories.ini文件路径
                    string categoriesFilePath = Path.Combine(enginePath, "Templates", "TemplateCategories.ini");

                    // 确保Templates目录存在
                    Directory.CreateDirectory(Path.GetDirectoryName(categoriesFilePath));

                    // 获取生成的类别文本
                    string categoriesText = content.ViewModel.GeneratedCategoriesText;

                    if (!string.IsNullOrEmpty(categoriesText))
                    {
                        // 调用AppendTextToFile函数将文本附加到文件
                        bool appendSuccess = await AppendTextToFile(categoriesFilePath, categoriesText);

                        if (appendSuccess)
                        {
                            // 通知用户保存成功
                            await ShowInfoAsync("类别已成功添加到TemplateCategories.ini文件。", "成功");

                            // 如果用户已选择图标，提示用户复制图标到正确位置
                            if (!string.IsNullOrEmpty(content.ViewModel.IconPath))
                            {
                                string targetIconPath = Path.Combine(enginePath, "Templates", "Media", content.ViewModel.CategoryKey + "_2X.png");
                                Directory.CreateDirectory(Path.GetDirectoryName(targetIconPath));

                                // 复制图标到目标位置
                                File.Copy(content.ViewModel.IconPath, targetIconPath, true);
                            }
                        }
                        else
                        {
                            await ShowInfoAsync("没有生成类别文本，请先填写必要信息并点击'生成Categories文本'按钮。", "提示");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync("保存类别失败：" + ex.Message, "错误");
                }
            }
            return result;
        }

        /// <summary>
        /// 显示压缩信息对话框
        /// </summary>
        /// <param name="inputPath">要压缩的文件或目录路径</param>
        /// <param name="outputDirectory">输出目录路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> ShowCompressInfoAsync(string inputPath, string outputDirectory, int compressionLevel = 5, bool solidCompress = true)
        {
            // 构建完整的输出压缩文件路径
            // 检查输入路径是否为.uproject文件
            string projectName = "Project";
            if (!string.IsNullOrEmpty(inputPath))
            {
                if (Path.GetExtension(inputPath) == ".uproject")
                {
                    projectName = Path.GetFileNameWithoutExtension(inputPath);
                }
                else
                {
                    // 如果是目录，查找其中的.uproject文件
                    if (Directory.Exists(inputPath))
                    {
                        var uprpojectFiles = Directory.GetFiles(inputPath, "*.uproject");
                        if (uprpojectFiles.Length > 0)
                        {
                            projectName = Path.GetFileNameWithoutExtension(uprpojectFiles[0]);
                        }
                    }
                }
            }
            string outputArchivePath = Path.Combine(outputDirectory, $"{projectName}.7z");

            var dialog = new ContentDialog
            {
                Title = "压缩项目",
                CloseButtonText = "关闭",
                PrimaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Close,
            };

            // 创建压缩信息控件
            var compressInfo = new CompressInfo();
            dialog.Content = compressInfo;

            // 设置Dialog属性
            compressInfo.Dialog = dialog;

            bool isCanceled = false;
            bool success = false;

            // 处理取消按钮
            dialog.PrimaryButtonClick += (sender, args) =>
            {
                args.Cancel = true; // 取消关闭对话框
                isCanceled = true;
                compressInfo.UpdateCompressInfo("正在取消压缩...");
            };

            // 异步执行压缩任务
            var compressTask = Task.Run(async () =>
            {
                try
                {
                    // 调用CompressCore.CompressFilesAsync进行压缩
                    success = await CompressCore.CompressFilesAsync(
                        inputPath,
                        outputArchivePath,
                        compressionLevel: compressionLevel,
                        soildcompress: solidCompress
                    );
                }
                catch (Exception ex)
                {
                    success = false;
                    compressInfo.UpdateCompressInfo($"压缩失败: {ex.Message}");
                    compressInfo.UpdateCurrentOperation($"错误: {ex.Message}");
                }
            });

            // 显示对话框
            await dialog.ShowAsync();

            // 等待压缩任务完成
            await compressTask;

            return success && !isCanceled;
        }
    }
}