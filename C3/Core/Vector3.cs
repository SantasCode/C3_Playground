namespace C3.Core
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3 Zero => new Vector3 { X = 0, Y = 0, Z = 0 };
        public float[] ToArray() => new[] { X, Y, Z };
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3() { }
        public float Length()
        {
            return (float)Math.Sqrt(X* X + Y * Y + Z * Z);
        }

        public Vector3 Transform(Matrix matrix)
        {
            var x = (X * matrix.M11) + (Y * matrix.M21) + (Z * matrix.M31) + matrix.M41;
            var y = (X * matrix.M12) + (Y * matrix.M22) + (Z * matrix.M32) + matrix.M42;
            var z = (X * matrix.M13) + (Y * matrix.M23) + (Z * matrix.M33) + matrix.M43;
            return new(x, y, z);
        }
    }
}
