using System.Text;
using unreal_GUI.Model.Basic;
using unreal_GUI.Model.Features;

namespace code_test
{
    [TestClass]
    public sealed class Parserini
    {
        private readonly string InputDirectoryPath;
        private readonly string OutputDirectoryPath;

        public Parserini()
        {
            // 使用Assembly.Location获取当前程序集位置，然后导航到项目目录
            string assemblyLocation = typeof(Parserini).Assembly.Location;
            string assemblyDir = Path.GetDirectoryName(assemblyLocation);

            // 从bin/Debug/... 导航到项目根目录
            string projectRoot = Path.GetFullPath(Path.Combine(assemblyDir!, "..", "..", ".."));

            // 构建输入和输出目录的完整路径
            InputDirectoryPath = Path.Combine(projectRoot, "input");
            OutputDirectoryPath = Path.Combine(projectRoot, "output");
        }

        [TestInitialize]
        public void TestInit() =>
            // 确保输出目录存在
            Directory.CreateDirectory(OutputDirectoryPath);

        [TestCleanup]
        public void TestCleanup()
        {
            // 测试清理代码
        }

        [TestMethod]
        public void ParserInputIni()
        {
            Console.WriteLine($"输入目录路径: {InputDirectoryPath}");
            Console.WriteLine($"输出目录路径: {OutputDirectoryPath}");

            // 检查输入目录是否存在
            Assert.IsTrue(Directory.Exists(InputDirectoryPath), "输入目录不存在");

            // 获取目录中的所有ini文件
            string[] iniFiles = Directory.GetFiles(InputDirectoryPath, "*.ini", SearchOption.TopDirectoryOnly);
            Assert.IsNotNull(iniFiles, "获取ini文件列表失败");
            Assert.IsNotEmpty(iniFiles, "输入目录中没有找到ini文件");

            Console.WriteLine($"找到 {iniFiles.Length} 个ini文件");

            // 处理input文件夹每个ini文件
            foreach (string iniFilePath in iniFiles)
            {
                ProcessIniFile(iniFilePath);
            }

            // 功能测试


            Console.WriteLine("\n✅ 所有文件处理完成！");
        }

        [TestMethod]
        public void TestReverseConversion()
        {
            Console.WriteLine($"输入目录路径: {InputDirectoryPath}");
            Console.WriteLine($"输出目录路径: {OutputDirectoryPath}");

            // 检查输入目录是否存在
            Assert.IsTrue(Directory.Exists(InputDirectoryPath), "输入目录不存在");

            // 获取目录中的所有ini文件
            string[] iniFiles = Directory.GetFiles(InputDirectoryPath, "*.ini", SearchOption.TopDirectoryOnly);
            Assert.IsNotNull(iniFiles, "获取ini文件列表失败");
            Assert.IsNotEmpty(iniFiles, "输入目录中没有找到ini文件");

            Console.WriteLine($"找到 {iniFiles.Length} 个ini文件");

            // 处理每个ini文件，将其转换为Category对象，再转换回DSL格式
            foreach (string iniFilePath in iniFiles)
            {
                ReverseConvertIniFile(iniFilePath);
            }

            Console.WriteLine("\n✅ 所有文件反向转换完成！");
        }

        //[TestMethod]
        //public async Task TestGenerateContentPack()
        //{
        //    Console.WriteLine($"输出目录路径: {OutputDirectoryPath}");

        //    // 确保输出目录存在
        //    Directory.CreateDirectory(OutputDirectoryPath);

        //    // 硬编码参数
        //    string enginePath = "E:\\UE5\\UE_5.5";
        //    string contentPackName = "TestContentPack";
        //    string description = "Test content pack for unit testing";
        //    string searchTags = "test, content, pack";
        //    string selectedFolderPath = "D:\\PROGRAM\\unreal-gui\\code_test\\input\\Creations";

        //    Console.WriteLine($"引擎路径: {enginePath}");
        //    Console.WriteLine($"内容包名称: {contentPackName}");
        //    Console.WriteLine($"项目路径: {selectedFolderPath}");

        //    // 检查项目路径是否存在
        //    Assert.IsTrue(Directory.Exists(selectedFolderPath), "项目路径不存在");

        //    // 创建FeatureCore实例
        //    var featureCore = new FeatureCore();

        //    // 订阅进度报告事件
        //    featureCore.ProgressReported += (sender, e) =>
        //    {
        //        Console.WriteLine($"进度: {e.Message}");
        //    };

        //    try
        //    {
        //        // 生成内容包（不自动安装到引擎目录）
        //        bool success = await featureCore.GenerateContentPackAsync(
        //            enginePath,
        //            contentPackName,
        //            description,
        //            searchTags,
        //            selectedFolderPath,
        //            autoPlaceCreatedPack: false);

        //        Assert.IsTrue(success, "生成内容包失败");

        //        // 复制生成的配置文件到输出目录
        //        string generatedContentPackDir = FeatureCore.GetGeneratedContentPackDir();
        //        string contentSettingsDir = Path.Combine(generatedContentPackDir, "ContentSettings");

        //        // 复制配置文件
        //        CopyConfigFilesToOutput(contentSettingsDir, contentPackName);

