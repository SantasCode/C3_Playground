using C3.Core;

namespace C3.Elements
{
    public class C3Particle
    {
        public string Name { get; set; }
        public ParticleVertex[] Vertices { get; set; }
        public ushort[]? Indicies { get; set; }
        public int TextureId { get; set; }
        public string? TextureName { get; set; }

        public uint Count { get; set; }
        public uint Row { get; set; }
        public ParticleFrame[] ParticleFrames { get; set; }
        public int Frame { get; set; }
        public uint FrameCount { get; set; }

        public Matrix? Matrix { get; set; }
    }
}
