using C3.Core;

namespace C3.Elements
{
    public class C3Particle
    {
        public string Name { get; set; }
        public string TextureName { get; set; }

        
        public uint Row { get; set; }
        public uint Count { get; set; }

        public uint FrameCount { get; set; }
        public ParticleFrame[] ParticleFrames { get; set; }
    }
}
