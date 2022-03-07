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
        private string _filterName;
        private string _fullPath;
        private string _extensions;
        private string _projFullPath;
        private string _extensionsTemp;

        public FilterWindow(string projFullPath)
        {
            InitializeComponent();
            _projFullPath = projFullPath;
            Extensions = Consts.FilterAllFiles;
        }

        public string FilterName
        {
            get => _filterName;
            set
            {
                _filterName = value;
                NotifyPropertyChanged();
            }
        }
        public string FolderPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                NotifyPropertyChanged();
            }
        }

        public string Extensions
        {
            get => _extensions;
            set
            {
                _extensions = value;
                NotifyPropertyChanged();
            }
        }
        private string ExtensionsTemp
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
                FolderPath = dialog.SelectedPath;
                string relpath = GetRelativePath(FolderPath, System.IO.Path.GetDirectoryName(_projFullPath));
                relpath = relpath.Replace("..\\", "dd\\");
                relpath = relpath.Replace(System.IO.Path.GetPathRoot(FolderPath), "");
                FilterName = relpath;
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
                Extensions = ExtensionsTemp;
            }
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Returns a relative path string from a full path based on a base path
        /// provided.
        /// </summary>
        /// <param name="fullPath">The path to convert. Can be either a file or a directory</param>
        /// <param name="basePath">The base path on which relative processing is based. Should be a directory.</param>
        /// <returns>
        /// String of the relative path.
        /// 
        /// Examples of returned values:
        ///  test.txt, ..\test.txt, ..\..\..\test.txt, ., .., subdir\test.txt
        /// </returns>
        public string GetRelativePath(string fullPath, string basePath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");

        }
    }
}
