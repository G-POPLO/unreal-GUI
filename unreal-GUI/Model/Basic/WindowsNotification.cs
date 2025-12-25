using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace unreal_GUI.Model.Basic
{
    internal class WindowsNotification
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
        /// 显示带进度条的通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">进度描述</param>
        /// <param name="progressValue">进度值 (0.0 - 1.0)</param>
        /// <param name="tag">通知标签</param>
        /// <param name="group">通知组</param>
        public static void ShowProgressNotification(string title, string message, double progressValue, string tag, string group)
        {
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = message,
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressText")
                })
                .GetToastContent();

            var toast = new ToastNotification(toastContent.GetXml())
            {
                Tag = tag,
                Group = group
            };

            // 设置进度数据
            var data = new NotificationData();
            data.Values["progressValue"] = progressValue.ToString("F2");
            data.Values["progressText"] = $"{progressValue:P0}";
            toast.Data = data;

            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        /// <summary>
        /// 更新进度通知
        /// </summary>
        /// <param name="progressValue">新的进度值 (0.0 - 1.0)</param>
        /// <param name="tag">通知标签</param>
        /// <param name="group">通知组</param>
        public static void UpdateProgressNotification(int progressValue, string tag, string group)
        {
            var data = new NotificationData();
            data.Values["progressValue"] = progressValue.ToString("F2");
            data.Values["progressText"] = $"{progressValue:P0}";
            ToastNotificationManagerCompat.CreateToastNotifier().Update(data, tag, group);
        }

        /// <summary>
        /// 显示带操作按钮的通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="message">通知内容</param>
        /// <param name="button1Text">按钮1文本</param>
        /// <param name="button1Action">按钮1操作</param>
        /// <param name="button1Url">按钮1要打开的URL</param>
        /// <param name="button2Text">按钮2文本</param>
        /// <param name="button2Action">按钮2操作</param>
        public static void ShowNotificationWithUrls(string title, string message, string button1Text, string button1Action, string button1Url, string button2Text, string button2Action)
        {
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddButton(new ToastButton()
                    .SetContent(button1Text)
                    .AddArgument("action", button1Action)
                    .AddArgument("url", button1Url))
                .AddButton(new ToastButton()
                    .SetContent(button2Text)
                    .AddArgument("action", button2Action))
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
