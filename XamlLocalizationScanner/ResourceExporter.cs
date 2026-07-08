using System.Text;
using System.Xml;

namespace XamlLocalizationScanner;

/// <summary>
/// 资源文件导出器，支持导出为 .resx 和 .json 格式
/// </summary>
public class ResourceExporter
{
    /// <summary>
    /// 导出为 .resx 格式（标准 .NET 资源文件）
    /// </summary>
    public void ExportToResx(List<LocalizationEntry> entries, string outputPath, string languageCode = "zh-CN")
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8
        };

        using var writer = XmlWriter.Create(outputPath, settings);

        writer.WriteStartDocument();
        writer.WriteStartElement("root");

        // 写入架构信息
        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "resmimetype");
        writer.WriteStartElement("value");
        writer.WriteString("text/microsoft-resx");
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "version");
        writer.WriteStartElement("value");
        writer.WriteString("2.0");
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "reader");
        writer.WriteStartElement("value");
        writer.WriteString("System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("resheader");
        writer.WriteAttributeString("name", "writer");
        writer.WriteStartElement("value");
        writer.WriteString("System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        writer.WriteEndElement();
        writer.WriteEndElement();

        // 写入资源条目
        foreach (var entry in entries)
        {
            writer.WriteStartElement("data");
            writer.WriteAttributeString("name", entry.Key);
            writer.WriteAttributeString("xml", "space", null, "preserve");

            writer.WriteStartElement("value");
            writer.WriteString(entry.Value);
            writer.WriteEndElement();

            if (!string.IsNullOrEmpty(entry.Comment))
            {
                writer.WriteStartElement("comment");
                writer.WriteString(entry.Comment);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    /// <summary>
    /// 导出为 JSON 格式（便于翻译工具使用）
    /// </summary>
    public void ExportToJson(List<LocalizationEntry> entries, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var comma = i < entries.Count - 1 ? "," : "";

            sb.AppendLine($"  \"{EscapeJsonString(entry.Key)}\": {{");
            sb.AppendLine($"    \"value\": \"{EscapeJsonString(entry.Value)}\",");
            sb.AppendLine($"    \"comment\": \"{EscapeJsonString(entry.Comment ?? "")}\"");
            sb.AppendLine($"  }}{comma}");
        }

        sb.AppendLine("}");

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 导出为 CSV 格式（便于在 Excel 中编辑）
    /// </summary>
    public void ExportToCsv(List<LocalizationEntry> entries, string outputPath)
    {
        var sb = new StringBuilder();

        // CSV 头
        sb.AppendLine("Key,Value,Source File,Line Number,Property Name,Element Name,Comment");

        foreach (var entry in entries)
        {
            sb.AppendLine($"\"{EscapeCsvField(entry.Key)}\",\"{EscapeCsvField(entry.Value)}\",\"{EscapeCsvField(entry.SourceFile)}\",{entry.LineNumber},\"{EscapeCsvField(entry.PropertyName)}\",\"{EscapeCsvField(entry.ElementName)}\",\"{EscapeCsvField(entry.Comment ?? "")}\"");
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 导出翻译模板（包含空的目标语言列）
    /// </summary>
    public void ExportTranslationTemplate(List<LocalizationEntry> entries, string outputPath, string sourceLanguage = "zh-CN", string targetLanguage = "en-US")
    {
        var sb = new StringBuilder();

        // CSV 头
        sb.AppendLine($"Key,Source ({sourceLanguage}),Target ({targetLanguage}),Comment");

        foreach (var entry in entries)
        {
            sb.AppendLine($"\"{EscapeCsvField(entry.Key)}\",\"{EscapeCsvField(entry.Value)}\",\"\",\"{EscapeCsvField(entry.Comment ?? "")}\"");
        }

        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 从 CSV 导入翻译结果
    /// </summary>
    public Dictionary<string, string> ImportFromCsv(string csvPath)
    {
        var translations = new Dictionary<string, string>();
        var lines = File.ReadAllLines(csvPath);

        // 跳过标题行
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // 简单的 CSV 解析（假设没有逗号在字段内）
            var parts = ParseCsvLine(line);
            if (parts.Length >= 3)
            {
                var key = parts[0].Trim('"');
                var targetValue = parts[2].Trim('"');
                if (!string.IsNullOrEmpty(targetValue))
                {
                    translations[key] = targetValue;
                }
            }
        }

        return translations;
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString());
        return [.. result];
    }

    private string EscapeJsonString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private string EscapeCsvField(string value)
    {
        return value.Replace("\"", "\"\"");
    }
}