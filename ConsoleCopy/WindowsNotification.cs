using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unreal_GUI_Reminder
{
    public class WindowsNotification
    {
        /// <summary>
        /// 发送简单的Windows通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        public static void SendNotification(string title, string content)
        {
            try
            {
                // 创建一个新的通知
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(content)
                    .Show(); // 发送通知
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送带进度条的Windows通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="progress">进度值(0-100)</param>
        /// <param name="progressStatus">进度状态文本</param>
        public static void SendNotificationWithProgress(string title, string content, int progress, string progressStatus)
        {
            try
            {
                // 创建带进度条的通知
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(content)
                    .AddProgressBar(
                        value: (double)progress / 100,
                        title: progressStatus,
                        valueStringOverride: $"{progress}%"
                    )
                    .Show(); // 发送通知
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送带进度条的通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送带操作按钮的Windows通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="buttonContent">按钮文本</param>
        /// <param name="buttonAction">按钮操作参数</param>
        /// <param name="protocol">协议（可选）</param>
        public static void SendNotificationWithButton(string title, string content, string buttonContent, string buttonAction, string protocol = null)
        {
            try
            {
                // 创建带按钮的通知
                var toastBuilder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(content);

                // 添加按钮
                if (protocol != null)
                {
                    toastBuilder.AddButton(new ToastButton()
                        .SetContent(buttonContent)
                        .AddArgument("action", "openUrl")
                        .AddArgument("url", protocol)
                        .SetBackgroundActivation());
                }
                else
                {
                    toastBuilder.AddButton(new ToastButton()
                        .SetContent(buttonContent)
                        .AddArgument("action", buttonAction)
                        .SetBackgroundActivation());
                }

                // 添加关闭按钮
                toastBuilder.AddButton(new ToastButtonDismiss("关闭"));

                // 发送通知
                toastBuilder.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送带按钮的通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送带多个按钮的Windows通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="buttons">按钮列表，每个按钮包含文本、操作和URL（可选）</param>
        public static void SendNotificationWithMultipleButtons(string title, string content, List<(string text, string action, string url)> buttons)
        {
            try
            {
                // 创建带多个按钮的通知
                var toastBuilder = new ToastContentBuilder()
                    .AddText(title)
                    .AddText(content);

                // 添加所有按钮
                foreach (var button in buttons)
                {
                    var toastButton = new ToastButton()
                        .SetContent(button.text)
                        .AddArgument("action", button.action)
                        .SetBackgroundActivation();

                    if (!string.IsNullOrEmpty(button.url))
                    {
                        toastButton.AddArgument("url", button.url);
                    }

                    toastBuilder.AddButton(toastButton);
                }

                // 发送通知
                toastBuilder.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送带多个按钮的通知失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 取消所有通知
        /// </summary>
        public static void ClearAllNotifications()
        {
            try
            {
                ToastNotificationManagerCompat.History.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除通知失败: {ex.Message}");
            }
        }
    }
}