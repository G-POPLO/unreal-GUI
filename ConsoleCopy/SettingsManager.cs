using Newtonsoft.Json;
using System;
using System.IO;

namespace unreal_GUI_Reminder
{
    internal class SettingsManager
    {
        private const string SettingsFilePath = "FabSettings.json";
        private static SettingsManager _instance;
        private FabSettings _settings;

        public static SettingsManager Instance => _instance ??= new SettingsManager();

        private SettingsManager()
        {
            LoadSettings();
        }

        public FabSettings Settings => _settings;

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    _settings = JsonConvert.DeserializeObject<FabSettings>(json) ?? new FabSettings();
                }
                else
                {
                    _settings = new FabSettings();
                    SaveSettings();
                }
            }
            catch
            {
                _settings = new FabSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch { }
        }

        public class FabSettings
        {
            public bool FabNotificationEnabled { get; set; } = false;
            public DateTime LimitedTime { get; set; } = new DateTime(1990, 1, 1);
            public bool HeadlessEnabled { get; set; } = false;
            public bool AutoStart { get; set; } = false;
            public bool OpenEpic { get; set; } = false;
        }
    }
}