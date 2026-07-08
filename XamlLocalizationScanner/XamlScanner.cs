using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace XamlLocalizationScanner;

/// <summary>
/// XAML 文件扫描器，用于提取硬编码文本
/// </summary>
public class XamlScanner
{
    // 需要扫描的文本属性
    private static readonly string[] TextProperties =
    [
        "Text", "Content", "Header", "Title", "ToolTip", "Label",
        "PlaceholderText", "Description", "Subtitle", "DialogTitle"
    ];

    // 需要跳过的属性值模式
    private static readonly string[] SkipPatterns =
    [
        "{Binding", "{x:Static", "{DynamicResource", "{StaticResource",
        "{x:Bind", "{TemplateBinding"
    ];

    // 中文、日文、韩文字符范围
    private static readonly Regex CjkPattern = new(
        @"[\u4e00-\u9fff\u3040-\u309f\u30a0-\u30ff\uac00-\ud7af]",
        RegexOptions.Compiled);

    private readonly string _basePath;
    private readonly bool _includeNonCjkText;

    /// <summary>
    /// 创建 XAML 扫描器实例
    /// </summary>
    /// <param name="basePath">项目根目录路径</param>
    /// <param name="includeNonCjkText">是否包含非 CJK 文本（如英文）</param>
    public XamlScanner(string basePath, bool includeNonCjkText = false)
    {
        _basePath = basePath;
        _includeNonCjkText = includeNonCjkText;
    }

