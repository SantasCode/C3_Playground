
namespace C3.Core
{
    public class Matrix
    {
        public float M11 { get; set; }
        public float M12 { get; set; }
        public float M13 { get; set; } 
        public float M14 { get; set; } 
        public float M21 { get; set; } 
        public float M22 { get; set; } 
        public float M23 { get; set; } 
        public float M24 { get; set; } 
        public float M31 { get; set; } 
        public float M32 { get; set; } 
        public float M33 { get; set; } 
        public float M34 { get; set; } 
        public float M41 { get; set; } 
        public float M42 { get; set; } 
        public float M43 { get; set; } 
        public float M44 { get; set; }

        public static Matrix Identity => new Matrix() { M11 = 1, M22 = 1, M33 = 1, M44 = 1 };

        /// <summary>
        /// Source: MonoGame https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Matrix.cs#L694
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="result"></param>
        public static Matrix CreateFromQuaternion(Quaternion quaternion)
        {
            var result = new Matrix();

            float num9 = quaternion.X * quaternion.X;
            float num8 = quaternion.Y * quaternion.Y;
            float num7 = quaternion.Z * quaternion.Z;
            float num6 = quaternion.X * quaternion.Y;
            float num5 = quaternion.Z * quaternion.W;
            float num4 = quaternion.Z * quaternion.X;
            float num3 = quaternion.Y * quaternion.W;
            float num2 = quaternion.Y * quaternion.Z;
            float num = quaternion.X * quaternion.W;
            result.M11 = 1f - (2f * (num8 + num7));
            result.M12 = 2f * (num6 + num5);
            result.M13 = 2f * (num4 - num3);
            result.M14 = 0f;
            result.M21 = 2f * (num6 - num5);
            result.M22 = 1f - (2f * (num7 + num9));
            result.M23 = 2f * (num2 + num);
            result.M24 = 0f;
            result.M31 = 2f * (num4 + num3);
            result.M32 = 2f * (num2 - num);
            result.M33 = 1f - (2f * (num8 + num9));
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;

            return result;
        }
    }
}
