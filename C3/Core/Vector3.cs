namespace C3.Core
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3 Zero => new Vector3 { X = 0, Y = 0, Z = 0 };
    }
}
