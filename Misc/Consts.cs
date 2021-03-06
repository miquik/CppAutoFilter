using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CppAutoFilter.Misc
{
    internal class Consts
    {
        public static XNamespace SN = "http://schemas.microsoft.com/developer/msbuild/2003";
        public static XNamespace CAF = "http://schema.cppautofilter/v1";
        public static XNamespace NoneSN = "";


        public const string FilterAllFiles = "%%all%%";
        public const string FilterSourceFiles = "%%sources%%";
        public const string FilterIncludeFiles = "%%includes%%";
        public const string FilterResFiles = "%%res%%";

        public static string SourceExt = "cpp;c;cc;cxx;c++;cppm;ixx;def;odl;idl;hpj;bat;asm;asmx;";
        public static string IncludeExt = "h;hh;hpp;hxx;h++;hm;inl;inc;ipp;xsd;";
        public static string ResourceExt = "rc;ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe;resx;tiff;tif;png;wav;mfcribbon-ms;";
    }
}
