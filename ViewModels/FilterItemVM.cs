using CppAutoFilter.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CppAutoFilter.ViewModels
{
    public class FilterItemVM : ObservableBase
    {
        private string _filterName;
        private string _fullPath;
        private string _extensions;
        private bool _createFolderTree;

        public FilterItemVM()
        {
            Extensions = Consts.FilterAllFiles;
            CreateFolderTree = true;
        }

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

        public bool CreateFolderTree
        {
            get => _createFolderTree;
            set
            {
                _createFolderTree = value;
                NotifyPropertyChanged();
            }
        }

        // public string Guid { get; set; }

        public XElement Serialize()
        {
            return new XElement(Consts.CAF + "Filter",
                    new XAttribute("Name", Name),
                    new XAttribute("FolderPath", FolderPath),
                    new XAttribute("Extensions", Extensions),
                    new XAttribute("CreateFolderTree", CreateFolderTree)
                    );
            // new XAttribute("Guid", Guid));
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
            var fSSF = elem.Attribute("CreateFolderTree");
            // var fGd = elem.Attribute("Guid");
            if (fName == null || String.IsNullOrEmpty(fName.Value) || fPath == null)
            {
                return null;
            }

            string fExtension = (fExt != null && String.IsNullOrEmpty(fExt.Value) == false) ? fExt.Value : Consts.FilterAllFiles;
            // string fGuid = (fGd != null && String.IsNullOrEmpty(fGd.Value) == false) ? fGd.Value : System.Guid.NewGuid().ToString().ToUpper();

            FilterItemVM filterItemVM = new FilterItemVM();
            filterItemVM.Name = fName.Value;
            filterItemVM.FolderPath = fPath.Value;
            filterItemVM.Extensions = fExtension;
            filterItemVM.CreateFolderTree = (fSSF != null && String.IsNullOrEmpty(fSSF.Value) == false) ? Convert.ToBoolean(fSSF.Value) : true;
            // filterItemVM.Guid = fGuid;
            return filterItemVM;
        }
    }

}
