using C3.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Elements
{
    public class C3Shape
    {
        public string Name { get; set; }
        public List<C3Line> Lines { get; set; }
        public string TextureName { get; set; }
        public uint Segment { get; set; }


    }
}