    /// <summary>
    /// 扫描指定目录下的所有 XAML 文件
    /// </summary>
    /// <param name="directory">要扫描的目录</param>
    /// <returns>本地化条目列表</returns>
    public List<LocalizationEntry> ScanDirectory(string directory)
    {
        var entries = new List<LocalizationEntry>();
        var xamlFiles = Directory.GetFiles(directory, "*.xaml", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
            .ToList();

        Console.WriteLine($"找到 {xamlFiles.Count} 个 XAML 文件");

        foreach (var file in xamlFiles)
        {
            var fileEntries = ScanFile(file);
            entries.AddRange(fileEntries);
        }

        // 去重并合并
        entries = DeduplicateEntries(entries);

        return entries;
    }

    /// <summary>
    /// 扫描单个 XAML 文件
    /// </summary>
    private List<LocalizationEntry> ScanFile(string filePath)
    {
        var entries = new List<LocalizationEntry>();
        var relativePath = GetRelativePath(filePath);

        try
        {
            var content = File.ReadAllText(filePath);
            var lines = content.Split('\n');

            // 使用正则表达式查找属性
            foreach (var propertyName in TextProperties)
            {
                // 匹配 PropertyName="value" 或 PropertyName='value'
                var pattern = $@"{propertyName}\s*=\s*[""']([^""']*)[""']";
                var matches = Regex.Matches(content, pattern, RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    var value = match.Groups[1].Value;

                    // 跳过绑定和资源引用
                    if (ShouldSkip(value))
                        continue;

                    // 检查是否包含 CJK 字符或是否包含所有文本
                    if (!_includeNonCjkText && !ContainsCjk(value))
                        continue;

                    // 跳过空白或纯数字
                    if (string.IsNullOrWhiteSpace(value) || IsNumeric(value))
                        continue;

                    // 计算行号
                    var lineNumber = GetLineNumber(content, match.Index);

                    // 提取元素信息
                    var elementInfo = ExtractElementInfo(content, match.Index);

                    var entry = new LocalizationEntry
                    {
                        Value = value,
                        SourceFile = relativePath,
                        LineNumber = lineNumber,
                        PropertyName = propertyName,
                        ElementName = elementInfo.ElementName,
                        ElementIdentifier = elementInfo.Identifier,
                        Comment = $"Source: {relativePath}:{lineNumber}, Element: {elementInfo.ElementName}"
                    };

                    entries.Add(entry);
                }
            }

            // 查找 Run 元素中的文本
            ScanRunElements(content, relativePath, entries);

            // 查找注释中的文本（可选）
            // ScanComments(content, relativePath, entries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"扫描文件失败 {relativePath}: {ex.Message}");
        }

        return entries;
    }

    /// <summary>
    /// 扫描 Run 元素中的文本
    /// </summary>
    private void ScanRunElements(string content, string relativePath, List<LocalizationEntry> entries)
    {
        // 匹配 <Run>文本</Run> 或 <Run Text="文本"/>
        var runPattern = @"<Run\s+[^>]*Text\s*=\s*[""']([^""']*)[""'][^>]*/>";
        var matches = Regex.Matches(content, runPattern, RegexOptions.Multiline);

        foreach (Match match in matches)
        {
            var value = match.Groups[1].Value;

            if (ShouldSkip(value) || string.IsNullOrWhiteSpace(value))
                continue;

            if (!_includeNonCjkText && !ContainsCjk(value))
                continue;

            var lineNumber = GetLineNumber(content, match.Index);

            entries.Add(new LocalizationEntry
            {
                Value = value,
                SourceFile = relativePath,
                LineNumber = lineNumber,
                PropertyName = "Run.Text",
                ElementName = "Run",
                Comment = $"Source: {relativePath}:{lineNumber}, Element: Run"
            });
        }

        // 匹配 <Run FontWeight="...">文本</Run>
        var runContentPattern = @"<Run[^>]*>([^<]+)</Run>";
        matches = Regex.Matches(content, runContentPattern, RegexOptions.Multiline);

        foreach (Match match in matches)
        {
            var value = match.Groups[1].Value.Trim();

            if (ShouldSkip(value) || string.IsNullOrWhiteSpace(value))
                continue;

            if (!_includeNonCjkText && !ContainsCjk(value))
                continue;

            var lineNumber = GetLineNumber(content, match.Index);

            entries.Add(new LocalizationEntry
            {
                Value = value,
                SourceFile = relativePath,
                LineNumber = lineNumber,
                PropertyName = "Run.Content",
                ElementName = "Run",
                Comment = $"Source: {relativePath}:{lineNumber}, Element: Run"
            });
        }
    }

    /// <summary>
    /// 检查是否应该跳过该值
    /// </summary>
    private bool ShouldSkip(string value)
    {
        return SkipPatterns.Any(pattern => value.Contains(pattern));
    }

    /// <summary>
    /// 检查字符串是否包含 CJK 字符
    /// </summary>
    private bool ContainsCjk(string value)
    {
        return CjkPattern.IsMatch(value);
    }

    /// <summary>
    /// 检查字符串是否为纯数字
    /// </summary>
    private bool IsNumeric(string value)
    {
        return double.TryParse(value.Trim(), out _);
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    private string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(_basePath, fullPath).Replace('\\', '/');
    }

    /// <summary>
    /// 获取行号
    /// </summary>
    private int GetLineNumber(string content, int index)
    {
        var lineCount = 1;
        for (var i = 0; i < index && i < content.Length; i++)
        {
            if (content[i] == '\n')
                lineCount++;
        }
        return lineCount;
    }

    /// <summary>
    /// 提取元素信息
    /// </summary>
    private (string ElementName, string? Identifier) ExtractElementInfo(string content, int matchIndex)
    {
        // 向前查找最近的开始标签
        var startTagIndex = content.LastIndexOf('<', matchIndex);
        if (startTagIndex < 0)
            return ("Unknown", null);

        // 查找标签结束位置
        var endTagIndex = content.IndexOf('>', startTagIndex);
        if (endTagIndex < 0 || endTagIndex > matchIndex + 200)
            return ("Unknown", null);

        var tagContent = content.Substring(startTagIndex, endTagIndex - startTagIndex + 1);

        // 提取元素名
        var elementMatch = Regex.Match(tagContent, @"<(\w+)");
        var elementName = elementMatch.Success ? elementMatch.Groups[1].Value : "Unknown";

        // 提取 x:Name 或 Name 属性
        var nameMatch = Regex.Match(tagContent, @"(?:x:)?Name\s*=\s*[""']([^""']*)[""']");
        var identifier = nameMatch.Success ? nameMatch.Groups[1].Value : null;

        return (elementName, identifier);
    }

    /// <summary>
    /// 去重并合并条目
    /// </summary>
    private List<LocalizationEntry> DeduplicateEntries(List<LocalizationEntry> entries)
    {
        var uniqueEntries = new Dictionary<string, LocalizationEntry>();

        foreach (var entry in entries)
        {
            // 使用值作为键进行分组（相同的文本值合并）
            var key = entry.Value;

            if (!uniqueEntries.ContainsKey(key))
            {
                uniqueEntries[key] = entry;
            }
            else
            {
                // 合并源文件信息
                var existing = uniqueEntries[key];
                existing.Comment += $"\n{entry.Comment}";
            }
        }

        return uniqueEntries.Values.ToList();
    }

    /// <summary>
    /// 生成资源键名
    /// </summary>
    public static string GenerateResourceKey(LocalizationEntry entry, int index)
    {
        var sb = new StringBuilder();

        // 优先使用元素标识符
        if (!string.IsNullOrEmpty(entry.ElementIdentifier))
        {
            sb.Append(entry.ElementIdentifier);
            sb.Append('_');
            sb.Append(entry.PropertyName);
        }
        else if (!string.IsNullOrEmpty(entry.ElementName) && entry.ElementName != "Unknown")
        {
            // 使用元素名
            sb.Append(entry.ElementName);
            sb.Append('_');
            sb.Append(entry.PropertyName);
            sb.Append('_');
            sb.Append(index);
        }
        else
        {
            // 使用属性名和索引
            sb.Append(entry.PropertyName);
            sb.Append('_');
            sb.Append(index);
        }

        // 清理键名（移除特殊字符）
        var key = sb.ToString();
        key = Regex.Replace(key, @"[^\w]", "_");
        key = Regex.Replace(key, @"_+", "_");
        key = key.Trim('_');

        return key;
    }
}