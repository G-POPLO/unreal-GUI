using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace reminder
{
    internal class ReadConfig
    {
        private readonly string _configPath;

        public ReadConfig(string configPath = "ShareSettings.ini")
        {
            _configPath = configPath;
        }

        /// <summary>
        /// 读取配置文件中的布尔值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public bool ReadBool(string key, bool defaultValue = false)
        {
            try
            {
                if (!File.Exists(_configPath))
                    return defaultValue;

                var lines = File.ReadAllLines(_configPath);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith(key + "="))
                    {
                        var value = trimmedLine.Substring(key.Length + 1);
                        return value.Equals("T", StringComparison.OrdinalIgnoreCase) || 
                               value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                               value.Equals("1", StringComparison.OrdinalIgnoreCase);
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 读取配置文件中的日期时间值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public DateTime ReadDateTime(string key, DateTime defaultValue)
        {
            try
            {
                if (!File.Exists(_configPath))
                    return defaultValue;

                var lines = File.ReadAllLines(_configPath);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith(key + "="))
                    {
                        var value = trimmedLine.Substring(key.Length + 1);
                        if (DateTime.TryParse(value, out DateTime result))
                            return result;
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 读取配置文件中的整数值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public int ReadInt(string key, int defaultValue = 0)
        {
            try
            {
                if (!File.Exists(_configPath))
                    return defaultValue;

                var lines = File.ReadAllLines(_configPath);
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith(key + "="))
                    {
                        var value = trimmedLine.Substring(key.Length + 1);
                        if (int.TryParse(value, out int result))
                            return result;
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
