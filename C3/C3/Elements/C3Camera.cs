using C3.Core;


namespace C3.Elements
{
    public class C3Camera
    {
        public string Name { get; set; }
        public Vector3[] From { get; set; }
        public Vector3[] To { get; set; }
        public float Near { get; set; }
        public float Far { get; set; }
        public float Fov { get; set; }

        public uint FrameCount { get; set; }
        public int Frame { get; set; }
    }
}
