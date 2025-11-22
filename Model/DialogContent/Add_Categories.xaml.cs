using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace unreal_GUI.Model.DialogContent
{
    /// <summary>
    /// 语言信息类
    /// </summary>
    public class LanguageInfo
    {
        //public string Code { get; set; }
        //public string Name { get; set; }
    }

    /// <summary>
    /// 描述信息类
    /// </summary>
    public class DescriptionItem
    {
        //public string LanguageCode { get; set; }
        //public string DisplayName { get; set; }
        //public string Description { get; set; }
    }
}

    /// <summary>
    /// Add_Categories.xaml 的交互逻辑
    /// 用于添加模板类别
    /// </summary>
//    public partial class Add_Categories : System.Windows.Controls.UserControl
    
//    {
//        // 可用语言列表
//        public List<LanguageInfo> AvailableLanguages { get; set; }
        
//        // 描述项列表
//        public List<DescriptionItem> DescriptionItems { get; set; }
        
//        // 图标路径
//        public string IconPath { get; set; }
        
//        // 类别Key
//        public string CategoryKey { get { return KeyTextBox.Text; } }
        
//        // 是否为主类别
//        public bool IsMajorCategory { get { return IsMajorCategoryCheckBox.IsChecked ?? true; } }
        
//        // 构造函数
//        public Add_Categories()
//        {
//            InitializeComponent();
//            InitializeData();
//            InitializeEvents();
//        }
        
//        // 初始化数据
//        private void InitializeData()
//        {
//            // 设置数据上下文
//            DataContext = this;
            
//            // 初始化语言列表
//            AvailableLanguages = new List<LanguageInfo>
//            {
//                new LanguageInfo { Code = "en", Name = "English" },
//                new LanguageInfo { Code = "zh-Hans", Name = "简体中文" },
//                new LanguageInfo { Code = "ja", Name = "日本語" },
//                new LanguageInfo { Code = "ko", Name = "한국어" },
//                new LanguageInfo { Code = "fr", Name = "Français" },
//                new LanguageInfo { Code = "de", Name = "Deutsch" },
//                new LanguageInfo { Code = "es", Name = "Español" },
//                new LanguageInfo { Code = "ru", Name = "Русский" }
//            };
            
//            // 初始化描述项列表并添加英文默认项
//            DescriptionItems = new List<DescriptionItem>();
//            var defaultItem = new DescriptionItem
//            {
//                LanguageCode = "en",
//                DisplayName = "Custom Category",
//                Description = "A custom template category"
//            };
//            DescriptionItems.Add(defaultItem);
            
//            // 设置默认Key
//            KeyTextBox.Text = "Custom";
            
//            // 绑定描述列表到ListBox
//            DescriptionsListBox.ItemsSource = DescriptionItems;
//        }
        
//        // 初始化事件
//        private void InitializeEvents()
//        {
//            // 图标选择按钮点击事件
//            BrowseIconButton.Click += BrowseIconButton_Click;
            
//            // 添加语言描述按钮点击事件
//            AddDescriptionButton.Click += AddDescriptionButton_Click;
            
//            // Key文本框变化事件
//            KeyTextBox.TextChanged += KeyTextBox_TextChanged;
            
//            // 为ListBox添加加载完成事件，以便设置初始值
//            DescriptionsListBox.Loaded += DescriptionsListBox_Loaded;
//        }
        
//        // ListBox加载完成后设置初始值
//        private void DescriptionsListBox_Loaded(object sender, RoutedEventArgs e)
//        {
//            // 为每个列表项设置默认值
//            for (int i = 0; i < DescriptionsListBox.Items.Count; i++)
//            {
//                var container = DescriptionsListBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
//                if (container != null)
//                {
//                    // 找到ComboBox并设置默认语言
//                    var comboBox = FindVisualChild<ComboBox>(container);
//                    if (comboBox != null && comboBox.Tag.ToString() == "Language")
//                    {
//                        comboBox.SelectedValue = DescriptionItems[i].LanguageCode;
//                        comboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
//                    }
                    
//                    // 找到TextBox并设置默认文本
//                    var textBox = FindVisualChild<TextBox>(container);
//                    if (textBox != null && textBox.Tag.ToString() == "Description")
//                    {
//                        textBox.Text = DescriptionItems[i].Description;
//                        textBox.TextChanged += DescriptionTextBox_TextChanged;
//                    }
                    
