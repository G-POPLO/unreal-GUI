using ModernWpf.Controls;
using System;
using System.Collections.Generic;
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

namespace unreal_GUI.Model
{
    /// <summary>
    /// ContentDialogContent.xaml 的交互逻辑
    /// </summary>
    public partial class Add_DialogContent : UserControl
    {
        public ContentDialog? Dialog { get; set; }
        
        public Add_DialogContent()
        {
            InitializeComponent();
            // 初始化时检查文本框状态
            UpdateAddButtonState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPathTextBox.Text = folderDialog.SelectedPath;
            }
        }
        
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAddButtonState();
        }
        
        private void UpdateAddButtonState()
        {
            if (Dialog != null)
            {
                Dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(ButtonNameTextBox.Text) && 
                                               !string.IsNullOrWhiteSpace(FolderPathTextBox.Text);
            }
        }
    }
}
