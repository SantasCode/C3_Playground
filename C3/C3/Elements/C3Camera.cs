using C3.Core;


namespace C3.Elements
{
    public class C3Camera
    {
        public string? Name;
        public Vector3[]? From;
        public Vector3[]? To;
        public float Near;
        public float Far;
        public float Fov;

        public uint FrameCount;
        public int Frame;
    }
}
