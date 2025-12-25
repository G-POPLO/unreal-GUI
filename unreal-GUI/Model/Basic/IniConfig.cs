using SoftCircuits.IniFileParser;
using System;
using System.IO;

namespace unreal_GUI.Model.Basic
{
    public class IniConfig
    {
        private string ConfigPath;
        private IniFile SharedConfig;

        public void WriteConfig()
        {
            ConfigPath = Path.Combine(AppContext.BaseDirectory, "ShareSettings.ini");

            SharedConfig = new IniFile();

            // 如果文件存在，先加载现有配置
            if (File.Exists(ConfigPath))
            {
                SharedConfig.Load(ConfigPath);
                ReadConfig();
            }
            // 如果文件不存在，创建文件并写入默认值
            else
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "FabNotificationEnabled", Properties.Settings.Default.FabNotificationEnabled);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "HeadlessEnabled", Properties.Settings.Default.HeadlessEnabled);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "BrowerType", Properties.Settings.Default.BrowerType);
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "LimitedTime", Properties.Settings.Default.LimitedTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Save();
            }
        }



        private void ReadConfig()
        {
            // 读取配置文件中的值
            bool fabNotificationEnabled = SharedConfig.GetSetting(IniFile.DefaultSectionName, "FabNotificationEnabled", Properties.Settings.Default.FabNotificationEnabled);
            bool headlessEnabled = SharedConfig.GetSetting(IniFile.DefaultSectionName, "HeadlessEnabled", Properties.Settings.Default.HeadlessEnabled);
            byte browerType = (byte)SharedConfig.GetSetting(IniFile.DefaultSectionName, "BrowerType", Properties.Settings.Default.BrowerType);
            DateTime limitedTime = DateTime.TryParse(SharedConfig.GetSetting(IniFile.DefaultSectionName, "LimitedTime", string.Empty), out DateTime result) ? result : Properties.Settings.Default.LimitedTime;

            // 比较并更新不一致的值
            if (fabNotificationEnabled != Properties.Settings.Default.FabNotificationEnabled)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "FabNotificationEnabled", Properties.Settings.Default.FabNotificationEnabled);
            }
            if (headlessEnabled != Properties.Settings.Default.HeadlessEnabled)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "HeadlessEnabled", Properties.Settings.Default.HeadlessEnabled);
            }
            if (browerType != Properties.Settings.Default.BrowerType)
            {
                SharedConfig.SetSetting(IniFile.DefaultSectionName, "BrowerType", Properties.Settings.Default.BrowerType);
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

    }
}
