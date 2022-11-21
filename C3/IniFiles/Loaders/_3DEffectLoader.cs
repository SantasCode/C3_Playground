using C3.IniFiles.FileSet;
using IniParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles.Loaders
{
    internal class _3DEffectLoader
    {
        internal static Dictionary<string, _3DEffect> Load(TextReader tr)
        {
            Dictionary<string, _3DEffect> result = new();

            IniDataParser parser = new IniDataParser();
            IniData data = parser.Parse(tr);

            foreach (var dataPiece in data.Sections)
            {
                _3DEffect effect = new();
                effect.RolePartInfo = new ();

                 effect.Name = dataPiece.Name;

                effect.RolePartInfo.PartCount = uint.Parse(dataPiece.Properties["Amount"]);

                for (int i = 0; i < effect.RolePartInfo.PartCount; i++)
                {
                    effect.RolePartInfo.Parts.Add(new()
                    {
                        MeshId = uint.Parse(dataPiece.Properties[$"EffectId{i}"]),
                        TextureId = uint.Parse(dataPiece.Properties[$"TextureId{i}"]),
                        Asb = uint.Parse(dataPiece.Properties[$"ASB{i}"]),
                        Adb = uint.Parse(dataPiece.Properties[$"ADB{i}"]),
                    });
                }
                result.Add(effect.Name, effect);
            }

            return result;
        }
    }
}
