using CppAutoFilter.Misc;
using CppAutoFilter.ViewModels;
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
        private string projectFullPath;
        private XDocument filterDoc;
        private XDocument projDoc;

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
            FiltersSettings = new FiltersVM();
        }

        public FiltersVM FiltersSettings
        {
            get => filtersSettings;
            set
            {
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
                    Extensions = fw.FilterItem.Extensions,
                    CreateFolderTree = fw.FilterItem.CreateFolderTree
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
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = (FilterItemVM)((ListView)sender).SelectedItem;
        }


        internal void UpdateProjectInfo(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            thisProject = project;

            //
            filterFullPath = thisProject.FullName + ".filters";
            projectFullPath = thisProject.FullName;
            bool exist = System.IO.File.Exists(filterFullPath);
            if (exist == false)
            {
                throw new IOException("Filter file not found");
            }
            // try parse existing settings
            try
            {
                projDoc = XDocument.Load(thisProject.FullName);
                filterDoc = XDocument.Load(filterFullPath);
                // parse CppAutoFilter element or create if not found
                XElement extElem = filterDoc.Root.Descendants(Consts.CAF + "CppAutoFilter").FirstOrDefault();
                if (extElem != null)
                {
                    FiltersSettings = FiltersVM.Deserialize(extElem);
                }
            }
            catch (Exception)
            {
                projDoc = null;
                filterDoc = null;
                throw new IOException("Invalid filter file");
            }
        }
        private XElement GetOrCreateImportGroup(XDocument doc, string childType, bool createIfNotFound = true)
        {
            // <ItemGroup>
            //    <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
            //      <Filter>provaf\ev</Filter>
            //    </ClCompile>
            XElement groupElem = doc.Root
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
                doc.Root.Add(groupElem);
                return groupElem;
            }
            return null;
        }

        private XElement CreateFilterElement(string filterName)
        {
            return new XElement(Consts.SN + "Filter",
                  new XAttribute("Include", filterName),
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

            if (filterDoc == null || projDoc == null)
            {
                MessageBox.Show("There are some error parsing .filters file");
                return;
            }

            // Unload project
            thisProject.DTE.ExecuteCommand("Project.UnloadProject");

            // check if CppAutoFilter extension settings are present, if not let's create it            
            bool isNew = false;
            XElement extElem = filterDoc.Root.Element(Consts.SN + "ProjectExtensions");
            XElement cafElem = filterDoc.Root.Descendants(Consts.CAF + "CppAutoFilter").FirstOrDefault();
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
#if NEXT_VERSION
                // session already exist, so let's propagate new changes
                // changes can be: new items added OR some items removed
                // 1. some items are removed
                // cafElem.Descendants(Consts.CAF + "Filter")
                var oldSettings = FiltersVM.Deserialize(cafElem);
                var elementsToRemove = oldSettings
                    .Filters
                    .Where(p => FiltersSettings.Filters.All(p2 => !Utils.IsSameFilterItem(p2, p)));


                // This can be addressed using the following LINQ expression:
                // var result = peopleList2.Where(p => !peopleList1.Any(p2 => p2.ID == p.ID));
                // An alternate way of expressing this via LINQ, which some developers find more readable:
                // var result = peopleList2.Where(p => peopleList1.All(p2 => p2.ID != p.ID));
                // var ss = cafElem.Descendants("Filter").Select(x => x.Attribute("Guid").Value);
#endif
                // Clean everything before process
                // TODO
            }
            GenerateAddNewItems(FiltersSettings);
            

            // Write new settings
            cafElem.ReplaceWith(FiltersSettings.Serialize());
            // save filter file
            filterDoc.Save(filterFullPath);
            projDoc.Save(projectFullPath);

            // Reload project
            thisProject.DTE.ExecuteCommand("Project.ReloadProject");
            Close();
        }


        private void GenerateAddNewItems(FiltersVM filtersVM)
        {
            // create filters and parse file
            XElement groupElem = GetOrCreateImportGroup(filterDoc, "Filter", false);
            if (groupElem == null)
            {
                // ?
                throw new Exception("Malformed filters file");
            }

            // Filters
            HashSet<string> filters = new HashSet<string>();
            Dictionary<string, Tuple<string, FilterItemVM>> filterContents = new Dictionary<string, Tuple<string, FilterItemVM>>();

            // Get Project Sessions
            XElement projIncludeElem = GetOrCreateImportGroup(projDoc, "ClInclude");
            XElement projCompileElem = GetOrCreateImportGroup(projDoc, "ClCompile");
            XElement projOtherElem = GetOrCreateImportGroup(projDoc, "None");

            // Get Filter Sessions
            XElement includeElem = GetOrCreateImportGroup(filterDoc, "ClInclude");
            XElement compileElem = GetOrCreateImportGroup(filterDoc, "ClCompile");
            XElement otherElem = GetOrCreateImportGroup(filterDoc, "None");

            // Process
            // order extension so priority is given to 'custom ext' then down to 'all files'
            foreach (var fi in filtersVM.Filters.OrderBy(x => x.Extensions, new ExtensionComparer()))
            {
                //<Filter Include="provaf">
                //    <UniqueIdentifier>{bc7222e4-73eb-4efc-907d-4a4ab2e3549d}</UniqueIdentifier>
                //</Filter>
                // 
                // 1. Add "Base" Filter
                filters.Add(fi.Name);

                // 2. Create filter-tree to mimic directory structure
                SearchOption soption = filtersVM.ScanSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                if (fi.CreateFolderTree)
                {
                    foreach (var dir in Directory.EnumerateDirectories(fi.FolderPath, "*", soption))
                    {
                        // create subfilter
                        filters.Add(dir.Replace(fi.FolderPath, fi.Name));
                    }
                }

                // 2. Scan selected folder for files
                foreach (var file in Directory
                    .EnumerateFiles(fi.FolderPath, "*", soption)
                    .Where(file => ExtSearchPattern(file, fi.Extensions)))
                {
                    // Add path to filter session and proj session
                    string relFile = Utils.GetRelativePath(file, Path.GetDirectoryName(projectFullPath));
                    if (filterContents.ContainsKey(relFile) == false)
                    {
                        // Add to contents
                        filterContents.Add(relFile, new Tuple<string, FilterItemVM>(file, fi));
                    }
                }
            }

            // Add all filters to section
            foreach (var filter in filters)
            {
                groupElem.Add(CreateFilterElement(filter));
            }

            foreach (var item in filterContents)
            {
                string relFile = item.Key;
                string file = item.Value.Item1;
                FilterItemVM fi = item.Value.Item2;
                //
                string filterSubName = fi.Name;
                if (fi.CreateFolderTree)
                {
                    filterSubName = Path.GetDirectoryName(file).Replace(fi.FolderPath, fi.Name);
                }
                string ext = GetExtension(file);
                if (Consts.IncludeExt.Contains(ext))
                {
                    //<ClInclude Include="src\ev\xp\ev_EditBinding.h" />
                    //  <Filter>provaf\ev</Filter>
                    //</ClInclude>
                    includeElem.Add(new XElement(Consts.SN + "ClInclude",
                        new XAttribute("Include", relFile),
                        new XElement(Consts.SN + "Filter", filterSubName)));
                    // Proj Session
                    // <ClInclude Include="src\ev\xp\ev_EditBinding.h" />
                    projIncludeElem.Add(new XElement(Consts.SN + "ClInclude",
                        new XAttribute("Include", relFile)));
                }
                else if (Consts.SourceExt.Contains(ext))
                {
                    // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
                    //     <Filter>provaf\ev</Filter>
                    // </ClCompile>
                    compileElem.Add(new XElement(Consts.SN + "ClCompile",
                        new XAttribute("Include", relFile),
                        new XElement(Consts.SN + "Filter", filterSubName)));
                    // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp" />
                    projCompileElem.Add(new XElement(Consts.SN + "ClCompile",
                        new XAttribute("Include", relFile)));
                }
                else
                {
                    //<None Include="src\util\win\ut_Win32Resources.rc2">
                    //  <Filter>provaf\utils</Filter>
                    //</None>
                    otherElem.Add(new XElement(Consts.SN + "None",
                        new XAttribute("Include", relFile),
                        new XElement(Consts.SN + "Filter", filterSubName)));
                    // <None Include="src\util\win\ut_Win32Resources.rc2" />
                    projOtherElem.Add(new XElement(Consts.SN + "None",
                        new XAttribute("Include", relFile)));
                }
            }
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }


        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


