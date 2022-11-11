using Cocona;
using System.IO;
using C3_Playground.CommandAttributes;
using C3.IniFiles;
using C3.Entities;
using C3.Loaders;
using C3;
using System.Runtime.CompilerServices;

namespace C3_Playground
{
    class Program
    {
        static void Log(string message) => Console.WriteLine(message);
        static void Main(string[] args)
        {
                CoconaApp.Run<Program>(args, options =>
                {
                    options.TreatPublicMethodsAsCommands = false;
                });
        }

        [Command("test-mesh")]
        public void C3_TestLoadMesh([Argument][FileExists] string file, [Option('v')]bool verbose = false)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
            {

                C3Model? model = C3ModelLoader.Load(br, verbose);
                if (model == null)
                {
                    Console.WriteLine($"null model file {file}");
                    return;
                }

                foreach (var phy in model.Meshs)
                    Console.WriteLine($"Phy - {phy.Name}");

            }
        }


        [Command("test-meshes")]
        public void C3_TestLoadMeshes([Argument][DirectoryExists] string fileDir, [Option('v')] bool verbose = false)
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(fileDir, "*.c3", SearchOption.AllDirectories))
            {
                count++;
                if (verbose) Console.WriteLine($"{file}");
                using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                {

                    C3Model? model = C3ModelLoader.Load(br, verbose);
                    if (model == null)
                    {
                        Console.WriteLine($"null model file - {file}");
                        continue;
                    }

                    if (model.Meshs.Count != model.Animations.Count)
                        Console.WriteLine($"PHY MOTI Mismatch - {file}");
                    if (model.Effects.Count != model.Cameras.Count)
                        Console.WriteLine($"PTCL CAME Mismatch - {file}");
                    if (model.Effects.Count > 0)
                        Console.WriteLine($"Has Effects - {file}");

                }
            }
            Console.WriteLine($"Finished reading {count} c3 files");
        }

        [Command("test-ini")]
        public void Ini_LoadTest([Argument][DirectoryExists] string clientDirectory)
        {
            Dictionary<uint, RolePartInfo> armetInfo = new();
            Dictionary<uint, RolePartInfo> armorInfo = new();
            Dictionary<uint, RolePartInfo> weaponInfo = new();
            Dictionary<uint, RolePartInfo> mountInfo = new();

            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/armet.ini")))
                armetInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/armor.ini")))
                armorInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/weapon.ini")))
                weaponInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/mount.ini")))
                mountInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);

        }
    }
}