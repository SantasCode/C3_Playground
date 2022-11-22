using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class CameraPerspective
    {
        public float? AspectRatio { get; set; }
        public required float Yfov { get; set; }
        public float? Zfar { get; set; }
        public required float Znear { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
