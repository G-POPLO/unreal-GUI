using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using unreal_GUI.ViewModel;

namespace unreal_GUI.Model
{
    /// <summary>
    /// ContentDialogContent.xaml 的交互逻辑
    /// </summary>
    public partial class Delete_DialogContent : UserControl
    {
        public Delete_DialogContent()
        {
            InitializeComponent();
            LoadCustomButtons();
        }

        private void LoadCustomButtons()
        {
            if (File.Exists("settings.json"))
            {
                try
                {
                    var json = File.ReadAllText("settings.json");
                    var settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json);
                    ButtonsListBox.ItemsSource = settings?.CustomButtons ?? [];
                }
                catch (Exception)
                {
                    // 如果JSON文件损坏或读取失败，初始化为空列表
                    ButtonsListBox.ItemsSource = new List<SettingsViewModel.CustomButton>();

                }
            }
            else
            {
                ButtonsListBox.ItemsSource = new List<SettingsViewModel.CustomButton>();

            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            
            // 从JSON文件中删除相应的条目
            if (ButtonsListBox.SelectedItem != null)
            {
                var selectedItem = ButtonsListBox.SelectedItem as SettingsViewModel.CustomButton;
                // 获取选中项的名称
                var name = selectedItem?.Name;
                
                // 读取现有设置
                if (File.Exists("settings.json"))
                {
                    var json = File.ReadAllText("settings.json");
                    var settings = JsonConvert.DeserializeObject<SettingsViewModel.SettingsData>(json);
                    
                    // 删除选中的条目
                    if (settings.CustomButtons != null)
                    {
                        settings.CustomButtons.RemoveAll(item => item.Name == name);
                        
                        // 保存更新后的设置
                        var jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
                        File.WriteAllText("settings.json", jsonSettings);
                        
                        // 更新 ListBox 的数据源
                        ButtonsListBox.ItemsSource = null;
                        ButtonsListBox.ItemsSource = settings.CustomButtons;
                    }
                }
            }
        }
    }
}
