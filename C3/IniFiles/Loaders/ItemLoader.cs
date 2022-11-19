using C3.IniFiles.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace C3.IniFiles.Loaders
{
    public static class ItemLoader
    {
        

        public static Dictionary<string, Item> Load(string clientDirectory, Dictionary<uint, string> models, Dictionary<uint, string> texture)
        {
            Dictionary<string, Item> result = new();
            Dictionary<uint, RolePartInfo> armetInfo = new();
            Dictionary<uint, RolePartInfo> armorInfo = new();
            Dictionary<uint, RolePartInfo> weaponInfo = new();

            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/armet.ini")))
                armetInfo = RolePartInfoLoader.Load(tr);
            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/armor.ini")))
                armorInfo = RolePartInfoLoader.Load(tr);
            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/weapon.ini")))
                weaponInfo = RolePartInfoLoader.Load(tr);

            var itemIds = LoadItemTypeDat(Path.Join(clientDirectory, @"ini/itemtype.dat"));

            foreach (var itemInfo in itemIds)
            {
                //Check each item for each body type in each of the item types.
                for(uint body = 1; body <= 4; body++)
                {
                    if(armetInfo.ContainsKey(body * 1_000_000 + itemInfo.Value))
                    {
                        //Armet is always body specific
                        Item? item = null;
                        if(!result.TryGetValue(itemInfo.Key, out item))
                        {
                            item = new();
                            item.Name = itemInfo.Key;
                            item.Type = ItemType.Helmet;
                        }


                    }
                    else if (armorInfo.ContainsKey(body * 1_000_000 + itemInfo.Value))
                    {
                        //Armor is always body specific
                        Item? item = null;
                        if (!result.TryGetValue(itemInfo.Key, out item))
                        {
                            item = new();
                            item.Name = itemInfo.Key;
                            item.Type = ItemType.Armor;
                        }
                    }
                    else if (weaponInfo.ContainsKey(itemInfo.Value))
                    {
                        //No weapons are bodytype specific
                    }
                }
            }
            return result;
        }

        private static Dictionary<string, uint> LoadItemTypeDat(string filePath)
        {
            Dictionary<string, uint> result = new();
            Span<byte> byteContents = File.ReadAllBytes(filePath);
            COEncryptedFile file = new();
            file.Decrypt(byteContents);


            using (TextReader tr = new StreamReader(new MemoryStream(byteContents.ToArray())))
            {
                Console.WriteLine(tr.ReadLine());

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
