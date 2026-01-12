using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace unreal_GUI.Model.Features
{
    /// <summary>
    /// 进度报告事件参数
    /// </summary>
    public class ProgressReportedEventArgs : EventArgs
    {
        /// <summary>
        /// 进度消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
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
        public event EventHandler<ProgressReportedEventArgs>? ProgressReported;

        /// <summary>
        /// 触发进度报告事件
        /// </summary>
        /// <param name="message">进度消息</param>
        private void OnProgressReported(string message)
        {
            ProgressReported?.Invoke(this, new ProgressReportedEventArgs { Message = message });
        }

        /// <summary>
        /// 生成内容包的主要方法
        /// </summary>
        /// <param name="enginePath">引擎路径</param>
        /// <param name="contentPackName">内容包名称</param>
        /// <param name="description">内容包描述</param>
        /// <param name="searchTags">搜索标签</param>
        /// <param name="selectedFolderPath">选中的文件夹路径</param>
        /// <param name="autoPlaceCreatedPack">是否自动安装到引擎目录</param>
        /// <returns>是否生成成功</returns>
        public async Task<bool> GenerateContentPackAsync(string enginePath, string contentPackName, string description, string searchTags,
                                                      string selectedFolderPath, bool autoPlaceCreatedPack = true)
        {
            string contentPackDir = string.Empty;
            bool result = false;

            try
            {
                OnProgressReported("初始化文件夹结构...");

                // 初始化文件夹结构
                InitBasicFolderStructure(out contentPackDir, out string contentSettingsDir, out string featurePackDir, out string samplesDir);

                // 移除空格的内容包名称
                string contentPackNameNoSpace = contentPackName.Replace(" ", string.Empty);

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
                string contentToUPackPath = await CreateContentToUPackTextFileAsync(contentPackDir, contentSettingsDir);

                OnProgressReported("复制资产到生成的内容包文件夹...");

                // 复制资产到生成的内容包文件夹
                bool shouldContinue = await DuplicateAssetsToGeneratedContentPackFolderAsync(samplesDir, contentPackName, selectedFolderPath);
                if (!shouldContinue)
                {
                    OnProgressReported("生成失败：没有资产被复制");
                    return false;
                }

                OnProgressReported("生成.upack文件...");

                // 生成.upack文件
                string generatedUpackPath = await CreateUPackGenerationBatFileAsync(enginePath, contentPackDir, contentToUPackPath, contentPackNameNoSpace, featurePackDir);

                // 自动安装到引擎目录
                if (autoPlaceCreatedPack)
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
                Console.WriteLine(errorMessage);
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
                        Console.WriteLine($"已清理临时目录: {contentPackDir}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"清理临时目录失败: {ex.Message}");
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
        private static void InitBasicFolderStructure(out string contentPackDir, out string contentSettingsDir,
                                             out string featurePackDir, out string samplesDir)
        {
            // 获取当前项目目录
            string projectDir = Directory.GetCurrentDirectory();

            // 主内容包目录
            contentPackDir = Path.Combine(projectDir, "GeneratedContentPack");

            // 创建主目录（如果不存在）
            if (!Directory.Exists(contentPackDir))
            {
                Directory.CreateDirectory(contentPackDir);
            }

            // 内容设置目录
            contentSettingsDir = Path.Combine(contentPackDir, "ContentSettings");
            if (!Directory.Exists(contentSettingsDir))
            {
                Directory.CreateDirectory(contentSettingsDir);
            }

            // 功能包目录
            featurePackDir = Path.Combine(contentPackDir, "FeaturePacks");
            if (!Directory.Exists(featurePackDir))
            {
                Directory.CreateDirectory(featurePackDir);
            }

            // 样本目录
            samplesDir = Path.Combine(contentPackDir, "Samples");
            if (!Directory.Exists(samplesDir))
            {
                Directory.CreateDirectory(samplesDir);
            }
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
                    Console.WriteLine($"错误：已存在同名内容包文件 {upackFilePath}");
                    return true;
                }

                // 检查样本文件夹是否存在
                string samplesFolderPath = Path.Combine(engineSamplesDir, contentPackNameNoSpace);
                if (Directory.Exists(samplesFolderPath))
                {
                    Console.WriteLine($"错误：已存在同名样本文件夹 {samplesFolderPath}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查同名内容包失败：{ex.Message}");
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
                string contentPackNameNoSpace = contentPackName.Replace(" ", string.Empty);

                // 配置文件内容
                string configContent = $"[AdditionalFilesToAdd]\n+Files=Samples/{contentPackNameNoSpace}/Content/*.*";

                // 写入配置文件
                File.WriteAllText(configFilePath, configContent);
                Console.WriteLine($"成功生成配置文件：{configFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成配置文件失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 生成JSON配置文件
        /// </summary>
        private static void InitJsonFile(string contentPackSettingsDir, string contentPackName,
                                 string description, string searchTags)
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
                    Name = new[]
                    {
                        new { Language = "en", Text = contentPackName }
                    },
                    Description = new[]
                    {
                        new { Language = "en", Text = description }
                    },
                    AssetTypes = Array.Empty<object>(),
                    SearchTags = new[]
                    {
                        new { Language = "en", Text = searchTags }
                    },
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
                Console.WriteLine($"成功生成JSON配置文件：{manifestFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成JSON配置文件失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 创建ContentToUPack.txt文件
        /// </summary>
        private static async Task<string> CreateContentToUPackTextFileAsync(string contentPackDir, string contentSettingsDir)
        {
            try
            {
                // 内容包设置完整路径
                string contentSettingsFullDir = Path.GetFullPath(contentSettingsDir);

                // 准备要打包的内容列表
                List<string> contentToPack = [
                    $"\"{Path.Combine(contentSettingsFullDir, "Config")}\"",
                    $"\"{Path.Combine(contentSettingsFullDir, "Media")}\"",
                    $"\"{Path.Combine(contentSettingsFullDir, "manifest.json")}\""
                ];

                // 创建ContentToUPack.txt文件
                string contentToUPackFilePath = Path.Combine(contentPackDir, "ContentToUPack.txt");
                await File.WriteAllLinesAsync(contentToUPackFilePath, contentToPack);

                Console.WriteLine($"成功创建ContentToUPack.txt文件：{contentToUPackFilePath}");
                return contentToUPackFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建ContentToUPack.txt文件失败：{ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 复制资产到生成的内容包文件夹
        /// </summary>
        private async Task<bool> DuplicateAssetsToGeneratedContentPackFolderAsync(string samplesDir, string contentPackName,
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
                string contentPackNameNoSpace = contentPackName.Replace(" ", string.Empty);

                // 目标目录
                string targetDir = Path.Combine(fullSamplesDir, contentPackNameNoSpace, "Content");

                // 如果目标目录存在，先删除
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                // 创建目标目录
                Directory.CreateDirectory(targetDir);

                // 复制文件夹
                await CopyDirectoryAsync(fullSelectedFolderPath, targetDir);

                // 检查是否有文件被复制
                string[] files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    Console.WriteLine("警告：没有文件被复制到目标目录");
                    return false;
                }

                // 构建资产路径映射，用于后续安装
                foreach (string file in files)
                {
                    // 计算相对路径
                    string relativePath = Path.GetRelativePath(targetDir, file);
                    // 构建目标路径（用于安装到引擎时使用）
                    string engineTargetPath = Path.Combine("/Content", relativePath);
                    // 添加到映射
                    AssetPathsMap[engineTargetPath] = Path.GetDirectoryName(file)!;
                }

                Console.WriteLine($"成功复制 {files.Length} 个文件到 {targetDir}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复制资产失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 异步复制文件夹
        /// </summary>
        /// <param name="sourceDir">源目录</param>
        /// <param name="destinationDir">目标目录</param>
        private static async Task CopyDirectoryAsync(string sourceDir, string destinationDir)
        {
            // 获取源目录下的所有文件和子目录
            string[] files = Directory.GetFiles(sourceDir);
            string[] subDirs = Directory.GetDirectories(sourceDir);

            // 创建目标目录（如果不存在）
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // 复制所有文件
            foreach (string file in files)
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                await CopyFileAsync(file, destFile);
            }

            // 递归复制所有子目录
            foreach (string subDir in subDirs)
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                await CopyDirectoryAsync(subDir, destSubDir);
            }
        }

        /// <summary>
        /// 异步复制文件
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="destinationFile">目标文件</param>
        private static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using FileStream sourceStream = File.Open(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream destinationStream = File.Create(destinationFile);
            await sourceStream.CopyToAsync(destinationStream);
        }

        /// <summary>
        /// 直接调用UnrealPak.exe生成.upack文件
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

                // 直接执行UnrealPak.exe
                using System.Diagnostics.Process process = new();
                process.StartInfo.FileName = unrealPakPath;
                process.StartInfo.Arguments = $"-Create=\"{contentToUPackFullPath}\" \"{generatedUpackPath}\"";
                process.StartInfo.WorkingDirectory = contentPackDir;
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
                    Console.WriteLine($"UnrealPak.exe执行失败：{error}");
                    throw new Exception($"UnrealPak.exe执行失败：{error}");
                }

                Console.WriteLine($"成功生成.upack文件：{generatedUpackPath}");
                Console.WriteLine($"UnrealPak输出：{output}");
                return generatedUpackPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成.upack文件失败：{ex.Message}");
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
                foreach (var assetPathPair in AssetPathsMap)
                {
                    string sourceDir = assetPathPair.Value;
                    string relativeDestPath = assetPathPair.Key;

                    // 构建完整的目标路径
                    string destDir = Path.Combine(sampleDestDir, "Content", relativeDestPath.TrimStart('/'));
                    destDir = Path.GetDirectoryName(destDir)!;

                    // 创建目标目录
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    // 复制文件
                    string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        string destFile = Path.Combine(destDir, Path.GetFileName(file));
                        File.Copy(file, destFile, true);
                    }
                }

                // 检查安装是否成功
                string[] installedFiles = Directory.GetFiles(sampleDestDir, "*", SearchOption.AllDirectories);
                if (installedFiles.Length == 0)
                {
                    Console.WriteLine("警告：没有文件被安装到引擎目录");
                    return false;
                }

                Console.WriteLine($"成功安装内容包到引擎目录，共 {installedFiles.Length} 个文件");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"安装内容包失败：{ex.Message}");
                return false;
            }
        }
    }
}
