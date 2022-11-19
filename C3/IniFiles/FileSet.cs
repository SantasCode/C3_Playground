using C3.IniFiles.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace C3.IniFiles
{
    internal class FileSet
    {
        private readonly string _clientPath;
        internal Dictionary<uint, RolePartInfo> armetInfo = new();
        internal Dictionary<uint, RolePartInfo> armorInfo = new();
        internal Dictionary<uint, RolePartInfo> weaponInfo = new();
        internal Dictionary<uint, string> _3DObj = new();
        internal Dictionary<uint, string> _3DTexture = new();
        internal Dictionary<uint, string> _3DMotion = new();
        internal Dictionary<uint, string> _3DEffectObj = new();

        internal FileSet(string ClientRootPath)
        {
            _clientPath = ClientRootPath;
            //Load - 3deffect.ini
            //Load - npc.ini
            //Load - itemtype.dat
            //Load - mount.ini
            LoadArmor();
            LoadArmet();
            LoadWeapon();
            Load3DObj();
            Load3DTexture();




            Load3DMotion();
            Load3DEffectObj();
        }
        private void LoadArmor()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/armor.ini")))
                armorInfo = RolePartInfoLoader.Load(tr);
        }
        private void LoadArmet()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/armet.ini")))
                armetInfo = RolePartInfoLoader.Load(tr);
        }
        private void LoadWeapon()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/weapon.ini")))
                weaponInfo = RolePartInfoLoader.Load(tr);
        }
        private void Load3DObj()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dobj.ini")))
                _3DTexture = KeyValueLoader.Load(tr);
        }
        private void Load3DTexture()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dtexture.ini")))
                _3DTexture = KeyValueLoader.Load(tr);
        }
        private void Load3DEffect() { }
        private void LoadNpc() { }
        private void LoadItemType() { }
        private void LoadMount() { }
        private void Load3DMotion()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dmotion.ini")))
                _3DMotion = KeyValueLoader.Load(tr);
        }
        private void Load3DEffectObj()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3deffectobj.ini")))
                _3DEffectObj = KeyValueLoader.Load(tr);
        }
    }
}
