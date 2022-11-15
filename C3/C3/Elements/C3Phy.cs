using C3.Core;

namespace C3.Elements
{
    public class C3Phy
    {
        public string Name { get; set; }

        public uint BlendCount { get; set; }

        public uint NVectorCount { get; set; }
        public uint AVectorCount { get; set; }
        public PhyVertex[] Vertices { get; set; }

        public uint NTriCount { get; set; }
        public uint ATriCount { get; set; }
        public ushort[] Indices { get; set; }

        public string TextureName { get; set; }
        
        public Vector3 BoxMin { get; set; }
        public Vector3 BoxMax { get; set; }

        public C3Key Key { get; set; }
        public uint TextureRow { get; set; }

        public Matrix? InitMatrix { get; set; }

        public Vector2? uvStep { get; set; }
    }
}
