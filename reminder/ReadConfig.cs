using System;
using SoftCircuits.IniFileParser;

namespace reminder
{
    public class ReadConfig
    {
        private readonly string _configPath;
        private readonly IniFile _iniFile;

        public ReadConfig(string configPath = "ShareSettings.ini")
        {
            _configPath = configPath;
            _iniFile = new IniFile();
            
            if (System.IO.File.Exists(_configPath))
            {
                _iniFile.Load(_configPath);
            }
        }

        /// <summary>
        /// 读取配置文件中的布尔值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public bool ReadBool(string key, bool defaultValue = false)
        {
            return _iniFile.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }

        /// <summary>
        /// 读取配置文件中的日期时间值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public DateTime ReadDateTime(string key, DateTime defaultValue)
        {
            string value = _iniFile.GetSetting(IniFile.DefaultSectionName, key, string.Empty);
            return DateTime.TryParse(value, out DateTime result) ? result : defaultValue;
        }

        /// <summary>
        /// 读取配置文件中的整数值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public int ReadInt(string key, int defaultValue = 0)
        {
            return _iniFile.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }
    }
}
