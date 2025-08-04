using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using unreal_GUI.ViewModel;
namespace unreal_GUI.Model
{
    public static class ModernDialog
    {
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
        /// 显示一个仅 OK 按钮的对话框
        /// </summary>
        public static async Task ShowInfoAsync(string message, string title = "提示")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "确定"
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// 显示带自定义按钮的对话框
        /// </summary>
        public static async Task<ContentDialogResult> ShowCustomAsync(
            string message,
            string title,
            string primaryButton,
            string? secondaryButton = null,
            string closeButton = "取消")
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
        /// 仅用于“快速访问”页面的输入框
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
                    
                    // 添加新的自定义按钮信息
                    if (settings.CustomButtons == null)
                    {
                        settings.CustomButtons = new List<SettingsViewModel.CustomButton>();
                    }
                    settings.CustomButtons.Add(new SettingsViewModel.CustomButton { Name = buttonName, Path = folderPath });
                    
                    // 保存更新后的设置
                    var jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText("settings.json", jsonSettings);
                    
                    
                }
            }
            return result;
        }
       

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
            
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "删除自定义文件夹";
            dialog.PrimaryButtonText = "关闭";           
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = content;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // 这里需要实现具体的逻辑来处理删除操作
                // 例如：从JSON文件中删除相应的条目
            }
            else
            {
                // 取消操作
            }
            return result;
        }
    }
}