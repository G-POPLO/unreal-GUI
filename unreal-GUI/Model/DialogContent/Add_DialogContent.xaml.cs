using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace unreal_GUI.Model
{
    /// <summary>
    /// ContentDialogContent.xaml 的交互逻辑
    /// </summary>
    public partial class Add_DialogContent : UserControl
    {
        public ContentDialog Dialog { get; set; }

        public Add_DialogContent()
        {
            InitializeComponent();
            // 初始化时检查文本框状态
            UpdateAddButtonState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                FolderPathTextBox.Text = folderDialog.FolderName;
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
                Dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(ButtonNameTextBox.Text) && !string.IsNullOrWhiteSpace(FolderPathTextBox.Text);

            }
        }
    }
}
