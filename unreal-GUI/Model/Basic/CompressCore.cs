using System;
using System.Diagnostics;
using System.IO;
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
        /// <param name="inputPath">要压缩的文件或目录路径</param>
        /// <param name="outputArchivePath">输出压缩文件路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> CompressFilesAsync(string inputPath, string outputArchivePath, int compressionLevel = 5, bool soildcompress = true)
        {
            try
            {
                // 验证输入路径
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    await ModernDialog.ShowErrorAsync("没有要压缩的文件或目录", "错误");
                    return false;
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

                // 获取项目根目录
                string projectRoot = string.Empty;
                if (Directory.Exists(inputPath))
                {
                    // 检查输入路径是否包含.uproject文件，或者本身就是项目根目录
                    var uprpojectFiles = Directory.GetFiles(inputPath, "*.uproject");
                    if (uprpojectFiles.Length > 0)
                    {
                        // 如果输入路径包含.uproject文件，这就是项目根目录
                        projectRoot = inputPath;
                    }
                }
                else if (File.Exists(inputPath) && Path.GetExtension(inputPath) == ".uproject")
                {
                    projectRoot = Path.GetDirectoryName(inputPath);
                }

                // 构建7za.exe的命令行参数
                // a 表示添加到压缩文件
                // -t 表示压缩格式
                // -mx 表示压缩级别
                // -y 表示所有确认都回答是
                var argumentsBuilder = new StringBuilder();
                string compressionFormat = "7z";
                argumentsBuilder.Append($"a -t{compressionFormat} -mx{compressionLevel} -y -bsp1 -bb0 -ms={(soildcompress ? "on" : "off")} ");

                // 根据输入路径类型构建不同的压缩策略
                if (!string.IsNullOrEmpty(projectRoot))
                {
                    // 切换到项目根目录，这样可以使用相对路径进行包含和排除
                    argumentsBuilder.Append($"-w\"{projectRoot}\" ");

                    // 如果输入路径是项目根目录下的子目录（如Config、Content等）
                    if (inputPath != projectRoot)
                    {
                        // 直接压缩指定的子目录或文件
                        string relativePath = Path.GetRelativePath(projectRoot, inputPath);
                        argumentsBuilder.Append($"\"{relativePath}\" ");
                    }
                    else
                    {
                        // 如果输入路径就是项目根目录，使用包含和排除规则
                        argumentsBuilder.Append(@"-ir!Config\* ");
                        argumentsBuilder.Append(@"-ir!Content\* ");
                        argumentsBuilder.Append(@"-ir!Plugins\* ");
                        argumentsBuilder.Append(@"-ir!Source\* ");
                        argumentsBuilder.Append(@"-ir!*.uproject ");
                        // 排除的目录
                        argumentsBuilder.Append(@"-xr!Plugins\*\Intermediate\* ");
                        argumentsBuilder.Append(@"-xr!Plugins\*\Binaries\* ");
                    }
                }
                else
                {
                    // 直接压缩输入的文件或目录
                    argumentsBuilder.Append($"\"{inputPath}\" ");
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
            return await CompressFilesAsync(inputPath, outputArchivePath, compressionLevel, SoildCompress);
        }

        /// <summary>
        /// 检查压缩包是否为固实压缩
        /// </summary>
        /// <param name="archivePath">压缩包路径</param>
        /// <returns>如果为固实压缩返回true，否则返回false</returns>
        public static async Task<bool> IsSolidArchiveAsync(string archivePath)
        {
            try
            {
                // 获取7za.exe的路径
                string appFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App");
                string sevenZipExePath = Path.Combine(appFolderPath, "7za.exe");

                // 构建命令获取压缩包信息
                string arguments = $"l -sccUTF-8 \"{archivePath}\"";

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

                // 读取输出
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // 检查退出代码
                if (process.ExitCode != 0)
                {
                    return false;
                }

                // 在输出中查找固实压缩标识
                // 7za的列表输出中，固实压缩会在某些地方显示相关信息
                return output.Contains("Solid");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 使用7za.exe更新压缩包（增量更新）
        /// </summary>
        /// <param name="inputPath">要添加的7z文件路径</param>
        /// <param name="archivePath">要更新的压缩包路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateArchiveAsync(string inputPath, string archivePath, int compressionLevel = 5)
        {
            try
            {
                // 验证输入路径 - 确保是7z文件
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    await ModernDialog.ShowErrorAsync("没有要更新的7z文件", "错误");
                    return false;
                }

                // 验证输入文件存在且是7z文件
                if (!File.Exists(inputPath))
                {
                    await ModernDialog.ShowErrorAsync($"文件不存在: {inputPath}", "错误");
                    return false;
                }
                if (!string.Equals(Path.GetExtension(inputPath), ".7z", StringComparison.OrdinalIgnoreCase))
                {
                    await ModernDialog.ShowErrorAsync($"只能添加7z文件，当前文件不是7z格式: {inputPath}", "错误");
                    return false;
                }

                // 检查压缩包是否存在
                if (!File.Exists(archivePath))
                {
                    await ModernDialog.ShowErrorAsync("要更新的压缩包不存在", "错误");
                    return false;
                }

                // 检查压缩包是否为固实压缩
                if (await IsSolidArchiveAsync(archivePath))
                {
                    await ModernDialog.ShowErrorAsync("无法更新固实压缩的压缩包，请重新创建压缩包", "错误");
                    return false;
                }

                // 获取7za.exe的路径
                string appFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App");
                string sevenZipExePath = Path.Combine(appFolderPath, "7za.exe");

                // 构建7za.exe的命令行参数
                // u 表示更新压缩包
                var argumentsBuilder = new StringBuilder();
                argumentsBuilder.Append($"u -mx{compressionLevel} -y -bsp1 -bb0 ");

                // 添加要添加的7z文件
                argumentsBuilder.Append($"\"{inputPath}\" ");


                // 指定目标压缩包路径
                argumentsBuilder.Append($"\"{archivePath}\"");

                // 创建进程信息
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExePath,
                    Arguments = argumentsBuilder.ToString(),
                    WorkingDirectory = appFolderPath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true
                };

                // 启动进程
                using var process = new Process { StartInfo = processStartInfo };
                process.Start();

                // 只读取错误输出
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
                if (process.ExitCode != 0)
                {
                    // 根据退出代码返回相应的错误信息
                    string errorMessage = process.ExitCode switch
                    {
                        1 => "警告：一些文件可能没有被更新或损坏",
                        2 => "严重错误",
                        7 => "命令行语法错误",
                        8 => "内存不足",
                        255 => "用户停止操作",
                        _ => $"未知错误，退出代码: {process.ExitCode}"
                    };

                    await ModernDialog.ShowErrorAsync($"增量更新失败：{errorMessage}", "错误");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"增量更新失败：{ex.Message}", "错误");
                return false;
            }
        }
    }
}
