using IniParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Loaders
{
    internal class KeyValueLoader
    {
        internal static Dictionary<uint, string> Load(TextReader tr)
        {
            Dictionary<uint, string> result = new();

            string? line = null;
            while ((line = tr.ReadLine()) != null)
            {
                string[] parts = line.Split("=");
                if(parts.Count() != 2)
                    continue;
                result.Add(uint.Parse(parts[0]), parts[1]);
            }

            return result;
        }
    }
}
