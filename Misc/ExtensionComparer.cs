using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppAutoFilter.Misc
{
    internal class ExtensionComparer : IComparer<string>
    {
        private int ExtToValue(string str)
        {
            switch (str)
            {
                case Consts.FilterAllFiles:
                    return 5;
                case Consts.FilterSourceFiles:
                    return 4;
                case Consts.FilterIncludeFiles:
                    return 3;
                case Consts.FilterResFiles:
                    return 2;
                default:
                    break;
            }
            return 1;
        }
        public int Compare(string stringA, string stringB)
        {
            int vA = ExtToValue(stringA);
            int vB = ExtToValue(stringB);
            //
            if (vA > vB)
            {
                return 1;
            }
            return vB > vA ? -1 : 0;
        }
    }
}
