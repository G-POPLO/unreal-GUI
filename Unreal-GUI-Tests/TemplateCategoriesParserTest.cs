using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static UnrealGUI.Model.FConfigFile;

namespace UnrealGUI.Tests
{
    [TestClass]
    public class TemplateCategoriesParserTest
    {
        [TestMethod]
        public void TestParseTemplateCategories()
        {
            // 从测试数据文件加载内容
            string testData = File.ReadAllText(Path.Combine("TestData", "SampleTemplateCategories.ini"));

            // 执行解析
            var sections = TemplateCategoriesParser.Parse(testData);
            
            // 验证结果
            Assert.AreEqual(2, sections.Count);
            
            var sampleCategory = sections.FirstOrDefault(s => s.Name == "SampleCategory");
            Assert.IsNotNull(sampleCategory);
            
            var localizedNamesEntry = sampleCategory.Entries.FirstOrDefault(e => e.Key == "LocalizedDisplayNames");
            Assert.IsNotNull(localizedNamesEntry);
            Assert.AreEqual(@"((""ja"", ""サンプル""), (""en"", ""Sample""))", localizedNamesEntry.RawValue);
            
            var iconEntry = sampleCategory.Entries.FirstOrDefault(e => e.Key == "Icon");
            Assert.IsNotNull(iconEntry);
            Assert.AreEqual(@"""\Content\Editor\Icons\sample_icon_40.png""", iconEntry.RawValue);
            
            var isMajorCategoryEntry = sampleCategory.Entries.FirstOrDefault(e => e.Key == "IsMajorCategory");
            Assert.IsNotNull(isMajorCategoryEntry);
            Assert.AreEqual("true", isMajorCategoryEntry.RawValue);
        }
        
        [TestMethod]
        public void TestParseArrayValues()
        {
            string arrayValue = @"((""ja"", ""サンプル""), (""en"", ""Sample""))";
            var items = TemplateCategoriesParser.ParseArray(arrayValue);
            
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(@"(""ja"", ""サンプル"")", items[0]);
            Assert.AreEqual(@"(""en"", ""Sample"")", items[1]);
        }
        
        [TestMethod]
        public void TestSerialize()
        {
            // 创建测试数据
            var sections = new System.Collections.Generic.List<ConfigSection>
            {
                new ConfigSection
                {
                    Name = "TestCategory",
                    Entries = new System.Collections.Generic.List<ConfigEntry>
                    {
                        new ConfigEntry { Key = "Key1", RawValue = "Value1" },
                        new ConfigEntry { Key = "Key2", RawValue = @"((""en"", ""Test""))" }
                    }
                }
            };
            
            // 序列化
            string serialized = TemplateCategoriesParser.Serialize(sections);
            
            // 验证序列化结果包含预期的内容
            Assert.IsTrue(serialized.Contains("[TestCategory]"));
            Assert.IsTrue(serialized.Contains("Key1=Value1"));
            Assert.IsTrue(serialized.Contains(@"Key2=((""en"", ""Test""))"));
        }
    }
}