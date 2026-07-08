# XAML 本地化扫描工具使用说明

## 功能概述

这个工具可以自动扫描 XAML 文件中的硬编码文本，提取并导出为多种格式的资源文件，为多语言翻译做准备。

## 主要功能

1. **自动扫描** - 扫描指定目录下的所有 XAML 文件
2. **智能识别** - 识别包含中文、日文、韩文等 CJK 字符的硬编码文本
3. **多种导出格式** - 支持 .resx、JSON、CSV 和翻译模板格式
4. **源码追踪** - 记录每个文本的源文件位置和元素信息

## 使用方法

### 基本扫描

```bash
# 扫描指定目录
dotnet run --project XamlLocalizationScanner -- scan <目录路径>

# 示例：扫描 unreal-GUI 项目
dotnet run --project XamlLocalizationScanner -- scan d:\PROGRAM\unreal-gui\unreal-GUI
```

### 自定义输出

```bash
# 指定输出目录
dotnet run --project XamlLocalizationScanner -- scan <目录路径> -o <输出目录>

# 指定输出格式（可多次指定）
dotnet run --project XamlLocalizationScanner -- scan <目录路径> -f resx -f json -f csv -f template

# 包含非 CJK 文本（如英文）
dotnet run --project XamlLocalizationScanner -- scan <目录路径> --include-non-cjk

# 指定源语言代码
dotnet run --project XamlLocalizationScanner -- scan <目录路径> --language zh-CN
```

### 输出格式说明

#### 1. .resx 格式（标准 .NET 资源文件）

文件名：`Strings.<语言代码>.resx`

这是标准的 .NET 资源文件格式，可以直接在 Visual Studio 中使用。

**使用方法：**
1. 将生成的 .resx 文件添加到项目中
2. 创建其他语言的资源文件（如 `Strings.en-US.resx`）
3. 在 XAML 中使用资源引用：`Text="{x:Static properties:Strings.KeyName}"`

#### 2. JSON 格式

文件名：`strings.<语言代码>.json`

适合现代前端框架和翻译工具使用。

#### 3. CSV 格式

文件名：`strings.<语言代码>.csv`

可以在 Excel 中打开编辑，方便批量处理。

#### 4. 翻译模板

文件名：`translation_template.csv`

包含源语言和目标语言列，方便翻译人员填写。

**CSV 格式示例：**
```
Key,Source (zh-CN),Target (en-US),Comment
"ui_Content_1","插件编译","","Plugin Compilation"
"ui_Content_2","项目重命名","","Project Rename"
```

## 扫描规则

### 扫描的属性

工具会扫描以下 XAML 属性中的文本：
- `Text` - TextBlock、Run 等元素的文本
- `Content` - Button、NavigationViewItem 等元素的内容
- `Header` - 标题文本
- `Title` - 窗口标题
- `ToolTip` - 工具提示文本
- `Label` - AppBarToggleButton 等元素的标签
- `PlaceholderText` - 占位符文本
- `Description` - 描述文本

### 跳过的内容

以下内容会被自动跳过：
- `{Binding ...}` - 数据绑定表达式
- `{x:Static ...}` - 静态资源引用
- `{DynamicResource ...}` - 动态资源引用
- `{StaticResource ...}` - 静态资源引用
- 纯数字文本
- 空白文本

### 默认行为

默认情况下，工具只提取包含 CJK（中日韩）字符的文本。如果需要提取所有文本（包括英文），请使用 `--include-non-cjk` 参数。

## 后续步骤

### 1. 翻译资源文件

使用生成的翻译模板文件进行翻译：
1. 打开 `translation_template.csv`
2. 在 "Target (en-US)" 列填写翻译内容
3. 保存文件

### 2. 创建多语言资源文件

为每种语言创建对应的资源文件：
- `Strings.zh-CN.resx` - 中文（默认）
- `Strings.en-US.resx` - 英文
- `Strings.ja-JP.resx` - 日文
- 其他语言...

### 3. 集成到项目

在 WPF 项目中集成资源文件：

**步骤：**
1. 在项目中创建 `Properties` 文件夹
2. 将 .resx 文件移动到该文件夹
3. Visual Studio 会自动生成资源类
4. 在 XAML 中引用资源

**XAML 引用示例：**
```xml
<!-- 添加命名空间 -->
xmlns:properties="clr-namespace:unreal_GUI.Properties"

<!-- 使用资源 -->
<TextBlock Text="{x:Static properties:Strings.ui_Content_1}"/>
```

## 示例输出

扫描 unreal-GUI 项目后生成的统计信息：

```
=== 扫描统计 ===
  MainWindow.xaml: 9 个条目
  View/About.xaml: 12 个条目
  View/Clear.xaml: 12 个条目
  View/Compile.xaml: 8 个条目
  View/DialogContent/Add_Categories.xaml: 12 个条目
  View/Settings.xaml: 22 个条目
  View/Templates.xaml: 31 个条目
  ...

总计：154 个本地化条目
```

## 工具架构

```
XamlLocalizationScanner/
├── LocalizationEntry.cs    - 本地化条目模型
├── XamlScanner.cs          - XAML 文件扫描器
├── ResourceExporter.cs     - 资源文件导出器
├── Program.cs              - 主程序入口
└── README.md               - 使用说明
```

## 注意事项

1. **备份原文件** - 在修改 XAML 文件之前，建议备份原文件
2. **验证翻译** - 翻译完成后，建议验证翻译质量
3. **测试多语言** - 在发布前测试各语言版本的显示效果
4. **资源键命名** - 工具自动生成资源键，建议根据实际需要调整命名

## 常见问题

**Q: 为什么某些文本没有被扫描到？**
A: 可能是因为文本使用了数据绑定或资源引用，或者文本不包含 CJK 字符。使用 `--include-non-cjk` 参数可以扫描所有文本。

**Q: 如何修改资源键名称？**
A: 可以在生成的 CSV 文件中手动修改键名，然后重新导出为其他格式。

**Q: 如何处理重复的文本？**
A: 工具会自动合并相同的文本值，在注释中记录所有出现的位置。

## 技术支持

如有问题或建议，请提交 Issue 或联系开发团队。