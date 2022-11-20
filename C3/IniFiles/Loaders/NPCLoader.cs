using C3.IniFiles.FileSet;
using IniParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Loaders
{
    internal class NPCLoader
    {
        internal static Dictionary<uint, Npc> Load(TextReader tr)
        {
            Dictionary<uint, Npc> result = new();

            IniDataParser parser = new IniDataParser();
            parser.Configuration.AllowDuplicateSections = true;
            parser.Configuration.DuplicatePropertiesBehaviour = IniParser.Configuration.IniParserConfiguration.EDuplicatePropertiesBehaviour.AllowAndKeepLastValue;
            IniData data = parser.Parse(tr);

            foreach (var dataPiece in data.Sections)
            {
                uint TypeId = uint.Parse(dataPiece.Name.Replace("NpcType", "").Replace("Npctype", ""));
                Npc npc = new();

                npc.Name = dataPiece.Properties["Name"];
                npc.SimpleObjId = ulong.Parse(dataPiece.Properties["SimpleObjID"]);
                npc.StandyByMotionId = ulong.Parse(dataPiece.Properties["StandByMotion"]);
                npc.BlazeByMotionId = ulong.Parse(dataPiece.Properties["BlazeMotion"]);
                npc.RestByMotionId = ulong.Parse(dataPiece.Properties["RestMotion"]);
                npc.Effect = dataPiece.Properties["Effect"];

                if (result.ContainsKey(TypeId)) TypeId += 1000;
                result.Add(TypeId, npc);
            }

            return result;
        }
    }
}
