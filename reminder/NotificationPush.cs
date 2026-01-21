using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace reminder
{
    internal class NotificationPush
    {
        /// <summary>
        /// 显示简单的文本通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知内容</param>
        public static void ShowSimpleNotification(string title, string message)
        {
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .GetToastContent();

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }




        /// <summary>
        /// 关闭指定的通知
        /// </summary>
        /// <param name="tag">通知标签</param>
        /// <param name="group">通知组</param>
        public static void DismissNotification(string tag, string group)
        {
            ToastNotificationManagerCompat.History.Remove(tag, group);
        }

        /// <summary>
        /// 显示带图标的通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知内容</param>
        /// <param name="iconPath">图标路径</param>
        public static void ShowNotificationWithIcon(string title, string message, string iconPath)
        {
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddAppLogoOverride(new Uri(iconPath))
                .GetToastContent();

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }



    }
}
