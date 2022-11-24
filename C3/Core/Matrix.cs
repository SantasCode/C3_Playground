
using System;
using System.Numerics;
using System.Xml.XPath;

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
        public float[] ToArray() => new[] { M11, M12, M13, M14, M21, M22, M23, M24, M31, M32, M33, M34, M41, M42, M43, M44 };

        public static Matrix Identity => new Matrix() { M11 = 1, M22 = 1, M33 = 1, M44 = 1 };
        public Matrix() { }
        public Matrix(Matrix m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;
            M14 = m.M14;
            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;
            M24 = m.M24;
            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
            M34 = m.M34;
            M41 = m.M41;
            M42 = m.M42;
            M43 = m.M43;
            M44 = m.M44;
        }
        public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M14 = m14;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M24 = m24;
            M31 = m31;
            M32 = m32;
            M33 = m33;
            M34 = m34;
            M41 = m41;
            M42 = m42;
            M43 = m43;
            M44 = m44;
        }

        public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
        {
            var m11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            var m12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            var m13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            var m14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            var m21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            var m22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            var m23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            var m24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            var m31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            var m32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            var m33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            var m34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            var m41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            var m42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            var m43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            var m44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
            Matrix ret = new Matrix();
            ret.M11 = m11;
            ret.M12 = m12;
            ret.M13 = m13;
            ret.M14 = m14;
            ret.M21 = m21;
            ret.M22 = m22;
            ret.M23 = m23;
            ret.M24 = m24;
            ret.M31 = m31;
            ret.M32 = m32;
            ret.M33 = m33;
            ret.M34 = m34;
            ret.M41 = m41;
            ret.M42 = m42;
            ret.M43 = m43;
            ret.M44 = m44;
            return ret;
        }

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
        public static Matrix quatToMatrix(Quaternion q)
        {
            Matrix res = Matrix.Identity;
            float sqw = q.W * q.W;
            float sqx = q.X * q.X;
            float sqy = q.Y * q.Y;
            float sqz = q.Z * q.Z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            float invs = 1 / (sqx + sqy + sqz + sqw);
            res.M11 = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
            res.M22 = (-sqx + sqy - sqz + sqw) * invs;
            res.M33 = (-sqx - sqy + sqz + sqw) * invs;

            float tmp1 = q.X * q.Y;
            float tmp2 = q.Z * q.W;
            res.M21 = 2.0f * (tmp1 + tmp2) * invs;
            res.M12 = 2.0f * (tmp1 - tmp2) * invs;

            tmp1 = q.X * q.Z;
            tmp2 = q.Y * q.W;
            res.M31 = 2.0f * (tmp1 - tmp2) * invs;
            res.M13 = 2.0f * (tmp1 + tmp2) * invs;
            tmp1 = q.Y * q.Z;
            tmp2 = q.X * q.W;
            res.M32 = 2.0f * (tmp1 + tmp2) * invs;
            res.M23 = 2.0f * (tmp1 - tmp2) * invs;

            return res;
        }

        public static Matrix CreateFromTranslation(Vector3 translation)
        {
            Matrix result = Matrix.Identity;
            result.M14 = translation.X;
            result.M24 = translation.Y;
            result.M34 = translation.Z;
            return result;
        }

        public static Matrix CreateFromScale(Vector3 scale)
        {
            Matrix result = Matrix.Identity;
            result.M11 = scale.X;
            result.M22 = scale.Y;
            result.M33 = scale.Z;
            return result;
        }

        public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            translation = new();
            translation.X = this.M41;
            translation.Y = this.M42;
            translation.Z = this.M43;

            float xs = (Math.Sign(M11 * M12 * M13 * M14) < 0) ? -1 : 1;
            float ys = (Math.Sign(M21 * M22 * M23 * M24) < 0) ? -1 : 1;
            float zs = (Math.Sign(M31 * M32 * M33 * M34) < 0) ? -1 : 1;

            scale = new();
            scale.X = xs * (float)Math.Sqrt(this.M11 * this.M11 + this.M12 * this.M12 + this.M13 * this.M13);
            scale.Y = ys * (float)Math.Sqrt(this.M21 * this.M21 + this.M22 * this.M22 + this.M23 * this.M23);
            scale.Z = zs * (float)Math.Sqrt(this.M31 * this.M31 + this.M32 * this.M32 + this.M33 * this.M33);

            if (scale.X == 0.0 || scale.Y == 0.0 || scale.Z == 0.0)
            {
                rotation = Quaternion.Identity;
                return false;
            }

            Matrix m1 = new Matrix(this.M11 / scale.X, M12 / scale.X, M13 / scale.X, 0,
                                   this.M21 / scale.Y, M22 / scale.Y, M23 / scale.Y, 0,
                                   this.M31 / scale.Z, M32 / scale.Z, M33 / scale.Z, 0,
                                   0, 0, 0, 1);

            rotation = Quaternion.CreateFromRotationMatrix(m1);
            return true;
        }

        /// <summary>
        /// Column Major Compose
        /// </summary>
        /// <param name="position"></param>
        /// <param name="quaternion"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Matrix Compose(Vector3 position, Quaternion quaternion, Vector3 scale )
        {
            Matrix m = new();
            float x = quaternion.X, y = quaternion.Y, z = quaternion.Z, w = quaternion.W;
            float x2 = x + x, y2 = y + y, z2 = z + z;
            float xx = x * x2, xy = x * y2, xz = x * z2;
            float yy = y * y2, yz = y * z2, zz = z * z2;
            float wx = w * x2, wy = w * y2, wz = w * z2;

            float sx = scale.X, sy = scale.Y, sz = scale.Z;

            m.M11 = (1 - (yy + zz)) * sx;
            m.M21 = (xy + wz) * sx;
            m.M31 = (xz - wy) * sx;
            m.M41 = 0;

            m.M12 = (xy - wz) * sy;
            m.M22= (1 - (xx + zz)) * sy;
            m.M32= (yz + wx) * sy;
            m.M42= 0;

            m.M13 = (xz + wy) * sz;
            m.M23 = (yz - wx) * sz;
            m.M33 = (1 - (xx + yy)) * sz;
            m.M43 = 0;

            m.M14 = position.X;
            m.M24 = position.Y;
            m.M34 = position.Z;
            m.M44 = 1;

            return m;
        }
        
        /// <summary>
        /// Column Major Decompose
        /// </summary>
        /// <param name="position"></param>
        /// <param name="quaternion"></param>
        /// <param name="scale"></param>
        public void DecomposeCM(out Vector3 position, out Quaternion quaternion, out Vector3 scale)
        {
            position = new();
            scale = new();

            float sx = new Vector3(M11, M21, M31).Length();
            float sy = new Vector3(M12, M22, M32).Length();
            float sz = new Vector3(M13, M23, M33).Length();

            // if determine is negative, we need to invert one scale
            float det = Determinant();
            if (det < 0) sx = -sx;

            position.X = M14;
            position.Y = M24;
            position.Z = M34;

            // scale the rotation part
            Matrix _m1 = new Matrix(this);

            float invSX = 1 / sx;
            float invSY = 1 / sy;
            float invSZ = 1 / sz;

            _m1.M11 *= invSX;
            _m1.M21 *= invSX;
            _m1.M31 *= invSX;

            _m1.M12 *= invSY;
            _m1.M22 *= invSY;
            _m1.M32 *= invSY;

            _m1.M13 *= invSZ;
            _m1.M23 *= invSZ;
            _m1.M33 *= invSZ;

            quaternion = Quaternion.SetFromRotationMatrix(_m1);

            scale.X = sx;
            scale.Y = sy;
            scale.Z = sz;

        }

        /// <summary>
        /// Column Major
        /// </summary>
        /// <returns></returns>
        public float Determinant()
        {
            float n11 = M11, n12 = M12, n13 = M13, n14 = M14;
            float n21 = M21, n22 = M22, n23 = M23, n24 = M24;
            float n31 = M31, n32 = M32, n33 = M33, n34 = M34;
            float n41 = M41, n42 = M42, n43 = M43, n44 = M44;

            //TODO: make this more efficient
            //( based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm )

            return (
                n41 * (
                    +n14 * n23 * n32
                     - n13 * n24 * n32
                     - n14 * n22 * n33
                     + n12 * n24 * n33
                     + n13 * n22 * n34
                     - n12 * n23 * n34
                ) +
                n42 * (
                    +n11 * n23 * n34
                     - n11 * n24 * n33
                     + n14 * n21 * n33
                     - n13 * n21 * n34
                     + n13 * n24 * n31
                     - n14 * n23 * n31
                ) +
                n43 * (
                    +n11 * n24 * n32
                     - n11 * n22 * n34
                     - n14 * n21 * n32
                     + n12 * n21 * n34
                     + n14 * n22 * n31
                     - n12 * n24 * n31
                ) +
                n44 * (
                    -n13 * n22 * n31
                     - n11 * n23 * n32
                     + n11 * n22 * n33
                     + n13 * n21 * n32
                     - n12 * n21 * n33
                     + n12 * n23 * n31
                )

            );

        }
        public Matrix Transpose()
        {
            return Transpose(this);
        }
        public static Matrix Transpose(Matrix matrix)
        {
            Matrix ret = new();

            ret.M11 = matrix.M11;
            ret.M12 = matrix.M21;
            ret.M13 = matrix.M31;
            ret.M14 = matrix.M41;

            ret.M21 = matrix.M12;
            ret.M22 = matrix.M22;
            ret.M23 = matrix.M32;
            ret.M24 = matrix.M42;

            ret.M31 = matrix.M13;
            ret.M32 = matrix.M23;
            ret.M33 = matrix.M33;
            ret.M34 = matrix.M43;

            ret.M41 = matrix.M14;
            ret.M42 = matrix.M24;
            ret.M43 = matrix.M34;
            ret.M44 = matrix.M44;

            return ret;
        }
    }
}
