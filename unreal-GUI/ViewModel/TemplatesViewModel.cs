using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using unreal_GUI.Model;
using unreal_GUI.Model.Basic;
using unreal_GUI.Model.Features;
using Windows.UI.Notifications;


namespace unreal_GUI.ViewModel
{
    public partial class TemplatesViewModel : ObservableObject
    {
        // FeatureCore实例，用于创建功能包
        private readonly FeatureCore _featureCore = new();

        [ObservableProperty]
        private string projectPath;

        [ObservableProperty]
        private string templateName;

        [ObservableProperty]
        private string templateDescriptionEn;

        [ObservableProperty]
        private string templateDescriptionZhHans;

        [ObservableProperty]
        private string templateDescriptionJa;

        [ObservableProperty]
        private string templateDescriptionKo;

        // 动态类别集合
        [ObservableProperty]
        private ObservableCollection<CategoryViewModel> templateCategories = [];

        [ObservableProperty]
        private CategoryViewModel selectedCategory;

        partial void OnSelectedCategoryChanged(CategoryViewModel? oldValue, CategoryViewModel? newValue)
        {
            // 当选中类别变化时，更新IsSelected属性
            if (oldValue != null)
            {
                oldValue.IsSelected = false;
            }
            if (newValue != null)
            {
                newValue.IsSelected = true;
            }
        }



        // 选择类别的命令
        [RelayCommand]
        private void SelectCategory(CategoryViewModel category)
        {
            SelectedCategory = category;
        }

        // 类别视图模型类
        public partial class CategoryViewModel : ObservableObject
        {
            [ObservableProperty]
            public string _key;

            [ObservableProperty]
            public string _displayName;

            [ObservableProperty]
            public string _description;

            [ObservableProperty]
            public bool _isMajorCategory;

            [ObservableProperty]
            private bool _isSelected;
        }

        // 模板图标和预览图
        [ObservableProperty]
        private BitmapImage templateIcon;

        [ObservableProperty]
        private BitmapImage templatePreview;

        // 图片文件路径（用于后续复制操作）
        [ObservableProperty]
        private string templateIconPath;

        [ObservableProperty]
        private string templatePreviewPath;

        [ObservableProperty]
        private string pictureTipText = string.Empty;

        [ObservableProperty]
        private string pictureTipLText = string.Empty;

        // 可用引擎列表
        [ObservableProperty]
        private ObservableCollection<EngineDisplayInfo> availableEngines;

        [ObservableProperty]
        private EngineDisplayInfo selectedEngine;

        partial void OnSelectedEngineChanged(EngineDisplayInfo? oldValue, EngineDisplayInfo? newValue)
        {
            // 当引擎选择变化时，加载对应的类别
            if (newValue != null)
            {
                // 直接调用异步方法并捕获异常
                LoadCategoriesAsync().ConfigureAwait(false);
            }
        }



        // 1. 添加Id属性用于绑定
        [ObservableProperty]
        private string id;

        // 控制是否允许项目创建
        [ObservableProperty]
        private bool isProjectSelected = true;

        [ObservableProperty]
        private bool enableMultiLanguageConfig = false;

        // 注意：EngineInfo类已在JsonConfig.cs中定义，这里使用自定义的显示包装类
        public class EngineDisplayInfo
        {
            public string DisplayName { get; set; }
            public string Path { get; set; }
        }


