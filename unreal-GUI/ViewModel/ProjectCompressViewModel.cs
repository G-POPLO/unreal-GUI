using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using unreal_GUI.Model.Basic;

namespace unreal_GUI.ViewModel
{
    public partial class ProjectCompressViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _projectPath;

        [ObservableProperty]
        private string _engineInfo;

        [ObservableProperty]
        private BitmapImage _projectThumbnail;

        [ObservableProperty]
        private int _compressLevel = 5;

        [ObservableProperty]
        private bool _solidCompress = true;

        [ObservableProperty]
        private bool _incrementalUpdate = false;

        //[ObservableProperty]
        //private bool _allowDeleteFiles = false;

        [ObservableProperty]
        private bool _filter = true;

        [ObservableProperty]
        private string _outputPath;

        [ObservableProperty]
        private string _inputPath;

        [ObservableProperty]
        private System.Windows.Visibility _is7zUpdateVisible = System.Windows.Visibility.Collapsed;

        [ObservableProperty]
        private System.Windows.Visibility _is7zOutputVisible = System.Windows.Visibility.Visible;

        [ObservableProperty]
        private string _compressButtonText = "开始压缩";


        // 增量更新是否可用（-mx <= 5时可用）
        public bool IsIncrementalUpdateEnabled => CompressLevel <= 5;

        // 允许删除文件是否可用（增量更新启用时可用）
        //public bool IsAllowDeleteEnabled => IncrementalUpdate;

        // 压缩级别变化时更新增量更新的可用性
        partial void OnCompressLevelChanged(int value)
        {
            // 如果压缩级别超过5，自动关闭增量更新
            if (value > 5 && IncrementalUpdate)
            {
                IncrementalUpdate = false;
            }
            // 通知UI IsIncrementalUpdateEnabled 属性已变化
            OnPropertyChanged(nameof(IsIncrementalUpdateEnabled));
        }

        // 增量更新变化时更新允许删除文件的可用性和_7zUpdate可见性
        partial void OnIncrementalUpdateChanged(bool value)
        {
            // 控制_7zUpdate可见性
            Is7zUpdateVisible = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            // 控制_7zOutput可见性（与_7zUpdate相反）
            Is7zOutputVisible = value ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            // 更新按钮文本
            CompressButtonText = value ? "开始增量更新" : "开始压缩";

            // 如果启用增量更新，自动关闭固实压缩（过滤器保持用户选择）
            if (value)
            {
                SolidCompress = false;
                // 注意：不再自动关闭过滤器，让用户可以控制增量更新时是否使用过滤
            }

            // 如果关闭增量更新，自动关闭允许删除文件
            //if (!value && AllowDeleteFiles)
            //{
            //    AllowDeleteFiles = false;
            //}
            // 通知UI IsAllowDeleteEnabled 属性已变化
            //OnPropertyChanged(nameof(IsAllowDeleteEnabled));
        }

