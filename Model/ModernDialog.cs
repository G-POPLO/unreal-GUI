using ModernWpf;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        /// 显示一个允许用户输入内容并带有确认/取消按钮的对话框
        /// </summary>
        public static async Task<(ContentDialogResult result, string input)> ShowInputDialogAsync(string title/*,/*string placeholderText*/)
        {
            var textBox = new TextBox
            {
                // PlaceholderText = placeholderText, // 注释掉不存在的属性
                Margin = new Thickness(0, 10, 0, 0),
                MinWidth = 300,
            };

            // 设置占位符文本提示

            //textBox.ToolTip = placeholderText;


            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            

            var result = await dialog.ShowAsync();


           
            return (result, textBox.Text);
        }
    }
}