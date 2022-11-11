using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace C3_Playground
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
    }
}
