using C3.Core;

namespace C3.Elements
{
    public class C3Phy
    {
        public string Name { get; set; } = "";

        public uint BlendCount { get; set; } = 0;

        public uint NVectorCount { get; set; } = 0;
        public uint AVectorCount { get; set; } = 0;
        public PhyVertex[] Vertices { get; set; }

        public uint NTriCount { get; set; } = 0;
        public uint ATriCount { get; set; } = 0;
        public ushort[] Indices { get; set; }

        public string TextureName { get; set; } = "";
        public int TexId { get; set; } = -1;
        public int Tex2Id { get; set; } = -1;
        public Vector3 BoxMin { get; set; } = Vector3.Zero;
        public Vector3 BoxMax { get; set; } = Vector3.Zero;

        public C3Motion? motion { get; set; } = null;

        public float A { get; set; } = 1.0f;
        public float R { get; set; } = 1.0f;
        public float G { get; set; } = 1.0f;
        public float B { get; set; } = 1.0f;

        public C3Key? Key { get; set; } = null;
        public bool Draw { get; set; } = true;

        public uint TextureRow { get; set; } = 1;

        public Matrix? InitMatrix { get; set; } = null;

        public Vector2? uvStep { get; set; } = new Vector2() { X = 0, Y = 0 };
    }
}
