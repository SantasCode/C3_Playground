using C3.Core;

namespace C3
{
    public  class ParticleFrame
    {
        public uint Count { get; set; }
        public Vector3[] Position { get; set; }
        public float[] Age { get; set; }
        public float[] Size { get; set; }
        public Matrix Matrix { get; set; }
    }
}
