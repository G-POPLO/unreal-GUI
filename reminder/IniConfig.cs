using SoftCircuits.IniFileParser;
using System.IO;

namespace reminder
{
    public class IniConfig
    {
        private readonly string ConfigPath;
        private readonly IniFile SharedConfig;

        public IniConfig(string configPath = "ShareSettings.ini")
        {
            // 使用程序所在目录作为基准路径，确保开机自启动时能正确找到配置文件
            string baseDirectory = AppContext.BaseDirectory;
            ConfigPath = Path.Combine(baseDirectory, configPath);
            SharedConfig = new IniFile();

            // 文件存在时加载，不存在时保持空配置，允许后续写入时创建
            if (File.Exists(ConfigPath))
            {
                SharedConfig.Load(ConfigPath);
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
            return SharedConfig.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }

        /// <summary>
        /// 读取配置文件中的日期时间值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>配置值</returns>
        public DateTime ReadDateTime(string key, DateTime defaultValue)
        {
            string value = SharedConfig.GetSetting(IniFile.DefaultSectionName, key, string.Empty);
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
            return SharedConfig.GetSetting(IniFile.DefaultSectionName, key, defaultValue);
        }

        /// <summary>
        /// 写入配置文件中的日期时间值
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="value">配置值</param>
        /// <returns>是否写入成功</returns>
        public bool WriteDateTime(string key, DateTime value)
        {
            try
            {
                // 使用与主程序一致的固定格式，避免不同区域/语言环境造成读写不一致
                SharedConfig.SetSetting(IniFile.DefaultSectionName, key, value.ToString("yyyy-MM-dd HH:mm:ss"));
                // 确保目录存在后再保存
                string? dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                SharedConfig.Save(ConfigPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入INI失败: {ex.Message}");
                return false;
            }
        }
    }
}
