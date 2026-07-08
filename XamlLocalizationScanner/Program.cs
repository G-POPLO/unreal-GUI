using System.CommandLine;
using System.CommandLine.Invocation;

namespace XamlLocalizationScanner;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("XAML 本地化扫描工具 - 扫描 XAML 文件并提取硬编码文本");

        // 扫描命令
        var scanCommand = new Command("scan", "扫描 XAML 文件并提取硬编码文本");
        var pathArgument = new Argument<string>("path", "要扫描的目录或文件路径");
        var outputOption = new Option<string>(["--output", "-o"], () => "Resources", "输出目录路径");
        var formatOption = new Option<string[]>(["--format", "-f"], () => ["resx", "json", "csv"], "输出格式（resx, json, csv, template）");
        var includeNonCjkOption = new Option<bool>(["--include-non-cjk", "-i"], () => false, "是否包含非 CJK 文本（如英文）");
        var languageOption = new Option<string>(["--language", "-l"], () => "zh-CN", "源语言代码");

        scanCommand.AddArgument(pathArgument);
        scanCommand.AddOption(outputOption);
        scanCommand.AddOption(formatOption);
        scanCommand.AddOption(includeNonCjkOption);
        scanCommand.AddOption(languageOption);

        scanCommand.SetHandler(async (context) =>
        {
            var path = context.ParseResult.GetValueForArgument(pathArgument);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var formats = context.ParseResult.GetValueForOption(formatOption);
            var includeNonCjk = context.ParseResult.GetValueForOption(includeNonCjkOption);
            var language = context.ParseResult.GetValueForOption(languageOption);

            await ScanAndExport(path, output, formats, includeNonCjk, language);
        });

        rootCommand.AddCommand(scanCommand);

        // 应用翻译命令
        var applyCommand = new Command("apply", "应用翻译到 XAML 文件（生成替换后的文件）");
        var xamlPathArgument = new Argument<string>("xaml-path", "XAML 文件或目录路径");
        var translationFileOption = new Option<string>(["--translation", "-t"], "翻译文件路径（CSV 格式）");
        var outputDirOption = new Option<string>(["--output-dir", "-o"], () => "Localized", "输出目录");

        applyCommand.AddArgument(xamlPathArgument);
        applyCommand.AddOption(translationFileOption);
        applyCommand.AddOption(outputDirOption);

        applyCommand.SetHandler(async (context) =>
        {
            var xamlPath = context.ParseResult.GetValueForArgument(xamlPathArgument);
            var translationFile = context.ParseResult.GetValueForOption(translationFileOption);
            var outputDir = context.ParseResult.GetValueForOption(outputDirOption);

            await ApplyTranslations(xamlPath, translationFile, outputDir);
        });

        rootCommand.AddCommand(applyCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ScanAndExport(string path, string outputDir, string[] formats, bool includeNonCjk, string language)
    {
        Console.WriteLine("=== XAML 本地化扫描工具 ===");
        Console.WriteLine($"扫描路径: {path}");
        Console.WriteLine($"输出目录: {outputDir}");
        Console.WriteLine($"输出格式: {string.Join(", ", formats)}");
        Console.WriteLine($"包含非 CJK 文本: {includeNonCjk}");
        Console.WriteLine($"源语言: {language}");
        Console.WriteLine();

        // 确定扫描路径
        var scanPath = Path.GetFullPath(path);
        if (!Directory.Exists(scanPath) && !File.Exists(scanPath))
        {
            Console.WriteLine($"错误: 路径不存在 - {scanPath}");
            return;
        }

        // 如果是文件，扫描其所在目录
        if (File.Exists(scanPath))
        {
            scanPath = Path.GetDirectoryName(scanPath) ?? scanPath;
        }

        // 创建扫描器
        var scanner = new XamlScanner(scanPath, includeNonCjk);

        // 扫描文件
        Console.WriteLine("正在扫描 XAML 文件...");
        var entries = scanner.ScanDirectory(scanPath);

        Console.WriteLine($"找到 {entries.Count} 个本地化条目");
        Console.WriteLine();

        if (entries.Count == 0)
        {
            Console.WriteLine("未找到需要本地化的文本");
            return;
        }

        // 生成资源键
        for (var i = 0; i < entries.Count; i++)
        {
            entries[i].Key = XamlScanner.GenerateResourceKey(entries[i], i + 1);
        }

        // 创建输出目录
        Directory.CreateDirectory(outputDir);

        // 导出
        var exporter = new ResourceExporter();

        foreach (var format in formats)
        {
            var outputPath = format switch
            {
                "resx" => Path.Combine(outputDir, $"Strings.{language}.resx"),
                "json" => Path.Combine(outputDir, $"strings.{language}.json"),
                "csv" => Path.Combine(outputDir, $"strings.{language}.csv"),
                "template" => Path.Combine(outputDir, "translation_template.csv"),
                _ => Path.Combine(outputDir, $"strings.{language}.{format}")
            };

            Console.WriteLine($"导出到: {outputPath}");

            try
            {
                switch (format)
                {
                    case "resx":
                        exporter.ExportToResx(entries, outputPath, language);
                        break;
                    case "json":
                        exporter.ExportToJson(entries, outputPath);
                        break;
                    case "csv":
                        exporter.ExportToCsv(entries, outputPath);
                        break;
                    case "template":
                        exporter.ExportTranslationTemplate(entries, outputPath, language, "en-US");
                        break;
                    default:
                        Console.WriteLine($"警告: 不支持的格式 - {format}");
                        continue;
                }

                Console.WriteLine($"  ✓ 成功导出 {entries.Count} 个条目");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ 导出失败: {ex.Message}");
            }
        }

        // 显示统计信息
        Console.WriteLine();
        Console.WriteLine("=== 扫描统计 ===");
        var groupedByFile = entries.GroupBy(e => e.SourceFile);
        foreach (var group in groupedByFile.OrderBy(g => g.Key))
        {
            Console.WriteLine($"  {group.Key}: {group.Count()} 个条目");
        }

        Console.WriteLine();
        Console.WriteLine("=== 完成 ===");
        Console.WriteLine($"输出目录: {Path.GetFullPath(outputDir)}");
    }

    static async Task ApplyTranslations(string xamlPath, string translationFile, string outputDir)
    {
        Console.WriteLine("=== 应用翻译 ===");
        Console.WriteLine($"XAML 路径: {xamlPath}");
        Console.WriteLine($"翻译文件: {translationFile}");
        Console.WriteLine($"输出目录: {outputDir}");
        Console.WriteLine();

        if (!File.Exists(translationFile))
        {
            Console.WriteLine($"错误: 翻译文件不存在 - {translationFile}");
            return;
        }

        // 导入翻译
        var exporter = new ResourceExporter();
        var translations = exporter.ImportFromCsv(translationFile);

        Console.WriteLine($"加载了 {translations.Count} 条翻译");

        // TODO: 实现 XAML 文件替换逻辑
        // 这需要更复杂的实现来替换 XAML 中的硬编码文本为资源引用

        Console.WriteLine("注意: 应用翻译功能尚未完全实现");
        Console.WriteLine("请手动将资源文件集成到项目中");
    }
}