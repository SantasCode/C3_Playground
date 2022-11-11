using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles
{
    public class RolePartInfo
    {
        public uint PartCount;
        public List<RolePartSubInfo> Parts = new();
    }

    public class RolePartSubInfo
    {
        public uint MeshId;
        public uint TextureId;

        ///These properties always have default values. Excluding for clarity.
        //public uint MixTex;
        //public uint MixOpt;
        //public uint Asb;
        //public uint Adb;
        //public string Material;
    }
}
