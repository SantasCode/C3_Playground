using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace C3.IniFiles.Loaders
{
    internal static class RolePartInfoLoader
    {
        internal static Dictionary<uint, RolePartInfo> Load(TextReader tr)
        {
            Dictionary<uint, RolePartInfo> result = new();

            IniDataParser parser = new IniDataParser();
            IniData data = parser.Parse(tr);

            foreach(var dataPiece in data.Sections)
            {
                RolePartInfo rolePartInfo = new RolePartInfo();

                uint Id = uint.Parse(dataPiece.Name);

                rolePartInfo.PartCount = uint.Parse(dataPiece.Properties["Part"]);
                
                for(int i = 0; i < rolePartInfo.PartCount; i++)
                {
                    rolePartInfo.Parts.Add(new()
                    {
                        MeshId = uint.Parse(dataPiece.Properties[$"Mesh{i}"]),
                        TextureId = uint.Parse(dataPiece.Properties[$"Texture{i}"])
                    });
                }
                result.Add(Id, rolePartInfo);
            }

            return result;
        }
    }
}
