using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

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
        private string _path;

        [ObservableProperty]
        private string _version;
    }

    /// <summary>
    /// 设置数据类
    /// </summary>
    public partial class SettingsData : ObservableObject
    {
        [ObservableProperty]
        private List<EngineInfo> _engines = [];

        [ObservableProperty]
        private List<CustomButton> _customButtons = [];
    }

    /// <summary>
    /// 自定义按钮类
    /// </summary>
    public partial class CustomButton : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _path;
    }
}
