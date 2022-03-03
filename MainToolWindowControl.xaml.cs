using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CppAutoFilter
{
    /// <summary>
    /// Interaction logic for MainToolWindowControl.
    /// </summary>
    public partial class MainToolWindowControl : UserControl
    {
        private Project thisProject;
        private string filterFullPath;
        private XDocument filterDoc;
        
        private FiltersVM filtersSettings;
        private FilterItemVM selectedItem = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainToolWindowControl"/> class.
        /// </summary>
        public MainToolWindowControl()
        {
            this.InitializeComponent();
        }

        public EnvDTE.Project Project { get; set; }
        public FiltersVM FiltersSettings { get => filtersSettings; set => filtersSettings = value; }
        public FilterItemVM SelectedItem { get => selectedItem; set => selectedItem = value; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedItem == null)
            {
                return;
            }

            FilterWindow fw = new FilterWindow();
            fw.FilterName = SelectedItem.Name;
            fw.FolderPath = SelectedItem.FolderPath;
            fw.Extensions = SelectedItem.Extensions;
            if (fw.ShowDialog() == true)
            {
                SelectedItem.Name = fw.FilterName;
                SelectedItem.FolderPath = fw.FolderPath;
                SelectedItem.Extensions = fw.Extensions;
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

            // 
            try
            {
                filterDoc = XDocument.Load(filterFullPath);
                // parse CppAutoFilter element or create if not found
                XElement extElem = filterDoc.Root.Descendants(sn + "CppAutoFilter").FirstOrDefault();
                if (extElem != null)
                {
                    // tbStructure.Text = extElem.Element(sn + "BaseFilterName").Value ?? "structure";
                    cbSubfolder.IsChecked = extElem.Element(sn + "LookSubfolder").Value == "true" ? true : false;
                    var list = extElem.Descendants(sn + "Folder").ToList();
                }
                else
                {
                    // create
                }
            }
            catch (Exception)
            {
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

        private void Generate(object sender, RoutedEventArgs e)
        {
            string filterFile = Project.FullName + ".filters";
            bool exist = System.IO.File.Exists(filterFile);
            if (exist == false)
            {
                throw new IOException("file di filtro non trovato");
            }
            // parse this file
            try
            {
                XNamespace sn = "http://schemas.microsoft.com/developer/msbuild/2003";
                XDocument doc = XDocument.Load(filterFile);

                // Remove all existing filter if any
                doc.Root
                    .Descendants(sn + "Filter")
                    .Where(x => ContainsFilter((string)x.Attribute("Include") ?? null, "")) // tbStructure.Text))
                    .Remove();

                // remove all ClInclude if belongs to this filter
                var clincludes = doc.Root
                    .Descendants(sn + "ClInclude")
                    .Where(x => x.Element(sn + "Filter") != null &&
                        ContainsFilter(x.Element(sn + "Filter").Value, "")); // tbStructure.Text));
                clincludes.Remove();

                var clcompiles = doc.Root
                    .Descendants(sn + "ClCompile")
                    .Where(x => x.Element(sn + "Filter") != null &&
                        ContainsFilter(x.Element(sn + "Filter").Value, "")); // tbStructure.Text); );
                clcompiles.Remove();
            }
            catch (System.Exception)
            {
                throw new IOException("Impossibile aprire il file di filtro");
            }

            // vcxproj;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {

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
        public static string GetRelativePath(string fullPath, string basePath)
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

        private void Reparse(object sender, RoutedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = (FilterItemVM)((ListView)sender).SelectedItem;
        }
    }
}