/*
// 2. Scan selected folder for files
// SearchOption soption = fi.ScanSubfolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
foreach (var file in Directory
    .EnumerateFiles(fi.FolderPath, "*", soption)
    .Where(file => ExtSearchPattern(file, fi.Extensions)))
{
    // Add path to filter session and proj session
    string filterSubName = Path.GetDirectoryName(file).Replace(fi.FolderPath, fi.Name);
    string ext = GetExtension(file);
    string relFile = Utils.GetRelativePath(file, Path.GetDirectoryName(projectFullPath));
    if (Consts.IncludeExt.Contains(ext))
    {
        //<ClInclude Include="src\ev\xp\ev_EditBinding.h" />
        //  <Filter>provaf\ev</Filter>
        //</ClInclude>
        includeElem.Add(new XElement(Consts.SN + "ClInclude",
            new XAttribute("Include", relFile),
            new XElement(Consts.SN + "Filter", filterSubName)));
        // Proj Session
        // <ClInclude Include="src\ev\xp\ev_EditBinding.h" />
        projIncludeElem.Add(new XElement(Consts.SN + "ClInclude",
            new XAttribute("Include", relFile)));
    }
    else if (Consts.SourceExt.Contains(ext))
    {
        // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp">
        //     <Filter>provaf\ev</Filter>
        // </ClCompile>
        compileElem.Add(new XElement(Consts.SN + "ClCompile",
            new XAttribute("Include", relFile),
            new XElement(Consts.SN + "Filter", filterSubName)));
        // <ClCompile Include="src\ev\xp\ev_EditBinding.cpp" />
        projCompileElem.Add(new XElement(Consts.SN + "ClCompile",
            new XAttribute("Include", relFile)));
    }
    else
    {
        //<None Include="src\util\win\ut_Win32Resources.rc2">
        //  <Filter>provaf\utils</Filter>
        //</None>
        otherElem.Add(new XElement(Consts.SN + "None",
            new XAttribute("Include", relFile),
            new XElement(Consts.SN + "Filter", filterSubName)));
        // <None Include="src\util\win\ut_Win32Resources.rc2" />
        projOtherElem.Add(new XElement(Consts.SN + "None",
            new XAttribute("Include", relFile)));
    }
}
*/

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
