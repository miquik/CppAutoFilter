using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CppAutoFilter
{
    /// <summary>
    /// Interaction logic for MainToolWindowControl.
    /// </summary>
    public partial class MainWindowControl : System.Windows.Window, INotifyPropertyChanged
    {
        private Project thisProject;
        private string filterFullPath;
        private XDocument filterDoc;

        private FiltersVM filtersSettings;
        private FilterItemVM selectedItem = null;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainToolWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
            //
            // DataContext = new FiltersVM();
            FiltersSettings = (FiltersVM)DataContext;
        }

        public FiltersVM FiltersSettings
        {
            get => filtersSettings;
            set {
                filtersSettings = value;
                NotifyPropertyChanged();
            }
        }
        public FilterItemVM SelectedItem { get => selectedItem; set => selectedItem = value; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Guid.NewGuid().ToString("N")
            FilterWindow fw = new FilterWindow(thisProject.FileName);
            if (fw.ShowDialog() == true)
            {
                FiltersSettings.Filters.Add(new FilterItemVM()
                {
                    Name = fw.FilterItem.Name,
                    FolderPath = fw.FilterItem.FolderPath,
                    Extensions = fw.FilterItem.Extensions
                    // Guid = Guid.NewGuid().ToString().ToUpper()
                });
            }
        }

        private void DelButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
            {
                return;
            }
            FiltersSettings.Filters.Remove(SelectedItem);
            SelectedItem = null;
        }

        internal void UpdateProjectInfo(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            thisProject = project;

            //
            filterFullPath = thisProject.FullName + ".filters";
            bool exist = System.IO.File.Exists(filterFullPath);
            if (exist == false)
            {
                throw new IOException("Filter file not found");
            }
            // try parse existing settings
            try
            {
                filterDoc = XDocument.Load(filterFullPath);
                // parse CppAutoFilter element or create if not found
                XElement extElem = filterDoc.Root.Descendants(Consts.SN + "CppAutoFilter").FirstOrDefault();
                if (extElem != null)
                {
                    FiltersSettings = FiltersVM.Deserialize(extElem);
                }
            }
            catch (Exception)
            {
                filterDoc = null;
                throw new IOException("Invalid filter file");
            }
        }


        /*
         * <Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <ItemGroup>
    <Filter Include="File di origine">
      <UniqueIdentifier>{4FC737F1-C7A5-4376-A066-2A32D752A2FF}</UniqueIdentifier>
      <Extensions>cpp;c;cc;cxx;c++;cppm;ixx;def;odl;idl;hpj;bat;asm;asmx</Extensions>
    </Filter>
    <Filter Include="File di intestazione">
      <UniqueIdentifier>{93995380-89BD-4b04-88EB-625FBE52EBFB}</UniqueIdentifier>
      <Extensions>h;hh;hpp;hxx;h++;hm;inl;inc;ipp;xsd</Extensions>
    </Filter>
    <Filter Include="File di risorse">
      <UniqueIdentifier>{67DA6AB6-F800-4c08-8B7A-83BB121AAD01}</UniqueIdentifier>
      <Extensions>rc;ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe;resx;tiff;tif;png;wav;mfcribbon-ms</Extensions>
    </Filter>
    <Filter Include="provaf">
      <UniqueIdentifier>{bc7222e4-73eb-4efc-907d-4a4ab2e3549d}</UniqueIdentifier>
    </Filter>
  </ItemGroup>
  <ItemGroup>
    <ClInclude Include="utils\ev\ev_EditBits.h">
      <Filter>File di intestazione</Filter>
    </ClInclude>
    <ClInclude Include="utils\ev\ev_Mouse.h">
      <Filter>provaf</Filter>
    </ClInclude>
  </ItemGroup>
  <ItemGroup>
    <ClCompile Include="utils\ev\ev_Mouse.cpp">
      <Filter>provaf</Filter>
    </ClCompile>
  </ItemGroup>
</Project>
        */

        /*
        private static bool ContainsFilter(string attr, string text)
        {
            if (attr == null)
            {
                return false;
            }
            if (attr == text)
            {
                return true;
            }
            if (Regex.IsMatch(attr, @"^" + text + @"\W"))
            {
                return true;
            }
            return false;
        }
        */

        private XElement GetOrCreateImportGroup(string childType, bool createIfNotFound = true)
        {
            /*
            <ItemGroup>
                <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                  <Filter>provaf\ev</Filter>
                </ClCompile>
            */
            XElement groupElem = filterDoc.Root
                .Elements(Consts.SN + "ItemGroup")
                .Where(x => x.Element(Consts.SN + childType) != null)
                .FirstOrDefault();

            if (groupElem != null)
            {
                return groupElem;
            }
            if (createIfNotFound)
            {
                groupElem = new XElement(Consts.SN + "ItemGroup");
                filterDoc.Root.Add(groupElem);
                return groupElem;
            }
            return null;
        }

        private XElement CreateFilterElement(string filterName)
        {
            return new XElement(Consts.SN + "Filter",
                  new XAttribute(Consts.SN + "Include", filterName),
                  new XElement(Consts.SN + "UniqueIdentifier", "{" + Guid.NewGuid().ToString().ToUpper() + "}"));
        }

        private static string GetExtension(string fileName)
        {
            return Path.GetExtension(fileName).Replace(".", "") + ";";
        }

        private static bool ExtSearchPattern(string fileName, string extArray)
        {
            if (extArray == Consts.FilterAllFiles)
            {
                return true;
            }

            string compareArray = extArray;
            if (extArray == Consts.FilterIncludeFiles)
            {
                compareArray = Consts.IncludeExt;
            }
            else if (extArray == Consts.FilterSourceFiles)
            {
                compareArray = Consts.SourceExt;
            }
            else if (extArray == Consts.FilterResFiles)
            {
                compareArray = Consts.ResourceExt;
            }

            string ext = GetExtension(fileName);
            return compareArray.Contains(ext);
        }

        private void Generate(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (filterDoc == null)
            {
                MessageBox.Show("There are some error parsing .filters file");
                return;
            }

            // check if CppAutoFilter extension settings are present, if not let's create it            
            bool isNew = false;
            XElement extElem = filterDoc.Root.Element(Consts.SN + "ProjectExtensions");
            XElement cafElem = filterDoc.Root.Descendants(Consts.SN + "CppAutoFilter").FirstOrDefault();
            if (cafElem == null)
            {
                // exist "ProjectExtensions" session?                
                if (extElem == null)
                {
                    extElem = new XElement(Consts.SN + "ProjectExtensions");
                    filterDoc.Root.AddFirst(extElem);
                }
                cafElem = FiltersVM.CreateEmptySession();
                extElem.Add(cafElem);
                isNew = true;
            }

            if (isNew == false)
            {
                // session already exist, so let's propagate new changes
                // changes can be: new items added OR some items removed
                // 1. some items are removed
                // FiltersSettings.Filters.

                // This can be addressed using the following LINQ expression:
                // var result = peopleList2.Where(p => !peopleList1.Any(p2 => p2.ID == p.ID));
                // An alternate way of expressing this via LINQ, which some developers find more readable:
                // var result = peopleList2.Where(p => peopleList1.All(p2 => p2.ID != p.ID));
                var ss = cafElem.Descendants("Filter").Select(x => x.Attribute("Guid").Value);
            }

            // create filters and parse file
            XElement groupElem = GetOrCreateImportGroup("Filter", false);
            if (groupElem == null)
            {
                // ?
                throw new Exception("Malformed filters file");
            }

            // Filters
            HashSet<string> filters = new HashSet<string>();

            // Add elements
            XElement includeElem = GetOrCreateImportGroup("ClInclude");
            XElement compileElem = GetOrCreateImportGroup("ClCompile");
            XElement otherElem = GetOrCreateImportGroup("None");

            // Process
            foreach (var fi in FiltersSettings.Filters)
            {
                //<Filter Include="provaf">
                //    <UniqueIdentifier>{bc7222e4-73eb-4efc-907d-4a4ab2e3549d}</UniqueIdentifier>
                //</Filter>
                // 
                // 1. Add "Base" Filter
                filters.Add(fi.Name);
                // groupElem.Add(CreateFilterElement(fi.Name));

                // 2. Scan selected folder
                SearchOption soption = FiltersSettings.ScanSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var file in Directory
                    .EnumerateFiles(fi.FolderPath, "*", soption)
                    .Where(file => ExtSearchPattern(file, fi.Extensions)))
                {
                    // create subfilter
                    string filterSubName = Path.GetDirectoryName(file).Replace(fi.FolderPath, fi.Name);
                    filters.Add(filterSubName);
                    //                    
                    // now add path to right filter
                    string ext = GetExtension(file);
                    if (Consts.IncludeExt.Contains(ext))
                    {
                        //<ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                        //  <Filter>provaf\ev</Filter>
                        //</ClCompile>
                        includeElem.Add(new XElement(Consts.SN + "ClInclude",
                            new XAttribute(Consts.SN + "Include", file),
                            new XElement(Consts.SN + "Filter", filterSubName)));

                    }
                    else if (Consts.SourceExt.Contains(ext))
                    {
                        // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                        //     <Filter>provaf\ev</Filter>
                        // </ClCompile>
                        compileElem.Add(new XElement(Consts.SN + "ClCompile",
                            new XAttribute(Consts.SN + "Include", file),
                            new XElement(Consts.SN + "Filter", filterSubName)));
                    }
                    else
                    {
                        //<None Include="src\util\win\ut_Win32Resources.rc2">
                        //  <Filter>provaf\utils</Filter>
                        //</None>
                        otherElem.Add(new XElement(Consts.SN + "None",
                            new XAttribute(Consts.SN + "Include", file),
                            new XElement(Consts.SN + "Filter", filterSubName)));
                    }
                }
            }

            // Add all filters to section
            foreach (var filter in filters)
            {
                groupElem.Add(CreateFilterElement(filter));
            }

            // Write new settings
            cafElem.ReplaceWith(FiltersSettings.Serialize());
            // filterDoc.Save();
            // vcxproj;
        }


        /*
        private void Generate(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (filterDoc == null)
            {
                MessageBox.Show("There are some error parsing .filters file");
                return;
            }

            // check if CppAutoFilter extension settings are present, if not let's create it            
            bool isNew = false;
            XElement extElem = filterDoc.Root.Element("ProjectExtensions");
            XElement cafElem = filterDoc.Root.Descendants("CppAutoFilter").FirstOrDefault();
            if (cafElem == null)
            {
                // exist "ProjectExtensions" session?                
                if (extElem == null)
                {
                    extElem = new XElement("ProjectExtensions");
                    filterDoc.Root.Add(extElem);
                }
                cafElem = FiltersVM.CreateEmptySession();
                extElem.Add(cafElem);
                isNew = true;
            }

            if (isNew == false)
            {
                // session already exist, so let's propagate new changes
                // changes can be: new items added OR some items removed
                // 1. some items are removed
                // FiltersSettings.Filters.

                // This can be addressed using the following LINQ expression:
                // var result = peopleList2.Where(p => !peopleList1.Any(p2 => p2.ID == p.ID));
                // An alternate way of expressing this via LINQ, which some developers find more readable:
                // var result = peopleList2.Where(p => peopleList1.All(p2 => p2.ID != p.ID));
                var ss = cafElem.Descendants("Filter").Select(x => x.Attribute("Guid").Value);
            }

            // create filters and parse file
            XElement groupElem = GetOrCreateImportGroup("Filter", false);
            if (groupElem == null)
            {
                // ?
                throw new Exception("Malformed filters file");
            }

            // Filters
            HashSet<string> filters = new HashSet<string>();

            // Add elements
            XElement includeElem = GetOrCreateImportGroup("ClInclude");
            XElement compileElem = GetOrCreateImportGroup("ClCompile");
            XElement otherElem = GetOrCreateImportGroup("None");

            // Process
            foreach (var fi in FiltersSettings.Filters)
            {
                //<Filter Include="provaf">
                //    <UniqueIdentifier>{bc7222e4-73eb-4efc-907d-4a4ab2e3549d}</UniqueIdentifier>
                //</Filter>
                // 
                // 1. Add "Base" Filter
                filters.Add(fi.Name);
                // groupElem.Add(CreateFilterElement(fi.Name));

                // 2. Scan selected folder
                SearchOption soption = FiltersSettings.ScanSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var file in Directory
                    .EnumerateFiles(fi.FolderPath, "*", soption)
                    .Where(file => ExtSearchPattern(file, fi.Extensions)))
                {
                    // create subfilter
                    string filterSubName = Path.GetDirectoryName(file).Replace(fi.FolderPath, fi.Name);
                    filters.Add(filterSubName);
                    //                    
                    // now add path to right filter
                    string ext = GetExtension(file);
                    if (Consts.IncludeExt.Contains(ext))
                    {
                        //<ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                        //  <Filter>provaf\ev</Filter>
                        //</ClCompile>
                        includeElem.Add(new XElement("ClInclude",
                            new XAttribute("Include", file),
                            new XElement("Filter", filterSubName)));

                    }
                    else if (Consts.SourceExt.Contains(ext))
                    {
                        // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                        //     <Filter>provaf\ev</Filter>
                        // </ClCompile>
                        compileElem.Add(new XElement("ClCompile",
                            new XAttribute("Include", file),
                            new XElement("Filter", filterSubName)));
                    }
                    else
                    {
                        //<None Include="src\util\win\ut_Win32Resources.rc2">
                        //  <Filter>provaf\utils</Filter>
                        //</None>
                        otherElem.Add(new XElement("None",
                            new XAttribute("Include", file),
                            new XElement("Filter", filterSubName)));
                    }
                }
            }

            // Add all filters to section
            foreach (var filter in filters)
            {
                groupElem.Add(CreateFilterElement(filter));
            }


            // Write new settings
            cafElem.ReplaceWith(FiltersSettings.Serialize());
            // filterDoc.Save();
            // vcxproj;
        }
        */

        /*
        public static XmlDocument RemoveXmlns(String xml)
        {
            XDocument d = XDocument.Parse(xml);
            d.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in d.Descendants())
                elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(d.CreateReader());

            return xmlDocument;
        }
        */

        /*
        private void Generate(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (filterDoc == null)
            {
                MessageBox.Show("There are some error parsing .filters file");
                return;
            }

            // check if CppAutoFilter extension settings are present, if not let's create it            
            bool isNew = false;
            XElement extElem = filterDoc.Root.Element(Consts.SN + "ProjectExtensions");
            XElement cafElem = filterDoc.Root.Descendants(Consts.SN + "CppAutoFilter").FirstOrDefault();
            if (cafElem == null)
            {
                // exist "ProjectExtensions" session?                
                if (extElem == null)
                {
                    extElem = new XElement(Consts.SN + "ProjectExtensions");
                    filterDoc.Root.Add(extElem);
                }
                cafElem = FiltersVM.CreateEmptySession();
                extElem.Add(cafElem);
                isNew = true;
            }

            if (isNew == false)
            {
                // session already exist, so let's propagate new changes
                // changes can be: new items added OR some items removed
                // 1. some items are removed
                // FiltersSettings.Filters.

                // This can be addressed using the following LINQ expression:
                // var result = peopleList2.Where(p => !peopleList1.Any(p2 => p2.ID == p.ID));
                // An alternate way of expressing this via LINQ, which some developers find more readable:
                // var result = peopleList2.Where(p => peopleList1.All(p2 => p2.ID != p.ID));
                var ss = cafElem.Descendants(Consts.SN + "Filter").Select(x => x.Attribute(Consts.SN + "Guid").Value);
            }

            // create filters and parse file
            XElement groupElem = GetOrCreateImportGroup("Filter", false);
            if (groupElem == null)
            {
                // ?
                throw new Exception("Malformed filters file");
            }


            // Add elements
            XElement includeElem = GetOrCreateImportGroup("ClInclude");
            XElement compileElem = GetOrCreateImportGroup("ClCompile");
            XElement otherElem = GetOrCreateImportGroup("None");

            // Process
            foreach (var fi in FiltersSettings.Filters)
            {
                //<Filter Include="provaf">
                //    <UniqueIdentifier>{bc7222e4-73eb-4efc-907d-4a4ab2e3549d}</UniqueIdentifier>
                //</Filter>
                // 1. Add Filter
                groupElem.Add(new XElement(Consts.SN + "Filter",
                    new XAttribute(Consts.SN + "Include", fi.Name),
                    new XElement(Consts.SN + "UniqueIdentifier", "{" + fi.Guid.ToLower() + "}")));

                // 2. Scan selected folder
                SearchOption soption = FiltersSettings.ScanSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var file in Directory
                    .EnumerateFiles(fi.FolderPath, "*", soption)
                    .Where(file => ExtSearchPattern(file, fi.Extensions)))
                {
                    // now add path to right filter
                    string ext = GetExtension(file);
                    if (Consts.IncludeExt.Contains(ext))
                    {
                        //<ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                        //  <Filter>provaf\ev</Filter>
                        //</ClCompile>
                        includeElem.Add(new XElement(Consts.SN + "ClInclude",
                            new XAttribute(Consts.SN + "Include", file),
                            new XElement(Consts.SN + "Filter", fi.Name)));

                    } else if (Consts.SourceExt.Contains(ext))
                    {
                        // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                        //     <Filter>provaf\ev</Filter>
                        // </ClCompile>
                        compileElem.Add(new XElement(Consts.SN + "ClCompile",
                            new XAttribute(Consts.SN + "Include", file),
                            new XElement(Consts.SN + "Filter", fi.Name)));
                    } else
                    {
                        //<None Include="src\util\win\ut_Win32Resources.rc2">
                        //  <Filter>provaf\utils</Filter>
                        //</None>
                        otherElem.Add(new XElement(Consts.SN + "None",
                            new XAttribute(Consts.SN + "Include", file),
                            new XElement(Consts.SN + "Filter", fi.Name)));
                    }
                }
            }


            // Write new settings
            cafElem.ReplaceWith(FiltersSettings.Serialize());
            // filterDoc.Save();
            // vcxproj;
        }
        */

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Reparse(object sender, RoutedEventArgs e)
        {

        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = (FilterItemVM)((ListView)sender).SelectedItem;
        }
    }
}