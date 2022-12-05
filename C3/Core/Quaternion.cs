
namespace C3.Core
{
    public class Quaternion
    {
        public float X,Y,Z,W;
        public static Quaternion Identity => new Quaternion(0, 0, 0, 1);
        public float[] ToArray() => new[] { X, Y, Z, W };
        public Quaternion() { }
        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Quaternion CreateFromRotationMatrix(Matrix matrix)
        {
            float num8 = (matrix.M11 + matrix.M22) + matrix.M33;
            Quaternion quaternion = new Quaternion();
            if (num8 > 0f)
            {
                float num = (float)Math.Sqrt((double)(num8 + 1f));
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (matrix.M23 - matrix.M32) * num;
                quaternion.Y = (matrix.M31 - matrix.M13) * num;
                quaternion.Z = (matrix.M12 - matrix.M21) * num;
                return quaternion;
            }
            if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                float num7 = (float)Math.Sqrt((double)(((1f + matrix.M11) - matrix.M22) - matrix.M33));
                float num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (matrix.M12 + matrix.M21) * num4;
                quaternion.Z = (matrix.M13 + matrix.M31) * num4;
                quaternion.W = (matrix.M23 - matrix.M32) * num4;
                return quaternion;
            }
            if (matrix.M22 > matrix.M33)
            {
                float num6 = (float)Math.Sqrt((double)(((1f + matrix.M22) - matrix.M11) - matrix.M33));
                float num3 = 0.5f / num6;
                quaternion.X = (matrix.M21 + matrix.M12) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (matrix.M32 + matrix.M23) * num3;
                quaternion.W = (matrix.M31 - matrix.M13) * num3;
                return quaternion;
            }
            float num5 = (float)Math.Sqrt((double)(((1f + matrix.M33) - matrix.M11) - matrix.M22));
            float num2 = 0.5f / num5;
            quaternion.X = (matrix.M31 + matrix.M13) * num2;
            quaternion.Y = (matrix.M32 + matrix.M23) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (matrix.M12 - matrix.M21) * num2;

            return quaternion;

        }

        public Quaternion Normalize()
        {
            double n = Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
            return new Quaternion((float)(X / n), (float)(Y / n), (float)(Z / n), (float)(W / n));
        }

        /// <summary>
        /// Column Major Matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Quaternion SetFromRotationMatrix(Matrix m)
        {
            Quaternion q = new();
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm

            // assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)

            float m11 = m.M11, m12 = m.M12, m13 = m.M13,
                    m21 = m.M21, m22 = m.M22, m23 = m.M23,
                    m31 = m.M31, m32 = m.M32, m33 = m.M33;

            float trace = m11 + m22 + m33;

            if (trace > 0)
            {

                float s = (float)(0.5 / Math.Sqrt(trace + 1.0f));

                q.W = 0.25f / s;
                q.X = (m32 - m23) * s;
                q.Y = (m13 - m31) * s;
                q.Z = (m21 - m12) * s;

            }
            else if (m11 > m22 && m11 > m33)
            {

                float s = (float)(2.0 * Math.Sqrt(1.0 + m11 - m22 - m33));

                q.W = (m32 - m23) / s;
                q.X = 0.25f * s;
                q.Y = (m12 + m21) / s;
                q.Z = (m13 + m31) / s;

            }
            else if (m22 > m33)
            {

                float s = (float)(2.0 * Math.Sqrt(1.0 + m22 - m11 - m33));

                q.W = (m13 - m31) / s;
                q.X = (m12 + m21) / s;
                q.Y = 0.25f * s;
                q.Z = (m23 + m32) / s;

            }
            else
            {

                float s = (float)(2.0 * Math.Sqrt(1.0 + m33 - m11 - m22));

                q.W = (m21 - m12) / s;
                q.X = (m13 + m31) / s;
                q.Y = (m23 + m32) / s;
                q.Z = 0.25f * s;

            }
            return q;
        }
    }
}
