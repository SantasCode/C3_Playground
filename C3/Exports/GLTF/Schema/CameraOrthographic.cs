using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports.GLTF.Schema
{
    internal class CameraOrthographic
    {
        public required float Xmag { get; set; }
        public required float Ymag { get; set; }
        public required float Zfar { get; set; }
        public required float Znear { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }
    }
}
