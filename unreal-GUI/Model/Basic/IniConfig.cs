using SoftCircuits.IniFileParser;
using System;
using System.IO;

namespace unreal_GUI.Model.Basic
{
    public class IniConfig
    {
        private string ConfigPath;
        private IniFile SharedConfig;

        // 仅加载配置文件，不触发任何写入，避免读取时副作用
        private void EnsureLoaded()
        {
            if (SharedConfig != null) return;
            ConfigPath = Path.Combine(AppContext.BaseDirectory, "ShareSettings.ini");
            SharedConfig = new IniFile();
            if (File.Exists(ConfigPath))
            {
                SharedConfig.Load(ConfigPath);
            }
        }

        public void CreateConfig()
        {
            EnsureLoaded();
            // 文件存在时同步差异，文件不存在时写入默认值
            if (File.Exists(ConfigPath))
            {
                OverWriteConfig();
            }
            else
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "AutoClaimEnabled", Properties.Settings.Default.AutoClaimEnabled);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "HeadlessEnabled", Properties.Settings.Default.HeadlessEnabled);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "BrowerType", Properties.Settings.Default.BrowerType);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "OpenEpic", Properties.Settings.Default.OpenEpic);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "HasUsingPro", Properties.Settings.Default.HasUsingPro);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "LimitedTime", Properties.Settings.Default.LimitedTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Save();
            }
        }



        private void OverWriteConfig()
        {
            // 读取配置文件中的值
            bool autoClaimEnabled = SharedConfig.GetSetting(IniFile.DefaultSectionName, "AutoClaimEnabled", Properties.Settings.Default.AutoClaimEnabled);
            bool headlessEnabled = SharedConfig.GetSetting(IniFile.DefaultSectionName, "HeadlessEnabled", Properties.Settings.Default.HeadlessEnabled);
            byte browerType = (byte)SharedConfig.GetSetting(IniFile.DefaultSectionName, "BrowerType", Properties.Settings.Default.BrowerType);
            bool openEpic = SharedConfig.GetSetting(IniFile.DefaultSectionName, "OpenEpic", Properties.Settings.Default.OpenEpic);
            bool hasUsingPro = SharedConfig.GetSetting(IniFile.DefaultSectionName, "HasUsingPro", Properties.Settings.Default.HasUsingPro);
            DateTime limitedTime = DateTime.TryParse(SharedConfig.GetSetting(IniFile.DefaultSectionName, "LimitedTime", string.Empty), out var parsed) ? parsed : Properties.Settings.Default.LimitedTime;

            // 比较并更新不一致的值
            if (autoClaimEnabled != Properties.Settings.Default.AutoClaimEnabled)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "AutoClaimEnabled", Properties.Settings.Default.AutoClaimEnabled);
            }
            if (headlessEnabled != Properties.Settings.Default.HeadlessEnabled)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "HeadlessEnabled", Properties.Settings.Default.HeadlessEnabled);
            }
            if (browerType != Properties.Settings.Default.BrowerType)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "BrowerType", Properties.Settings.Default.BrowerType);
            }
            if (openEpic != Properties.Settings.Default.OpenEpic)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "OpenEpic", Properties.Settings.Default.OpenEpic);
            }
            if (hasUsingPro != Properties.Settings.Default.HasUsingPro)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "HasUsingPro", Properties.Settings.Default.HasUsingPro);
            }
            if (limitedTime != Properties.Settings.Default.LimitedTime)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "LimitedTime", Properties.Settings.Default.LimitedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            Save();
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private void Save()
        {
            SharedConfig.Save(ConfigPath);
        }

        /// <summary>
        /// 写入日期时间配置到 INI 文件
        /// </summary>
        public void WriteDateTime(string key, DateTime value)
        {
            EnsureLoaded();
            SharedConfig.SetSetting(IniFile.DefaultSectionName, key, value.ToString("yyyy-MM-dd HH:mm:ss"));
            Save();
        }

        /// <summary>
        /// 读取布尔值配置
        /// </summary>
        public bool ReadBool(string key, bool defaultValue = false)
        {
            EnsureLoaded();
            return SharedConfig.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }

        /// <summary>
        /// 读取日期时间配置
        /// </summary>
        public DateTime ReadDateTime(string key, DateTime defaultValue)
        {
            EnsureLoaded();
            string value = SharedConfig.GetSetting(IniFile.DefaultSectionName, key, string.Empty);
            return DateTime.TryParse(value, out DateTime result) ? result : defaultValue;
        }

        /// <summary>
        /// 读取字节值配置
        /// </summary>
        public byte ReadByte(string key, byte defaultValue)
        {
            EnsureLoaded();
            return (byte)SharedConfig.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }

        /// <summary>
        /// 读取字符串配置
        /// </summary>
        public string ReadString(string key, string defaultValue = null)
        {
            EnsureLoaded();
            return SharedConfig.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }
    }
}
