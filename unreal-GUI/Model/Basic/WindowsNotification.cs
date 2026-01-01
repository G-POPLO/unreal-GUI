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
            try
            {
                var toastContent = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .GetToastContent();

                var toast = new ToastNotification(toastContent.GetXml());
                ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                // 记录错误或进行其他处理
                Console.WriteLine($"显示通知时发生错误: {ex.Message}");
            }
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
            try
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
            catch (Exception ex)
            {
                // 记录错误或进行其他处理
                Console.WriteLine($"显示带按钮通知时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 关闭指定的通知
        /// </summary>
        /// <param name="tag">通知标签</param>
        /// <param name="group">通知组</param>
        public static void DismissNotification(string tag, string group)
        {
            try
            {
                ToastNotificationManagerCompat.History.Remove(tag, group);
            }
            catch (Exception ex)
            {
                // 记录错误或进行其他处理
                Console.WriteLine($"关闭通知时发生错误: {ex.Message}");
            }
        }

    }
}