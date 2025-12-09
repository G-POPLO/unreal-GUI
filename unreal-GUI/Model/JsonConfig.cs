using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace unreal_GUI.Model
{
    internal class JsonConfig
    { }

    /// <summary>
    /// 引擎信息类
    /// </summary>
    public partial class EngineInfo : ObservableObject
    {
        [ObservableProperty]
        [property: JsonPropertyName("path")]
        private string _path;

        [ObservableProperty]
        [property: JsonPropertyName("version")]
        private string _version;
    }

    /// <summary>
    /// 设置数据类
    /// </summary>
    public partial class SettingsData : ObservableObject
    {
        [ObservableProperty]
        [property: JsonPropertyName("engines")]
        private List<EngineInfo> _engines = [];

        [ObservableProperty]
        [property: JsonPropertyName("customButtons")]
        private List<CustomButton> _customButtons = [];
    }

    /// <summary>
    /// 自定义按钮类
    /// </summary>
    public partial class CustomButton : ObservableObject
    {
        [ObservableProperty]
        [property: JsonPropertyName("name")]
        private string _name;

        [ObservableProperty]
        [property: JsonPropertyName("path")]
        private string _path;
    }
}
