using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unreal_GUI.Model.Basic
{
    public class CompressCore
    {
        /// <summary>
        /// 使用7za.exe解压文件
        /// </summary>
        /// <param name="archivePath">压缩文件路径</param>
        /// <param name="extractPath">解压目标路径</param>
        /// <param name="onProgressChanged">进度变化回调（可选）</param>
        /// <returns>解压是否成功</returns>
        public static async Task<bool> ExtractArchiveAsync(string archivePath, string extractPath)
        {
            try
            {
                // 获取7za.exe的路径（应用程序根目录的App文件夹中）
                string appFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App");
                string sevenZipExePath = Path.Combine(appFolderPath, "7za.exe");

                // 确保目标目录存在
                Directory.CreateDirectory(extractPath);

                // 构建7za.exe的命令行参数
                // x 表示解压文件
                // -o 表示输出目录
                // -y 表示所有确认都回答是
                string arguments = $"x \"{archivePath}\" -o\"{extractPath}\" -y";

                // 创建进程信息
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExePath,
                    Arguments = arguments,
                    WorkingDirectory = appFolderPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                // 启动进程
                using var process = new Process { StartInfo = processStartInfo };
                process.Start();



                // 等待进程完成
                await process.WaitForExitAsync();

                // 检查进程退出代码并处理错误
                if (process.ExitCode != 0)
                {
                    // 读取可能的错误信息
                    await Task.Run(async () =>
                    {
                        string errorLine;
                        while ((errorLine = process.StandardError.ReadLine()) != null)
                        {
                            Console.Error.WriteLine($"错误: {errorLine}");
                        }
                    });

                    // 根据退出代码返回相应的错误信息
                    string errorMessage = process.ExitCode switch
                    {
                        1 => "警告：一些文件可能没有被压缩或损坏",
                        2 => "严重错误",
                        7 => "命令行语法错误",
                        8 => "内存不足",
                        255 => "用户停止操作",
                        _ => $"未知错误，退出代码: {process.ExitCode}"
                    };

                    await ModernDialog.ShowErrorAsync($"压缩失败：{errorMessage}", "错误");
                    return false;
                }

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                // 记录错误
                await ModernDialog.ShowErrorAsync($"解压失败：{ex.Message}", "错误");
                return false;
            }
        }

        /// <summary>
        /// 使用7za.exe压缩文件或目录
        /// </summary>
        /// <param name="inputPaths">要压缩的文件或目录路径列表</param>
        /// <param name="outputArchivePath">输出压缩文件路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> CompressFilesAsync(IEnumerable<string> inputPaths, string outputArchivePath, int compressionLevel = 5, bool soildcompress = true)
        {
            try
            {
                // 验证输入路径
                if (inputPaths == null || !inputPaths.Any())
                {
                    await ModernDialog.ShowErrorAsync("没有要压缩的文件或目录", "错误");
                    return false;
                }

                // 验证压缩级别
                if (compressionLevel < 0 || compressionLevel > 9)
                {
                    compressionLevel = 5; // 默认值
                }

                // 获取7za.exe的路径（应用程序根目录的App文件夹中）
                string appFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App");
                string sevenZipExePath = Path.Combine(appFolderPath, "7za.exe");

                // 确保输出目录存在
                string outputDirectory = Path.GetDirectoryName(outputArchivePath);
                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // 获取项目根目录（假设第一个输入路径是项目目录下的某个文件夹）
                string projectRoot = string.Empty;
                if (inputPaths.Any())
                {
                    string firstPath = inputPaths.First();
                    if (Directory.Exists(firstPath))
                    {
                        projectRoot = firstPath;
                        // 如果路径是Config、Content等子目录，获取其父目录作为项目根目录
                        while (Path.GetFileName(projectRoot) != "Config" && Path.GetFileName(projectRoot) != "Content" &&
                               Path.GetFileName(projectRoot) != "Plugins" && Path.GetFileName(projectRoot) != "Source")
                        {
                            string parent = Path.GetDirectoryName(projectRoot);
                            if (string.IsNullOrEmpty(parent)) break;
                            projectRoot = parent;
                        }
                        projectRoot = Path.GetDirectoryName(projectRoot);
                    }
                    else if (File.Exists(firstPath) && Path.GetExtension(firstPath) == ".uproject")
                    {
                        projectRoot = Path.GetDirectoryName(firstPath);
                    }
                }

                // 构建7za.exe的命令行参数
                // a 表示添加到压缩文件
                // -t 表示压缩格式
                // -mx 表示压缩级别
                // -y 表示所有确认都回答是
                var argumentsBuilder = new StringBuilder();
                string compressionFormat = "7z";
                argumentsBuilder.Append($"a -t{compressionFormat} -mx{compressionLevel} -y -bsp1 -bb0 -ms={(soildcompress ? "on" : "off")} ");

                // 添加包含和排除参数
                if (!string.IsNullOrEmpty(projectRoot))
                {
                    // 切换到项目根目录，这样可以使用相对路径进行包含和排除
                    argumentsBuilder.Append($"-w\"{projectRoot}\" ");

                    // 包含的目录和文件
                    argumentsBuilder.Append(@"-ir!Config\* ");
                    argumentsBuilder.Append(@"-ir!Content\* ");
                    argumentsBuilder.Append(@"-ir!Plugins\* ");
                    argumentsBuilder.Append(@"-ir!Source\* ");
                    argumentsBuilder.Append(@"-ir!*.uproject ");
                    // 排除的目录
                    argumentsBuilder.Append(@"-xr!Plugins\*\Intermediate\* ");
                    argumentsBuilder.Append(@"-xr!Plugins\*\Binaries\* ");
                }

                // 指定输出路径
                argumentsBuilder.Append($"\"{outputArchivePath}\"");

                // 创建进程信息
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExePath,
                    Arguments = argumentsBuilder.ToString(),
                    WorkingDirectory = projectRoot ?? appFolderPath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true
                };

                // 启动进程
                using var process = new Process { StartInfo = processStartInfo };
                process.Start();

                // 不需要异步读取输出，因为输出会直接显示在终端窗口中
                // 只读取错误输出，以便在出错时能够捕获并处理它们
                _ = Task.Run(() =>
               {
                   string errorLine;
                   while ((errorLine = process.StandardError.ReadLine()) != null)
                   {
                       Console.Error.WriteLine($"错误: {errorLine}");
                   }
               });

                // 等待进程完成
                await process.WaitForExitAsync();

                // 检查进程退出代码
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"压缩失败：{ex.Message}", "错误");
                return false;
            }
        }

        /// <summary>
        /// 使用7za.exe压缩单个文件或目录
        /// </summary>
        /// <param name="inputPath">要压缩的文件或目录路径</param>
        /// <param name="outputArchivePath">输出压缩文件路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> CompressFileAsync(string inputPath, string outputArchivePath, int compressionLevel = 5, bool SoildCompress = true)
        {
            return await CompressFilesAsync([inputPath], outputArchivePath, compressionLevel, SoildCompress);
        }
    }
}
