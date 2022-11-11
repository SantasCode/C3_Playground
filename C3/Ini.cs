using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3
{
    internal static class Ini
    {
        /*
        [000]
        HitEffect=zf2-e325
        HitSound = sound / boxing_h01.wav
        BlkEffect=zf2-e325
        BlkSound = sound / boxing_h01.wav
        */
        internal static bool IsKey(string line, out string? Id)
        {
            if(line.StartsWith("[") && line.EndsWith("]"))
            {
                Id = line.Substring(1, line.Length - 2);
                return true;
            }
            Id = null;
            return false;
        }
        internal static bool IsValue(string line, out string? Key, out string? Value)
        {
            if (line.Contains("="))
            {
                Key = line.Split('=')[0];
                Value = line.Split('=')[1];
                return true;
            }
            Key = null;
            Value = null;
            return false;
        }
    }
}