//                    // 找到删除按钮并添加事件
//                    var deleteButton = FindVisualChild<Button>(container);
//                    if (deleteButton != null && deleteButton.Tag.ToString() == "RemoveDescription")
//                    {
//                        deleteButton.Click += RemoveDescriptionButton_Click;
//                    }
//                }
//            }
//        }
        
//        // 语言选择变化事件
//        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            var comboBox = sender as ComboBox;
//            if (comboBox != null)
//            {
//                var index = DescriptionsListBox.Items.IndexOf(comboBox.DataContext);
//                if (index >= 0 && index < DescriptionItems.Count)
//                {
//                    DescriptionItems[index].LanguageCode = comboBox.SelectedValue?.ToString();
//                }
//            }
//        }
        
//        // 描述文本变化事件
//        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            var textBox = sender as TextBox;
//            if (textBox != null)
//            {
//                var index = DescriptionsListBox.Items.IndexOf(textBox.DataContext);
//                if (index >= 0 && index < DescriptionItems.Count)
//                {
//                    DescriptionItems[index].Description = textBox.Text;
//                }
//            }
//        }
        
//        // 删除描述项
//        private void RemoveDescriptionButton_Click(object sender, RoutedEventArgs e)
//        {
//            var button = sender as Button;
//            if (button != null)
//            {
//                var index = DescriptionsListBox.Items.IndexOf(button.DataContext);
//                if (index >= 0 && index < DescriptionItems.Count)
//                {
//                    // 至少保留一项
//                    if (DescriptionItems.Count > 1)
//                    {
//                        DescriptionItems.RemoveAt(index);
//                        DescriptionsListBox.Items.Refresh();
//                    }
//                    else
//                    {
//                        StatusTextBlock.Text = "至少需要保留一项描述。";
//                    }
//                }
//            }
//        }
        
//        // 添加新的语言描述
//        private void AddDescriptionButton_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                // 找出未使用的语言代码
//                string unusedLanguage = AvailableLanguages
//                    .Where(lang => !DescriptionItems.Any(item => item.LanguageCode == lang.Code))
//                    .Select(lang => lang.Code)
//                    .FirstOrDefault();
                
//                // 如果有未使用的语言
//                if (!string.IsNullOrEmpty(unusedLanguage))
//                {
//                    var newItem = new DescriptionItem
//                    {
//                        LanguageCode = unusedLanguage,
//                        DisplayName = $"Custom Category ({unusedLanguage})",
//                        Description = $"A custom template category in {unusedLanguage}"
//                    };
//                    DescriptionItems.Add(newItem);
//                    DescriptionsListBox.Items.Refresh();
//                }
//                else
//                {
//                    // 所有语言都已添加，可以使用现有的语言代码添加另一个描述
//                    var newItem = new DescriptionItem
//                    {
//                        LanguageCode = "en", // 默认使用英文
//                        DisplayName = "Additional Description",
//                        Description = "Additional description for the category"
//                    };
//                    DescriptionItems.Add(newItem);
//                    DescriptionsListBox.Items.Refresh();
//                }
//            }
//            catch (Exception ex)
//            {
//                StatusTextBlock.Text = $"添加描述失败: {ex.Message}";
//            }
//        }
        
//        // 浏览图标文件
//        private void BrowseIconButton_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
//                {
//                    Title = "选择图标文件",
//                    Filter = "PNG图片|*.png|JPEG图片|*.jpg|所有图片|*.png;*.jpg;*.bmp;*.gif",
//                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
//                };
                
//                if (openFileDialog.ShowDialog() == DialogResult.OK)
//                {
//                    string selectedIconPath = openFileDialog.FileName;
                    
//                    // 验证图片尺寸
//                    using (var bitmap = new System.Drawing.Bitmap(selectedIconPath))
//                    {
//                        // 图标应该是方形的，且尺寸合理（建议256x256以内）
//                        if (bitmap.Width != bitmap.Height || bitmap.Width > 256 || bitmap.Height > 256)
//                        {
//                            StatusTextBlock.Text = "建议使用方形图标，最大尺寸256x256像素。";
//                        }
//                    }
                    
//                    // 设置图标路径并更新预览
//                    IconPath = selectedIconPath;
//                    UpdateIconPreview(IconPath);
//                }
//            }
//            catch (Exception ex)
//            {
//                StatusTextBlock.Text = $"选择图标失败: {ex.Message}";
//            }
//        }
        
