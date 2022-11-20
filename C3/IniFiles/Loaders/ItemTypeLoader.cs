using C3.IniFiles.Entities;
using C3.IniFiles.FileSet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace C3.IniFiles.Loaders
{
    public static class ItemTypeLoader
    {
        public static Dictionary<string, uint> Load(string filePath)
        {
            Dictionary<string, uint> result = new();
            Span<byte> byteContents = File.ReadAllBytes(filePath);
            COEncryptedFile file = new();
            file.Decrypt(byteContents);


            using (TextReader tr = new StreamReader(new MemoryStream(byteContents.ToArray())))
            {
                tr.ReadLine();//Amount

                string? line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] parts = line.Split(' ');
                    if(parts.Length > 2)
                    {
                        if (uint.Parse(parts[0]) >= 1_000_000) continue;
                        if (!result.ContainsKey(parts[1]))
                        {
                            result.Add(parts[1], uint.Parse(parts[0]) / 10 * 10);
                        }
                    }
                }
            }

            return result;
        }
    }
}
