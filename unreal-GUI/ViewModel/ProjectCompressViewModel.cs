using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace unreal_GUI.ViewModel
{
    public partial class ProjectCompressViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _projectPath = string.Empty;

        [ObservableProperty]
        private string _engineInfo = "未选择项目";

        [ObservableProperty]
        private string _projectSize = "0 B";

        [ObservableProperty]
        private BitmapImage _projectThumbnail = null;

        [ObservableProperty]
        private int _compressLevel = 5;

        [ObservableProperty]
        private bool _solidCompress = true;

        [ObservableProperty]
        private bool _incrementalUpdate = false;

        [ObservableProperty]
        private bool _allowDeleteFiles = false;

        [ObservableProperty]
        private string _outputPath = string.Empty;

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
                CalculateProjectSize();
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
        private void Compress()
        {
            // 这里只是占位符，实际压缩逻辑将在后端实现
            // 由于要求暂时不写后端代码，所以这里只做简单的验证
            if (string.IsNullOrWhiteSpace(ProjectPath))
            {
                MessageBox.Show("请选择项目文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                MessageBox.Show("请选择输出路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 显示压缩选项摘要
            string summary = $"压缩项目: {Path.GetFileNameWithoutExtension(ProjectPath)}" +
                           $"\n压缩级别: {CompressLevel}" +
                           $"\n固实压缩: {SolidCompress}" +
                           $"\n增量更新: {IncrementalUpdate}";

            if (IncrementalUpdate)
            {
                summary += $"\n允许删除文件: {AllowDeleteFiles}";
            }

            summary += $"\n输出路径: {OutputPath}";

            MessageBox.Show(summary, "压缩选项摘要", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ParseProjectInfo()
        {
            try
            {
                // 这里只是模拟解析.uproject文件
                // 实际实现需要解析JSON格式的.uproject文件
                string projectName = Path.GetFileNameWithoutExtension(ProjectPath);
                string projectDir = Path.GetDirectoryName(ProjectPath);
                
                // 假设解析出的引擎信息
                EngineInfo = $"项目名称: {projectName}\n" +
                            $"项目路径: {projectDir}\n" +
                            $"引擎版本: 5.3.2\n" +
                            $"创建日期: 2024-01-15";
            }
            catch (Exception ex)
            {
                EngineInfo = "解析项目信息失败: " + ex.Message;
            }
        }

        private void CalculateProjectSize()
        {
            try
            {
                string projectDir = Path.GetDirectoryName(ProjectPath);
                if (Directory.Exists(projectDir))
                {
                    long size = GetDirectorySize(projectDir);
                    ProjectSize = FormatFileSize(size);
                }
            }
            catch (Exception ex)
            {
                ProjectSize = "计算项目体积失败: " + ex.Message;
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

        private long GetDirectorySize(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            long size = 0;

            // 遍历所有文件
            foreach (FileInfo file in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }

            return size;
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblBytes = bytes;

            if (bytes > 0)
            {
                while (dblBytes >= 1024 && i < suffix.Length - 1)
                {
                    dblBytes /= 1024;
                    i++;
                }
            }

            return $"{dblBytes:0.##} {suffix[i]}";
        }
    }
}