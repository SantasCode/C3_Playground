using C3;
using C3.Core;
using C3_Playground.CommandAttributes;
using Cocona;

namespace C3_Playground.Commands
{
    internal class Test
    {
        [Command("mesh")]
        public void LoadMesh([Argument][FileExists] string file, [Option('v')] bool verbose = false)
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
                    foreach (var index in BoneIndexes)
                    {
                        Console.WriteLine(index);
                    }
                    Console.WriteLine("Enter to do next");
                    Console.Read();
                }

            }
        }

        [Command("info")]
        public void PrintInfo ([Argument][FileExists] string file)
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

            for (int i = 0; i < myModel.Meshs.Count; i++)
            {
                Console.WriteLine($"PHY-->{myModel.Meshs[i].Name}, Bone Count: {myModel.Animations[i].BoneCount}");
            }
            for (int i = 0; i < myModel.Animations.Count; i++)
            {
                Console.WriteLine($"MOTI-->Index: {i} Bone Count: {myModel.Animations[i].BoneCount}");
            }
        }

        [Command("meshes")]
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

        [Command("vbody")]
        public void C3_Testvbody([Argument][DirectoryExists] string fileDir, [Option('v')] bool verbose = false)
        {
            int count = 0;
            GameData game = new GameData(fileDir);
            var armors = game.GetArmorC3();
            foreach (var armor in armors)
            {
                count++;
                if (verbose) Console.WriteLine($"{armor}");
                using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(fileDir, armor))))
                {

                    C3Model? model = C3ModelLoader.Load(br, verbose);
                    if (model == null)
                    {
                        Console.WriteLine($"null model file - {armor}");
                        continue;
                    }

                    var body = model.Meshs.Where(p => p.Name.ToLower() == "v_body").FirstOrDefault();
                    if (body == null)
                        continue;

                    //Has vbody.
                    if (model.Meshs.Count < 4)
                        Console.WriteLine($"Model has vbody with insuffecient parts. - {new FileInfo(armor).Name}");
                }
            }
            Console.WriteLine($"Finished reading {count} c3 files");
        }

        [Command("matrices")]
        public void C3_TestLoadMatrices([Argument][DirectoryExists] string fileDir, [Option('v')] bool verbose = false)
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(fileDir, "*.c3", SearchOption.AllDirectories))
            {
                count++;
                using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                {

                    C3Model? model = C3ModelLoader.Load(br, verbose);
                    if (model == null)
                    {
                        Console.WriteLine($"null model file - {file}");
                        continue;
                    }

                    if (model.Meshs.Count > 0)
                    {
                        foreach (var mesh in model.Meshs)
                        {
                            if (!Matrix.Identity.Equals(mesh.InitMatrix))
                                Console.WriteLine($"InitMatrix is not identity: {mesh.Name} - {file}");

                        }
                    }

                }
            }
            Console.WriteLine($"Finished reading {count} c3 files");
        }

        [Command("ini")]
        public void Ini_LoadTest([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type == C3.IniFiles.Entities.ItemType.Armor) continue;
                if (item.Type == C3.IniFiles.Entities.ItemType.Helmet) continue;

                foreach (var model in item.BaseModel)
                {
                    C3Model? mesh = null;
                    using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, model.Value.Item1))))
                        mesh = C3ModelLoader.Load(br, false);

                    if (mesh == null) continue;

                    if (mesh.Meshs.Count > 1)
                        Console.WriteLine($"Item ({item.Name}/{model.Value.Item1}) has more than 1 mesh");

                }
            }
        }
    }
}