        // 浏览图标命令
        [RelayCommand]
        private async Task BrowseIconAsync()
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG图片文件 (*.png)|*.png|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择模板图标";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 验证图片尺寸
                    using var img = System.Drawing.Image.FromFile(openFileDialog.FileName);
                    if (img.Width >= 128 && img.Height >= 128)
                    {
                        // 保存图片路径（用于后续复制到Media文件夹）
                        TemplateIconPath = openFileDialog.FileName;

                        // 更新UI显示
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                        bitmap.EndInit();
                        TemplateIcon = bitmap;
                    }
                    else
                    {
                        await ModernDialog.ShowInfoAsync("图标尺寸太小，推荐使用128x128像素或更大的图片。", "提示");
                    }
                }
                catch (Exception ex)
                {
                    await ModernDialog.ShowInfoAsync($"选择图标失败: {ex.Message}", "提示");
                }
            }
        }

        // 浏览预览图命令
        [RelayCommand]
        private async Task BrowsePreviewAsync()
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG图片文件 (*.png)|*.png|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择模板预览图";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 验证图片尺寸
                    using var img = System.Drawing.Image.FromFile(openFileDialog.FileName);
                    if (img.Width >= 400 && img.Height >= 200)
                    {
                        // 保存图片路径（用于后续复制到Media文件夹）
                        TemplatePreviewPath = openFileDialog.FileName;

                        // 更新UI显示
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                        bitmap.EndInit();
                        TemplatePreview = bitmap;

                        // 更新提示文本
                        PictureTipText = "已选择模板预览图。";
                        PictureTipLText = Path.GetFileName(openFileDialog.FileName);
                    }
                    else
                    {
                        await ModernDialog.ShowInfoAsync("预览图尺寸太小，推荐使用400x200像素或更大的图片。", "提示");
                    }
                }
                catch (Exception ex)
                {
                    await ModernDialog.ShowInfoAsync($"选择预览图失败: {ex.Message}", "提示");
                }
            }
        }

        // 创建模板或功能包命令
        [RelayCommand]
        private async Task CreateTemplateAsync()
        {
            // 验证必要信息
            if (string.IsNullOrEmpty(ProjectPath))
            {
                await ModernDialog.ShowInfoAsync("请选择项目文件夹。", "提示");
                return;
            }

            if (string.IsNullOrEmpty(TemplateName))
            {
                await ModernDialog.ShowInfoAsync("请输入模板名称。", "提示");
                return;
            }

            if (string.IsNullOrEmpty(TemplateDescriptionEn))
            {
                await ModernDialog.ShowInfoAsync("请输入英文描述（必填）。", "提示");
                return;
            }

            if (SelectedEngine == null)
            {
                await ModernDialog.ShowInfoAsync("请选择引擎版本。", "提示");
                return;
            }

            try
            {
                // 根据IsProjectSelected的值决定创建模板还是功能包
                if (IsProjectSelected)
                {
                    // 创建模板项目
                    // 创建TemplateDefs.ini文件
                    if (!await CreateTemplateDefsIniAsync())
                    {
                        return;
                    }

                    // 创建Media文件夹并复制图片（项目复制之前）
                    await CreateMediaFolderAndCopyImagesAsync();

                    // 复制项目到引擎Templates目录
                    if (!await CopyProjectToTemplatesAsync())
                    {
                        return;
                    }

                    await ModernDialog.ShowInfoAsync($"模板 '{TemplateName}' 创建成功！", "成功");
                }
                else
                {
                    // 创建功能包
                    // 检查UnrealPak是否可用
                    if (!FeatureCore.IsUnrealPakAvailable(SelectedEngine.Path))
                    {
                        await ModernDialog.ShowErrorAsync("UnrealPak.exe不存在，请检查引擎路径是否正确。", "错误");
                        return;
                    }

                    // 使用英文描述作为功能包描述
                    string description = TemplateDescriptionEn;
                    // 使用模板名称作为搜索标签（暂时这样处理，后续可以根据需要扩展为多个标签）
                    string searchTags = TemplateName;

                    // 订阅资产复制进度事件
                    _featureCore.AssetCopyProgressReported += OnAssetCopyProgressReported;

                    try
                    {
                        // 生成功能包
                        bool success = await _featureCore.GenerateContentPackAsync(
                            SelectedEngine.Path,
                            TemplateName,
                            description,
                            searchTags,
                            ProjectPath,
                            true); // 自动安装到引擎目录

                        if (success)
                        {
                            await ModernDialog.ShowInfoAsync($"功能包 '{TemplateName}' 创建成功！", "成功");
                        }
                        else
                        {
                            await ModernDialog.ShowErrorAsync($"创建功能包失败。", "错误");
                        }
                    }
                    finally
                    {
                        // 取消订阅资产复制进度事件
                        _featureCore.AssetCopyProgressReported -= OnAssetCopyProgressReported;
                    }
                }
            }
            catch (Exception ex)
            {
                string operationName = IsProjectSelected ? "模板" : "功能包";
                await ModernDialog.ShowErrorAsync($"创建{operationName}失败: {ex.Message}", "错误");
            }
        }

        // 创建新类别命令
        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            try
            {
                // 检查是否已选择引擎
                if (SelectedEngine == null)
                {
                    await ModernDialog.ShowInfoAsync("请先选择Unreal Engine版本。", "提示");
                    return;
                }

                // 获取TemplateCategories.ini文件路径
                string templateCategoriesPath = this.GetTemplateCategoriesPath();
                if (string.IsNullOrEmpty(templateCategoriesPath))
                {
                    await ModernDialog.ShowInfoAsync("无法获取模板类别配置文件路径。", "提示");
                    return;
                }

                // 确保模板目录存在
                string templateDir = Path.GetDirectoryName(templateCategoriesPath);
                Directory.CreateDirectory(templateDir);
                // 备份TemplateCategories.ini文件
                await BackupTemplateCategoriesAsync();
                // 显示添加类别对话框，并传入引擎路径
                await ModernDialog.ShowCategoriesDialogAsync(SelectedEngine.Path);

            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"添加类别失败: {ex.Message}", "错误");
            }


        }

        // 重置命令
        [RelayCommand]
        private async Task RecoveryAsync()
        {
            try
            {
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
                var targetPath = GetTemplateCategoriesPath();

                if (string.IsNullOrEmpty(targetPath))
                {
                    await ModernDialog.ShowInfoAsync("无法获取目标配置文件路径。", "提示");
                    return;
                }

                // 首先尝试恢复原始备份
                var originalBackupPath = Path.Combine(backupDir, "TemplateCategories_Backup.ini");

                string selectedBackupPath = null;
                string backupDescription = "";

                if (File.Exists(originalBackupPath))
                {
                    selectedBackupPath = originalBackupPath;
                    backupDescription = "原始备份文件";
                }
                else
                {
                    // 查找最近一次的备份文件
                    var backupFiles = Directory.GetFiles(backupDir, "TemplateCategories_*.ini")
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .ToArray();

                    if (backupFiles.Length > 0)
                    {
                        selectedBackupPath = backupFiles[0];
                        var backupFileInfo = new FileInfo(selectedBackupPath);
                        backupDescription = $"最近备份 ({backupFileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss})";
                    }
                }

                if (!string.IsNullOrEmpty(selectedBackupPath) && File.Exists(selectedBackupPath))
                {
                    File.Copy(selectedBackupPath, targetPath, true);
                    await ModernDialog.ShowInfoAsync($"已从{backupDescription}恢复模板配置文件。", "成功");
                }
                else
                {
                    await ModernDialog.ShowInfoAsync("未找到任何备份文件，无法恢复配置文件。", "提示");
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"恢复配置文件失败: {ex.Message}", "提示");
            }

            if (AvailableEngines.Count > 0)
            {
                SelectedEngine = AvailableEngines[0];
            }
        }

        // 重置表单字段
        [RelayCommand]
        private async Task Reset()
        {
            try
            {
                // 清空所有输入字段
                ProjectPath = string.Empty;
                TemplateName = string.Empty;
                TemplateDescriptionEn = string.Empty;
                TemplateDescriptionZhHans = string.Empty;
                TemplateDescriptionJa = string.Empty;
                TemplateDescriptionKo = string.Empty;
                Id = string.Empty;
                TemplateIconPath = string.Empty;
                TemplatePreviewPath = string.Empty;
                PictureTipText = string.Empty;
                PictureTipLText = string.Empty;

                // 清空图片显示
                TemplateIcon = null;
                TemplatePreview = null;

                // 重置选项
                IsProjectSelected = true;
                EnableMultiLanguageConfig = false;

                // 只有在选择了引擎时才重新加载类别
                if (SelectedEngine != null)
                {
                    await LoadCategoriesAsync();
                }

                await ModernDialog.ShowInfoAsync("表单已重置。", "成功");
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"重置表单失败: {ex.Message}", "错误");
            }
        }

        // 打开备份文件夹命令
        [RelayCommand]
        private static async Task OpenBackupAsync()
        {
            try
            {
                string backupFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");

                // 如果备份文件夹不存在，创建它
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                    await ModernDialog.ShowInfoAsync("备份文件夹不存在，已创建新的备份文件夹。", "信息");
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = backupFolderPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"打开备份文件夹失败: {ex.Message}", "错误");
            }
        }


        // 构造函数
        public TemplatesViewModel()
        {
            AvailableEngines = [];
            TemplateCategories = [];
            _ = InitializeEngineListAsync();
        }

        // 初始化引擎列表 - 使用SettingsViewModel中的数据结构获取引擎信息
        private async Task InitializeEngineListAsync()
        {
            try
            {
                if (File.Exists("settings.json"))
                {
                    string jsonContent = File.ReadAllText("settings.json");
                    // 从jsonContent反序列化设置数据
                    var settings = JsonSerializer.Deserialize<SettingsData>(jsonContent);

                    if (settings != null && settings.Engines != null)
                    {
                        foreach (var engine in settings.Engines.Where(e => e != null))
                        {
                            AvailableEngines.Add(new()
                            {
                                DisplayName = $"Unreal Engine {engine.Version}",
                                Path = engine.Path
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"初始化引擎列表失败: {ex.Message}", "提示");
            }
        }

        [RelayCommand]
        private async Task BrowseProjectAsync()
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Unreal Engine项目文件 (*.uproject)|*.uproject|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择Unreal Engine项目文件";
            openFileDialog.Multiselect = false;
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {

                string projectFilePath = openFileDialog.FileName;
                // 获取项目文件夹路径（.uproject文件所在的目录）
                ProjectPath = Path.GetDirectoryName(projectFilePath);
                string projectName = Path.GetFileNameWithoutExtension(projectFilePath);

                Id = await ReadProjectIdAsync(ProjectPath); // 绑定ID

                // 如果没有手动选择图标，尝试使用默认图标
                if (TemplateIcon == null)
                {
                    await LoadDefaultProjectIconAsync();
                }
            }
            else
            {
                await ModernDialog.ShowInfoAsync("请选择有效的Unreal Engine项目文件（.uproject）", "提示");
            }
        }

        // 获取模板路径
        private string GetTemplateCategoriesPath()
        {
            return SelectedEngine == null ? string.Empty : Path.Combine(SelectedEngine.Path, "Templates", "TemplateCategories.ini");
        }

        // 加载并解析类别
        private async Task LoadCategoriesAsync()
        {
            try
            {
                // 先清除当前类别集合
                TemplateCategories.Clear();
                SelectedCategory = null;

                // 获取类别文件路径
                string categoriesPath = GetTemplateCategoriesPath();

                // 读取文件内容
                if (!File.Exists(categoriesPath))
                {
                    // 如果文件不存在，创建默认类别
                    await Task.Run(() => AddDefaultCategories());
                    return;
                }

                // 异步读取文件内容
                string fileContent = await File.ReadAllTextAsync(categoriesPath);

                // 解析类别
                var categories = await Task.Run(() => CategoriesParser.ParseCategories(categoriesPath));

                // 转换为视图模型
                foreach (var category in categories)
                {
                    // 获取当前语言的显示名称（优先使用简体中文）
                    string displayName = category.LocalizedDisplayNames.FirstOrDefault(ldn =>
                        ldn.Language.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase))?.Text ??
                        category.LocalizedDisplayNames.FirstOrDefault()?.Text ?? category.Key;

                    // 获取当前语言的描述（优先使用简体中文）
                    string description = category.LocalizedDescriptions.FirstOrDefault(ldn =>
                        ldn.Language.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase))?.Text ??
                        category.LocalizedDescriptions.FirstOrDefault()?.Text ?? string.Empty;

                    TemplateCategories.Add(new CategoryViewModel
                    {
                        Key = category.Key,
                        DisplayName = displayName,
                        Description = description,
                        IsMajorCategory = category.IsMajorCategory,
                        IsSelected = false
                    });
                }

                // 默认选择第一个类别
                if (TemplateCategories.Count > 0)
                {
                    SelectedCategory = TemplateCategories[0];
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"加载类别失败: {ex.Message}", "错误");
                // 出错时添加默认类别
                AddDefaultCategories();
            }
        }

        // 添加默认类别
        private void AddDefaultCategories()
        {
            // 添加默认的五个类别
            List<CategoryViewModel> categories = [
                new() {
                    Key = "Games",
                    DisplayName = "游戏 (Games)",
                    Description = "游戏开发相关模板",
                    IsMajorCategory = true,
                    IsSelected = false
                },
                new() {
                    Key = "ME",
                    DisplayName = "电影、电视和现场活动 (ME)",
                    Description = "媒体娱乐相关模板",
                    IsMajorCategory = true,
                    IsSelected = false
                },
                new() {
                    Key = "AEC",
                    DisplayName = "建筑、工程和建造 (AEC)",
                    Description = "建筑设计相关模板",
                    IsMajorCategory = true,
                    IsSelected = false
                },
                new() {
                    Key = "MFG",
                    DisplayName = "汽车、产品设计和生产 (MFG)",
                    Description = "制造设计相关模板",
                    IsMajorCategory = true,
                    IsSelected = false
                },
                new() {
                    Key = "SIM",
                    DisplayName = "模拟、室外地理、AR/VR项目 (SIM)",
                    Description = "模拟和虚拟现实相关模板",
                    IsMajorCategory = true,
                    IsSelected = false
                }
            ];

            foreach (var category in categories)
            {
                TemplateCategories.Add(category);
            }

            // 默认选择第一个类别
            if (TemplateCategories.Count > 0)
            {
                SelectedCategory = TemplateCategories[0];
            }
        }


        // 读取项目ID
        private static async Task<string> ReadProjectIdAsync(string projectPath)
        {
            try
            {
                var configPath = Path.Combine(projectPath, "Config", "DefaultGame.ini");
                if (File.Exists(configPath))
                {
                    foreach (var line in await File.ReadAllLinesAsync(configPath))
                    {
                        if (line.Trim().StartsWith("ProjectID=", StringComparison.OrdinalIgnoreCase))
                        {
                            return line.Split('=', 2)[1].Trim('"').Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"读取项目ID失败: {ex.Message}", "提示");
            }
            return string.Empty;
        }

        // 加载默认项目图标
        private Task LoadDefaultProjectIconAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectPath))
                    return Task.CompletedTask;

                // 检查Saved\AutoScreenshot.png是否存在
                string defaultIconPath = Path.Combine(ProjectPath, "Saved", "AutoScreenshot.png");

                if (File.Exists(defaultIconPath))
                {
                    // 验证图片尺寸
                    using var img = System.Drawing.Image.FromFile(defaultIconPath);
                    if (img.Width >= 64 && img.Height >= 64) // 最小尺寸要求
                    {
                        TemplateIconPath = defaultIconPath;

                        // 更新UI显示
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(defaultIconPath, UriKind.Absolute);
                        bitmap.EndInit();
                        TemplateIcon = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                // 静默失败，不影响用户操作
                Debug.WriteLine($"加载默认图标失败: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        // 备份TemplateCategories.ini文件
        private async Task<bool> BackupTemplateCategoriesAsync()
        {
            try
            {
                var sourcePath = GetTemplateCategoriesPath();
                if (!File.Exists(sourcePath))
                {
                    await ModernDialog.ShowInfoAsync("找不到TemplateCategories.ini文件。", "提示");
                    return false;
                }

                // 创建Backup文件夹
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
                Directory.CreateDirectory(backupDir);

                // 备份文件，添加时间戳
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(backupDir, $"TemplateCategories_{timestamp}.ini");
                File.Copy(sourcePath, backupPath, true);

                //同时创建一个没有时间戳的备份，用于重置功能
                var resetBackupPath = Path.Combine(backupDir, "TemplateCategories_Backup.ini");
                File.Copy(sourcePath, resetBackupPath, true);

                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"备份文件失败: {ex.Message}", "提示");
                return false;
            }
        }

        // 写入TemplateDefs.ini文件
        private async Task<bool> CreateTemplateDefsIniAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectPath) || string.IsNullOrEmpty(TemplateName) || string.IsNullOrEmpty(TemplateDescriptionEn))
                {
                    return false;
                }

                // 获取选中的类别
                string category = SelectedCategory?.Key ?? "Games"; // 默认类别

                // 写入TemplateDefs.ini的内容
                string iniContent = $"[/Script/GameProjectGeneration.TemplateProjectDefs]\n\n" +
                               $"Categories={category}\n" +
                               $"ProjectID={Id}\n" +
                               $"bAllowProjectCreation={(IsProjectSelected ? "true" : "false")}\n" +
                               $"LocalizedDisplayNames=(Language=\"en\", Text=\"{TemplateName}\")\n" +
                               $"LocalizedDescriptions=(Language=\"en\", Text=\"{TemplateDescriptionEn}\")\n";

                // 添加其他语言描述（如果提供）
                if (!string.IsNullOrEmpty(TemplateDescriptionZhHans))
                {
                    iniContent += $"LocalizedDisplayNames=(Language=\"zh-Hans\", Text=\"{TemplateName}\")\n" +
                                $"LocalizedDescriptions=(Language=\"zh-Hans\", Text=\"{TemplateDescriptionZhHans}\")\n";
                }

                if (!string.IsNullOrEmpty(TemplateDescriptionJa))
                {
                    iniContent += $"LocalizedDisplayNames=(Language=\"ja\", Text=\"{TemplateName}\")\n" +
                                $"LocalizedDescriptions=(Language=\"ja\", Text=\"{TemplateDescriptionJa}\")\n";
                }

                if (!string.IsNullOrEmpty(TemplateDescriptionKo))
                {
                    iniContent += $"LocalizedDisplayNames=(Language=\"ko\", Text=\"{TemplateName}\")\n" +
                                $"LocalizedDescriptions=(Language=\"ko\", Text=\"{TemplateDescriptionKo}\")\n";
                }

                // 添加忽略的文件夹和文件
                iniContent += $"\nFoldersToIgnore=Media\n" +
                            $"FilesToIgnore=\"Config/TemplateDefs.ini\"\n" +
                            $"\n; Ignore template-specific files\n" +
                            $"FilesToIgnore=\"%TEMPLATENAME%.uproject\"\n" +
                            $"FilesToIgnore=\"%TEMPLATENAME%.png\"\n" +
                            $"FilesToIgnore=\"Config/TemplateDefs.ini\"\n" +
                            $"FilesToIgnore=\"Manifest.json\"\n" +
                            $"\n; Rename the source code directory\n" +
                            $"FolderRenames=(From=\"Source/%TEMPLATENAME%\", To=\"Source/%PROJECTNAME%\")\n" +
                            $"FolderRenames=(From=\"Source/%TEMPLATENAME%Editor\", To=\"Source/%PROJECTNAME%Editor\")\n" +
                            $"\n; Filename replacement rules (Case sensitivity of string matching when bCaseSensitive=true)\n" +
                            $"FilenameReplacements=(Extensions=(\"cpp\",\"h\",\"ini\",\"cs\"), From=\"%TEMPLATENAME_UPPERCASE%\", To=\"%PROJECTNAME_UPPERCASE%\", bCaseSensitive=true)\n" +
                            $"FilenameReplacements=(Extensions=(\"cpp\",\"h\",\"ini\",\"cs\"), From=\"%TEMPLATENAME_LOWERCASE%\", To=\"%PROJECTNAME_LOWERCASE%\", bCaseSensitive=true)\n" +
                            $"FilenameReplacements=(Extensions=(\"cpp\",\"h\",\"ini\",\"cs\"), From=\"%TEMPLATENAME%\", To=\"%PROJECTNAME%\", bCaseSensitive=false)\n" +
                            $"\n; File content replacement \n" +
                            $"ReplacementsInFiles=(Extensions=(\"cpp\",\"h\",\"ini\",\"cs\",\"uplugin\"), From=\"%TEMPLATENAME%\", To=\"%PROJECTNAME%\", bCaseSensitive=false)\n" +
                            $"ReplacementsInFiles=(Extensions=(\"cpp\",\"h\",\"ini\",\"cs\",\"uplugin\"), From=\"%TEMPLATENAME_UPPERCASE%\", To=\"%PROJECTNAME_UPPERCASE%\", bCaseSensitive=true)\n" +
                            $"ReplacementsInFiles=(Extensions=(\"cpp\",\"h\",\"ini\",\"cs\",\"uplugin\"), From=\"%TEMPLATENAME_LOWERCASE%\", To=\"%PROJECTNAME_LOWERCASE%\", bCaseSensitive=true)\n";

                // 创建Config目录并写入文件
                string configDir = Path.Combine(ProjectPath, "Config");
                Directory.CreateDirectory(configDir);

                string templateDefsPath = Path.Combine(configDir, "TemplateDefs.ini");
                await File.WriteAllTextAsync(templateDefsPath, iniContent);

                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"写入TemplateDefs.ini失败: {ex.Message}", "提示");
                return false;
            }
        }

        // 复制项目到引擎Templates目录
        private async Task<bool> CopyProjectToTemplatesAsync()
        {
            try
            {
                var targetDir = Path.Combine(SelectedEngine.Path, "Templates", TemplateName);

                //如果目标目录已存在，删除它
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                // 获取要复制的文件总数用于显示进度
                var allFiles = Directory.GetFiles(ProjectPath, "*", SearchOption.AllDirectories);
                var totalFiles = allFiles.Length;

                // 如果文件较多（超过50个），显示进度通知
                if (totalFiles > 50)
                {
                    await CopyProjectWithProgressAsync(ProjectPath, targetDir, totalFiles);
                }
                else
                {
                    // 文件较少时直接复制
                    await CopyDirectoryAsync(ProjectPath, targetDir);
                }

                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"复制项目到Templates目录失败: {ex.Message}", "提示");
                return false;
            }
        }


        // 带进度的目录复制方法
