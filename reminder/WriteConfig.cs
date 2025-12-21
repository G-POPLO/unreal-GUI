using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace reminder
{
    public class WriteConfig
    {
        private readonly string _configPath;

        public WriteConfig(string configPath = "ShareSettings.ini")
        {
            _configPath = configPath;
        }

        public void WriteBool(string key, bool value)
        {
            try
            {
                var lines = new List<string>();
                
                // 如果文件存在，读取现有内容
                if (File.Exists(_configPath))
                {
                    lines = File.ReadAllLines(_configPath).ToList();
                }
                
                // 查找是否已存在该键
                bool keyFound = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    var trimmedLine = lines[i].Trim();
                    if (trimmedLine.StartsWith(key + "="))
                    {
                        lines[i] = $"{key}={(value ? "T" : "F")}";
                        keyFound = true;
                        break;
                    }
                }
                
                // 如果键不存在，添加新的键值对
                if (!keyFound)
                {
                    lines.Add($"{key}={(value ? "T" : "F")}");
                }
                
                // 写入文件
                File.WriteAllLines(_configPath, lines);
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
                var lines = new List<string>();
                
                // 如果文件存在，读取现有内容
                if (File.Exists(_configPath))
                {
                    lines = File.ReadAllLines(_configPath).ToList();
                }
                
                // 格式化日期时间
                string dateTimeString = value.ToString("yyyy-MM-dd HH:mm:ss");
                
                // 查找是否已存在该键
                bool keyFound = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    var trimmedLine = lines[i].Trim();
                    if (trimmedLine.StartsWith(key + "="))
                    {
                        lines[i] = $"{key}={dateTimeString}";
                        keyFound = true;
                        break;
                    }
                }
                
                // 如果键不存在，添加新的键值对
                if (!keyFound)
                {
                    lines.Add($"{key}={dateTimeString}");
                }
                
                // 写入文件
                File.WriteAllLines(_configPath, lines);
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
                var lines = new List<string>();
                
                // 如果文件存在，读取现有内容
                if (File.Exists(_configPath))
                {
                    lines = File.ReadAllLines(_configPath).ToList();
                }
                
                // 查找是否已存在该键
                bool keyFound = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    var trimmedLine = lines[i].Trim();
                    if (trimmedLine.StartsWith(key + "="))
                    {
                        lines[i] = $"{key}={value}";
                        keyFound = true;
                        break;
                    }
                }
                
                // 如果键不存在，添加新的键值对
                if (!keyFound)
                {
                    lines.Add($"{key}={value}");
                }
                
                // 写入文件
                File.WriteAllLines(_configPath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入配置项 {key} 失败: {ex.Message}");
            }
        }
    }
}