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
    public class FilterItemVM : ObservableBase
    {
        private string _filterName;
        private string _fullPath;
        private string _extensions;

        public string Name
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

        public XElement Serialize()
        {
            return new XElement("Filter",
                    new XAttribute("Name", Name),
                    new XAttribute("FolderPath", FolderPath),
                    new XAttribute("Extensions", Extensions));
        }

        public static FilterItemVM Deserialize(XElement elem)
        {
            if (elem.Name.LocalName != "Filter")
            {
                return null;
            }
            var fName = elem.Attribute("Name");
            var fPath = elem.Attribute("FolderPath");
            var fExt = elem.Attribute("Extensions");
            if (fName == null || String.IsNullOrEmpty(fName.Value) || fPath == null)
            {
                return null;
            }

            if (fExt == null || String.IsNullOrEmpty(fExt.Value))
            {
                fExt.Value = "%%all%%";
            }
            FilterItemVM filterItemVM = new FilterItemVM();
            filterItemVM.Name = fName.Value;
            filterItemVM.FolderPath = fPath.Value;
            filterItemVM.Extensions = fExt.Value;
            return filterItemVM;
        }
    }


    public class FiltersVM : ObservableBase
    {
        private bool _scanSubFolder;
        private ObservableCollection<FilterItemVM> _filters;

        public FiltersVM()
        {
            _scanSubFolder = false;
            _filters = new ObservableCollection<FilterItemVM>();
        }

        public bool ScanSubfolder
        {
            get => _scanSubFolder;
            set
            {
                _scanSubFolder = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<FilterItemVM> Filters
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
                    new XElement("ScanSubfolder", ScanSubfolder),
                    new XElement("Filters", Filters.Select<FilterItemVM, XElement>(x => x.Serialize())));
        }

        public static FiltersVM Deserialize(XElement elem)
        {
            if (elem.Name.LocalName != "CppAutoFilter")
            {
                return null;
            }

            if (elem.Element(ObservableBase.sn + "Filters") == null ||
                elem.Element(ObservableBase.sn + "Filters").Elements(ObservableBase.sn + "Filter").Count() == 0)
            {
                return null;
            }

            var scanSub = false;
            if (elem.Element(ObservableBase.sn + "ScanSubfolder") != null)
            {
                var ss = elem.Element(ObservableBase.sn + "ScanSubfolder");
                if (ss.Value.ToLower() == "true" || ss.Value == "1")
                {
                    scanSub = true;
                }
            }

            List<FilterItemVM> flist = new List<FilterItemVM>();
            foreach (var el in elem.Element(ObservableBase.sn + "Filters").Elements(ObservableBase.sn + "Filter"))
            {
                var fivm = FilterItemVM.Deserialize(el);
                if (fivm != null)
                {
                    flist.Add(fivm);
                }
            }

            FiltersVM filtersVM = new FiltersVM();
            filtersVM.ScanSubfolder = scanSub;
            filtersVM.Filters = new ObservableCollection<FilterItemVM>(flist);
            return filtersVM;
        }
    }
}
