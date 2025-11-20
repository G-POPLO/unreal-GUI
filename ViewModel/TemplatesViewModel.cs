using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Input;

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
            // 初始化引擎列表（实际应用中应从设置或系统中获取）
            InitializeEngineList();
        }

        // 初始化引擎列表
        private void InitializeEngineList()
        {
            // 这里只是示例数据，实际应用中应从设置或扫描系统获取
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.0", Path = "C:\\Program Files\\Epic Games\\UE_5.0" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.1", Path = "C:\\Program Files\\Epic Games\\UE_5.1" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.2", Path = "C:\\Program Files\\Epic Games\\UE_5.2" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.3", Path = "C:\\Program Files\\Epic Games\\UE_5.3" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.4", Path = "C:\\Program Files\\Epic Games\\UE_5.4" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.5", Path = "C:\\Program Files\\Epic Games\\UE_5.5" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.6", Path = "C:\\Program Files\\Epic Games\\UE_5.6" });
            AvailableEngines.Add(new EngineInfo { DisplayName = "Unreal Engine 5.7", Path = "C:\\Program Files\\Epic Games\\UE_5.7" });

            // 默认选择第一个引擎
            if (AvailableEngines.Count > 0)
            {
                SelectedEngine = AvailableEngines[0];
            }
        }

        // 浏览项目文件夹命令
        [RelayCommand]
        private void BrowseProject()
        {
            // 实际实现中应打开文件夹选择对话框
            StatusMessage = "浏览项目文件夹功能待实现。";
        }

        // 浏览图标命令
        [RelayCommand]
        private void BrowseIcon()
        {
            // 实际实现中应打开文件选择对话框选择PNG图标
            StatusMessage = "浏览图标功能待实现。";
        }

        // 浏览预览图命令
        [RelayCommand]
        private void BrowsePreview()
        {
            // 实际实现中应打开文件选择对话框选择PNG预览图
            StatusMessage = "浏览预览图功能待实现。";
        }

        // 创建模板命令
        [RelayCommand]
        private void CreateTemplate()
        {
            // 验证必要信息
            if (string.IsNullOrEmpty(ProjectPath))
            {
                StatusMessage = "请选择项目文件夹。";
                return;
            }

            if (string.IsNullOrEmpty(TemplateName))
            {
                StatusMessage = "请输入模板名称。";
                return;
            }

            if (string.IsNullOrEmpty(TemplateDescriptionEn))
            {
                StatusMessage = "请输入英文描述（必填）。";
                return;
            }

            if (SelectedEngine == null)
            {
                StatusMessage = "请选择引擎版本。";
                return;
            }

            // 实际实现中应执行模板创建逻辑
            StatusMessage = $"模板创建功能待实现。将创建模板 '{TemplateName}'。";
        }

        // 重置命令
        [RelayCommand]
        private void Reset()
        {
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
            
            if (AvailableEngines.Count > 0)
            {
                SelectedEngine = AvailableEngines[0];
            }
            
            StatusMessage = "已重置所有设置。";
        }
    }

    // 引擎信息类
    public class EngineInfo
    {
        public string DisplayName { get; set; }
        public string Path { get; set; }
    }
}