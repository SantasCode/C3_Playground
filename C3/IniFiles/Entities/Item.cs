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
        Bow,
        Shield,
        Armor,
        Helmet,
        Other
    }
    public class Item
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public Dictionary<uint, (string, string)> BaseModel { get; set; } = new();

        public (string, string)? Get(uint BodyType = 0)
        {
            if (BaseModel.ContainsKey(BodyType))
            {
                return (BaseModel[BodyType]);
            }
            return null;
        }
    }
}