//        // 更新图标预览
//        private void UpdateIconPreview(string iconPath)
//        {
//            try
//            {
//                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
//                {
//                    // 创建一个BitmapImage并设置到预览控件
//                    BitmapImage bitmapImage = new BitmapImage();
//                    bitmapImage.BeginInit();
//                    bitmapImage.UriSource = new Uri(iconPath, UriKind.Absolute);
//                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
//                    bitmapImage.EndInit();
                    
//                    // 找到预览Image控件并设置源
//                    var iconPreview = FindVisualChild<System.Windows.Controls.Image>(this);
//                    if (iconPreview != null && iconPreview.Tag.ToString() == "IconPreview")
//                    {
//                        iconPreview.Source = bitmapImage;
//                        iconPreview.Visibility = Visibility.Visible;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                StatusTextBlock.Text = $"更新预览失败: {ex.Message}";
//            }
//        }
        
//        // Key文本变化事件
//        private void KeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            ValidateInput();
//        }
        
//        // 验证输入
//        private void ValidateInput()
//        {
//            StatusTextBlock.Text = "";
            
//            // 验证Key是否为空
//            if (string.IsNullOrWhiteSpace(KeyTextBox.Text))
//            {
//                StatusTextBlock.Text = "Key不能为空";
//            }
//        }
        
//        // 查找视觉子元素的辅助方法
//        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
//        {
//            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
//            {
//                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
//                if (child != null && child is T)
//                    return (T)child;
//                else
//                {
//                    T childOfChild = FindVisualChild<T>(child);
//                    if (childOfChild != null)
//                        return childOfChild;
//                }
//            }
//            return null;
//        }
        
//        // 保存新类别
//        public bool SaveCategory(string templateCategoriesPath)
//        {
//            try
//            {
//                // 验证输入
//                if (string.IsNullOrWhiteSpace(KeyTextBox.Text))
//                {
//                    StatusTextBlock.Text = "请输入有效的Key";
//                    return false;
//                }
                
//                // 读取现有文件内容
//                string existingContent = string.Empty;
//                if (File.Exists(templateCategoriesPath))
//                {
//                    existingContent = File.ReadAllText(templateCategoriesPath);
//                }
                
//                // 构建新的类别内容
//                string newCategoryContent = $"\n[/Script/GameProjectGeneration.TemplateCategoryDef\'{CategoryKey}\']";
//                newCategoryContent += $"\nKey=\"{CategoryKey}\"";
//                newCategoryContent += $"\nIsMajorCategory={(IsMajorCategory ? "True" : "False")}";
                
//                // 添加图标路径（如果有）
//                if (!string.IsNullOrEmpty(IconPath))
//                {
//                    newCategoryContent += $"\nIcon=\"{Path.GetFileName(IconPath)}\"";
//                }
                
//                // 添加本地化显示名称和描述
//                foreach (var item in DescriptionItems)
//                {
//                    if (!string.IsNullOrEmpty(item.DisplayName))
//                    {
//                        newCategoryContent += $"\nLocalizedDisplayNames=(Language=\"{item.LanguageCode}\", Text=\"{item.DisplayName}\")";
//                    }
//                    if (!string.IsNullOrEmpty(item.Description))
//                    {
//                        newCategoryContent += $"\nLocalizedDescriptions=(Language=\"{item.LanguageCode}\", Text=\"{item.Description}\")";
//                    }
//                }
                
//                // 将新类别添加到文件末尾
//                string updatedContent = existingContent + newCategoryContent;
                
//                // 写入文件
//                File.WriteAllText(templateCategoriesPath, updatedContent);
                
//                // 如果有图标，复制到Media文件夹
//                if (!string.IsNullOrEmpty(IconPath))
//                {
//                    string mediaFolder = Path.Combine(Path.GetDirectoryName(templateCategoriesPath), "Media");
//                    Directory.CreateDirectory(mediaFolder);
//                    string targetIconPath = Path.Combine(mediaFolder, Path.GetFileName(IconPath));
//                    File.Copy(IconPath, targetIconPath, true);
//                }
                
//                return true;
//            }
//            catch (Exception ex)
//            {
//                StatusTextBlock.Text = $"保存失败: {ex.Message}";
//                return false;
//            }
//        }
//    }
//}
