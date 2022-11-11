using System.Text;
using C3.Core;

namespace C3
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadASCIIString(this BinaryReader binaryReader, int Length)
        {
            string result = ASCIIEncoding.ASCII.GetString(binaryReader.ReadBytes(Length));
            int index = result.IndexOf('\0');
            if (index < 0)
                return result;
            return result.Substring(0, index);
        }
        public static string ReadASCIIString(this BinaryReader binaryReader, uint Length)
        {
            return binaryReader.ReadASCIIString((int)Length);
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3()
            {
                X = br.ReadSingle(),
                Y = br.ReadSingle(),
                Z = br.ReadSingle()
            };
        }
        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2()
            {
                X = br.ReadSingle(),
                Y = br.ReadSingle()
            };
        }

        public static Matrix ReadMatrix(this BinaryReader br)
        {
            return new Matrix()
            {
                M11 = br.ReadSingle(),
                M12 = br.ReadSingle(),
                M13 = br.ReadSingle(),
                M14 = br.ReadSingle(),
                M21 = br.ReadSingle(),
                M22 = br.ReadSingle(),
                M23 = br.ReadSingle(),
                M24 = br.ReadSingle(),
                M31 = br.ReadSingle(),
                M32 = br.ReadSingle(),
                M33 = br.ReadSingle(),
                M34 = br.ReadSingle(),
                M41 = br.ReadSingle(),
                M42 = br.ReadSingle(),
                M43 = br.ReadSingle(),
                M44 = br.ReadSingle()
            };
        }
        public static Quaternion ReadQuaternion(this BinaryReader br)
        {
            var q = new Quaternion();
            q.X = br.ReadSingle();
            q.Y = br.ReadSingle();
            q.Z = br.ReadSingle();
            q.W = br.ReadSingle();

            return q;
        }
        public static ChunkHeader ReadChunkHeader(this BinaryReader br)
        {
            return new ChunkHeader()
            {
                Id = br.ReadASCIIString(4),
                Size = br.ReadUInt32()
            };
        }
    }
}
