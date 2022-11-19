using System;
using System.Collections.Generic;
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
        public string Mesh { get; set; }
        public string Texture { get; set; }
        public string Type { get; set; }
    }
}
