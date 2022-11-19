using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Entities
{
    public enum ItemType
    {
        OneHander,
        TwoHander,
        Shield,
        Armor,
        Helmet
    }
    public class Item
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public Dictionary<uint, (string, string)> Models { get; set; } = new();

        public (string, string)? Get(uint BodyType)
        {
            if (Models.ContainsKey(BodyType))
            {
                return (Models[BodyType]);
            }
            return null;
        }
    }
}
