using C3.IniFiles.Loaders;

namespace C3.IniFiles.FileSet
{
    internal class FileSet
    {
        private readonly string _clientPath;

        internal Dictionary<ulong, RolePartInfo> Armet = new();
        internal Dictionary<ulong, RolePartInfo> Armor = new();
        internal Dictionary<ulong, RolePartInfo> Weapon = new();
        internal Dictionary<ulong, RolePartInfo> Mount = new();
        internal Dictionary<ulong, string> _3DObjs = new();
        internal Dictionary<ulong, string> _3DTextures = new();
        internal Dictionary<ulong, string> _3DMotions = new();
        internal Dictionary<ulong, string> MountMotions = new();
        internal Dictionary<ulong, string> _3DEffectObjs = new();
        internal Dictionary<string, _3DEffect> _3DEffects = new();
        internal Dictionary<uint, Npc> Npcs = new();
        internal Dictionary<string, uint> Itemtype = new();
        internal Dictionary<ulong, RolePartInfo> _3DSimpleObj = new();

        public FileSet(string ClientRootPath)
        {
            _clientPath = ClientRootPath;

            LoadArmor();
            LoadArmet();
            LoadWeapon();
            Load3DObj();
            Load3DTexture();
            Load3DEffect();
            LoadNpc();
            LoadItemType();
            LoadMount();
            Load3DMotion();
            LoadMountMotion();
            Load3DEffectObj();
            Load3DSimpleObj();
        }
        private void LoadArmor()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/armor.ini")))
                Armor = RolePartInfoLoader.Load(tr);
        }
        private void LoadArmet()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/armet.ini")))
                Armet = RolePartInfoLoader.Load(tr);
        }
        private void LoadWeapon()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/weapon.ini")))
                Weapon = RolePartInfoLoader.Load(tr);
        }
        private void Load3DObj()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dobj.ini")))
                _3DObjs = KeyValueLoader.Load(tr);
        }
        private void Load3DTexture()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dtexture.ini")))
                _3DTextures = KeyValueLoader.Load(tr);
        }
        private void Load3DEffect() 
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3deffect.ini")))
                _3DEffects = _3DEffectLoader.Load(tr);
        }
        private void LoadNpc() 
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/npc.ini")))
                Npcs = NPCLoader.Load(tr);
        }
        private void LoadItemType() 
        {
            Itemtype = ItemTypeLoader.Load(Path.Combine(_clientPath, "ini/itemtype.dat"));
        }
        private void LoadMount() 
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/mount.ini")))
                Mount = RolePartInfoLoader.Load(tr);
        }
        private void Load3DMotion()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dmotion.ini")))
                _3DMotions = KeyValueLoader.Load(tr);
        }
        private void LoadMountMotion()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/mountmotion.ini")))
                MountMotions = KeyValueLoader.Load(tr);
        }
        private void Load3DEffectObj()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3deffectobj.ini")))
                _3DEffectObjs = KeyValueLoader.Load(tr);
        }
        private void Load3DSimpleObj()
        {
            using (TextReader tr = new StreamReader(Path.Combine(_clientPath, "ini/3dSimpleObj.ini")))
                _3DSimpleObj = _3DSimpleObjLoader.Load(tr);
        }
    }
}
