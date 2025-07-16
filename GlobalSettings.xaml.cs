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
using System.Windows.Shapes;
using System.Windows.Forms;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MSFSAutoStart
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class GlobalSettings : Window
    {
        public GlobalSettings()
        {
            InitializeComponent();
            LoadSettings();
        }
        private void LoadSettings()
        {
            CommunityFolderTextBox.Text = Properties.Settings.Default.CommunityFolder;
            AutoStartCheckBox.IsChecked = Properties.Settings.Default.EnableAutoStart;
            //AutoStopCheckBox.IsChecked = Properties.Settings.Default.EnableAutoStop;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.CommunityFolder = CommunityFolderTextBox.Text;
            Properties.Settings.Default.EnableAutoStart = AutoStartCheckBox.IsChecked == true;
            //Properties.Settings.Default.EnableAutoStop = AutoStopCheckBox.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        private void BrowseCommunityFolder_Click(object sender, RoutedEventArgs e)
        {
            // Using Windows Forms FolderBrowserDialog (add reference to System.Windows.Forms)
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = CommunityFolderTextBox.Text;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CommunityFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
