using SoftCircuits.IniFileParser;

namespace reminder
{
    public class ReadConfig
    {
        private readonly string ConfigPath;
        private readonly IniFile SharedConfig;

        public ReadConfig(string configPath = "ShareSettings.ini")
        {
            ConfigPath = configPath;
            SharedConfig = new IniFile();

            // 检查文件是否存在
            if (!System.IO.File.Exists(ConfigPath))
            {
                // 配置文件不存在，直接退出程序
                Console.WriteLine("配置文件不存在，程序将退出");
                Environment.Exit(0);
            }

            // 文件存在，加载配置
            SharedConfig.Load(ConfigPath);
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
    }
}
