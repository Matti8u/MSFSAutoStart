using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;



namespace MSFSAutoStart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<UserFile> UserFiles { get; set; } = new ObservableCollection<UserFile>();
        private bool _validXMLFile = false;
        public bool ValidXMLFile
        {
            get => _validXMLFile;
            set
            {
                _validXMLFile = value;
                OnPropertyChanged(nameof(ValidXMLFile));
            }
        }

        public MainWindow()
        {

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            InitializeComponent();
            DataContext = this;  // So bindings work
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Assuming you store the CommunityFolder path in your settings:
            string communityFolder = Properties.Settings.Default.CommunityFolder;

            if (string.IsNullOrWhiteSpace(communityFolder))
            {
                System.Windows.MessageBox.Show("Community Folder is not set. Please select it in the settings.",
                                "Missing Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {

                GetFile();

            }
        }

        public class SharedSettings
        {
            public string CommunityFolder { get; set; } = "";
            public bool EnableAutoStart { get; set; } = true;
            public bool EnableAutoStop { get; set; } = false;
            public ObservableCollection<String> AutoStopPrograms { get; set; } = new ObservableCollection<String>();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                var newFile = new UserFile(openFileDialog.FileName);
                UserFiles.Add(newFile);  // Add to collection and UI updates automatically

                if (ValidXMLFile == true)
                {
                    AddAddonToXml(newFile);
                }
                XmlDataGrid.Items.Refresh();
            }
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = XmlDataGrid.SelectedItem as UserFile;
            if (selectedFile != null)
            {
                UserFiles.Remove(selectedFile);
                if (ValidXMLFile)
                {
                    RemoveAddonFromXml(selectedFile);
                }
                XmlDataGrid.Items.Refresh();
            }
        }
        
        private void Options_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is UserFile selectedFile)
            {
                var settingsWindow = new FileConfig(selectedFile);
                settingsWindow.Owner = this; // optional, for modal behavior
                bool? result = settingsWindow.ShowDialog();

                
                
               XmlDataGrid.Items.Refresh();
                UpdateAddonInXml(selectedFile);
              
                
            }
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new GlobalSettings();
            settingsWindow.Owner = this;       // Optional: sets this window as the owner
            settingsWindow.ShowDialog();       // Opens Settings window modally (blocks until closed)
            string communityFolder = Properties.Settings.Default.CommunityFolder;

            if (string.IsNullOrWhiteSpace(communityFolder))
            {
                System.Windows.MessageBox.Show("Community Folder is not set. Please select it in the settings.",
                                "Missing Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {

                GetFile();
                // Now update the <Disabled> value in EXE.xml based on your global setting
                string exeXmlPath = GetExeXmlPath(communityFolder);
                if (File.Exists(exeXmlPath))
                {
                    var doc = XDocument.Load(exeXmlPath);
                    var root = doc.Element("SimBase.Document");
                    if (root != null)
                    {
                        // Assuming you have a bool global setting like Properties.Settings.Default.GlobalDisabled
                        bool disabledValue = Properties.Settings.Default.EnableAutoStart;

                        var disabledElement = root.Element("Disabled");
                        if (disabledElement != null)
                        {
                            disabledElement.Value = disabledValue ? "False" : "True";

                        }
                        else
                        {
                            root.AddFirst(new XElement("Disabled", disabledValue ? "True" : "False"));
                        }
                        doc.Save(exeXmlPath);
                    }
                }

            }
        }

        private void GetFile()
        {
            string communityFolder = Properties.Settings.Default.CommunityFolder;
            var firstParent = Directory.GetParent(communityFolder);
            var secondParent = firstParent != null ? Directory.GetParent(firstParent.FullName) : null;
            UserFiles.Clear();

            if (secondParent != null)
            {
                ValidXMLFile = true;
                string exeXmlPath = System.IO.Path.Combine(secondParent.FullName, "EXE.xml");
                if (File.Exists(exeXmlPath))
                {
                    
                    var doc = XDocument.Load(exeXmlPath);
                    var addons = doc.Descendants("Launch.Addon");

                    foreach (var addon in addons)
                    {
                        var path = addon.Element("Path")?.Value ?? string.Empty;
                        string cmdLine = addon.Element("CommandLine")?.Value ?? "";
                        bool disabled = bool.TryParse(addon.Element("Disabled")?.Value, out bool d) && d;

                        bool autoStartEnabled = !disabled;

                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            var userFile = new UserFile(path, autoStartEnabled)
                            {
                                CmdLineArg = cmdLine,
                                AutoStopEnabled = false,
                            };

                            UserFiles.Add(userFile);
                        }

                    }
                }
                else
                {
                    var defaultXml = new XDocument(
                       new XElement("SimBase.Document",
                       new XAttribute("Type", "Launch"),
                       new XAttribute("version", "1,0"),
                       new XElement("Descr", "Launch"),
                       new XElement("Filename", "EXE.xml"),
                       new XElement("Disabled", "False"),
                       new XElement("Launch.ManualLoad", "False")
                       )
                    );

                    defaultXml.Save(exeXmlPath);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Could not resolve EXE.xml path (community folder too shallow).");
                ValidXMLFile = false;
            }
        }

        private void AddAddonToXml(UserFile userFile)
        {

            try
            {


                string communityFolder = Properties.Settings.Default.CommunityFolder;
                var firstParent = Directory.GetParent(communityFolder);
                var secondParent = firstParent != null ? Directory.GetParent(firstParent.FullName) : null;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                string exeXmlPath = System.IO.Path.Combine(secondParent.FullName, "EXE.xml");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                var doc = XDocument.Load(exeXmlPath);
                var root = doc.Element("SimBase.Document");
                if (root != null)
                {
                    root.Add(
                        new XElement("Launch.Addon",
                            new XElement("Disabled", userFile.AutoStartEnabled ? "False" : "True"),
                            new XElement("ManualLoad", "False"),
                            new XElement("Name", System.IO.Path.GetFileNameWithoutExtension(userFile.FilePath)),
                            new XElement("Path", userFile.FilePath),
                            string.IsNullOrWhiteSpace(userFile.CmdLineArg) ? null : new XElement("CommandLine", userFile.CmdLineArg),
                            new XElement("NewConsole", "False")
                        )
                    );

                    doc.Save(exeXmlPath);
                }
            }
            catch (IOException ioEx)
            {
                System.Windows.MessageBox.Show($"File IO error: {ioEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                System.Windows.MessageBox.Show($"Access denied: {uaEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Xml.XmlException xmlEx)
            {
                System.Windows.MessageBox.Show($"XML error: {xmlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Generic catch-all for unexpected errors
                System.Windows.MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string GetExeXmlPath(string communityFolder)
        {
            var firstParent = Directory.GetParent(communityFolder);
            var secondParent = firstParent != null ? Directory.GetParent(firstParent.FullName) : null;
            if (secondParent != null)
            {
                return System.IO.Path.Combine(secondParent.FullName, "EXE.xml");
            }
            return string.Empty;
        }

        private void RemoveAddonFromXml(UserFile userFile)
        {
            string communityFolder = Properties.Settings.Default.CommunityFolder;
            string exeXmlPath = GetExeXmlPath(communityFolder);

            if (File.Exists(exeXmlPath))
            {
                var doc = XDocument.Load(exeXmlPath);
                var root = doc.Element("SimBase.Document");

                if (root != null)
                {
                    // Find the Launch.Addon element with the matching Path
                    var addonToRemove = root.Elements("Launch.Addon")
                                           .FirstOrDefault(x => string.Equals(
                                                x.Element("Path")?.Value,
                                                userFile.FilePath,
                                                StringComparison.OrdinalIgnoreCase));

                    if (addonToRemove != null)
                    {
                        addonToRemove.Remove();
                        doc.Save(exeXmlPath);
                    }
                }
            }
        }


        private void UpdateAddonInXml(UserFile userFile)
        {
            string communityFolder = Properties.Settings.Default.CommunityFolder;
            var firstParent = Directory.GetParent(communityFolder);
            var secondParent = firstParent != null ? Directory.GetParent(firstParent.FullName) : null;

            if (secondParent == null) return;

            string exeXmlPath = System.IO.Path.Combine(secondParent.FullName, "EXE.xml");
            if (!File.Exists(exeXmlPath)) return;

            var doc = XDocument.Load(exeXmlPath);
            var root = doc.Element("SimBase.Document");
            if (root == null) return;

            // Find the addon matching the file path
            var addon = root.Elements("Launch.Addon")
                            .FirstOrDefault(a =>
                                string.Equals(a.Element("Path")?.Value, userFile.FilePath, StringComparison.OrdinalIgnoreCase));

            if (addon == null) return;
            

            // Update or add Disabled element
            var disabledElem = addon.Element("Disabled");
            if (disabledElem == null)
            {
                disabledElem = new XElement("Disabled");
                addon.Add(disabledElem);
            }
            disabledElem.Value = userFile.AutoStartEnabled ? "False" : "True";

            // Update or add CommandLine element
            var cmdElem = addon.Element("CommandLine");
            if (cmdElem == null)
            {
                cmdElem = new XElement("CommandLine");
                addon.Add(cmdElem);
            }
            cmdElem.Value = userFile.CmdLineArg ?? "";

            doc.Save(exeXmlPath);
        }



    }
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }
}