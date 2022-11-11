using C3.Core;

namespace C3.Elements
{
    public class C3Phy
    {
        public string Name = "";

        public uint BlendCount = 0;

        public uint NVectorCount = 0;
        public uint AVectorCount = 0;
        public PhyVertex[]? Vertices;

        public uint NTriCount = 0;
        public uint ATriCount = 0;
        public ushort[]? Indices;

        public string TextureName = "";
        public int TexId = -1;
        public int Tex2Id = -1;
        public Vector3 BoxMin = Vector3.Zero;
        public Vector3 BoxMax = Vector3.Zero;

        public C3Motion? motion = null;

        public float A = 1.0f;
        public float R = 1.0f;
        public float G = 1.0f;
        public float B = 1.0f;

        public C3Key? Key = null;
        public bool Draw = true;

        public uint TextureRow = 1;

        public Matrix? InitMatrix = null;

        public Vector2? uvStep = new Vector2() { X = 0, Y = 0 };
    }
}
