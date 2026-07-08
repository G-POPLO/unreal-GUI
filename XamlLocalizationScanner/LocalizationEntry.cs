namespace XamlLocalizationScanner;

/// <summary>
/// 表示一个本地化条目
/// </summary>
public class LocalizationEntry
{
    /// <summary>
    /// 资源键名
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 原始文本值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 源文件路径（相对路径）
    /// </summary>
    public string SourceFile { get; set; } = string.Empty;

    /// <summary>
    /// 行号
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// 属性名（如 Text, Content, Header 等）
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// 元素名（如 TextBlock, Button 等）
    /// </summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>
    /// 元素的 x:Name 或 Name 属性（如果有）
    /// </summary>
    public string? ElementIdentifier { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Comment { get; set; }

    public override string ToString()
    {
        return $"[{Key}] {Value} ({SourceFile}:{LineNumber})";
    }
}