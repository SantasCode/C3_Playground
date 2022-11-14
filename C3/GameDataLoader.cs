using C3.IniFiles;
using C3.IniFiles.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3
{
    public static class GameDataLoader
    {
        public static GameData Load(string Directory)
        {
            GameData gameData = new();


            Dictionary<uint, string> textures = new();
            Dictionary<uint, string> modelObj = new();

            if (Exists(Directory, "ini/3dtexture.ini"))
                using (TextReader tr = new StreamReader(Path.Combine(Directory, "ini/3dtexture.ini")))
                    textures = KeyValueLoader.Load(tr);

            if (Exists(Directory, "ini/3dobj.ini"))
                using (TextReader tr = new StreamReader(Path.Combine(Directory, "ini/3dobj.ini")))
                    modelObj = KeyValueLoader.Load(tr);

            LoadArmet(Directory, ref gameData, textures, modelObj);
            LoadArmor(Directory, ref gameData, textures, modelObj);
            LoadWeapon(Directory, ref gameData, textures, modelObj);
            LoadMount(Directory, ref gameData, textures, modelObj);


            //Need to define (Monsters, Players, Mounts) , NPCs, (Weapons, Armor, Armet), Effects


            return gameData;
        }

        private static bool Exists(string Directory, string path) => File.Exists(Path.Combine(Directory, path));

        private static void LoadArmet(string directory, ref GameData gameData, Dictionary<uint, string> textures, Dictionary<uint, string> modelobj)
        {
            Dictionary<uint, RolePartInfo> armetInfo = new();

            if (Exists(directory, "ini/armet.ini"))
                using (TextReader tr = new StreamReader(Path.Combine(directory, "ini/armet.ini")))
                    armetInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            else
                Console.WriteLine($"[GameDataLoader] Failed to load ini/armet.ini");
        }

        private static void LoadArmor(string directory, ref GameData gameData, Dictionary<uint, string> textures, Dictionary<uint, string> modelobj)
        {
            Dictionary<uint, RolePartInfo> armorInfo = new();

            if (Exists(directory, "ini/armor.ini"))
                using (TextReader tr = new StreamReader(Path.Combine(directory, "ini/armor.ini")))
                    armorInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            else
                Console.WriteLine($"[GameDataLoader] Failed to load ini/armor.ini");
        }

        private static void LoadWeapon(string directory, ref GameData gameData, Dictionary<uint, string> textures, Dictionary<uint, string> modelobj)
        {
            Dictionary<uint, RolePartInfo> weaponInfo = new();

            if (Exists(directory, "ini/weapon.ini"))
                using (TextReader tr = new StreamReader(Path.Combine(directory, "ini/weapon.ini")))
                    weaponInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            else
                Console.WriteLine($"[GameDataLoader] Failed to load ini/weapon.ini");
        }

        private static void LoadMount(string directory, ref GameData gameData, Dictionary<uint, string> textures, Dictionary<uint, string> modelobj)
        {
            Dictionary<uint, RolePartInfo> mountInfo = new();

            if (Exists(directory, "ini/mount.ini"))
                using (TextReader tr = new StreamReader(Path.Combine(directory, "ini/mount.ini")))
                    mountInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            else
                Console.WriteLine($"[GameDataLoader] Failed to load ini/mount.ini");
        }
    }
}
