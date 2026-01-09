using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using unreal_GUI.Model.Basic;

namespace unreal_GUI.ViewModel
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _versionText;

        public AboutViewModel()
        {
            _versionText = "当前版本：" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        [RelayCommand]
        private static void OpenUpdateUrl()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/G-POPLO/unreal-GUI/releases/",
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private static async Task OpenFeedbackUrl()
        {
            // 先显示指引对话框
            var feedbackGuidelines = @"在 GitHub 发起 Issue 前，请确保：

• 使用清晰明确的标题，简要描述问题
• 详细描述问题现象，包括：
  - 期望的行为
  - 实际的行为
• 如果是功能请求，请说明使用场景和价值
• 如有错误信息，请提供复现步骤（如果适用)
• 检查是否已存在类似的 Issue，避免重复
• 使用合适的标签分类您的 Issue

感谢您为项目改进做出的贡献！";

            await ModernDialog.ShowInfoAsync(feedbackGuidelines, "GitHub Issue 提交指引");

            // 然后跳转到 GitHub Issues 页面
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/G-POPLO/unreal-GUI/issues",
                UseShellExecute = true
            });
        }

        [RelayCommand]
        private static void OpenProjectProgressUrl()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/users/G-POPLO/projects/2/views/2",
                UseShellExecute = true
            });

        }
        [RelayCommand]
        private static void OpenWikiUrl()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/G-POPLO/unreal-GUI/wiki",
                UseShellExecute = true
            });

        }
    }
}