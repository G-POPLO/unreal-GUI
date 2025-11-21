using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Media;
using unreal_GUI.Model.DialogContent;
using unreal_GUI.Model;
using System.Threading.Tasks;

namespace unreal_GUI.ViewModel
{
    public partial class TemplatesViewModel : ObservableObject
    {
        // 项目路径
        [ObservableProperty]
        private string projectPath = string.Empty;

        // 模板名称
        [ObservableProperty]
        private string templateName = string.Empty;

        // 模板描述（多语言）
        [ObservableProperty]
        private string templateDescriptionEn = string.Empty;

        [ObservableProperty]
        private string templateDescriptionZhHans = string.Empty;

        [ObservableProperty]
        private string templateDescriptionJa = string.Empty;

        [ObservableProperty]
        private string templateDescriptionKo = string.Empty;

        // 模板类别
        [ObservableProperty]
        private bool isCategoryGames = true;

        [ObservableProperty]
        private bool isCategoryME = false;

        [ObservableProperty]
        private bool isCategoryAEC = false;

        [ObservableProperty]
        private bool isCategoryMFG = false;

        // 模板图标和预览图
        [ObservableProperty]
        private BitmapImage templateIcon = null;

        [ObservableProperty]
        private BitmapImage templatePreview = null;
        
        // 图片文件路径（用于后续复制操作）
        [ObservableProperty]
        private string templateIconPath = string.Empty;
        
        [ObservableProperty]
        private string templatePreviewPath = string.Empty;

        // 可用引擎列表
        [ObservableProperty]
        private ObservableCollection<EngineInfo> availableEngines = [];

        [ObservableProperty]
        private EngineInfo selectedEngine = null;

        // 状态信息
        [ObservableProperty]
        private string statusMessage = "请填写必要信息并选择相应选项来创建自定义模板。";

        // 构造函数
        public TemplatesViewModel()
        {
            // 初始化引擎列表
            _ = InitializeEngineListAsync();
        }

        // 初始化引擎列表
        private async Task InitializeEngineListAsync()
        {
            try
            {
                // 从settings.json文件读取引擎列表
                if (File.Exists("settings.json"))
                {
                    var json = File.ReadAllText("settings.json");
                    dynamic settings = JsonConvert.DeserializeObject(json);
                    if (settings?.Engines != null)
                    {
                        foreach (var engine in settings.Engines)
                        {
                            string path = engine.path?.ToString();
                            string version = engine.version?.ToString() ?? "未知版本";
                            AvailableEngines.Add(new EngineInfo 
                            {
                                DisplayName = $"{Path.GetFileName(path)} ({version})",
                                Path = path
                            });
                        }
                    }
                }
                
                // 如果没有找到引擎信息，则尝试从注册表获取
                if (AvailableEngines.Count == 0)
                {
                    return;
                }
                 
                
                // 默认选择第一个引擎
                if (AvailableEngines.Count > 0)
                {
                    SelectedEngine = AvailableEngines[0];
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"初始化引擎列表失败: {ex.Message}", "错误");
            }
        }
        
        // 从注册表获取引擎信息
        //private void TryGetEnginesFromRegistry()
        //{
        //    try
        //    {
        //        using var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\EpicGames\\Unreal Engine");
        //        if (key != null)
        //        {
        //            foreach (var subKeyName in key.GetSubKeyNames())
        //            {
        //                using var subKey = key.OpenSubKey(subKeyName);
        //                var path = subKey?.GetValue("InstalledDirectory") as string;
        //                if (!string.IsNullOrEmpty(path))
        //                {
        //                    AvailableEngines.Add(new EngineInfo { DisplayName = $"Unreal Engine {subKeyName}", Path = path });
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        // 忽略注册表读取错误
        //    }
        //}

