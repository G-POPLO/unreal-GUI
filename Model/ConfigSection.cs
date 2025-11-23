using System.Collections.Generic;

namespace unreal_GUI.Model
{
    public class ConfigSection
    {
        public string Name { get; set; }
        public List<ConfigEntry> Entries { get; } = [];
    }

    public class ConfigEntry
    {
        public string Key { get; set; }
        public string RawValue { get; set; } // 保留原始值（含括号、引号等）
    }
}