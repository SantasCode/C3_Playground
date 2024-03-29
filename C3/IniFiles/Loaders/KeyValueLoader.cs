﻿using IniParser;
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
        internal static Dictionary<ulong, string> Load(TextReader tr)
        {
            Dictionary<ulong, string> result = new();

            string? line = null;
            while ((line = tr.ReadLine()) != null)
            {
                string[] parts = line.Split("=");
                if(parts.Count() != 2)
                    continue;
                result.Add(ulong.Parse(parts[0]), parts[1]);
            }

            return result;
        }
    }
}
