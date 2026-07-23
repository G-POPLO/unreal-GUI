﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public partial List<EngineInfo> Engines { get; set; } = [];

        [ObservableProperty]
        [JsonPropertyName("customButtons")]
        public partial List<CustomButton> CustomButtons { get; set; } = [];

        [ObservableProperty]
        [JsonPropertyName("defaultOutputPath")]
        public partial string DefaultOutputPath { get; set; } = string.Empty;
    }


    /// <summary>
    /// 引擎信息类
    /// </summary>
    public partial class EngineInfo : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("path")]
        public partial string Path { get; set; } = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("version")]
        public partial string Version { get; set; } = string.Empty;
    }



    /// <summary>
    /// 自定义按钮类
    /// </summary>
    public partial class CustomButton : ObservableObject
    {
        [ObservableProperty]
        [JsonPropertyName("name")]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("path")]
        public partial string Path { get; set; } = string.Empty;
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
