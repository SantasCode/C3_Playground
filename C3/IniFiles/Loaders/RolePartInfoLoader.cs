using IniParser;
using C3.IniFiles.FileSet;

namespace C3.IniFiles.Loaders
{
    internal static class RolePartInfoLoader
    {
        internal static Dictionary<ulong, RolePartInfo> Load(TextReader tr, bool LoadBlend = false)
        {
            Dictionary<ulong, RolePartInfo> result = new();

            IniDataParser parser = new IniDataParser();
            IniData data = parser.Parse(tr);

            foreach(var dataPiece in data.Sections)
            {
                RolePartInfo rolePartInfo = new RolePartInfo();

                ulong Id = ulong.Parse(dataPiece.Name);

                rolePartInfo.PartCount = uint.Parse(dataPiece.Properties["Part"]);
                
                for(int i = 0; i < rolePartInfo.PartCount; i++)
                {
                    rolePartInfo.Parts.Add(new()
                    {
                        MeshId = ulong.Parse(dataPiece.Properties[$"Mesh{i}"]),
                        TextureId = ulong.Parse(dataPiece.Properties[$"Texture{i}"]),
                    });
                }
                result.Add(Id, rolePartInfo);
            }

            return result;
        }
    }
}
