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

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains a transformation of 3d-vector by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">Source <see cref="Vector3"/>.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <returns>Transformed <see cref="Vector3"/>.</returns>
        public static Vector3 Transform(Vector3 position, Matrix matrix)
        {
            Transform(ref position, ref matrix, out position);
            return position;
        }

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains a transformation of 3d-vector by the specified <see cref="Matrix"/>.
        /// </summary>
        /// <param name="position">Source <see cref="Vector3"/>.</param>
        /// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
        /// <param name="result">Transformed <see cref="Vector3"/> as an output parameter.</param>
        public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
        {
            result = new();
            var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41;
            var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42;
            var z = (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }
    }
}
