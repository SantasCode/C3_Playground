using System.Security.Cryptography.X509Certificates;

namespace C3.Core
{
    public class Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public static Vector4 Zero => new Vector4 { X = 0, Y = 0, Z = 0, W = 0 };
        public float[] ToArray() => new[] { X, Y, Z, W };
        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        public Vector4() { }
    }
}