        [RelayCommand]
        private void SelectProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Unreal Project Files (*.uproject)|*.uproject"
            };

            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                ProjectPath = dialog.FileName;
                ParseProjectInfo();
                LoadProjectThumbnail();
            }
        }

        [RelayCommand]
        private void SelectOutput()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择输出文件夹",
                UseDescriptionForTitle = true
            };

            // 如果已有输出路径，设置为初始目录
            if (!string.IsNullOrWhiteSpace(OutputPath) && Directory.Exists(OutputPath))
            {
                dialog.SelectedPath = OutputPath;
            }

            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                OutputPath = dialog.SelectedPath;
            }
        }

        [RelayCommand]
        private void Select7z()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "7z Archive Files (*.7z)|*.7z"
            };

            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                InputPath = dialog.FileName;
            }
        }

        [RelayCommand]
        private async Task Compress()
        {
            try
            {
                // 验证项目路径
                if (!File.Exists(ProjectPath))
                {
                    await ModernDialog.ShowErrorAsync("项目文件不存在，请选择有效的项目文件", "错误");
                    return;
                }

                // 获取项目目录
                string projectDir = Path.GetDirectoryName(ProjectPath);
                string projectName = Path.GetFileNameWithoutExtension(ProjectPath);

                bool success = false;

                if (IncrementalUpdate)
                {
                    // 增量更新模式
                    if (string.IsNullOrEmpty(InputPath))
                    {
                        await ModernDialog.ShowErrorAsync("增量更新模式下，请选择要更新的7z文件", "错误");
                        return;
                    }

                    if (!File.Exists(InputPath))
                    {
                        await ModernDialog.ShowErrorAsync("要更新的7z文件不存在", "错误");
                        return;
                    }

                    if (Path.GetExtension(InputPath) != ".7z")
                    {
                        await ModernDialog.ShowErrorAsync("增量更新只能使用7z格式的压缩包", "错误");
                        return;
                    }

                    // 获取现有压缩包的路径作为输出路径
                    string outputArchivePath = InputPath;

                    // 检查压缩包是否为固实压缩
                    bool isSolid = await CompressCore.IsSolidArchiveAsync(outputArchivePath);
                    if (isSolid)
                    {
                        await ModernDialog.ShowErrorAsync("无法更新固实压缩的压缩包，请重新创建压缩包或取消固实压缩", "错误");
                        return;
                    }

                    // 调用增量更新方法
                    success = await CompressCore.UpdateArchiveAsync(
                        projectDir,
                        outputArchivePath,
                        compressionLevel: CompressLevel,
                        filter: Filter
                    );
                }
                else
                {
                    // 正常压缩模式
                    if (string.IsNullOrEmpty(OutputPath))
                    {
                        await ModernDialog.ShowErrorAsync("请设置输出路径", "错误");
                        return;
                    }

                    // 构建完整的输出压缩文件路径
                    string outputArchivePath = Path.Combine(OutputPath, $"{projectName}.7z");

                    // 调用压缩方法
                    success = await CompressCore.CompressFilesAsync(
                         projectDir,
                         outputArchivePath,
                         compressionLevel: CompressLevel,
                         soildcompress: SolidCompress,
                         filter: Filter
                     );
                }

                if (success)
                {
                    SoundFX.PlaySound(0);
                    string successMessage = IncrementalUpdate ? "增量更新完成" : "压缩完成";
                    await ModernDialog.ShowInfoAsync(successMessage, "成功");
                }

            }
            catch (Exception ex)
            {
                string errorMessage = IncrementalUpdate ? $"增量更新过程中发生错误: {ex.Message}" : $"压缩过程中发生错误: {ex.Message}";
                await ModernDialog.ShowErrorAsync(errorMessage, "错误");
            }
        }

        private void ParseProjectInfo()
        {
            try
            {
                string projectName = Path.GetFileNameWithoutExtension(ProjectPath);
                string projectDir = Path.GetDirectoryName(ProjectPath);

                // 读取并解析.uproject文件
                string jsonContent = File.ReadAllText(ProjectPath);
                using var doc = System.Text.Json.JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                // 获取引擎版本
                string engineVersion = root.GetProperty("EngineAssociation").GetString() ?? "未知";

                // 获取文件修改时间
                DateTime lastWriteTime = File.GetLastWriteTime(ProjectPath);
                string formattedTime = lastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                // 更新引擎信息
                EngineInfo = $"项目名称: {projectName}\n" +
                             $"项目路径: {projectDir}\n" +
                             $"引擎版本: {engineVersion}\n" +
                             $"修改时间: {formattedTime}";
            }
            catch (Exception ex)
            {
                EngineInfo = "解析项目信息失败: " + ex.Message;
            }
        }


        private void LoadProjectThumbnail()
        {
            try
            {
                string projectDir = Path.GetDirectoryName(ProjectPath);
                string thumbnailPath = Path.Combine(projectDir, "Saved", "AutoScreenshot.png");

                if (File.Exists(thumbnailPath))
                {
                    using var stream = File.OpenRead(thumbnailPath);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    ProjectThumbnail = bitmap;
                }
                else
                {
                    ProjectThumbnail = null;
                }
            }
            catch (Exception)
            {
                ProjectThumbnail = null;
            }
        }


    }
}