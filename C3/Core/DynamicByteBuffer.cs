using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Core
{
    internal class DynamicByteBuffer
    {

        private List<byte> buffer;
        //Buffer of unknown length at start.
        internal DynamicByteBuffer(int? Capacity = null)
        {
            if (Capacity == null)
                buffer = new();
            else
                buffer = new(Capacity.Value);

        }
        public int Count => buffer.Count;


        public byte[] ToArray() => buffer.ToArray();
        public string ToBase64() => Convert.ToBase64String(ToArray());


        public void Write(byte[] bytes)
        {
            foreach (var b in bytes)
                buffer.Add(b);
        }
        public void Write(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes);
        }
        public void Write(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes);
        }
        public void Write(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes);
        }
        public void Write(ushort value) 
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes);
        }
        public void Write(short value) 
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes);
        }
        public void Write(float[] value)
        {
            foreach (var f in value)
                Write(f);
        }
        public void Write(Matrix value) 
        {
            Write(value.ToArray());
        }
        public void Write(Vector2 value) { Write(value.ToArray()); }
        public void Write(Vector3 value) { Write(value.ToArray()); }
        public void Write(Vector4 value) { Write(value.ToArray()); }
        public void Write(Quaternion value) { Write(value.ToArray()); }


    }
}
