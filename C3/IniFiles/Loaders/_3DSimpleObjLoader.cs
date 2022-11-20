using C3.IniFiles.FileSet;
using IniParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Loaders
{
    internal class _3DSimpleObjLoader
    {
        internal static Dictionary<ulong, RolePartInfo> Load(TextReader tr)
        {
            Dictionary<ulong, RolePartInfo> result = new();

            IniDataParser parser = new IniDataParser();
            IniData data = parser.Parse(tr);

            foreach (var dataPiece in data.Sections)
            {
                RolePartInfo rolePartInfo = new();

                ulong Id = ulong.Parse(dataPiece.Name.Replace("ObjIDType", ""));

                rolePartInfo.PartCount = uint.Parse(dataPiece.Properties["PartAmount"]);

                for (int i = 0; i < rolePartInfo.PartCount; i++)
                {
                    rolePartInfo.Parts.Add(new()
                    {
                        MeshId = ulong.Parse(dataPiece.Properties[$"Part{i}"]),
                        TextureId = ulong.Parse(dataPiece.Properties[$"Texture{i}"]),
                    });
                }
                result.Add(Id, rolePartInfo);
            }

            return result;
        }
    }
}
