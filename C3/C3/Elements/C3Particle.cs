using C3.Core;

namespace C3.Elements
{
    public  class C3Particle
    {
        public string? Name;
        public ParticleVertex[]? Vertices;
        public ushort[]? Indicies;
        public int TextureId;
        public string? TextureName;

        public uint Count;
        public uint Row;
        public ParticleFrame[]? ParticleFrames;
        public int Frame;
        public uint FrameCount;

        public Matrix? Matrix;
    }
}
