using Microsoft.Win32;
using System;
using System.IO;

namespace unreal_GUI.Model.Features
{
    internal class AutoStart
    {
        private const string RunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AppName = "FabReminder";

        /// <summary>
        /// 设置开机自启
        /// </summary>
        /// <param name="enable">是否启用开机自启</param>
        public static void SetAutoStart(bool enable)
        {
            string exePath = Path.Combine(AppContext.BaseDirectory, "reminder.exe");

            using RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (enable)
            {
                // 添加开机自启项
                key.SetValue(AppName, exePath);

            }
            else
            {
                // 删除开机自启项
                key.DeleteValue(AppName, false);

            }
        }
    }
}