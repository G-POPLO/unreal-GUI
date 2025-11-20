using iNKORE.UI.WPF.Modern.Controls;
using Markdig;
using Markdig.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using unreal_GUI.ViewModel;

namespace unreal_GUI.Model
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
                CloseButtonText = "否"
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
            var dialog = new ContentDialog
            {
                Title = title,
                Content = markdownViewer,
                PrimaryButtonText = "是",
                CloseButtonText = "否"
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
                FontSize = 18,
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
                FontSize = 18,
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
                CloseButtonText = closeButton
            };

            return await dialog.ShowAsync();
        }

        /// <summary>
        /// 显示"添加自定义按钮"的对话框
        /// </summary>
        public static async Task<ContentDialogResult> ShowAddCustomDialogAsync()
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
                    var settings = new SettingsViewModel.SettingsData();
                    if (File.Exists("settings.json"))
                    {
                        var json = File.ReadAllText("settings.json");
                        settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json) ?? new SettingsViewModel.SettingsData();
                    }

                    // 若不存在配置，则初始化列表
                    if (settings.CustomButtons == null)
                    {
                        settings.CustomButtons = [];
                    }
                    settings.CustomButtons.Add(new SettingsViewModel.CustomButton { Name = buttonName, Path = folderPath });
                    // 保存更新后的设置
                    var jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
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
                var settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json);

                if (settings?.CustomButtons != null)
                {
                    content.ButtonsListBox.ItemsSource = settings.CustomButtons;
                }

            }

            ContentDialog dialog = new()
            {
                Title = "删除自定义文件夹",
                PrimaryButtonText = "关闭",
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };
            var result = await dialog.ShowAsync();


            if (result == ContentDialogResult.Primary)
            {
                // 关闭时通知QuickAccess页面刷新
                NavigationRequested?.Invoke(null, "QuickAccess");
            }

            return result;

        }
    }
}