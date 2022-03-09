using CppAutoFilter.Misc;
using CppAutoFilter.ViewModels;
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
        private string _projFullPath;
        private string _extensionsTemp;
        private FilterItemVM _filterItemVM;

        public FilterWindow(string projFullPath)
        {
            InitializeComponent();
            _projFullPath = projFullPath;
            FilterItem = (FilterItemVM)DataContext;
            // Extensions = Consts.FilterAllFiles;
        }

        public FilterItemVM FilterItem
        {
            get => _filterItemVM;
            set
            {
                _filterItemVM = value;
                NotifyPropertyChanged();
            }
        }
        public string ExtensionsTemp
        {
            get => _extensionsTemp;
            set
            {
                _extensionsTemp = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Browse(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if ((bool)dialog.ShowDialog(this))
            {
                FilterItem.FolderPath = dialog.SelectedPath;
                string relpath = Utils.GetRelativePath(FilterItem.FolderPath, System.IO.Path.GetDirectoryName(_projFullPath));
                relpath = relpath.Replace("..\\", "dd\\");
                relpath = relpath.Replace(System.IO.Path.GetPathRoot(FilterItem.FolderPath), "");
                FilterItem.Name = relpath;
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            if (rbCustom.IsChecked == true)
            {
                if (!ExtensionsTemp.EndsWith(";"))
                {
                    ExtensionsTemp += ";";
                }
                FilterItem.Extensions = ExtensionsTemp;
            }
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }     
    }
}
