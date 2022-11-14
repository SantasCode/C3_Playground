using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Loaders
{
    internal class ArmorInfoLoader
    {
        internal static Dictionary<uint, RolePartInfo> Load(TextReader tr) => RolePartInfoLoader.Load(tr);
    }
}
