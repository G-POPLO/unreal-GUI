using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;
using unreal_GUI.Model;
using unreal_GUI.Model.Basic;
using unreal_GUI.Model.Features;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace unreal_GUI.ViewModel
{
    public partial class AddCategoriesViewModel : ObservableObject
    {
        // 基本信息属性
        [ObservableProperty]
        private string categoryKey = "User";

        [ObservableProperty]
        private bool isMajorCategory = true;

        [ObservableProperty]
        private bool enableMultiLanguageConfig = false;

        [ObservableProperty]
        private BitmapImage? categoryIcon;

        [ObservableProperty]
        private string iconFileName = string.Empty;

        [ObservableProperty]
        private string iconPath = string.Empty;

        // 多语言描述属性
        [ObservableProperty]
        private string descriptionEn = string.Empty;

        [ObservableProperty]
        private string? descriptionZh;

        [ObservableProperty]
        private string? descriptionJa;

        [ObservableProperty]
        private string? descriptionKo;

        // 用于显示名称（在XAML中未直接绑定，但用于生成Categories文本）
        [ObservableProperty]
        private string displayName = string.Empty;

        // 用于获取生成的Categories文本
        [ObservableProperty]
        private string generatedCategoriesText = string.Empty;

        private readonly CategoriesParser _categoriesParser;



        public AddCategoriesViewModel()
        {
            _categoriesParser = new CategoriesParser();
            // 设置默认图标
            SetDefaultIcon();
        }

        /// <summary>
        /// 设置默认图标
        /// </summary>
        private void SetDefaultIcon()
        {
            try
            {
                // 获取默认图标路径（应用程序根目录的Image文件夹）
                string appRoot = AppDomain.CurrentDomain.BaseDirectory;
                string defaultIconPath = Path.Combine(appRoot, "Image", "Blank_2x.png");
                
                if (File.Exists(defaultIconPath))
                {
                    // 加载默认图标
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(defaultIconPath);
                    bitmap.EndInit();
                    CategoryIcon = bitmap;
                    
                    // 设置默认图标信息
                    IconPath = defaultIconPath;
                    IconFileName = "Blank_2x.png";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载默认图标失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 浏览图标命令
        /// </summary>
        [RelayCommand]
        private void BrowseIcon()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Images (*.png)|*.png|All files (*.*)|*.*",
                Title = "选择类别图标"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedImagePath = openFileDialog.FileName;

                // 检查图片比例是否符合3:1
                if (!PhotoEditCore.IsCorrectRatio(selectedImagePath))
                {
                    // 图片不符合3:1比例，自动从中心裁剪
                    try
                    {
                        // 创建临时裁剪后的图片文件
                        string tempDirectory = Path.Combine(Path.GetTempPath(), "UnrealGUI");
                        if (!Directory.Exists(tempDirectory))
                        {
                            Directory.CreateDirectory(tempDirectory);
                        }

                        string croppedImageName = $"{CategoryKey}_2X.png";
                        string croppedImagePath = Path.Combine(tempDirectory, croppedImageName);

                        // 自动裁剪图片为3:1比例
                        bool cropSuccess = PhotoEditCore.AutoCropTo3to1Ratio(selectedImagePath, croppedImagePath);

                        if (cropSuccess)
                        {
                            selectedImagePath = croppedImagePath;
                        }
                        else
                        {
                            MessageBox.Show("图片裁剪失败，将使用原图", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"图片裁剪时发生错误：{ex.Message}，将使用原图", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                // 设置图标路径和文件名
                IconPath = selectedImagePath;
                IconFileName = Path.GetFileName(selectedImagePath);

                // 加载图标到UI
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(IconPath);
                bitmap.EndInit();
                CategoryIcon = bitmap;

                // 复制图片到引擎Templates目录
                //CopyImageToEngineTemplates(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// 请求跳转到图片编辑页面的事件
        /// </summary>
        //public event EventHandler<string> RequestImageEdit;

        /// <summary>
        /// 复制图片到引擎的Templates目录
        /// </summary>
        /// <param name="ImagePath">源图片路径</param>
        private void CopyImageToEngineTemplates(string ImagePath)
        {
            try
            {
                // 检查settings.json文件是否存在
                if (!File.Exists("settings.json"))
                {
                    MessageBox.Show("未找到settings.json文件，请先在设置中配置引擎路径", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 读取并解析JSON文件
                var json = File.ReadAllText("settings.json");
                var settings = JsonSerializer.Deserialize<SettingsData>(json);

                if (settings == null || settings.Engines == null || settings.Engines.Count == 0)
                {
                    MessageBox.Show("未在settings.json中找到引擎配置，请先在设置中配置引擎路径", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 验证CategoryKey是否已设置
                if (string.IsNullOrWhiteSpace(CategoryKey))
                {
                    MessageBox.Show("请先设置Category Key再选择图标", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                int copiedCount = 0;
                string targetFileName = $"{CategoryKey}_2X.png";

                // 复制到每个引擎的Templates目录
                foreach (var engineInfo in settings.Engines)
                {
                    try
                    {
                        string templatesDir = Path.Combine(engineInfo.Path, "Templates");
                        string mediaDir = Path.Combine(templatesDir, "Media");
                        string targetPath = Path.Combine(mediaDir, targetFileName);

                        // 确保Media目录存在
                        if (!Directory.Exists(mediaDir))
                        {
                            Directory.CreateDirectory(mediaDir);
                        }

                        // 复制文件
                        File.Copy(ImagePath, targetPath, true);
                        copiedCount++;
                    }
                    catch (Exception ex)
                    {
                        // 记录单个引擎复制失败，但继续尝试其他引擎
                        System.Diagnostics.Debug.WriteLine($"复制到引擎 {engineInfo.Path} 失败: {ex.Message}");
                    }
                }

                if (copiedCount > 0)
                {
                    MessageBox.Show($"成功将图标复制到 {copiedCount} 个引擎的Templates目录\n文件名: {targetFileName}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("未能将图标复制到任何引擎目录，请检查引擎路径和权限设置", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制图标时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 生成Categories文本命令
        /// </summary>
        [RelayCommand]
        private void GenerateCategories()
        {
            // 验证必填字段（现在图标总是有默认的，所以只需要验证Key和英文描述）
            if (string.IsNullOrWhiteSpace(CategoryKey) ||
                string.IsNullOrWhiteSpace(DescriptionEn))
            {
                // 可以在这里添加错误处理逻辑
                GeneratedCategoriesText = "请填写所有必填字段（Key和英文描述）";
                return;
            }

            // 如果未设置显示名称，则使用Key作为显示名称
            var displayNameToUse = string.IsNullOrWhiteSpace(DisplayName) ? CategoryKey : DisplayName;

            // 调用CategoriesParser生成Categories文本
            GeneratedCategoriesText = CategoriesParser.GenerateCategoriesText(
                key: CategoryKey,
                displayName: displayNameToUse,
                isMajorCategory: IsMajorCategory,

                DescriptionEn: DescriptionEn,
                DescriptionZh: DescriptionZh,
                DescriptionJa: DescriptionJa,
                DescriptionKo: DescriptionKo
            );
        }

        /// <summary>
        /// 重置表单命令
        /// </summary>
        [RelayCommand]
        private void ResetForm()
        {
            CategoryKey = string.Empty;
            IsMajorCategory = true;
            EnableMultiLanguageConfig = false;
            DisplayName = string.Empty;
            DescriptionEn = string.Empty;
            DescriptionZh = string.Empty;
            DescriptionJa = string.Empty;
            DescriptionKo = string.Empty;
            GeneratedCategoriesText = string.Empty;
            
            // 重新设置默认图标
            SetDefaultIcon();
        }
    }
}