        //        Console.WriteLine("\n✅ 内容包生成测试完成！");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"测试失败: {ex.Message}");
        //        Console.WriteLine(ex.StackTrace);
        //        Assert.Fail($"测试失败: {ex.Message}");
        //    }
        //}

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

        private void ProcessIniFile(string iniFilePath)
        {
            Console.WriteLine($"正在处理文件: {iniFilePath}");

            // 创建CategoriesParser实例
            var parser = new unreal_GUI.Model.Basic.CategoriesParser();

            // 解析Categories
            var categories = CategoriesParser.ParseCategories(iniFilePath);

            // 输出解析结果到文件
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(iniFilePath);
            string outputFileName = $"FConfigFile_parsed_{fileNameWithoutExtension}.txt";
            string outputFilePath = Path.Combine(OutputDirectoryPath, outputFileName);

            using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine($"解析文件: {iniFilePath}");
                writer.WriteLine($"找到 {categories.Count} 个分类");
                writer.WriteLine(new string('=', 50));

                foreach (var category in categories)
                {
                    writer.WriteLine($"Key: {category.Key}");
                    writer.WriteLine($"Icon: {category.Icon}");
                    writer.WriteLine($"IsMajorCategory: {category.IsMajorCategory}");

                    writer.WriteLine("LocalizedDisplayNames:");
                    foreach (var displayName in category.LocalizedDisplayNames)
                    {
                        writer.WriteLine($"  {displayName.Language}: {displayName.Text}");
                    }

                    writer.WriteLine("LocalizedDescriptions:");
                    foreach (var description in category.LocalizedDescriptions)
                    {
                        writer.WriteLine($"  {description.Language}: {description.Text}");
                    }

                    writer.WriteLine(new string('-', 30));
                }
            }

            Console.WriteLine($"解析结果已保存到: {outputFilePath}");

            // 验证解析结果
            Assert.IsNotEmpty(categories, "未解析到任何分类");


        }

        /// <summary>
        /// 将解析后的Category信息转换回DSL文本格式
        /// </summary>
        /// <param name="iniFilePath">原始INI文件路径</param>
        private void ReverseConvertIniFile(string iniFilePath)
        {
            Console.WriteLine($"正在反向转换文件: {iniFilePath}");

            // 创建CategoriesParser实例
            var parser = new CategoriesParser();

            // 解析Categories
            var categories = CategoriesParser.ParseCategories(iniFilePath);

            // 将Category对象转换回DSL格式并保存到新文件
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(iniFilePath);
            string outputFileName = $"FConfigFile_reversed_{fileNameWithoutExtension}.ini";
            string outputFilePath = Path.Combine(OutputDirectoryPath, outputFileName);

            using (var writer = new StreamWriter(outputFilePath, false, Encoding.UTF8))
            {
                // 写入文件头注释
                writer.WriteLine("; This file was automatically generated from parsed Category objects");
                writer.WriteLine("; Original file: " + iniFilePath);
                writer.WriteLine();
                writer.WriteLine("[/Script/GameProjectGeneration.TemplateCategories]");
                writer.WriteLine();

                // 为每个Category生成DSL文本
                foreach (var category in categories)
                {
                    string dslText = ConvertCategoryToDSL(category);
                    writer.WriteLine(dslText);
                }
            }

            Console.WriteLine($"反向转换结果已保存到: {outputFilePath}");

            // 验证生成的文件是否存在
            Assert.IsTrue(File.Exists(outputFilePath), "反向转换文件未生成");
        }

        /// <summary>
        /// 将单个Category对象转换为DSL文本
        /// </summary>
        /// <param name="category">Category对象</param>
        /// <returns>DSL文本</returns>
        private static string ConvertCategoryToDSL(CategoriesParser.Category category)
        {
            var sb = new StringBuilder();

            // 开始Categories条目
            sb.Append("Categories=(");

            // 添加Key
            sb.Append($"Key=\"{category.Key}\"");

            // 添加LocalizedDisplayNames
            sb.Append(", LocalizedDisplayNames=(");
            for (int i = 0; i < category.LocalizedDisplayNames.Count; i++)
            {
                var displayName = category.LocalizedDisplayNames[i];
                sb.Append($"(Language=\"{displayName.Language}\",Text=\"{displayName.Text}\")");
                if (i < category.LocalizedDisplayNames.Count - 1)
                    sb.Append(',');
            }
            sb.Append(')');

            // 添加LocalizedDescriptions
            sb.Append(", LocalizedDescriptions=(");
            for (int i = 0; i < category.LocalizedDescriptions.Count; i++)
            {
                var description = category.LocalizedDescriptions[i];
                sb.Append($"(Language=\"{description.Language}\",Text=\"{description.Text}\")");
                if (i < category.LocalizedDescriptions.Count - 1)
                    sb.Append(',');
            }
            sb.Append(')');

            // 添加Icon
            sb.Append($", Icon=\"{category.Icon}\"");

            // 添加IsMajorCategory
            sb.Append($", IsMajorCategory={category.IsMajorCategory.ToString().ToLower()}");

            // 结束Categories条目
            sb.Append(')');

            return sb.ToString();
        }
    }
}