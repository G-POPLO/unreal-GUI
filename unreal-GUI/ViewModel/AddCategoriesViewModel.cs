using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace unreal_GUI.ViewModel
{
    /// <summary>
    /// 语言信息类
    /// </summary>
    public class LanguageInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// 显示名称项类
    /// </summary>
    public partial class DisplayNameItem : ObservableObject
    {
        [ObservableProperty]
        private string languageCode;

        [ObservableProperty]
        private string displayName;

        // 属性变更事件
        public event PropertyChangedEventHandler PropertyChanged;

        // 当DisplayName属性变化时触发
        partial void OnDisplayNameChanged(string? value)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
        }
    }

    /// <summary>
    /// 描述信息类
    /// </summary>
    public partial class DescriptionItem : ObservableObject
    {
        [ObservableProperty]
        private string languageCode;

        [ObservableProperty]
        private string description;
    }

    /// <summary>
    /// 添加模板类别视图模型
    /// </summary>
    public partial class AddCategoriesViewModel : ObservableObject
    {
        // 可用语言列表
        [ObservableProperty]
        private List<LanguageInfo> availableLanguages =
        [
            new LanguageInfo { Code = "en", Name = "English" },
            new LanguageInfo { Code = "zh-Hans", Name = "简体中文" },
            new LanguageInfo { Code = "ja", Name = "日本語" },
            new LanguageInfo { Code = "ko", Name = "한국어" }
        ];

        // 显示名称项列表
        [ObservableProperty]
        private ObservableCollection<DisplayNameItem> displayNameItems = [];

        // 描述项列表
        [ObservableProperty]
        private ObservableCollection<DescriptionItem> descriptionItems = [];

        // 图标路径
        [ObservableProperty]
        private string iconPath;

        // 图标文件名称
        [ObservableProperty]
        private string iconFileName;

        // 类别Key
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string categoryKey = "User";

        // 是否为主类别
        [ObservableProperty]
        private bool isMajorCategory = true;

        // 错误信息
        [ObservableProperty]
        private string errorMessage;

        // 图标预览
        [ObservableProperty]
        private BitmapImage categoryIcon;

        // 保存命令是否可执行
        public bool CanSave => !string.IsNullOrWhiteSpace(CategoryKey) &&
                              DisplayNameItems.Any(item => !string.IsNullOrWhiteSpace(item.DisplayName));

        // 保存命令
        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            // 保存逻辑将由调用方处理
            OnSaveRequested?.Invoke(this, EventArgs.Empty);
        }

        // 保存请求事件
        public event EventHandler OnSaveRequested;

        // 添加显示名称项命令
        [RelayCommand]
        private void AddDisplayName()
        {
            var newItem = new DisplayNameItem
            {
                LanguageCode = AvailableLanguages.FirstOrDefault()?.Code ?? "en",
                DisplayName = string.Empty
            };
            DisplayNameItems.Add(newItem);
        }

        // 移除显示名称项命令
        [RelayCommand]
        private void RemoveDisplayName(DisplayNameItem item)
        {
            if (item != null && DisplayNameItems.Contains(item))
            {
                DisplayNameItems.Remove(item);
            }
        }

        // 添加描述项命令
        [RelayCommand]
        private void AddDescription()
        {
            var newItem = new DescriptionItem
            {
                LanguageCode = AvailableLanguages.FirstOrDefault()?.Code ?? "en",
                Description = string.Empty
            };
            DescriptionItems.Add(newItem);
        }

        // 移除描述项命令
        [RelayCommand]
        private void RemoveDescription(DescriptionItem item)
        {
            if (item != null && DescriptionItems.Contains(item))
            {
                DescriptionItems.Remove(item);
            }
        }

        // 浏览图标命令
        [RelayCommand]
        private void BrowseIcon()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PNG Images|*.png|All files|*.*",
                Title = "选择类别图标"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IconPath = openFileDialog.FileName;
                IconFileName = Path.GetFileName(IconPath);

                // 显示预览图片
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(IconPath);
                    bitmap.EndInit();
                    CategoryIcon = bitmap;
                    ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"无法加载图片: {ex.Message}";
                }
            }
        }

        // 构造函数
        public AddCategoriesViewModel()
        {
            // 添加默认的显示名称项
            AddDisplayName();

            // 监听DisplayNameItems集合变化，更新CanSave状态
            DisplayNameItems.CollectionChanged += (s, e) =>
            {
                // 添加新项的属性变更监听
                if (e.NewItems != null)
                {
                    foreach (DisplayNameItem item in e.NewItems)
                    {
                        item.PropertyChanged += DisplayNameItem_PropertyChanged;
                    }
                }
                // 移除被删除项的属性变更监听
                if (e.OldItems != null)
                {
                    foreach (DisplayNameItem item in e.OldItems)
                    {
                        item.PropertyChanged -= DisplayNameItem_PropertyChanged;
                    }
                }
                SaveCommand.NotifyCanExecuteChanged();
            };
        }

        // 当DisplayNameItem的属性变化时
        private void DisplayNameItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DisplayNameItem.DisplayName))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }
}