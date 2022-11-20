using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.FileSet
{
    internal class Npc
    {
        public string Name { get; set; }
        public ulong SimpleObjId { get; set; }
        public ulong StandyByMotionId { get; set; }
        public ulong BlazeByMotionId { get; set; }
        public ulong RestByMotionId { get; set; }
        public string? Effect { get; set; }
    }
}