#pragma warning disable CA1822 // 将成员标记为 static
        private async Task CopyProjectWithProgressAsync(string sourceDir, string destinationDir, int totalFiles)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            // Toast 通知初始化（带进度条）
            string toastTag = "copy-progress";
            string toastGroup = "copy-group";
            var toastContent = new ToastContentBuilder()
                .AddText($"正在复制项目文件")
                .AddText($"文件总数: {totalFiles}")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = "复制进度",
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressText"),
                    Status = new BindableString("currentFile")
                })
                .GetToastContent();

            // 创建通知并设置初始数据
            var toast = new ToastNotification(toastContent.GetXml())
            {
                Tag = toastTag,
                Group = toastGroup,
                Data = new NotificationData()
                {
                    Values =
                    {
                        ["progressValue"] = "0.00",
                        ["progressText"] = "0%",
                        ["currentFile"] = "准备中..."
                    },
                    SequenceNumber = 1
                }
            };

            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);

            // 在后台线程执行复制操作，避免阻塞UI线程
            await Task.Run(async () =>
            {
                var progressState = new ProgressState
                {
                    FilesCopied = 0,
                    SequenceNumber = 1,
                    TotalFiles = totalFiles,
                    ToastTag = toastTag,
                    ToastGroup = toastGroup
                };

                // 递归复制目录
                await CopyDirectoryAsync(sourceDir, destinationDir, progressState);

                // 复制完成后更新通知状态
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var finalData = new NotificationData
                    {
                        SequenceNumber = progressState.SequenceNumber++
                    };
                    finalData.Values["progressValue"] = "1.00";
                    finalData.Values["progressText"] = "100%";
                    finalData.Values["currentFile"] = "复制完成！";
                    ToastNotificationManagerCompat.CreateToastNotifier().Update(finalData, toastTag, toastGroup);
                });

                // 稍后关闭通知
                await Task.Delay(2000);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ToastNotificationManagerCompat.History.Remove(toastTag, toastGroup);
                });
            });
        }

        // 进度状态类
        private class ProgressState
        {
            public int FilesCopied { get; set; }
            public uint SequenceNumber { get; set; }
            public int TotalFiles { get; set; }
            public string ToastTag { get; set; } = string.Empty;
            public string ToastGroup { get; set; } = string.Empty;
        }

        // 异步目录复制方法（支持进度报告）
        private static async Task CopyDirectoryAsync(string sourceDir, string destinationDir, ProgressState? progressState = null)
        {
            if (string.IsNullOrEmpty(sourceDir) || string.IsNullOrEmpty(destinationDir))
            {
                throw new ArgumentException("源目录和目标目录路径不能为空");
            }

            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException($"源目录不存在: {sourceDir}");
            }

            // 创建目标目录
            Directory.CreateDirectory(destinationDir);

            // 复制文件
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destinationDir, fileName);

                if (progressState != null)
                {
                    // 带进度的异步复制
                    using var sourceStream = File.OpenRead(file);
                    using var destStream = File.Create(destFile);
                    var buffer = new byte[81920];
                    int read;
                    while ((read = await sourceStream.ReadAsync(buffer)) > 0)
                    {
                        await destStream.WriteAsync(buffer.AsMemory(0, read));
                    }

                    progressState.FilesCopied++;

                    // 更新进度（每复制10个文件或文件总数少于100时每个都更新）
                    if (progressState.FilesCopied % 10 == 0 || progressState.TotalFiles < 100)
                    {
                        double progress = progressState.FilesCopied / (double)progressState.TotalFiles;
                        int percent = (int)(progress * 100);

                        var data = new NotificationData
                        {
                            SequenceNumber = progressState.SequenceNumber++
                        };
                        data.Values["progressValue"] = progress.ToString("F2");
                        data.Values["progressText"] = $"{percent}%";
                        data.Values["currentFile"] = $"正在复制: {fileName}";

                        // 在UI线程上更新Toast通知
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ToastNotificationManagerCompat.CreateToastNotifier().Update(data, progressState.ToastTag, progressState.ToastGroup);
                        });
                    }
                }
                else
                {
                    // 简单的异步复制
                    await Task.Run(() => File.Copy(file, destFile, true));
                }
            }

            // 递归复制子目录
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(destinationDir, dirName);
                await CopyDirectoryAsync(dir, destDir, progressState);
            }
        }

        // 同步目录复制方法（内部调用异步版本）
        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            CopyDirectoryAsync(sourceDir, destinationDir).Wait();
        }

        [RelayCommand]
        private async Task OpenTemplateAsync()
        {
            try
            {
                if (SelectedEngine == null)
                {
                    await ModernDialog.ShowInfoAsync("请先选择引擎版本。", "提示");
                    return;
                }

                string templateFolderPath = Path.Combine(SelectedEngine.Path, "Templates");

                // 如果模板文件夹不存在，显示提示
                if (!Directory.Exists(templateFolderPath))
                {
                    await ModernDialog.ShowInfoAsync("模板文件夹不存在。", "提示");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = templateFolderPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"打开模板文件夹失败: {ex.Message}", "错误");
            }
        }

        // 复制图片到项目的Media文件夹
        private async Task CreateMediaFolderAndCopyImagesAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectPath) || string.IsNullOrEmpty(TemplateName))
                {
                    return;
                }

                // 创建项目的Media目录
                var projectMediaDir = Path.Combine(ProjectPath, "Media");
                Directory.CreateDirectory(projectMediaDir);

                // 检查是否有手动选择的图标
                bool iconCopied = false;
                if (!string.IsNullOrEmpty(TemplateIconPath) && File.Exists(TemplateIconPath))
                {
                    var iconDestPath = Path.Combine(projectMediaDir, $"{TemplateName}.png");
                    File.Copy(TemplateIconPath, iconDestPath, true);
                    iconCopied = true;
                }

                // 如果没有手动选择图标，尝试使用Saved目录下的AutoScreenshot.png作为默认图标
                if (!iconCopied)
                {
                    var savedScreenshotPath = Path.Combine(ProjectPath, "Saved", "AutoScreenshot.png");
                    if (File.Exists(savedScreenshotPath))
                    {
                        var iconDestPath = Path.Combine(projectMediaDir, $"{TemplateName}.png");
                        File.Copy(savedScreenshotPath, iconDestPath, true);
                        //await ModernDialog.ShowInfoAsync($"已使用项目Saved目录中的AutoScreenshot.png作为默认图标", "信息");
                    }
                }

                // 复制预览图（如果有）
                if (!string.IsNullOrEmpty(TemplatePreviewPath) && File.Exists(TemplatePreviewPath))
                {
                    var previewDestPath = Path.Combine(projectMediaDir, $"{TemplateName}_Preview.png");
                    File.Copy(TemplatePreviewPath, previewDestPath, true);
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"创建Media文件夹并复制图片失败: {ex.Message}", "错误");
            }
        }

        // 处理资产复制进度事件
        private void OnAssetCopyProgressReported(object sender, AssetCopyProgressEventArgs e)
        {
            // Toast 通知初始化（带进度条）
            string toastTag = "asset-copy-progress";
            string toastGroup = "asset-copy-group";

            // 仅在主线程上执行UI更新
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // 如果是第一次显示进度，则显示初始通知
                if (e.FilesCopied == 1)
                {
                    var toastContent = new ToastContentBuilder()
                        .AddText($"正在复制内容包资产")
                        .AddText($"文件总数: {e.TotalFiles}")
                        .AddVisualChild(new AdaptiveProgressBar()
                        {
                            Title = "资产复制进度",
                            Value = new BindableProgressBarValue("progressValue"),
                            ValueStringOverride = new BindableString("progressText"),
                            Status = new BindableString("currentFile")
                        })
                        .GetToastContent();

                    // 创建通知并设置初始数据
                    var toast = new ToastNotification(toastContent.GetXml())
                    {
                        Tag = toastTag,
                        Group = toastGroup,
                        Data = new NotificationData()
                        {
                            Values =
                            {
                                ["progressValue"] = "0.00",
                                ["progressText"] = "0%",
                                ["currentFile"] = "准备中..."
                            },
                            SequenceNumber = 1
                        }
                    };

                    ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
                }

                // 更新进度
                double progress = e.FilesCopied / (double)e.TotalFiles;
                int percent = (int)(progress * 100);

                var data = new NotificationData
                {
                    SequenceNumber = (uint)e.FilesCopied  // 使用文件计数作为序列号
                };
                data.Values["progressValue"] = progress.ToString("F2");
                data.Values["progressText"] = $"{percent}%";
                data.Values["currentFile"] = $"正在复制: {e.CurrentFile}";

                ToastNotificationManagerCompat.CreateToastNotifier().Update(data, toastTag, toastGroup);

                // 如果复制完成，延迟移除通知
                if (e.FilesCopied == e.TotalFiles)
                {
                    // 延迟2秒后移除通知
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ToastNotificationManagerCompat.History.Remove(toastTag, toastGroup);
                        });
                    });
                }
            });
        }
    }
}