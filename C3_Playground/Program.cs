using C3;
using C3.Exports;
using C3.IniFiles;
using C3_Playground.CommandAttributes;
using C3_Playground.Preview.Model;
using Cocona;
using System.Text;
using System.Text.Json;

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
        [Command("preview")]
        public void Preview([Argument][FileExists] string file = "", [Argument][FileExists] string textureFile = "", [Option('w')] int Width = 1024, [Option('h')] int Height = 768)
        {
            using (var game = new Preview.RenderWindow(file, textureFile, Width, Height))
                game.Run();
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
                List<uint> BoneIndexes = new();
                foreach (var phy in model.Meshs)
                {
                    if (phy.Vertices != null)
                    {
                        foreach (var vert in phy.Vertices)
                        {
                        }
                    }
                    Console.WriteLine($"Phy - {phy.Name}");
                    BoneIndexes.Sort();
                    foreach(var index in BoneIndexes)
                    {
                        Console.WriteLine(index);
                    }
                    Console.WriteLine("Enter to do next");
                    Console.Read();
                }

            }
        }

        [Command("print-info")]
        public void C3_PrintMesh([Argument][FileExists] string file)
        {
            C3Model? myModel;
            using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                myModel = C3ModelLoader.Load(br);

            if (myModel == null) return;

            Console.WriteLine($"{file}");
            Console.WriteLine($"Mesh: {myModel.Meshs.Count} Anim: {myModel.Animations.Count}");
            Console.WriteLine($"Cameras: {myModel.Cameras.Count} Effects: {myModel.Effects.Count}");
            Console.WriteLine($"Shape: {myModel.Shapes.Count} Smotions: {myModel.ShapeMotions.Count}");

            if (myModel.Meshs.Count == myModel.Animations.Count)
                Console.WriteLine($"Equal parts of each.");

            for(int i = 0; i < myModel.Meshs.Count; i++)
            {
                Console.WriteLine($"PHY-->{myModel.Meshs[i].Name}, Bone Count: {myModel.Animations[i].BoneCount}");
            }
            for (int i = 0; i < myModel.Animations.Count; i++)
            {
                Console.WriteLine($"MOTI-->Index: {i} Bone Count: {myModel.Animations[i].BoneCount}");
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
            //Dictionary<uint, RolePartInfo> armetInfo = new();
            //Dictionary<uint, RolePartInfo> armorInfo = new();
            //Dictionary<uint, RolePartInfo> weaponInfo = new();
            //Dictionary<uint, RolePartInfo> mountInfo = new();

            //using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/armet.ini")))
            //    armetInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            //using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/armor.ini")))
            //    armorInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            //using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/weapon.ini")))
            //    weaponInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            //using (TextReader tr = new StreamReader(Path.Combine(clientDirectory, "ini/mount.ini")))
            //    mountInfo = C3.IniFiles.Loaders.RolePartInfoLoader.Load(tr);
            //C3.IniFiles.Loaders.ItemLoader.Load(@"S:\Programming\CO Dev\Clients\5165");
            Dictionary<string, uint> result = new();
            Span<byte> byteContents = File.ReadAllBytes(@"S:\Programming\CO Dev\Clients\5165\ini\monster.dat");
            COEncryptedFile file = new();
            file.Decrypt(byteContents);
            File.WriteAllText(@"C:\Temp\monster.txt", Encoding.ASCII.GetString(byteContents));
        }

        [Command("export-obj")]
        public void Export_obj([Argument][FileExists] string filePath, [Argument] string outputPath)
        {
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if(model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                using (TextWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    ObjExporter.Export(model, tw);
            }
        }

        [Command("export-json")]
        public void Export_json([Argument][FileExists] string filePath, [Argument] string outputPath)
        {
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if (model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                string jsonText = JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true });
                using (TextWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    tw.Write(jsonText);
            }
        }
    }
}