using ModernWpf;
using ModernWpf.Controls;
using System.Threading.Tasks;
using System.Windows;

namespace unreal_GUI
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
            string secondaryButton = null,
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
    }
}