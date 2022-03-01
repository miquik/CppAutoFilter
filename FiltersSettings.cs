using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CppAutoFilter
{
    public class FiltersItem : INotifyPropertyChanged
    {
        private string _filterName;
        private string _fullPath;

        string FilterName 
        {
            get => _filterName;
            set
            {
                _filterName = value;
                NotifyPropertyChanged();
            }
        }
        string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class FiltersSettings : INotifyPropertyChanged
    {
        private bool _scanSubFolder;
        private ObservableCollection<FiltersItem> _filters;

        public FiltersSettings()
        {
            _scanSubFolder = false;
            _filters = new ObservableCollection<FiltersItem>();
        }

        public bool ScanSubFolder 
        {
            get => _scanSubFolder;
            set 
            {
                _scanSubFolder = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<FiltersItem> Filters
        {
            get => _filters;
            set
            {
                _filters = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
