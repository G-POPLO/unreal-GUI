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
        /// <param name="projectPath">要压缩的文件或目录路径</param>
        /// <param name="outputArchivePath">输出压缩文件路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> CompressFilesAsync(string projectPath, string outputArchivePath, int compressionLevel = 5, bool soildcompress = true, bool filter = true)
        {
            try
            {
                // 验证输入路径
                if (string.IsNullOrWhiteSpace(projectPath))
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

                // 获取项目根目录（假设传入的路径就是项目根目录）
                //string projectRoot = projectPath;

                // 构建7za.exe的命令行参数
                // a 表示添加到压缩文件
                // -t 表示压缩格式
                // -mx 表示压缩级别
                // -y 表示所有确认都回答是
                var argumentsBuilder = new StringBuilder();
                string compressionFormat = "7z";
                argumentsBuilder.Append($"a -t{compressionFormat} -mx{compressionLevel} -y -bsp1 -bb0 -ms={(soildcompress ? "on" : "off")} ");

                // 根据过滤设置添加包含和排除规则
                if (filter)
                {
                    // 使用包含和排除规则压缩项目
                    argumentsBuilder.Append($"-w\"{projectPath}\" ");
                    argumentsBuilder.Append(@"-ir!Config\* ");
                    argumentsBuilder.Append(@"-ir!Content\* ");
                    argumentsBuilder.Append(@"-ir!Plugins\* ");
                    argumentsBuilder.Append(@"-ir!Source\* ");
                    argumentsBuilder.Append(@"-ir!*.uproject ");
                    // 排除的目录
                    argumentsBuilder.Append(@"-xr!Plugins\*\Intermediate\* ");
                    argumentsBuilder.Append(@"-xr!Plugins\*\Binaries\* ");
                }
                else
                {
                    // 不使用过滤器，压缩整个项目目录
                    argumentsBuilder.Append($"\"{projectPath}\" ");
                }

                // 指定输出路径
                argumentsBuilder.Append($"\"{outputArchivePath}\"");

                // 创建进程信息
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExePath,
                    Arguments = argumentsBuilder.ToString(),
                    WorkingDirectory = projectPath,
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
                string arguments = $"l -slt \"{archivePath}\"";

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
                return output.Contains("Solid = +");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 使用7za.exe更新压缩包（增量更新）
        /// </summary>
        /// <param name="projectPath">要压缩的项目目录路径</param>
        /// <param name="targetArchivePath">要更新的压缩包路径</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <param name="filter">是否启用文件过滤器</param>
        /// <returns>更新是否成功</returns>
        public static async Task<bool> UpdateArchiveAsync(string projectPath, string targetArchivePath, int compressionLevel = 5, bool filter = true)
        {
            try
            {
                // 验证项目目录
                if (string.IsNullOrWhiteSpace(projectPath))
                {
                    await ModernDialog.ShowErrorAsync("没有要压缩的项目目录", "错误");
                    return false;
                }

                // 验证项目目录存在
                if (!Directory.Exists(projectPath))
                {
                    await ModernDialog.ShowErrorAsync($"项目目录不存在: {projectPath}", "错误");
                    return false;
                }

                // 检查目标压缩包是否存在
                if (!File.Exists(targetArchivePath))
                {
                    await ModernDialog.ShowErrorAsync("要更新的压缩包不存在", "错误");
                    return false;
                }

                // 检查目标压缩包是否为固实压缩
                if (await IsSolidArchiveAsync(targetArchivePath))
                {
                    await ModernDialog.ShowErrorAsync("无法更新固实压缩的压缩包，请重新创建压缩包或取消固实压缩", "错误");
                    return false;
                }

                // 获取7za.exe的路径
                string appFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App");
                string sevenZipExePath = Path.Combine(appFolderPath, "7za.exe");

                // 构建7za.exe的命令行参数            
                var argumentsBuilder = new StringBuilder();

                // 根据过滤设置构建不同的参数
                if (filter)
                {
                    // 启用过滤器：使用相对路径
                    // 关于这方面的内容请参考：https://documentation.help/7-Zip/update1.htm
                    argumentsBuilder.Append($"u \"{targetArchivePath}\" -up1q0r2x1y2z1w2 -mx{compressionLevel} -y -ms=off ");
                    argumentsBuilder.Append("-ir!Config\\* ");
                    argumentsBuilder.Append("-ir!Content\\* ");
                    argumentsBuilder.Append("-ir!Plugins\\* ");
                    argumentsBuilder.Append("-ir!Source\\* ");
                    argumentsBuilder.Append("-ir!*.uproject ");
                    // 排除的目录
                    argumentsBuilder.Append("-xr!Plugins\\*\\Intermediate\\* ");
                    argumentsBuilder.Append("-xr!Plugins\\*\\Binaries\\* ");
                }
                else
                {
                    // 不使用过滤器，更新整个项目目录
                    argumentsBuilder.Append($"u \"{targetArchivePath}\" -up1q0r2x1y2z1w2 -mx{compressionLevel} -y -ms=off \"*\" ");
                }

                // 创建进程信息（工作目录设置为项目目录）
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExePath,
                    Arguments = argumentsBuilder.ToString(),
                    WorkingDirectory = projectPath,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true
                };

                // 启动进程
                using var process = new Process { StartInfo = processStartInfo };
                process.Start();

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