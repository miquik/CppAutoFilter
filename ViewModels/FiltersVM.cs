using CppAutoFilter.Misc;
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

namespace CppAutoFilter.ViewModels
{
    public class FiltersVM : ObservableBase
    {
        private bool _scanSubFolder;
        private ObservableCollection<FilterItemVM> _filters;

        public FiltersVM()
        {
            ScanSubfolder = true;
            Filters = new ObservableCollection<FilterItemVM>();
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
            return new XElement(Consts.CAF + "CppAutoFilter",
                    new XElement(Consts.CAF + "ScanSubfolder", ScanSubfolder),
                    new XElement(Consts.CAF + "Filters", Filters.Select<FilterItemVM, XElement>(x => x.Serialize())));
        }

        public static XElement CreateEmptySession()
        {
            return new XElement(Consts.CAF + "CppAutoFilter",
                    new XElement(Consts.CAF + "ScanSubfolder", "true"),
                    new XElement(Consts.CAF + "Filters"));
        }

        public static FiltersVM Deserialize(XElement elem)
        {
            if (elem.Name.LocalName != "CppAutoFilter")
            {
                return null;
            }

            if (elem.Element(Consts.CAF + "Filters") == null ||
                elem.Element(Consts.CAF + "Filters").Elements(Consts.CAF + "Filter").Count() == 0)
            {
                return null;
            }

            var scanSub = false;
            if (elem.Element(Consts.CAF + "ScanSubfolder") != null)
            {
                var ss = elem.Element(Consts.CAF + "ScanSubfolder");
                if (ss.Value.ToLower() == "true" || ss.Value == "1")
                {
                    scanSub = true;
                }
            }

            List<FilterItemVM> flist = new List<FilterItemVM>();
            foreach (var el in elem.Element(Consts.CAF + "Filters").Elements(Consts.CAF + "Filter"))
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
