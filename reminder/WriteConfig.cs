using System;
using SoftCircuits.IniFileParser;

namespace reminder
{
    public class WriteConfig
    {
        private readonly string _configPath;
        private readonly IniFile _iniFile;

        public WriteConfig(string configPath = "ShareSettings.ini")
        {
            _configPath = configPath;
            _iniFile = new IniFile();
            
            if (System.IO.File.Exists(_configPath))
            {
                _iniFile.Load(_configPath);
            }
        }

        public void WriteBool(string key, bool value)
        {
            try
            {
                _iniFile.SetSetting(IniFile.DefaultSectionName, key, value);
                _iniFile.Save(_configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入配置项 {key} 失败: {ex.Message}");
            }
        }

        public void WriteDateTime(string key, DateTime value)
        {
            try
            {
                string dateTimeString = value.ToString("yyyy-MM-dd HH:mm:ss");
                _iniFile.SetSetting(IniFile.DefaultSectionName, key, dateTimeString);
                _iniFile.Save(_configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入配置项 {key} 失败: {ex.Message}");
            }
        }

        public void WriteInt(string key, int value)
        {
            try
            {
                _iniFile.SetSetting(IniFile.DefaultSectionName, key, value);
                _iniFile.Save(_configPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入配置项 {key} 失败: {ex.Message}");
            }
        }
    }
}