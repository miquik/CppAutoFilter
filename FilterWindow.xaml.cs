using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace CppAutoFilter
{
    /// <summary>
    /// Logica di interazione per FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window, INotifyPropertyChanged
    {        
        public FilterWindow()
        {
            InitializeComponent();
        }

        public string FolderPath { get; set; }
        public string FilterName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Browse(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if ((bool)dialog.ShowDialog(this))
            {
                FolderPath = dialog.SelectedPath;
                // TODO: suggest filter name
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
