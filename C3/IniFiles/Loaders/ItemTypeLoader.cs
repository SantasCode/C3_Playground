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
                        uint itemId = uint.Parse(parts[0]);

                        ItemType type = (itemId / 1_000) switch
                        {
                            >= 111 and <= 116 => ItemType.Helmet,
                            >= 130 and <= 137 => ItemType.Armor,
                            >= 181 and <= 191 => ItemType.Armor,
                            >= 410 and <= 490 => ItemType.OneHander,
                            >= 500 and <= 580 => ItemType.TwoHander,
                            601 => ItemType.OneHander,
                            900 => ItemType.Shield,
                            _ => ItemType.Other
                        };

                        if (type == ItemType.Other) continue;

                        if (!result.ContainsKey(parts[1]))
                        {
                            switch (type)
                            {
                                case ItemType.Armor:
                                case ItemType.Helmet:
                                    result.Add(parts[1], itemId / 10 * 10);
                                    break;
                                default:
                                    result.Add(parts[1], itemId);
                                    break;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
