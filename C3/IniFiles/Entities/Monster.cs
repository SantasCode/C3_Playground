using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Entities
{
    public class Monster
    {
        public string Name { get; set; }
        public List<(string, string)> BaseModel { get; set; } = new();
        public Dictionary<string, string> Motion { get; set; } = new();
    }
}
