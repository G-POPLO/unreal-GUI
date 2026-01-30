using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace unreal_GUI.ViewModel
{
    public partial class TutorialViewModel : ObservableObject
    {
        [ObservableProperty]
        private BitmapImage? tutorialImage;

        [ObservableProperty]
        private string markdownText = string.Empty;

        public TutorialViewModel()
        {
            LoadDefaultContent();
        }

        private void LoadDefaultContent()
        {
            MarkdownText = """
# 欢迎使用 Unreal-GUI

这是一个功能强大的 Unreal Engine 项目管理工具。

## 主要功能

- **项目管理**: 轻松管理多个 Unreal Engine 项目
- **模板系统**: 快速创建和配置项目模板
- **编译工具**: 集成编译和构建功能
- **设置管理**: 自定义引擎路径和项目配置

## 快速开始

1. 在设置中配置你的 Unreal Engine 路径
2. 创建或导入项目
3. 使用模板快速启动新项目

## 获取帮助

如有问题，请查看文档或联系支持团队。
""";
        }
    }
}
