using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CppAutoFilter
{
    public class FiltersItem : ObservableBase
    {
        private string _filterName;
        private string _fullPath;
        private string _extensions;

        public string FilterName 
        {
            get => _filterName;
            set
            {
                _filterName = value;
                NotifyPropertyChanged();
            }
        }
        public string FullPath
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

        public XElement Serialize()
        {
            return new XElement("Folder",
                    new XAttribute("FilterName", FilterName),
                    new XAttribute("Path", FullPath),
                    new XAttribute("Extensions", Extensions));
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

        public XElement Serialize()
        {
            return new XElement("CppAutoFilter",
                    new XElement("LookSubfolder", ScanSubFolder),
                    new XElement("Folders", Filters.Select<FiltersItem, XElement>(x => x.Serialize())));
        }
    }
}
