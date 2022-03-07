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

        public string Guid { get; set; }

        public XElement Serialize()
        {
            return new XElement("Filter",
                    new XAttribute("Name", Name),
                    new XAttribute("FolderPath", FolderPath),
                    new XAttribute("Extensions", Extensions),
                    new XAttribute("Guid", Guid));
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
            var fGd = elem.Attribute("Guid");
            if (fName == null || String.IsNullOrEmpty(fName.Value) || fPath == null)
            {
                return null;
            }

            string fExtension = (fExt != null && String.IsNullOrEmpty(fExt.Value) == false) ? fExt.Value : Consts.FilterAllFiles;
            string fGuid = (fGd != null && String.IsNullOrEmpty(fGd.Value) == false) ? fGd.Value : System.Guid.NewGuid().ToString().ToUpper();

            FilterItemVM filterItemVM = new FilterItemVM();
            filterItemVM.Name = fName.Value;
            filterItemVM.FolderPath = fPath.Value;
            filterItemVM.Extensions = fExtension;
            filterItemVM.Guid = fGuid;
            return filterItemVM;
        }
    }


    public class FiltersVM : ObservableBase
    {
        private bool _scanSubFolder;
        private ObservableCollection<FilterItemVM> _filters;

        public FiltersVM()
        {
            _scanSubFolder = true;
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

        public static XElement CreateEmptySession()
        {
            return new XElement("CppAutoFilter",
                    new XElement("ScanSubfolder", "true"),
                    new XElement("Filters"));
        }

        public static FiltersVM Deserialize(XElement elem)
        {
            if (elem.Name.LocalName != "CppAutoFilter")
            {
                return null;
            }

            if (elem.Element(Consts.SN + "Filters") == null ||
                elem.Element(Consts.SN + "Filters").Elements(Consts.SN + "Filter").Count() == 0)
            {
                return null;
            }

            var scanSub = false;
            if (elem.Element(Consts.SN + "ScanSubfolder") != null)
            {
                var ss = elem.Element(Consts.SN + "ScanSubfolder");
                if (ss.Value.ToLower() == "true" || ss.Value == "1")
                {
                    scanSub = true;
                }
            }

            List<FilterItemVM> flist = new List<FilterItemVM>();
            foreach (var el in elem.Element(Consts.SN + "Filters").Elements(Consts.SN + "Filter"))
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
