using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace unreal_GUI.Model
{
    public class CategoriesParser
    {
        public class LocalizedText
        {
            public string Language { get; set; }
            public string Text { get; set; }
        }

        public class Category
        {
            public string Key { get; set; }
            public List<LocalizedText> LocalizedDisplayNames { get; set; } = [];
            public List<LocalizedText> LocalizedDescriptions { get; set; } = [];
            public string Icon { get; set; }
            public bool IsMajorCategory { get; set; }
        }

        /// <summary>
        /// 解析INI文件中的Categories部分
        /// </summary>
        /// <param name="filePath">INI文件路径</param>
        /// <returns>解析出的Category列表</returns>
        public static List<Category> ParseCategories(string filePath)
        {
            var categories = new List<Category>();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // 跳过注释和空行
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(';'))
                    continue;

                // 查找Categories=开头的行
                if (trimmedLine.StartsWith("Categories="))
                {
                    Category currentCategory = new();

                    // 提取括号内的内容
                    var contentMatch = Regex.Match(trimmedLine, @"Categories=\((.*)\)");
                    if (contentMatch.Success)
                    {
                        var content = contentMatch.Groups[1].Value;

                        // 解析Key
                        var keyMatch = Regex.Match(content, @"Key\s*=\s*""([^""]+)""");
                        if (keyMatch.Success)
                        {
                            currentCategory.Key = keyMatch.Groups[1].Value;
                        }

                        // 解析LocalizedDisplayNames
                        currentCategory.LocalizedDisplayNames = ParseLocalizedTextList(content, "LocalizedDisplayNames");

                        // 解析LocalizedDescriptions
                        currentCategory.LocalizedDescriptions = ParseLocalizedTextList(content, "LocalizedDescriptions");

                        // 解析Icon
                        var iconMatch = Regex.Match(content, @"Icon\s*=\s*""([^""]+)""");
                        if (iconMatch.Success)
                        {
                            currentCategory.Icon = iconMatch.Groups[1].Value;
                        }

                        // 解析IsMajorCategory
                        var isMajorCategoryMatch = Regex.Match(content, @"IsMajorCategory\s*=\s*(true|false)");
                        if (isMajorCategoryMatch.Success)
                        {
                            currentCategory.IsMajorCategory = bool.Parse(isMajorCategoryMatch.Groups[1].Value);
                        }

                        categories.Add(currentCategory);
                    }
                }
            }

            return categories;
        }

        /// <summary>
        /// 解析本地化文本列表
        /// </summary>
        /// <param name="content">包含本地化文本的字符串</param>
        /// <param name="fieldName">字段名称(LocalizedDisplayNames或LocalizedDescriptions)</param>
        /// <returns>本地化文本列表</returns>
        private static List<LocalizedText> ParseLocalizedTextList(string content, string fieldName)
        {
            var result = new List<LocalizedText>();

            // 构建正则表达式匹配字段
            var fieldRegex = new Regex($@"{fieldName}\s*=\s*\(\s*((?:\([^\)]*\)\s*,?\s*)*)\)", RegexOptions.Singleline);
            var fieldMatch = fieldRegex.Match(content);

            if (fieldMatch.Success)
            {
                var listContent = fieldMatch.Groups[1].Value;

                // 匹配每个(Language = "...", Text = "...")项
                var itemRegex = new Regex(@"\(Language\s*=\s*""([^""]+)"",\s*Text\s*=\s*""([^""]*)""\)");
                var itemMatches = itemRegex.Matches(listContent);

                foreach (Match itemMatch in itemMatches)
                {
                    result.Add(new LocalizedText
                    {
                        Language = itemMatch.Groups[1].Value,
                        Text = itemMatch.Groups[2].Value
                    });
                }
            }

            return result;
        }


        /// <summary>
        /// 根据用户提供的参数生成Categories的格式字符串
        /// </summary>
        public static string GenerateCategoriesText
            (
            string key,
            string displayName,
            bool isMajorCategory,
            string DescriptionEn,
            string? DescriptionZh,
            string? DescriptionJa,
            string? DescriptionKo
            )
        {
            var sb = new StringBuilder();

            // 开始Categories条目
            sb.Append("Categories=(");

            // 添加Key
            sb.Append($"Key=\"{key}\"");

            // 添加LocalizedDisplayNames
            sb.Append(", LocalizedDisplayNames=(");
            // 英文显示名称（必填）
            sb.Append($"(Language=\"en\",Text=\"{displayName}\")");
            // 其他语言显示名称（可选）
            if (!string.IsNullOrEmpty(DescriptionZh))
                sb.Append($",(Language=\"zh-Hans\",Text=\"{displayName}\")");
            if (!string.IsNullOrEmpty(DescriptionJa))
                sb.Append($",(Language=\"ja\",Text=\"{displayName}\")");
            if (!string.IsNullOrEmpty(DescriptionKo))
                sb.Append($",(Language=\"ko\",Text=\"{displayName}\")");
            sb.Append(')');

            // 添加LocalizedDescriptions
            sb.Append(", LocalizedDescriptions=(");
            // 英文描述（必填）
            sb.Append($"(Language=\"en\",Text=\"{DescriptionEn}\")");
            // 其他语言描述（可选）
            if (!string.IsNullOrEmpty(DescriptionZh))
                sb.Append($",(Language=\"zh-Hans\",Text=\"{DescriptionZh}\")");
            if (!string.IsNullOrEmpty(DescriptionJa))
                sb.Append($",(Language=\"ja\",Text=\"{DescriptionJa}\")");
            if (!string.IsNullOrEmpty(DescriptionKo))
                sb.Append($",(Language=\"ko\",Text=\"{DescriptionKo}\")");
            sb.Append(')');

            // 处理图标路径，转换为相对路径格式 Media/{key}_2X.png
            string iconRelativePath = $"Media/{key}_2X.png";
            sb.Append($", Icon=\"{iconRelativePath}\"");

            // 添加IsMajorCategory
            sb.Append($", IsMajorCategory={isMajorCategory.ToString().ToLower()}");

            // 结束Categories条目
            sb.Append(')');

            return sb.ToString();
        }
    }
}