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
using unreal_GUI.Model.DialogContent;
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

            // 关闭对话框时也刷新页面
            //NavigationRequested?.Invoke(null, "QuickAccess");

            return result;
        }

        // 查找视觉子元素的辅助方法
        //public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        //{
        //    if (parent == null) return null;

        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
        //        if (child is T)
        //        {
        //            return (T)child;
        //        }

        //        T childOfChild = FindVisualChild<T>(child);
        //        if (childOfChild != null)
        //        {
        //            return childOfChild;
        //        }
        //    }

        //    return null;
        //}

        /// <summary>
        /// 显示添加模板类别对话框
        /// </summary>
        /// <param name="enginePath">引擎安装路径，用于定位TemplateCategories.ini文件</param>
        public static async Task<ContentDialogResult> ShowCategoriesDialogAsync(string enginePath = null)
        {
            var content = new Add_Categories();

            ContentDialog dialog = new()
            {
                Title = "添加模板类别",
                PrimaryButtonText = "保存",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = content,
                IsPrimaryButtonEnabled = true
            };

            // 设置对话框引用
            content.Dialog = dialog;

            var result = await dialog.ShowAsync();

            // 处理保存逻辑
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    // 生成类别配置内容
                    string categoryContent = GenerateCategoryContent(content);

                    // 如果提供了引擎路径，则写入TemplateCategories.ini文件
                    if (!string.IsNullOrEmpty(enginePath))
                    {
                        string templateCategoriesPath = Path.Combine(enginePath, "Templates", "TemplateCategories.ini");
                        await SaveToTemplateCategoriesIni(templateCategoriesPath, categoryContent);
                        await ShowInfoAsync($"类别已成功添加到引擎的模板配置中", "保存成功");
                    }
                    else
                    {
                        // 如果没有提供引擎路径，仅显示生成的内容
                        await ShowInfoAsync($"类别配置已生成:\n\n{categoryContent}", "生成成功");
                    }
                }
                catch (Exception ex)
                {
                    await ShowErrorAsync($"保存类别失败: {ex.Message}", "错误");
                }
            }

            return result;
        }

        /// <summary>
        /// 将类别配置保存到TemplateCategories.ini文件
        /// </summary>
        /// <param name="filePath">TemplateCategories.ini文件路径</param>
        /// <param name="categoryContent">要添加的类别配置内容</param>
        private static async Task SaveToTemplateCategoriesIni(string filePath, string categoryContent)
        {
            // 确保目录存在
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 如果文件不存在，创建新文件并写入类别内容
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, categoryContent);
                return;
            }

            // 读取现有文件内容
            string existingContent = await File.ReadAllTextAsync(filePath);

            // 检查文件是否已包含Categories=(
            if (existingContent.Contains("Categories=(", StringComparison.OrdinalIgnoreCase))
            {
                // 如果文件已包含Categories=(", 我们需要在最后一个括号前插入新类别
                int lastCloseBracketIndex = existingContent.LastIndexOf(")");
                if (lastCloseBracketIndex > 0)
                {
                    // 移除末尾的括号
                    string contentWithoutLastBracket = existingContent.Substring(0, lastCloseBracketIndex);
                    
                    // 检查是否需要添加逗号
                    if (!contentWithoutLastBracket.EndsWith("(") && !contentWithoutLastBracket.TrimEnd().EndsWith(","))
                    {
                        contentWithoutLastBracket += ",";
                    }
                    
                    // 插入新类别（去掉开头的"Categories=("和结尾的")"）
                    string trimmedCategoryContent = categoryContent.Replace("Categories=(", "").TrimEnd(')');
                    string updatedContent = contentWithoutLastBracket + Environment.NewLine + trimmedCategoryContent + ")";
                    
                    await File.WriteAllTextAsync(filePath, updatedContent);
                }
                else
                {
                    // 如果格式不正确，使用简单的追加方式
                    string updatedContent = existingContent.TrimEnd() + Environment.NewLine + categoryContent;
                    await File.WriteAllTextAsync(filePath, updatedContent);
                }
            }
            else
            {
                // 如果文件不包含Categories=("，则直接写入类别内容
                await File.WriteAllTextAsync(filePath, categoryContent);
            }
        }

        // 生成类别配置内容
        private static string GenerateCategoryContent(Add_Categories content)
        {
            var sb = new System.Text.StringBuilder();

            sb.Append("Categories=(");
            sb.Append($"Key=\"{content.ViewModel.CategoryKey}\", ");

            // 添加本地化显示名称
            sb.Append("LocalizedDisplayNames=(");
            bool hasDisplayName = false;
            foreach (var displayNameItem in content.ViewModel.DisplayNameItems)
            {
                if (!string.IsNullOrWhiteSpace(displayNameItem.DisplayName))
                {
                    if (hasDisplayName)
                    {
                        sb.Append(',');
                    }
                    // 按照参考格式，每个Language-Text对需要用额外的括号包围
                    sb.Append($"((Language=\"{displayNameItem.LanguageCode}\",Text=\"{displayNameItem.DisplayName}\"))");
                    hasDisplayName = true;
                }
            }
            sb.Append(')');

            // 添加本地化描述
            sb.Append(", LocalizedDescriptions=(");
            bool hasDescription = false;
            foreach (var descriptionItem in content.ViewModel.DescriptionItems)
            {
                if (!string.IsNullOrWhiteSpace(descriptionItem.Description))
                {
                    if (hasDescription)
                    {
                        sb.Append(',');
                    }
                    // 按照参考格式，每个Language-Text对需要用额外的括号包围
                    sb.Append($"((Language=\"{descriptionItem.LanguageCode}\",Text=\"{descriptionItem.Description}\"))");
                    hasDescription = true;
                }
            }
            sb.Append(')');

            // 添加图标路径
            if (!string.IsNullOrEmpty(content.ViewModel.IconFileName))
            {
                sb.Append($", Icon=\"Media/{content.ViewModel.IconFileName}\")");
            }
            else
            {
                // 如果没有图标，仍然需要添加类别结束括号
                sb.Append(')');
            }

            // 添加是否为主类别（确保前面有逗号分隔）
            if (!string.IsNullOrEmpty(content.ViewModel.IconFileName))
            {
                sb.Append($", IsMajorCategory={content.ViewModel.IsMajorCategory.ToString().ToLower()})");
            }
            else
            {
                // 如果没有图标，需要在括号外添加IsMajorCategory
                sb.Replace(")", $", IsMajorCategory={content.ViewModel.IsMajorCategory.ToString().ToLower()})", sb.Length - 1, 1);
            }
            
            return sb.ToString();
        }
    }
}