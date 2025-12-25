using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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

        [ObservableProperty]
        private bool _allowDeleteFiles = false;

        [ObservableProperty]
        private string _outputPath;


        // 增量更新是否可用（-mx <= 5时可用）
        public bool IsIncrementalUpdateEnabled => CompressLevel <= 5;

        // 允许删除文件是否可用（增量更新启用时可用）
        public bool IsAllowDeleteEnabled => IncrementalUpdate;

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

        // 增量更新变化时更新允许删除文件的可用性
        partial void OnIncrementalUpdateChanged(bool value)
        {
            // 如果关闭增量更新，自动关闭允许删除文件
            if (!value && AllowDeleteFiles)
            {
                AllowDeleteFiles = false;
            }
            // 通知UI IsAllowDeleteEnabled 属性已变化
            OnPropertyChanged(nameof(IsAllowDeleteEnabled));
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
                //CalculateProjectSize();
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

                // 验证输出路径
                if (string.IsNullOrEmpty(OutputPath))
                {
                    await ModernDialog.ShowErrorAsync("请设置输出路径", "错误");
                    return;
                }

                // 获取项目目录
                string projectDir = Path.GetDirectoryName(ProjectPath);

                // 构建要压缩的路径列表
                var inputPaths = new List<string>
                {
                    Path.Combine(projectDir, "Config"),
                    Path.Combine(projectDir, "Content"),
                    Path.Combine(projectDir, "Plugins"),
                    Path.Combine(projectDir, "Source"),
                    ProjectPath // .uproject文件
                };

                // 构建完整的输出压缩文件路径
                string projectName = Path.GetFileNameWithoutExtension(ProjectPath);
                string outputArchivePath = Path.Combine(OutputPath, $"{projectName}.7z");

                // 直接调用压缩核心方法
                bool success = await CompressCore.CompressFilesAsync(
                     inputPaths,
                     outputArchivePath,
                     compressionLevel: CompressLevel,
                     soildcompress: SolidCompress
                 );

                if (success)
                {
                    SoundFX.PlaySound(0);
                    await ModernDialog.ShowInfoAsync("压缩完成", "成功");
                }

            }
            catch (Exception ex)
            {
                await ModernDialog.ShowErrorAsync($"压缩过程中发生错误: {ex.Message}", "错误");
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