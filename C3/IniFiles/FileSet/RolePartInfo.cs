using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.FileSet
{
    internal class RolePartInfo
    {
        public uint PartCount;
        public List<RolePartSubInfo> Parts = new();
    }

    internal class RolePartSubInfo
    {
        public ulong MeshId { get; set; }
        public ulong TextureId { get; set; }
        public uint Asb { get; set; }   
        public uint Adb { get; set; }

        ///These properties always have default values. Excluding for clarity.
        //public uint MixTex;
        //public uint MixOpt;
        //public string Material;
    }
}
