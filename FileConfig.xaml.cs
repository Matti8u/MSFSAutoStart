﻿using System;
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
using MSFSAutoStart;

namespace MSFSAutoStart
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FileConfig : Window
    {

        public FileConfig(UserFile userFile)
        {
            InitializeComponent();
            DataContext = userFile;

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = true;
            Close();
        }

    }
}
