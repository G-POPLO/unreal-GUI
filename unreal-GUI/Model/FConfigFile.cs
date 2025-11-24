using System;
using System.Collections.Generic;
using System.Text;

namespace unreal_GUI.Model
{
    internal class FConfigFile
    {
        private static readonly System.Buffers.SearchValues<char> s_Chars = System.Buffers.SearchValues.Create(" ,=()\"\n\t");
        internal static readonly char[] separator = ['\r', '\n'];

        public static List<ConfigSection> Parse(string content)
        {
            var sections = new List<ConfigSection>();
            ConfigSection currentSection = null;

            foreach (var line in content.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();

                // 跳过注释
                if (trimmed.StartsWith(';') || trimmed.StartsWith('#')) continue;

                // 节头 [Section]
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    var sectionName = trimmed.Substring(1, trimmed.Length - 2);
                    currentSection = new ConfigSection { Name = sectionName };
                    sections.Add(currentSection);
                    continue;
                }

                // 键值对：Key=Value
                var equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex <= 0 || currentSection == null) continue;

                var key = trimmed.Substring(0, equalsIndex).Trim();
                var value = trimmed.Substring(equalsIndex + 1).Trim();

                currentSection.Entries.Add(new ConfigEntry
                {
                    Key = key,
                    RawValue = value
                });
            }

            return sections;
        }

        // 辅助：解析带引号的字符串（支持 "" 转义）
        public static string UnescapeQuotedString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                value = value.Substring(1, value.Length - 2);

                return value.Replace("\"\"", "\"");
            }

            return value;
        }

        // 辅助：将字符串转为安全的 INI 值（加引号 + 转义）
        public static string EscapeForIni(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";

            // 如果包含特殊字符，必须加引号
            bool needsQuotes = text.AsSpan().IndexOfAny(s_Chars) >= 0;
            if (!needsQuotes) return text;

            var escaped = text.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        // 序列化回字符串
        public static string Serialize(List<ConfigSection> sections)
        {
            var lines = new List<string>();

            foreach (var section in sections)
            {
                lines.Add($"[{section.Name}]");
                foreach (var entry in section.Entries)
                {
                    lines.Add($"{entry.Key}={entry.RawValue}");
                }
                lines.Add(""); // 空行分隔
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    /// <summary>
    /// 专门用于解析TemplateCategories.ini文件的解析器
    /// 支持节、键值对、数组、带引号的字符串，并能忽略注释
    /// </summary>
    internal class TemplateCategoriesParser
    {
        public static List<ConfigSection> Parse(string content)
        {
            var sections = new List<ConfigSection>();
            ConfigSection currentSection = null;

            foreach (var line in content.Split(FConfigFile.separator, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();

                // 跳过注释
                if (trimmed.StartsWith(';') || trimmed.StartsWith('#')) continue;

                // 节头 [Section]
                if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                {
                    var sectionName = trimmed.Substring(1, trimmed.Length - 2);
                    currentSection = new ConfigSection { Name = sectionName };
                    sections.Add(currentSection);
                    continue;
                }

                // 键值对：Key=Value
                var equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex <= 0 || currentSection == null) continue;

                var key = trimmed.Substring(0, equalsIndex).Trim();
                var value = trimmed.Substring(equalsIndex + 1).Trim();

                // 处理数组值 (A,B,C) 或 (A,B,C,)
                if (value.StartsWith('(') && value.EndsWith(')'))
                {
                    // 保留原始数组格式
                    currentSection.Entries.Add(new ConfigEntry
                    {
                        Key = key,
                        RawValue = value
                    });
                }
                else
                {
                    // 处理普通键值对或带引号的字符串
                    currentSection.Entries.Add(new ConfigEntry
                    {
                        Key = key,
                        RawValue = value
                    });
                }
            }

            return sections;
        }

        /// <summary>
        /// 将解析后的数据序列化回INI格式字符串
        /// </summary>
        public static string Serialize(List<ConfigSection> sections)
        {
            var sb = new StringBuilder();

            foreach (var section in sections)
            {
                sb.AppendLine($"[{section.Name}]");
                foreach (var entry in section.Entries)
                {
                    sb.AppendLine($"{entry.Key}={entry.RawValue}");
                }
                sb.AppendLine(); // 空行分隔
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取指定节中的指定键的值
        /// </summary>
        public static string GetValue(List<ConfigSection> sections, string sectionName, string key)
        {
            var section = sections.Find(s => s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
            if (section == null) return null;

            var entry = section.Entries.Find(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return entry?.RawValue;
        }

        /// <summary>
        /// 解析数组值，例如：(A,B,C) 或 ((Language="en",Text="Hello"),(Language="zh",Text="你好"))
        /// </summary>
        public static List<string> ParseArray(string arrayValue)
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(arrayValue)) return result;

            // 移除外层括号
            if (arrayValue.StartsWith('(') && arrayValue.EndsWith(')'))
            {
                arrayValue = arrayValue.Substring(1, arrayValue.Length - 2);
            }

            if (string.IsNullOrEmpty(arrayValue)) return result;

            // 简单处理：按逗号分割（不考虑嵌套引号的情况）
            // 对于复杂的嵌套结构，需要更复杂的解析逻辑
            var items = SplitRespectingQuotesAndParentheses(arrayValue);
            result.AddRange(items);

            return result;
        }

        /// <summary>
        /// 分割字符串，但会考虑引号和括号的嵌套关系
        /// </summary>
        private static List<string> SplitRespectingQuotesAndParentheses(string input)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            int parenLevel = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"' && (i == 0 || input[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }

                if (!inQuotes)
                {
                    if (c == '(')
                    {
                        parenLevel++;
                    }
                    else if (c == ')')
                    {
                        parenLevel--;
                    }
                    else if (c == ',' && parenLevel == 0)
                    {
                        result.Add(current.ToString().Trim());
                        current.Clear();
                        continue;
                    }
                }

                current.Append(c);
            }

            // 添加最后一个元素
            var lastItem = current.ToString().Trim();
            if (!string.IsNullOrEmpty(lastItem))
            {
                result.Add(lastItem);
            }

            return result;
        }
    }
}
