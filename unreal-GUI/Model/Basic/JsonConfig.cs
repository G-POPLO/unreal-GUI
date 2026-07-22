using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace unreal_GUI.Model
{
    internal class JsonConfig
    { }

    /// <summary>
    /// 设置数据类
    /// </summary>
    public partial class SettingsData : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("engines")]
        private List<EngineInfo> _engines = [];

        [ObservableProperty]
        [JsonPropertyName("customButtons")]
        private List<CustomButton> _customButtons = [];

        [ObservableProperty]
        [JsonPropertyName("defaultOutputPath")]
        private string _defaultOutputPath = string.Empty;
    }


    /// <summary>
    /// 引擎信息类
    /// </summary>
    public partial class EngineInfo : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("path")]
        private string _path;

        [ObservableProperty]
        [JsonPropertyName("version")]
        private string _version;
    }



    /// <summary>
/// 自定义按钮类
/// </summary>
public partial class CustomButton : ObservableObject
{
    [ObservableProperty]
    [JsonPropertyName("name")]
    private string _name;

    [ObservableProperty]
    [JsonPropertyName("path")]
    private string _path;
}

/// <summary>
/// LauncherInstalled.dat 数据结构（Epic Games Launcher 安装列表）
/// </summary>
public class LauncherInstalledData
{
    [JsonPropertyName("InstallationList")]
    public List<LauncherInstalledEntry> InstallationList { get; set; } = [];
}

/// <summary>
/// LauncherInstalled.dat 中的单个条目
/// </summary>
public class LauncherInstalledEntry
{
    [JsonPropertyName("InstallLocation")]
    public string InstallLocation { get; set; } = string.Empty;

    [JsonPropertyName("ArtifactId")]
    public string ArtifactId { get; set; } = string.Empty;
}
}