        // 浏览项目文件夹命令
        [RelayCommand]
        private async Task BrowseProjectAsync()
        {
            using var folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "选择Unreal Engine项目文件夹";
            folderBrowser.ShowNewFolderButton = false;
            
            // 设置初始目录为常见的项目位置
            folderBrowser.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            DialogResult result = folderBrowser.ShowDialog();
            
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                // 验证是否为有效的UE项目文件夹（检查是否存在.uproject文件）
                string[] uprojectFiles = Directory.GetFiles(folderBrowser.SelectedPath, "*.uproject");
                if (uprojectFiles.Length > 0)
                {
                    ProjectPath = folderBrowser.SelectedPath;
                    await ModernDialog.ShowInfoAsync($"已选择项目: {Path.GetFileName(folderBrowser.SelectedPath)}", "信息");
                }
                else
                {
                    await ModernDialog.ShowInfoAsync("所选文件夹不是有效的Unreal Engine项目（未找到.uproject文件）", "错误");
                }
            }
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
                    System.Drawing.Image img = System.Drawing.Image.FromFile(openFileDialog.FileName);
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

                        await ModernDialog.ShowInfoAsync("已选择模板图标。", "信息");
                    }
                    else
                    {
                        await ModernDialog.ShowInfoAsync("图标尺寸太小，推荐使用128x128像素或更大的图片。", "提示");
                    }
                }
                catch (Exception ex)
                {
                    await ModernDialog.ShowInfoAsync($"选择图标失败: {ex.Message}", "错误");
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
                    System.Drawing.Image img = System.Drawing.Image.FromFile(openFileDialog.FileName);
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

                        await ModernDialog.ShowInfoAsync("已选择模板预览图。", "信息");
                    }
                    else
                    {
                        await ModernDialog.ShowInfoAsync("预览图尺寸太小，推荐使用400x200像素或更大的图片。", "提示");
                    }
                }
                catch (Exception ex)
                {
                    await ModernDialog.ShowInfoAsync($"选择预览图失败: {ex.Message}", "错误");
                }
            }
        }

        // 创建模板命令
        [RelayCommand]
        private async void CreateTemplateAsync()
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
                // 步骤1: 备份TemplateCategories.ini文件
                if (!BackupTemplateCategoriesAsync())
                {
                    return;
                }
                
                // 步骤2: 创建TemplateDefs.ini文件
                if (!CreateTemplateDefsIniAsync())
                {
                    return;
                }
                
                // 步骤3: 复制项目到引擎Templates目录
                if (!CopyProjectToTemplatesAsync())
                {
                    return;
                }
                
                // 步骤4: 添加图标和预览图
                CopyImagesToMediaFolder();
                
                await ModernDialog.ShowInfoAsync($"模板 '{TemplateName}' 创建成功！", "成功");
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"创建模板失败: {ex.Message}", "错误");
            }
        }
        
        // 创建新类别命令
        [RelayCommand]
        private  async void AddCategoryAsync()
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
                string templateCategoriesPath = GetTemplateCategoriesPath();
                if (string.IsNullOrEmpty(templateCategoriesPath))
                {
                    await ModernDialog.ShowInfoAsync("无法获取模板类别配置文件路径。", "错误");
                    return;
                }
                
                // 确保模板目录存在
                string templateDir = Path.GetDirectoryName(templateCategoriesPath);
                Directory.CreateDirectory(templateDir);
                
                // 备份TemplateCategories.ini文件
                if (!BackupTemplateCategoriesAsync())
                {
                    return;
                }
                
                // 优化与ModernDialog的交互：创建一个对话框并直接与Add_Categories内容交互
                var addCategoriesContent = new Add_Categories();
                
                var dialog = new ContentDialog
                {
                    Title = "添加模板类别",
                    PrimaryButtonText = "保存",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = addCategoriesContent
                };
                
                // 显示对话框并处理结果
                ContentDialogResult result = await dialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    // 直接使用已经引用的addCategoriesContent实例保存类别
                        if (addCategoriesContent.SaveCategory(templateCategoriesPath))
                        {
                            await ModernDialog.ShowInfoAsync("新类别创建成功！", "成功");
                        }
                        else
                        {
                            await ModernDialog.ShowInfoAsync("创建类别失败，请检查输入信息。", "错误");
                        }
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"添加类别失败: {ex.Message}", "错误");
            }
        }
        
        // 重置命令
        [RelayCommand]
        private async Task ResetAsync()
        {
            try
            {
                // 恢复原始的TemplateCategories.ini文件
                var backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup", "TemplateCategories_Backup.ini");
                var targetPath = GetTemplateCategoriesPath();
                
                if (File.Exists(backupPath) && !string.IsNullOrEmpty(targetPath))
                {
                    File.Copy(backupPath, targetPath, true);
                    await ModernDialog.ShowInfoAsync("已重置模板配置文件。", "成功");
                }
                else
                {
                    await ModernDialog.ShowInfoAsync("未找到备份文件，无法重置。", "错误");
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"重置失败: {ex.Message}", "错误");
            }
            
            // 重置表单
            ProjectPath = string.Empty;
            TemplateName = string.Empty;
            TemplateDescriptionEn = string.Empty;
            TemplateDescriptionZhHans = string.Empty;
            TemplateDescriptionJa = string.Empty;
            TemplateDescriptionKo = string.Empty;
            IsCategoryGames = true;
            IsCategoryME = false;
            IsCategoryAEC = false;
            IsCategoryMFG = false;
            TemplateIcon = null;
            TemplatePreview = null;
            TemplateIconPath = string.Empty;
            TemplatePreviewPath = string.Empty;
            
            if (AvailableEngines.Count > 0)
            {
                SelectedEngine = AvailableEngines[0];
            }
        }
        
        // 获取模板路径
        private string GetTemplateCategoriesPath()
        {
            if (SelectedEngine == null) return string.Empty;
            return Path.Combine(SelectedEngine.Path, "Templates", "TemplateCategories.ini");
        }
        
        // 读取项目ID
        private async Task<string> ReadProjectIdAsync(string projectPath)
        {
            try
            {
                var configPath = Path.Combine(projectPath, "Config", "DefaultGame.ini");
                if (File.Exists(configPath))
                {
                    foreach (var line in File.ReadAllLines(configPath))
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
                await ModernDialog.ShowInfoAsync($"读取项目ID失败: {ex.Message}", "错误");
            }
            return string.Empty;
        }
        
        // 备份TemplateCategories.ini文件
        private async Task<bool> BackupTemplateCategoriesAsync()
        {
            try
            {
                var sourcePath = GetTemplateCategoriesPath();
                if (!File.Exists(sourcePath))
                {
                    await ModernDialog.ShowInfoAsync("找不到TemplateCategories.ini文件。", "错误");
                    return false;
                }
                
                // 创建Backup文件夹
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
                Directory.CreateDirectory(backupDir);
                
                // 备份文件，添加时间戳
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(backupDir, $"TemplateCategories_{timestamp}.ini");
                File.Copy(sourcePath, backupPath, true);
                
                // 同时创建一个没有时间戳的备份，用于重置功能
                var resetBackupPath = Path.Combine(backupDir, "TemplateCategories_Backup.ini");
                File.Copy(sourcePath, resetBackupPath, true);
                
                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"备份文件失败: {ex.Message}", "错误");
                return false;
            }
        }
        
        // 创建TemplateDefs.ini文件
        private async Task<bool> CreateTemplateDefsIniAsync()
        {
            try
            {
                // 获取选中的类别
                string category = "AEC"; // 默认类别
                if (IsCategoryGames) category = "Games";
                else if (IsCategoryME) category = "ME";
                else if (IsCategoryMFG) category = "MFG";
                
                // 构建TemplateDefs.ini内容
                var iniContent = $"[/Script/GameProjectGeneration.TemplateProjectDefs]\n" +
                               $"Categories={category}\n" +
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
                
                // 创建Config目录并写入文件
                var configDir = Path.Combine(ProjectPath, "Config");
                Directory.CreateDirectory(configDir);
                
                var templateDefsPath = Path.Combine(configDir, "TemplateDefs.ini");
                File.WriteAllText(templateDefsPath, iniContent);
                
                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"创建TemplateDefs.ini失败: {ex.Message}", "错误");
                return false;
            }
        }
        
        // 复制项目到引擎Templates目录
        private async Task<bool> CopyProjectToTemplatesAsync()
        {
            try
            {
                var targetDir = Path.Combine(SelectedEngine.Path, "Templates", TemplateName);
                
                // 如果目标目录已存在，删除它
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }
                
                // 复制整个项目目录
                CopyDirectory(ProjectPath, targetDir);
                
                return true;
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"复制项目到Templates目录失败: {ex.Message}", "错误");
                return false;
            }
        }
        
        // 复制图标和预览图到Media文件夹
        private async void CopyImagesToMediaFolder()
        {
            try
            {
                var mediaDir = Path.Combine(SelectedEngine.Path, "Templates", "Media");
                Directory.CreateDirectory(mediaDir);
                
                // 复制图标
                if (!string.IsNullOrEmpty(TemplateIconPath) && File.Exists(TemplateIconPath))
                {
                    var targetIconPath = Path.Combine(mediaDir, $"{TemplateName}.png");
                    File.Copy(TemplateIconPath, targetIconPath, true);
                }
                
                // 复制预览图
                if (!string.IsNullOrEmpty(TemplatePreviewPath) && File.Exists(TemplatePreviewPath))
                {
                    var targetPreviewPath = Path.Combine(mediaDir, $"{TemplateName}_Preview.png");
                    File.Copy(TemplatePreviewPath, targetPreviewPath, true);
                }
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"复制图片文件失败: {ex.Message}", "错误");
            }
        }
        
        // 复制目录的辅助方法
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // 创建目标目录
            Directory.CreateDirectory(destinationDir);
            
            // 复制文件
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destinationDir, fileName);
                File.Copy(file, destFile, true);
            }
            
            // 递归复制子目录
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(destinationDir, dirName);
                CopyDirectory(dir, destDir);
            }
        }
        
        // 创建新Categories类别的功能（可以通过对话框调用）
        public async Task AddNewCategoryAsync(string key, string nameEn, string nameZhHans, string nameJa, string nameKo, 
                                 string descEn, string descZhHans, string descJa, string descKo, string iconPath)
        {
            try
            {
                var templateCategoriesPath = GetTemplateCategoriesPath();
                if (!File.Exists(templateCategoriesPath))
                {
                    await ModernDialog.ShowInfoAsync("找不到TemplateCategories.ini文件。", "错误");
                    return;
                }
                
                // 构建新类别的INI内容
                var categoryContent = $"\nCategories = (\n" +
                                    $"    Key = \"{key}\",\n" +
                                    $"    LocalizedDisplayNames = (\n";
                
                // 添加显示名称
                categoryContent += $"        (Language = \"en\",     Text = \"{nameEn}\")";
                if (!string.IsNullOrEmpty(nameZhHans)) categoryContent += $",\n        (Language = \"zh-Hans\", Text = \"{nameZhHans}\")";
                if (!string.IsNullOrEmpty(nameJa)) categoryContent += $",\n        (Language = \"ja\",     Text = \"{nameJa}\")";
                if (!string.IsNullOrEmpty(nameKo)) categoryContent += $",\n        (Language = \"ko\",     Text = \"{nameKo}\")";
                
                categoryContent += $"\n    ),\n" +
                                 $"    LocalizedDescriptions = (\n";
                
                // 添加描述
                if (!string.IsNullOrEmpty(descEn))
                {
                    categoryContent += $"        (\n            Language = \"en\",\n            Text = \"{descEn}\"\n        )";
                }
                if (!string.IsNullOrEmpty(descZhHans))
                {
                    categoryContent += $",\n        (\n            Language = \"zh-Hans\",\n            Text = \"{descZhHans}\"\n        )";
                }
                if (!string.IsNullOrEmpty(descJa))
                {
                    categoryContent += $",\n        (\n            Language = \"ja\",\n            Text = \"{descJa}\"\n        )";
                }
                if (!string.IsNullOrEmpty(descKo))
                {
                    categoryContent += $",\n        (\n            Language = \"ko\",\n            Text = \"{descKo}\"\n        )";
                }
                
                categoryContent += $"\n    ),\n";
                
                // 添加图标（如果有）
                if (!string.IsNullOrEmpty(iconPath))
                {
                    categoryContent += $"    Icon = \"{iconPath}\",\n";
                }
                
                categoryContent += $"    IsMajorCategory = true\n)";
                
                // 读取现有内容并追加新类别
                var existingContent = File.ReadAllText(templateCategoriesPath);
                existingContent += categoryContent;
                
                // 写回文件
                File.WriteAllText(templateCategoriesPath, existingContent);
                
                await ModernDialog.ShowInfoAsync($"成功添加新类别 '{key}'。", "成功");
            }
            catch (Exception ex)
            {
                await ModernDialog.ShowInfoAsync($"添加新类别失败: {ex.Message}", "错误");
            }
        }
    }

    // 引擎信息类
    public class EngineInfo
    {
        public string DisplayName { get; set; }
        public string Path { get; set; }
    }
}