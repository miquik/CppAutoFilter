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
    public class FiltersItem : ObservableBase
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
    }


    public class FiltersSettings : ObservableBase
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
    }
}
