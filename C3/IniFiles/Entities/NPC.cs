using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Entities
{
    public class NPC
    {
        public List<(string, string)> BaseModel { get; set; } = new();
        public Dictionary<string, string> Motion { get; set; } = new();
        public Effect Effect { get; set; }
    }
}
