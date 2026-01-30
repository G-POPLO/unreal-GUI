using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using unreal_GUI.Model.Basic;
using unreal_GUI.Model.Features;

namespace code_test
{
    [TestClass]
    public class ContentPackageTest
    {
        private readonly string OutputDirectoryPath;

        public ContentPackageTest()
        {
            // 使用Assembly.Location获取当前程序集位置，然后导航到项目目录
            string assemblyLocation = typeof(ContentPackageTest).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation);

            // 从bin/Debug/... 导航到项目根目录
            string projectRoot = Path.GetFullPath(Path.Combine(assemblyDir!, "..", "..", ".."));

            // 构建输出目录的完整路径
            OutputDirectoryPath = Path.Combine(projectRoot, "output");
        }

        [TestInitialize]
        public void TestInit() =>
            // 确保输出目录存在
            Directory.CreateDirectory(OutputDirectoryPath);

        [TestMethod]
        public async Task TestGenerateContentPack()
        {
            Console.WriteLine($"输出目录路径: {OutputDirectoryPath}");

            // 确保输出目录存在
            Directory.CreateDirectory(OutputDirectoryPath);

            // 硬编码参数
            string enginePath = "E:\\UE5\\UE_5.5";
            string contentPackName = "TestContentPack";
            string description = "Test content pack for unit testing";
            string searchTags = "test, content, pack";
            string selectedFolderPath = "D:\\PROGRAM\\unreal-gui\\code_test\\input\\Creations";

            Console.WriteLine($"引擎路径: {enginePath}");
            Console.WriteLine($"内容包名称: {contentPackName}");
            Console.WriteLine($"项目路径: {selectedFolderPath}");

            // 检查项目路径是否存在
            Assert.IsTrue(Directory.Exists(selectedFolderPath), "项目路径不存在");

            // 创建FeatureCore实例
            var featureCore = new FeatureCore();

            // 订阅进度报告事件
            // featureCore.ProgressReported += (sender, e) =>
            // {
            //     Console.WriteLine($"进度: {e.Message}");
            // };

            try
            {
                // 生成内容包（自动安装到引擎目录）
                bool success = await featureCore.GenerateContentPackAsync(
                    enginePath,
                    contentPackName,
                    description,
                    searchTags,
                    selectedFolderPath,
                    autoPlaceCreatedPack: true);

                Assert.IsTrue(success, "生成内容包失败");

                // 复制生成的配置文件到输出目录
                string generatedContentPackDir = FeatureCore.GetGeneratedContentPackDir();
                string contentSettingsDir = Path.Combine(generatedContentPackDir, "ContentSettings");

                // 复制配置文件
                CopyConfigFilesToOutput(contentSettingsDir, contentPackName);

                Console.WriteLine("\n✅ 内容包生成测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Assert.Fail($"测试失败: {ex.Message}");
            }
        }

        private void CopyConfigFilesToOutput(string contentSettingsDir, string contentPackName)
        {
            // 复制配置文件到输出目录
            string configDir = Path.Combine(contentSettingsDir, "Config");
            string configFilePath = Path.Combine(configDir, "config.ini");

            if (File.Exists(configFilePath))
            {
                string outputConfigPath = Path.Combine(OutputDirectoryPath, $"{contentPackName}_config.ini");
                File.Copy(configFilePath, outputConfigPath, true);
                Console.WriteLine($"已复制配置文件到: {outputConfigPath}");
            }

            // 复制manifest.json文件
            string manifestFilePath = Path.Combine(contentSettingsDir, "manifest.json");
            if (File.Exists(manifestFilePath))
            {
                string outputManifestPath = Path.Combine(OutputDirectoryPath, $"{contentPackName}_manifest.json");
                File.Copy(manifestFilePath, outputManifestPath, true);
                Console.WriteLine($"已复制manifest文件到: {outputManifestPath}");
            }

            // 复制ContentToUPack.txt文件
            string generatedContentPackDir = FeatureCore.GetGeneratedContentPackDir();
            string contentToUPackPath = Path.Combine(generatedContentPackDir, "ContentToUPack.txt");
            if (File.Exists(contentToUPackPath))
            {
                string outputContentToUPackPath = Path.Combine(OutputDirectoryPath, $"{contentPackName}_ContentToUPack.txt");
                File.Copy(contentToUPackPath, outputContentToUPackPath, true);
                Console.WriteLine($"已复制ContentToUPack.txt文件到: {outputContentToUPackPath}");
            }
        }
    }
}
