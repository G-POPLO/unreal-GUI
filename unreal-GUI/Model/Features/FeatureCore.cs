using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace unreal_GUI.Model.Features
{
    /// <summary>
    /// 进度报告事件参数
    /// </summary>
    /// <remarks>
    /// 构造函数
    /// </remarks>
    /// <param name="message">进度消息</param>
    public class ProgressReportedEventArgs(string message) : EventArgs
    {
        /// <summary>
        /// 进度消息
        /// </summary>
        public string Message { get; set; } = message;
    }

    /// <summary>
    /// 资产复制进度报告事件参数
    /// </summary>
    /// <remarks>
    /// 构造函数
    /// </remarks>
    /// <param name="filesCopied">已复制的文件数</param>
    /// <param name="totalFiles">总文件数</param>
    /// <param name="currentFile">当前正在复制的文件名</param>
    public class AssetCopyProgressEventArgs(int filesCopied, int totalFiles, string currentFile = "") : EventArgs
    {
        /// <summary>
        /// 已复制的文件数
        /// </summary>
        public int FilesCopied { get; set; } = filesCopied;

        /// <summary>
        /// 总文件数
        /// </summary>
        public int TotalFiles { get; set; } = totalFiles;

        /// <summary>
        /// 当前正在复制的文件名
        /// </summary>
        public string CurrentFile { get; set; } = currentFile;

        /// <summary>
        /// 进度百分比
        /// </summary>
        public double ProgressPercentage => TotalFiles > 0 ? (double)FilesCopied / TotalFiles * 100 : 0;
    }

    public class FeatureCore
    {
        // 存储资产路径映射，用于复制和安装
        private readonly Dictionary<string, string> AssetPathsMap = [];

        // 缓存的JSON序列化选项，避免每次创建新实例
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// 进度报告事件
        /// </summary>
        //public event EventHandler<ProgressReportedEventArgs>? ProgressReported;

        /// <summary>
        /// 资产复制进度报告事件
        /// </summary>
        public event EventHandler<AssetCopyProgressEventArgs>? AssetCopyProgressReported;

        /// <summary>
        /// 触发进度报告事件
        /// </summary>
        /// <param name="message">进度消息</param>
        private static void OnProgressReported(string message)
        {
            // 输出到控制台
            Debug.WriteLine(message);

            // 触发事件
            //ProgressReported?.Invoke(this, new ProgressReportedEventArgs(message));
        }

        /// <summary>
        /// 触发资产复制进度报告事件
        /// </summary>
        /// <param name="args">资产复制进度事件参数</param>
        private void OnAssetCopyProgressReported(AssetCopyProgressEventArgs args)
        {
            AssetCopyProgressReported?.Invoke(this, args);
        }

        /// <summary>
        /// 移除内容包名称中的空格
        /// </summary>
        /// <param name="contentPackName">内容包名称</param>
        /// <returns>移除空格后的内容包名称</returns>
        private static string NormalizeContentPackName(string contentPackName) => contentPackName.Replace(" ", string.Empty);

        /// <summary>
        /// 生成内容包的主要方法
        /// </summary>
        /// <param name="enginePath">引擎路径</param>
        /// <param name="contentPackName">内容包名称</param>
        /// <param name="description">内容包描述</param>
        /// <param name="searchTags">搜索标签</param>
        /// <param name="selectedFolderPath">选中的文件夹路径</param>
        /// <param name="autoInstall">是否自动安装到引擎目录</param>
        /// <returns>是否生成成功</returns>
        public async Task<bool> GenerateContentPackAsync(string enginePath, string contentPackName, string description, string searchTags,
                                                      string selectedFolderPath, bool autoInstall = true)
        {
            string contentPackDir = string.Empty;
            bool result = false;

            try
            {
                // 参数验证
                if (string.IsNullOrWhiteSpace(enginePath))
                    throw new ArgumentException("引擎路径不能为空", nameof(enginePath));

                if (string.IsNullOrWhiteSpace(contentPackName))
                    throw new ArgumentException("内容包名称不能为空", nameof(contentPackName));

                if (!Directory.Exists(selectedFolderPath))
                    throw new DirectoryNotFoundException($"选中的文件夹不存在: {selectedFolderPath}");

                OnProgressReported("初始化文件夹结构...");

                // 使用选中的文件夹路径作为基础目录
                string baseDir = selectedFolderPath;

                // 初始化文件夹结构
                InitBasicFolderStructure(baseDir, out contentPackDir, out string contentSettingsDir, out string featurePackDir, out string samplesDir);

                // 移除空格的内容包名称
                string contentPackNameNoSpace = NormalizeContentPackName(contentPackName);

                // 检查是否存在同名内容包
                if (await CheckSameNameContentPackAsync(enginePath, contentPackNameNoSpace))
                {
                    OnProgressReported("生成失败：已存在同名内容包");
                    return false;
                }

                OnProgressReported("生成配置文件...");

                // 生成配置文件
                InitConfigFile(contentSettingsDir, contentPackName);
                InitJsonFile(contentSettingsDir, contentPackName, description, searchTags);

                OnProgressReported("创建ContentToUPack.txt文件...");

                // 创建ContentToUPack.txt文件
                string contentToUPackPath = await CreateContentToPackFileAsync(contentPackDir, contentSettingsDir);

                OnProgressReported("复制资产到生成的内容包文件夹...");

                // 复制资产到生成的内容包文件夹
                bool shouldContinue = await CopyAssetsToGeneratedContentPackAsync(samplesDir, contentPackName, selectedFolderPath);
                if (!shouldContinue)
                {
                    OnProgressReported("生成失败：没有资产被复制");
                    return false;
                }

                OnProgressReported("生成.upack文件...");

                // 生成.upack文件
                string generatedUpackPath = await CreateUPackGenerationBatFileAsync(enginePath, contentPackDir, contentToUPackPath, contentPackNameNoSpace, featurePackDir);

                // 自动安装到引擎目录
                if (autoInstall)
                {
                    OnProgressReported("安装内容包到引擎目录...");
                    bool placementResult = await HandleCreatedContentPackPlacementAsync(enginePath, generatedUpackPath, contentPackNameNoSpace);

                    if (placementResult)
                    {
                        OnProgressReported("内容包生成成功！");
                    }
                    else
                    {
                        OnProgressReported("内容包生成成功，但安装到引擎目录失败");
                    }

                    result = placementResult;
                }
                else
                {
                    OnProgressReported("内容包生成成功！");
                    result = true;
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                string errorMessage = $"生成内容包失败: {ex.Message}";
                Debug.WriteLine($"{errorMessage}\n{ex.StackTrace}");
                OnProgressReported(errorMessage);
                result = false;
            }
            finally
            {
                // 清理临时生成的内容包目录
                if (!string.IsNullOrEmpty(contentPackDir) && Directory.Exists(contentPackDir))
                {
                    try
                    {
                        Directory.Delete(contentPackDir, true);
                        Debug.WriteLine($"已清理临时目录: {contentPackDir}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"清理临时目录失败: {ex.Message}");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 检查UnrealPak.exe是否存在
        /// </summary>
        /// <param name="enginePath">引擎路径</param>
        /// <returns>是否存在</returns>
        public static bool IsUnrealPakAvailable(string enginePath)
        {
            string unrealPakPath = Path.Combine(enginePath, "Engine", "Binaries", "Win64", "UnrealPak.exe");
            return File.Exists(unrealPakPath);
        }



        /// <summary>
        /// 获取生成的内容包目录
        /// </summary>
        /// <returns>生成的内容包目录</returns>
        public static string GetGeneratedContentPackDir()
        {
            string projectDir = Directory.GetCurrentDirectory();
            return Path.Combine(projectDir, "GeneratedContentPack");
        }

        /// <summary>
        /// 创建基本文件夹结构
        /// </summary>
        private static void InitBasicFolderStructure(string baseDir, out string contentPackDir, out string contentSettingsDir,
                                             out string featurePackDir, out string samplesDir)
        {
            // 使用指定的基础目录
            string projectDir = baseDir;
            OnProgressReported(baseDir);

            // 主内容包目录
            contentPackDir = Path.Combine(projectDir, "GeneratedContentPack");

            // 设置子目录路径
            contentSettingsDir = Path.Combine(contentPackDir, "ContentSettings");
            featurePackDir = Path.Combine(contentPackDir, "FeaturePacks");
            samplesDir = Path.Combine(contentPackDir, "Samples");

            // 创建所有必要的目录，Directory.CreateDirectory 会自动创建不存在的父目录
            Directory.CreateDirectory(contentSettingsDir);
            Directory.CreateDirectory(featurePackDir);
            Directory.CreateDirectory(samplesDir);
        }

        /// <summary>
        /// 检查是否存在同名内容包
        /// </summary>
        private static async Task<bool> CheckSameNameContentPackAsync(string enginePath, string contentPackNameNoSpace)
        {
            try
            {
                // 引擎功能包目录
                string engineFeaturePacksDir = Path.Combine(enginePath, "FeaturePacks");
                string engineSamplesDir = Path.Combine(enginePath, "Samples");

                // 检查.upack文件是否存在
                string upackFilePath = Path.Combine(engineFeaturePacksDir, $"{contentPackNameNoSpace}.upack");
                if (File.Exists(upackFilePath))
                {
                    Debug.WriteLine($"错误：已存在同名内容包文件 {upackFilePath}");
                    return true;
                }

                // 检查样本文件夹是否存在
                string samplesFolderPath = Path.Combine(engineSamplesDir, contentPackNameNoSpace);
                if (Directory.Exists(samplesFolderPath))
                {
                    Debug.WriteLine($"错误：已存在同名样本文件夹 {samplesFolderPath}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"检查同名内容包失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 生成配置文件
        /// </summary>
        private static void InitConfigFile(string contentPackSettingsDir, string contentPackName)
        {
            try
            {
                // 创建Config目录
                string contentSettingsConfigDir = Path.Combine(contentPackSettingsDir, "Config");
                if (!Directory.Exists(contentSettingsConfigDir))
                {
                    Directory.CreateDirectory(contentSettingsConfigDir);
                }

                // 配置文件路径
                string configFilePath = Path.Combine(contentSettingsConfigDir, "config.ini");

                // 移除空格的内容包名称
                string contentPackNameNoSpace = NormalizeContentPackName(contentPackName);

                // 配置文件内容 - 将Samples目录下的内容映射到包的Content目录
                string configContent = $"[AdditionalFilesToAdd]\n+Files=Samples/{contentPackNameNoSpace}/*.*";

                // 写入配置文件
                File.WriteAllText(configFilePath, configContent);
                Debug.WriteLine($"成功生成配置文件：{configFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生成配置文件失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 生成JSON配置文件
        /// </summary>
        private static void InitJsonFile(string contentPackSettingsDir, string contentPackName,
                                 string description, string? searchTags)
        {
            try
            {
                // 创建Media目录（用于存放预览图和缩略图）
                string mediaDir = Path.Combine(contentPackSettingsDir, "Media");
                if (!Directory.Exists(mediaDir))
                {
                    Directory.CreateDirectory(mediaDir);
                }

                // 准备JSON数据
                var manifestData = new
                {
                    Version = "1",
                    Name = new[] { new { Language = "en", Text = contentPackName } },
                    Description = new[] { new { Language = "en", Text = description } },
                    AssetTypes = Array.Empty<object>(),
                    SearchTags = new[] { new { Language = "en", Text = "Custom" } },
                    ClassTypes = string.Empty,
                    Category = "Content",
                    Thumbnail = "ContentPackThumbnail64x64.png",
                    Screenshots = new[] { "ContentPackPreview.png" }
                };

                // 序列化JSON
                string jsonString = JsonSerializer.Serialize(manifestData, _jsonSerializerOptions);

                // 写入文件
                string manifestFilePath = Path.Combine(contentPackSettingsDir, "manifest.json");
                File.WriteAllText(manifestFilePath, jsonString);
                Debug.WriteLine($"成功生成JSON配置文件：{manifestFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生成JSON配置文件失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 创建ContentToUPack.txt文件
        /// </summary>
        private static async Task<string> CreateContentToPackFileAsync(string contentPackDir, string contentSettingsDir)
        {
            try
            {
                // 获取各个子目录的完整路径
                string configPath = Path.Combine(contentSettingsDir, "Config");
                string mediaPath = Path.Combine(contentSettingsDir, "Media");
                string manifestPath = Path.Combine(contentSettingsDir, "manifest.json");

                List<string> contentToPack = [];

                // 添加Config目录下的所有文件（如果存在）
                if (Directory.Exists(configPath))
                {
                    var configFiles = Directory.GetFiles(configPath, "*.*", SearchOption.AllDirectories).ToList();
                    contentToPack.AddRange(
                        configFiles.Select(file => $"\"{Path.GetFullPath(file).Replace('\\', '/')}\"")
                    );
                }

                // 添加Media目录下的所有文件（如果存在）
                if (Directory.Exists(mediaPath))
                {
                    var mediaFiles = Directory.GetFiles(mediaPath, "*.*", SearchOption.AllDirectories).ToList();
                    contentToPack.AddRange(
                        mediaFiles.Select(file => $"\"{Path.GetFullPath(file).Replace('\\', '/')}\"")
                    );
                }

                // 添加manifest.json文件（如果存在）
                if (File.Exists(manifestPath))
                {
                    string manifestFullPath = Path.GetFullPath(manifestPath).Replace('\\', '/');
                    contentToPack.Add($"\"{manifestFullPath}\"");
                }

                // 创建ContentToUPack.txt文件
                string contentToUPackFilePath = Path.Combine(contentPackDir, "ContentToUPack.txt");
                await File.WriteAllLinesAsync(contentToUPackFilePath, contentToPack);

                Debug.WriteLine($"成功创建ContentToUPack.txt文件：{contentToUPackFilePath}");
                contentToPack.ForEach(item => Debug.WriteLine($"添加到打包列表: {item}"));
                return contentToUPackFilePath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"创建ContentToUPack.txt文件失败：{ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 复制资产到生成的内容包文件夹
        /// </summary>
        private async Task<bool> CopyAssetsToGeneratedContentPackAsync(string samplesDir, string contentPackName,
                                                                                string selectedFolderPath)
        {
            try
            {
                // 清空资产路径映射
                AssetPathsMap.Clear();

                // 获取完整路径
                string fullSamplesDir = Path.GetFullPath(samplesDir);
                string fullSelectedFolderPath = Path.GetFullPath(selectedFolderPath);

                // 移除空格的内容包名称
                string contentPackNameNoSpace = NormalizeContentPackName(contentPackName);

                // 目标目录
                string targetDir = Path.Combine(fullSamplesDir, contentPackNameNoSpace, "Content");

                // 如果目标目录存在，先删除
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                // 创建目标目录
                Directory.CreateDirectory(targetDir);

                // 检查源目录是否包含Content文件夹
                string sourceContentDir = Path.Combine(fullSelectedFolderPath, "Content");

                string sourceDirToCopy = Directory.Exists(sourceContentDir) ? sourceContentDir : fullSelectedFolderPath;

                // 计算要复制的文件总数
                var allFiles = Directory.GetFiles(sourceDirToCopy, "*", SearchOption.AllDirectories);
                int totalFiles = allFiles.Length;

                if (totalFiles == 0)
                {
                    Debug.WriteLine("警告：源目录中没有文件可复制");
                    return false;
                }

                // 创建进度跟踪对象
                var progressTracker = new AssetCopyProgressTracker(totalFiles, this);

                // 复制文件夹（带进度）
                await CopyDirectoryAsync(sourceDirToCopy, targetDir, progressTracker);

                // 检查是否有文件被复制
                string[] files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    Debug.WriteLine("警告：没有文件被复制到目标目录");
                    return false;
                }

                // 构建资产路径映射，用于后续安装
                foreach (string file in files)
                {
                    string relativePath = Path.GetRelativePath(targetDir, file);
                    string engineTargetPath = Path.Combine("/Content", relativePath);
                    AssetPathsMap[engineTargetPath] = Path.GetDirectoryName(file)!;
                }

                Debug.WriteLine($"成功复制 {files.Length} 个文件到 {targetDir}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"复制资产失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 资产复制进度跟踪器
        /// </summary>
        private class AssetCopyProgressTracker(int totalFiles, FeatureCore owner)
        {
            public int TotalFiles { get; } = totalFiles;
            public int FilesCopied { get; private set; } = 0;
            private readonly FeatureCore _owner = owner;

            public void IncrementProgress(string currentFile)
            {
                FilesCopied++;
                var args = new AssetCopyProgressEventArgs(FilesCopied, TotalFiles, Path.GetFileName(currentFile));
                _owner.OnAssetCopyProgressReported(args);
            }
        }

        /// <summary>
        /// 异步复制文件夹
        /// </summary>
        /// <param name="sourceDir">源目录</param>
        /// <param name="destinationDir">目标目录</param>
        /// <param name="progressTracker">进度跟踪器（可选）</param>
        private static async Task CopyDirectoryAsync(string sourceDir, string destinationDir, AssetCopyProgressTracker? progressTracker = null)
        {
            // 创建目标目录（如果不存在）
            Directory.CreateDirectory(destinationDir);

            // 复制所有文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                await CopyFileAsync(file, destFile, progressTracker);
            }

            // 递归复制所有子目录
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                await CopyDirectoryAsync(subDir, destSubDir, progressTracker);
            }
        }

        /// <summary>
        /// 异步复制文件
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destinationFile">目标文件</param>
        /// <param name="progressTracker">进度跟踪器（可选）</param>
        private static async Task CopyFileAsync(string sourceFile, string destinationFile, AssetCopyProgressTracker? progressTracker = null)
        {
            await Task.Run(() => File.Copy(sourceFile, destinationFile, true));

            // 如果提供了进度跟踪器，则更新进度
            progressTracker?.IncrementProgress(sourceFile);
        }

        /// <summary>
        /// 创建BAT文件并调用UnrealPak.exe生成.upack文件
        /// </summary>
        /// <returns>生成的.upack文件路径</returns>
        private static async Task<string> CreateUPackGenerationBatFileAsync(string enginePath, string contentPackDir, string contentToUPackPath,
                                                           string contentPackName, string featurePackDir)
        {
            try
            {
                // UnrealPak.exe路径
                string unrealPakPath = Path.Combine(enginePath, "Engine", "Binaries", "Win64", "UnrealPak.exe");

                // 检查UnrealPak.exe是否存在
                if (!File.Exists(unrealPakPath))
                {
                    throw new FileNotFoundException("UnrealPak.exe not found", unrealPakPath);
                }

                // 构建.upack文件路径
                string featurePacksFullPath = Path.GetFullPath(featurePackDir);
                string generatedUpackPath = Path.Combine(featurePacksFullPath, $"{contentPackName}.upack");

                // 构建ContentToUPack.txt完整路径
                string contentToUPackFullPath = Path.GetFullPath(contentToUPackPath);

                // 创建BAT文件内容，使用正斜杠作为路径分隔符
                string batContent = $"\"{unrealPakPath.Replace('\\', '/')}\" -Create=\"{contentToUPackFullPath.Replace('\\', '/')}\" \"{generatedUpackPath.Replace('\\', '/')}\" -abslog=\"{Path.Combine(contentPackDir, "UnrealPak.log").Replace('\\', '/')}\"";

                // BAT文件路径
                string batFilePath = Path.Combine(contentPackDir, "GenerateContentPack.bat");

                // 写入BAT文件
                await File.WriteAllTextAsync(batFilePath, batContent);

                // 执行BAT文件
                using Process process = new();
                process.StartInfo.FileName = batFilePath;
                process.StartInfo.WorkingDirectory = contentPackDir;  // 设置工作目录为contentPackDir
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                // 检查执行结果
                if (process.ExitCode != 0)
                {
                    Debug.WriteLine($"生成.upack文件失败：{error}");
                    throw new Exception($"生成.upack文件失败：{error}");
                }

                Debug.WriteLine($"成功生成.upack文件：{generatedUpackPath}");
                Debug.WriteLine($"UnrealPak输出：{output}");
                return generatedUpackPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生成.upack文件失败：{ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 处理生成的内容包的安装
        /// </summary>
        private async Task<bool> HandleCreatedContentPackPlacementAsync(string enginePath, string generatedUpackPath, string contentPackNameNoSpace)
        {
            try
            {
                // 引擎功能包目录和样本目录
                string engineFeaturePacksDir = Path.Combine(enginePath, "FeaturePacks");
                string engineSamplesDir = Path.Combine(enginePath, "Samples");

                // 检查引擎目录是否存在
                if (!Directory.Exists(engineFeaturePacksDir) || !Directory.Exists(engineSamplesDir))
                {
                    throw new DirectoryNotFoundException("Engine directories not found");
                }

                // 复制.upack文件到引擎功能包目录
                string upackDestPath = Path.Combine(engineFeaturePacksDir, $"{contentPackNameNoSpace}.upack");
                File.Copy(generatedUpackPath, upackDestPath, true);

                // 创建样本目录
                string sampleDestDir = Path.Combine(engineSamplesDir, contentPackNameNoSpace);
                if (!Directory.Exists(sampleDestDir))
                {
                    Directory.CreateDirectory(sampleDestDir);
                }

                // 复制资产到样本目录
                foreach (var (relativeDestPath, sourceDir) in AssetPathsMap)
                {
                    // 获取目标文件的完整路径
                    string destFilePath = Path.Combine(sampleDestDir, "Content", relativeDestPath.TrimStart('/'));

                    // 创建目标目录（如果不存在）
                    string? destDir = Path.GetDirectoryName(destFilePath);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    // 获取要复制的源文件名（从目标路径中提取）
                    string sourceFileName = Path.GetFileName(relativeDestPath);
                    string sourceFilePath = Path.Combine(sourceDir, sourceFileName);

                    // 确保源文件存在后再复制
                    if (File.Exists(sourceFilePath))
                    {
                        File.Copy(sourceFilePath, destFilePath, true);
                    }
                }

                // 检查安装是否成功
                string[] installedFiles = Directory.GetFiles(sampleDestDir, "*", SearchOption.AllDirectories);
                if (installedFiles.Length == 0)
                {
                    Debug.WriteLine("警告：没有文件被安装到引擎目录");
                    return false;
                }

                Debug.WriteLine($"成功安装内容包到引擎目录，共 {installedFiles.Length} 个文件");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"安装内容包失败：{ex.Message}");
                return false;
            }
        }
    }
}
