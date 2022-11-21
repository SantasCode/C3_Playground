using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.FileSet
{
    internal class _3DEffect
    {
        public RolePartInfo RolePartInfo { get; set; }
        public int Delay { get; set; }
        public int LoopTime { get; set; }
        public int FrameInterval { get; set; }
        public int LoopInterval { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int OffsetZ { get; set; }
        public bool ColorEnable { get; set; }
        public string Name { get; set; }
    }
}
