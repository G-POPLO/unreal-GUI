using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace unreal_GUI.Model
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
        public static async Task<bool> ExtractArchiveAsync(string archivePath, string extractPath, Action<int> onProgressChanged = null)
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

                // 异步读取输出，以便跟踪进度
                await Task.Run(() =>
                {
                    string line;
                    while ((line = process.StandardOutput.ReadLine()) != null)
                    {
                        // 尝试解析进度信息
                        // 7za.exe的输出格式可能类似："Extracting  filename" 或包含百分比
                        // 这里简化处理，实际应用中可能需要更复杂的解析
                        if (line.Contains('%') && int.TryParse(line.Split('%')[0].Trim(), out int progress))
                        {
                            onProgressChanged?.Invoke(progress);
                        }
                    }
                });

                // 等待进程完成
                await process.WaitForExitAsync();

                // 检查进程退出代码
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
        /// <param name="onProgressChanged">进度变化回调（可选）</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <param name="compressionFormat">压缩格式（默认7z）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> CompressFilesAsync(IEnumerable<string> inputPaths, string outputArchivePath, Action<int> onProgressChanged = null, int compressionLevel = 5, string compressionFormat = "7z")
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

                // 构建7za.exe的命令行参数
                // a 表示添加到压缩文件
                // -t 表示压缩格式
                // -mx 表示压缩级别
                // -y 表示所有确认都回答是
                string arguments = $"a -t{compressionFormat} -mx{compressionLevel} -y \"{outputArchivePath}\"";

                // 添加要压缩的文件或目录
                foreach (string path in inputPaths)
                {
                    arguments += $" \"{path}\"";
                }

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

                // 异步读取输出，以便跟踪进度
                await Task.Run(() =>
                {
                    string line;
                    while ((line = process.StandardOutput.ReadLine()) != null)
                    {
                        // 尝试解析进度信息
                        // 7za.exe的输出格式可能类似："Compressing  filename" 或包含百分比
                        if (line.Contains('%') && int.TryParse(line.Split('%')[0].Trim(), out int progress))
                        {
                            onProgressChanged?.Invoke(progress);
                        }
                    }
                });

                // 等待进程完成
                await process.WaitForExitAsync();

                // 检查进程退出代码
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                // 记录错误
                await ModernDialog.ShowErrorAsync($"压缩失败：{ex.Message}", "错误");
                return false;
            }
        }

        /// <summary>
        /// 使用7za.exe压缩单个文件或目录
        /// </summary>
        /// <param name="inputPath">要压缩的文件或目录路径</param>
        /// <param name="outputArchivePath">输出压缩文件路径</param>
        /// <param name="onProgressChanged">进度变化回调（可选）</param>
        /// <param name="compressionLevel">压缩级别（0-9，默认5）</param>
        /// <param name="compressionFormat">压缩格式（默认7z）</param>
        /// <returns>压缩是否成功</returns>
        public static async Task<bool> CompressFileAsync(string inputPath, string outputArchivePath, Action<int> onProgressChanged = null, int compressionLevel = 5, string compressionFormat = "7z")
        {
            return await CompressFilesAsync([inputPath], outputArchivePath, onProgressChanged, compressionLevel, compressionFormat);
        }
    }
}
